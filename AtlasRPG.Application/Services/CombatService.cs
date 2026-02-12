// AtlasRPG.Application/Services/CombatService.cs
using AtlasRPG.Core.Entities.Combat;
using AtlasRPG.Core.Entities.Runs;
using AtlasRPG.Core.Entities.GameData;
using AtlasRPG.Core.ValueObjects;
using Microsoft.EntityFrameworkCore;
using AtlasRPG.Core.Entities.Items;
using AtlasRPG.Infrastructure.Data;

namespace AtlasRPG.Application.Services
{
    public class CombatService
    {
        private readonly ApplicationDbContext _context;
        private readonly StatCalculatorService _statCalculator;
        private readonly DamageCalculatorService _damageCalculator;
        private readonly Random _random;

        public CombatService(
            ApplicationDbContext context,
            StatCalculatorService statCalculator,
            DamageCalculatorService damageCalculator)
        {
            _context = context;
            _statCalculator = statCalculator;
            _damageCalculator = damageCalculator;
            _random = new Random();
        }

        // ─────────────────────────────────────────────────────────────
        // PVP COMBAT  (Run vs Run)
        // ─────────────────────────────────────────────────────────────
        public async Task<CombatResult> SimulateCombat(
            Run playerRun,
            Run opponentRun,
            Guid activeSkillId)
        {
            var playerStats = await BuildStatsFromRun(playerRun);
            var opponentStats = await BuildStatsFromRun(opponentRun);

            var activeSkill = await _context.SkillDefinitions
                .FirstOrDefaultAsync(s => s.Id == activeSkillId);

            return RunCombatLoop(playerStats, opponentStats, activeSkill);
        }

        // ─────────────────────────────────────────────────────────────
        // PVE COMBAT  (Run vs PveMonster)
        // ─────────────────────────────────────────────────────────────
        public async Task<CombatResult> SimulatePveCombat(
            Run playerRun,
            PveMonster monster,
            Guid activeSkillId)
        {
            var playerStats = await BuildStatsFromRun(playerRun);

            // ✅ Monster'ı CharacterStats'a dönüştür
            // PVE zorluk çarpanı zaten PveEncounterService'te uygulandı
            var monsterStats = BuildStatsFromMonster(monster);

            var activeSkill = await _context.SkillDefinitions
                .FirstOrDefaultAsync(s => s.Id == activeSkillId);

            return RunCombatLoop(playerStats, monsterStats, activeSkill);
        }

        // ─────────────────────────────────────────────────────────────
        // ORTAK COMBAT LOOP
        // ─────────────────────────────────────────────────────────────
        private CombatResult RunCombatLoop(
            CharacterStats playerStats,
            CharacterStats opponentStats,
            SkillDefinition? activeSkill)
        {
            var combatResult = new CombatResult
            {
                MatchSeed = Guid.NewGuid().ToString("N")
            };

            // ── Passive referansları (kısa erişim) ───────────────────
            var pPb = playerStats.PassiveBonuses;   // player passive bonuses

            int playerSkillCooldown = 0;
            int opponentSkillCooldown = 0;
            int roundNumber = 1;
            const int MaxNormalRounds = 15;
            const int MaxTotalRounds = 50;

            bool playerActsFirst = playerStats.Initiative >= opponentStats.Initiative;

            // İlk cast flag'i (turn başı = her zaman true ilk skill için)
            bool playerFirstSkillUsed = false;

            // Eğer CD azaltması varsa effective CD hesapla
            int effectiveSkillCooldown = activeSkill != null
                ? ApplyPassiveCooldownReduction(activeSkill.Cooldown, pPb)
                : 0;

            // First Strike: initiative üstünlüğü olan taraf
            bool playerHasFirstStrike = playerActsFirst;

            // Hit tracking (Brace: turn içi ilk hit)
            bool playerFirstHitReceived = false;
            bool opponentFirstHitReceived = false;

            while (playerStats.CurrentHp > 0 && opponentStats.CurrentHp > 0 && roundNumber <= MaxTotalRounds)
            {
                var round = new CombatRound { RoundNumber = roundNumber };

                decimal playerDotDamage = SkillEffectApplier.TickStatusEffects(playerStats);
                decimal opponentDotDamage = SkillEffectApplier.TickStatusEffects(opponentStats);

                playerStats.CurrentHp -= playerDotDamage;
                opponentStats.CurrentHp -= opponentDotDamage;

                round.EventLog = $"{(double)playerDotDamage:F1}|{(double)opponentDotDamage:F1}";

                if (playerStats.CurrentHp <= 0 || opponentStats.CurrentHp <= 0)
                {
                    round.PlayerHpRemaining = playerStats.CurrentHp;
                    round.OpponentHpRemaining = opponentStats.CurrentHp;
                    combatResult.Rounds.Add(round);
                    break;
                }

                // ── Sudden Death (round 16+) ──────────────────────────
                if (roundNumber > MaxNormalRounds)
                {
                    combatResult.WasSuddenDeath = true;
                    int sdStacks = roundNumber - MaxNormalRounds;
                    combatResult.SuddenDeathStacks = sdStacks;
                    ApplySuddenDeath(playerStats, opponentStats, sdStacks);

                    round.PlayerHpRemaining = playerStats.CurrentHp;
                    round.OpponentHpRemaining = opponentStats.CurrentHp;
                    combatResult.Rounds.Add(round);

                    if (playerStats.CurrentHp <= 0 || opponentStats.CurrentHp <= 0)
                        break;

                    roundNumber++;
                    continue;
                }

                // ── Player auto-cast ──────────────────────────────────
                string playerAction = "BasicAttack";
                decimal playerMultiplier = 1.0m;

                if (activeSkill != null
                    && playerSkillCooldown == 0
                    && playerStats.CurrentMana >= activeSkill.ManaCost)
                {
                    playerAction = activeSkill.SkillId;
                    playerMultiplier = activeSkill.Multiplier;

                    // ── Mana cost (passive reduction) ─────────────────
                    decimal effectiveManaCost = activeSkill.ManaCost;
                    effectiveManaCost -= pPb.ManaCostReduction;
                    if (!playerFirstSkillUsed)
                        effectiveManaCost *= pPb.FirstSkillManaCostMult;
                    effectiveManaCost = Math.Max(0, effectiveManaCost);

                    playerStats.CurrentMana -= effectiveManaCost;
                    playerFirstSkillUsed = true;
                    playerSkillCooldown = effectiveSkillCooldown;

                    SkillEffectApplier.ApplyOnUse(
                        playerStats, opponentStats,
                        activeSkill.EffectJson,
                        actingFirst: playerActsFirst);
                }

                // ── Opponent her zaman basic attack ───────────────────
                const string opponentAction = "BasicAttack";
                const decimal opponentMultiplier = 1.0m;

                // ── Saldırı sırası ────────────────────────────────────
                if (playerActsFirst)
                {
                    ExecuteActionWithPassives(
                        playerStats, opponentStats,
                        playerAction, playerMultiplier,
                        round, isPlayer: true,
                        attackerGoesFirst: playerHasFirstStrike,
                        ref opponentFirstHitReceived,
                        activeSkill: playerAction != "BasicAttack" ? activeSkill : null); // ← EKLENDI

                    if (opponentStats.CurrentHp > 0 && !opponentStats.IsStunned) // ← IsStunned kontrolü
                    {
                        ExecuteActionWithPassives(
                            opponentStats, playerStats,
                            opponentAction, opponentMultiplier,
                            round, isPlayer: false,
                            attackerGoesFirst: !playerHasFirstStrike,
                            ref playerFirstHitReceived);
                    }
                }
                else
                {
                    ExecuteActionWithPassives(
                        opponentStats, playerStats,
                        opponentAction, opponentMultiplier,
                        round, isPlayer: false,
                        attackerGoesFirst: !playerHasFirstStrike,
                        ref playerFirstHitReceived);
                    // activeSkill yok

                    if (playerStats.CurrentHp > 0)
                        ExecuteActionWithPassives(
                            playerStats, opponentStats,
                            playerAction, playerMultiplier,
                            round, isPlayer: true,
                            attackerGoesFirst: playerHasFirstStrike,
                            ref opponentFirstHitReceived,
                            activeSkill: playerAction != "BasicAttack" ? activeSkill : null); // ← EKLENDI
                }

                playerStats.IsStunned = false;
                opponentStats.IsStunned = false;

                round.PlayerHpRemaining = playerStats.CurrentHp;
                round.OpponentHpRemaining = opponentStats.CurrentHp;
                combatResult.Rounds.Add(round);

                // ── Cooldown tick (round sonu) ────────────────────────
                if (playerSkillCooldown > 0) playerSkillCooldown--;
                if (opponentSkillCooldown > 0) opponentSkillCooldown--;

                roundNumber++;
            }

            // ── Sonuç ─────────────────────────────────────────────────
            bool playerAlive = playerStats.CurrentHp > 0;
            bool opponentAlive = opponentStats.CurrentHp > 0;

            combatResult.IsVictory = playerAlive && !opponentAlive;
            combatResult.TotalRounds = roundNumber - 1;

            combatResult.PlayerTotalDamageDealt = combatResult.Rounds.Sum(r => r.PlayerDamage);
            combatResult.PlayerTotalDamageTaken = combatResult.Rounds.Sum(r => r.OpponentDamage);
            combatResult.PlayerCriticalHits = combatResult.Rounds.Count(r => r.PlayerCrit);

            combatResult.OpponentTotalDamageDealt = combatResult.Rounds.Sum(r => r.OpponentDamage);
            combatResult.OpponentTotalDamageTaken = combatResult.Rounds.Sum(r => r.PlayerDamage);
            combatResult.OpponentCriticalHits = combatResult.Rounds.Count(r => r.OpponentCrit);

            return combatResult;
        }

        // ─────────────────────────────────────────────────────────────
        // YARDIMCI: Aksiyon çalıştır
        // ─────────────────────────────────────────────────────────────
        private void ExecuteActionWithPassives(
            CharacterStats attacker,
            CharacterStats defender,
            string actionName,
            decimal multiplier,
            CombatRound round,
            bool isPlayer,
            bool attackerGoesFirst,
            ref bool defenderFirstHitReceived,
            SkillDefinition? activeSkill = null)
        {
            var pPb = attacker.PassiveBonuses;
            var dPb = defender.PassiveBonuses;

            // ── Attacker-side damage multiplier ──────────────────────
            decimal bonusMult = 1.0m;

            // First Strike (N30 First Blood — sadece combat'ın ilk round'u)
            if (attackerGoesFirst)
                bonusMult *= pPb.FirstStrikeDamageMult;

            // Execution Window (N06) — hedef HP eşiğin altındaysa
            decimal defenderHpRatio = defender.MaxHp > 0
                ? defender.CurrentHp / defender.MaxHp
                : 0m;
            if (defenderHpRatio < pPb.ExecutionThreshold && pPb.ExecutionWindowMult > 1.0m)
                bonusMult *= pPb.ExecutionWindowMult;

            // vs Marked (N25 Hunted) — basit placeholder;
            // gerçek Mark status sistemi eklenince burada kontrol yapılacak
            // if (defenderIsMarked) bonusMult *= pPb.DamageVsMarked;

            decimal effectiveMultiplier = multiplier * bonusMult;

            // ── Standart hasar hesabı ─────────────────────────────────
            var action = _damageCalculator.CalculateAttack(
                attacker,
                defender,
                actionName,
                effectiveMultiplier);

            // ── Defender-side damage taken modifier ───────────────────
            if (action.DidHit && action.FinalDamage > 0)
            {
                // ✅ OnHit skill efektleri
                if (isPlayer && activeSkill != null)
                {
                    SkillEffectApplier.ApplyOnHit(
                        attacker, defender,
                        activeSkill.EffectJson,
                        attacker.BaseDamage);
                }

                // Mark varsa DamageTaken artır
                var markEffect = defender.StatusEffects.FirstOrDefault(e => e.Type == "Mark");
                if (markEffect != null)
                    action.FinalDamage *= markEffect.DamageTakenMult;

                decimal takenMult = 1.0m;

                // Brace (N12) — turn içi ilk hit
                if (!defenderFirstHitReceived && dPb.FirstHitDamageTakenMult < 1.0m)
                {
                    takenMult *= dPb.FirstHitDamageTakenMult;
                    defenderFirstHitReceived = true;
                }

                // Unyielding (N13) — düşük HP
                decimal defHpRatio = defender.MaxHp > 0
                    ? defender.CurrentHp / defender.MaxHp : 0m;
                if (defHpRatio < dPb.LowHpThreshold && dPb.LowHpDamageTakenMult < 1.0m)
                    takenMult *= dPb.LowHpDamageTakenMult;

                // Guarded (N39) — block başarılıysa
                if (action.WasBlocked && dPb.BlockSuccessDamageTakenMult < 1.0m)
                    takenMult *= dPb.BlockSuccessDamageTakenMult;

                action.FinalDamage *= takenMult;
            }

            // ── HP'yi güncelle ────────────────────────────────────────
            defender.CurrentHp -= action.FinalDamage;

            // ── Round'a yaz ───────────────────────────────────────────
            if (isPlayer)
            {
                round.PlayerAction = actionName;
                round.PlayerHit = action.DidHit;
                round.PlayerCrit = action.DidCrit;
                round.PlayerDamage = action.FinalDamage;
                round.PlayerBlocked = action.WasBlocked;
            }
            else
            {
                round.OpponentAction = actionName;
                round.OpponentHit = action.DidHit;
                round.OpponentCrit = action.DidCrit;
                round.OpponentDamage = action.FinalDamage;
                round.OpponentBlocked = action.WasBlocked;
            }
        }


        // ─────────────────────────────────────────────────────────────
        // YARDIMCI: Sudden Death hasarı
        // ─────────────────────────────────────────────────────────────
        private void ApplySuddenDeath(CharacterStats player, CharacterStats opponent, int stacks)
        {
            decimal playerDamage = _damageCalculator.CalculateSuddenDeathDamage(player.MaxHp, stacks);
            decimal opponentDamage = _damageCalculator.CalculateSuddenDeathDamage(opponent.MaxHp, stacks);

            player.CurrentHp -= playerDamage;
            opponent.CurrentHp -= opponentDamage;
        }

        // ─────────────────────────────────────────────────────────────
        // YARDIMCI: Run → CharacterStats (DB'den equipment yükler)
        // ─────────────────────────────────────────────────────────────
        private async Task<CharacterStats> BuildStatsFromRun(Run run)
        {
            var baseStat = await _context.BaseStatDefinitions
                .FirstOrDefaultAsync(b => b.Race == run.Race)
                ?? throw new InvalidOperationException($"BaseStatDefinition not found for race: {run.Race}");

            // ✅ DÜZELTME 1: .AsSplitQuery() kaldırıldı — tek sorgu olarak çalışır,
            // SQL Server'da WITH syntax hatası vermez
            var runWithData = await _context.Runs
                .Include(r => r.Equipment)
                    .ThenInclude(e => e.Weapon)
                    .ThenInclude(ri => ri!.Item)
                    .ThenInclude(i => i.Affixes)
                    .ThenInclude(a => a.AffixDefinition)
                .Include(r => r.Equipment)
                    .ThenInclude(e => e.Offhand)
                    .ThenInclude(ri => ri!.Item)
                    .ThenInclude(i => i.Affixes)
                    .ThenInclude(a => a.AffixDefinition)
                .Include(r => r.Equipment)
                    .ThenInclude(e => e.Armor)
                    .ThenInclude(ri => ri!.Item)
                    .ThenInclude(i => i.Affixes)
                    .ThenInclude(a => a.AffixDefinition)
                .Include(r => r.Equipment)
                    .ThenInclude(e => e.Belt)
                    .ThenInclude(ri => ri!.Item)
                    .ThenInclude(i => i.Affixes)
                    .ThenInclude(a => a.AffixDefinition)
                .Include(r => r.AllocatedPassives)
                // ✅ AsSplitQuery() KALDIRILDI
                .FirstOrDefaultAsync(r => r.Id == run.Id)
                ?? run;

            var allocatedNodeIds = runWithData.AllocatedPassives
                .Select(p => p.NodeId)
                .ToHashSet(); // HashSet — Contains O(1)

            List<PassiveNodeDefinition> allocatedDefs;

            if (allocatedNodeIds.Count > 0)
            {
                // ✅ DÜZELTME 2: Contains() kullanmak yerine tüm node'ları çek,
                // hafızada filtrele — 60 node küçük bir tablo, sıkıntı yok.
                // EF Core 8 List<string>.Contains() → OPENJSON CTE üretir → SQL Server hatası
                var allNodes = await _context.PassiveNodeDefinitions
                    .AsNoTracking()
                    .ToListAsync();

                allocatedDefs = allNodes
                    .Where(nd => allocatedNodeIds.Contains(nd.NodeId))
                    .ToList();
            }
            else
            {
                allocatedDefs = new List<PassiveNodeDefinition>();
            }

            return _statCalculator.CalculateRunStats(runWithData, baseStat, allocatedDefs);
        }

        private static int ApplyPassiveCooldownReduction(int baseCooldown, PassiveBonuses pb)
        {
            if (pb.CooldownReduction <= 0) return baseCooldown;
            if (baseCooldown < (int)pb.CooldownReductionThreshold) return baseCooldown;
            return Math.Max(2, baseCooldown - (int)pb.CooldownReduction);
        }

        // ─────────────────────────────────────────────────────────────
        // YARDIMCI: PveMonster → CharacterStats
        // ─────────────────────────────────────────────────────────────
        private static CharacterStats BuildStatsFromMonster(PveMonster monster)
        {
            return new CharacterStats
            {
                // HP / Mana
                MaxHp = monster.MaxHp,
                CurrentHp = monster.MaxHp,
                MaxMana = 0,       // Monster mana kullanmaz
                CurrentMana = 0,

                // Hasar — tüm hasar türleri aynı (monster build'i yok)
                MeleeDamage = monster.BaseDamage,
                RangedDamage = monster.BaseDamage,
                SpellDamage = monster.BaseDamage,
                IncreasedDamage = 1.0m,

                // Savunma
                Armor = monster.Armor,
                IncreasedArmor = 1.0m,
                Evasion = monster.Evasion,
                IncreasedEvasion = 1.0m,
                Ward = monster.Ward,
                IncreasedWard = 1.0m,

                // İsabet / Kritik / Blok
                Accuracy = monster.Accuracy,
                IncreasedAccuracy = 1.0m,
                CritChance = monster.CritChance,
                CritMultiplier = monster.CritMultiplier,
                BlockChance = monster.BlockChance,
                BlockReduction = 0.35m,

                // Tempo
                Initiative = monster.Initiative,

                // Resistances
                FireResist = monster.ResistFire,
                ColdResist = monster.ResistCold,
                LightningResist = monster.ResistLightning,
                ChaosResist = monster.ResistChaos,

                // ArmorPen yok
                ArmorPenetration = 0m
            };
        }
    }
}
