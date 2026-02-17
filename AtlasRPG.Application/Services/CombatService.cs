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

        // PVP COMBAT
        public async Task<CombatResult> SimulateCombat(
            Run playerRun, Run opponentRun, Guid activeSkillId)
        {
            var playerStats = await BuildStatsFromRun(playerRun);
            var opponentStats = await BuildStatsFromRun(opponentRun);
            var activeSkill = await _context.SkillDefinitions
                .FirstOrDefaultAsync(s => s.Id == activeSkillId);
            return RunCombatLoop(playerStats, opponentStats, activeSkill);
        }

        // PVE COMBAT
        public async Task<CombatResult> SimulatePveCombat(
            Run playerRun, PveMonster monster, Guid activeSkillId)
        {
            var playerStats = await BuildStatsFromRun(playerRun);
            var monsterStats = BuildStatsFromMonster(monster);
            var activeSkill = await _context.SkillDefinitions
                .FirstOrDefaultAsync(s => s.Id == activeSkillId);
            return RunCombatLoop(playerStats, monsterStats, activeSkill);
        }

        // ORTAK COMBAT LOOP
        private CombatResult RunCombatLoop(
            CharacterStats playerStats,
            CharacterStats opponentStats,
            SkillDefinition? activeSkill)
        {
            var combatResult = new CombatResult { MatchSeed = Guid.NewGuid().ToString("N") };
            var pPb = playerStats.PassiveBonuses;

            int playerSkillCooldown = 0;
            int opponentSkillCooldown = 0;
            int roundNumber = 1;
            const int MaxNormalRounds = 15;
            const int MaxTotalRounds = 50;

            bool playerActsFirst = playerStats.Initiative >= opponentStats.Initiative;
            bool playerFirstSkillUsed = false;
            int effectiveSkillCooldown = activeSkill != null
                ? ApplyPassiveCooldownReduction(activeSkill.Cooldown, pPb) : 0;

            bool playerHasFirstStrike = playerActsFirst;
            bool playerFirstHitReceived = false;
            bool opponentFirstHitReceived = false;

            // Stun sayaclari: > 0 iken o taraf aksiyonunu skip eder
            int playerStunRoundsRemaining = 0;
            int opponentStunRoundsRemaining = 0;

            while (playerStats.CurrentHp > 0 && opponentStats.CurrentHp > 0 && roundNumber <= MaxTotalRounds)
            {
                var round = new CombatRound { RoundNumber = roundNumber };

                // Sudden Death (round 16+)
                if (roundNumber > MaxNormalRounds)
                {
                    combatResult.WasSuddenDeath = true;
                    int sdStacks = roundNumber - MaxNormalRounds;
                    combatResult.SuddenDeathStacks = sdStacks;
                    ApplySuddenDeath(playerStats, opponentStats, sdStacks);
                    round.PlayerHpRemaining = playerStats.CurrentHp;
                    round.OpponentHpRemaining = opponentStats.CurrentHp;
                    combatResult.Rounds.Add(round);
                    if (playerStats.CurrentHp <= 0 || opponentStats.CurrentHp <= 0) break;
                    roundNumber++;
                    continue;
                }

                // Round basi DOT tick
                decimal playerDotDmg = SkillEffectApplier.TickStatusEffects(playerStats);
                decimal opponentDotDmg = SkillEffectApplier.TickStatusEffects(opponentStats);
                playerStats.CurrentHp -= playerDotDmg;
                opponentStats.CurrentHp -= opponentDotDmg;

                if (playerDotDmg > 0 || opponentDotDmg > 0)
                {
                    string dotLog = "";
                    if (playerDotDmg > 0) dotLog += $"DOT->Player: -{playerDotDmg:F0} | ";
                    if (opponentDotDmg > 0) dotLog += $"DOT->Opp: -{opponentDotDmg:F0}";
                    round.EventLog = AppendLog(round.EventLog, dotLog.TrimEnd(' ', '|'));
                }

                if (playerStats.CurrentHp <= 0 || opponentStats.CurrentHp <= 0)
                {
                    round.PlayerHpRemaining = playerStats.CurrentHp;
                    round.OpponentHpRemaining = opponentStats.CurrentHp;
                    combatResult.Rounds.Add(round);
                    break;
                }

                if (playerStunRoundsRemaining > 0 &&
                    !playerStats.StatusEffects.Any(e => e.Type == "Stun"))
                {
                    // Counter'ı sıfırla (StatusEffect zaten yok)
                    playerStunRoundsRemaining = 0;
                }
                if (opponentStunRoundsRemaining > 0 &&
                    !opponentStats.StatusEffects.Any(e => e.Type == "Stun"))
                {
                    // Counter'ı sıfırla
                    opponentStunRoundsRemaining = 0;
                }

                // Stun: bu roundda skip mi?
                bool playerIsStunned = playerStunRoundsRemaining > 0;
                bool opponentIsStunned = opponentStunRoundsRemaining > 0;

                // Player auto-cast
                string playerAction = "BasicAttack";
                decimal playerMultiplier = 1.0m;

                if (!playerIsStunned && activeSkill != null
                    && playerSkillCooldown == 0
                    && playerStats.CurrentMana >= activeSkill.ManaCost)
                {
                    playerAction = activeSkill.SkillId;
                    playerMultiplier = activeSkill.Multiplier;

                    decimal effectiveManaCost = activeSkill.ManaCost - pPb.ManaCostReduction;
                    if (!playerFirstSkillUsed) effectiveManaCost *= pPb.FirstSkillManaCostMult;
                    effectiveManaCost = Math.Max(0, effectiveManaCost);

                    playerStats.CurrentMana -= effectiveManaCost;
                    playerFirstSkillUsed = true;
                    playerSkillCooldown = effectiveSkillCooldown;

                    SkillEffectApplier.ApplyOnUse(
                        playerStats, opponentStats, activeSkill.EffectJson, actingFirst: playerActsFirst);
                }

                const string opponentAction = "BasicAttack";
                const decimal opponentMultiplier = 1.0m;

                // Saldiri sirasi
                if (playerActsFirst)
                {
                    if (!playerIsStunned)
                    {
                        // ── MultiHit kontrolü ──────────────────────────
                        int hitCount = 1;
                        decimal eachHitMulti = 1.0m;
                        if (playerAction != "BasicAttack"
                            && activeSkill?.EffectJson != null
                            && activeSkill.EffectJson != "{}")
                        {
                            try
                            {
                                var jdoc = System.Text.Json.JsonDocument.Parse(activeSkill.EffectJson);
                                if (jdoc.RootElement.TryGetProperty("multiHit", out var mh))
                                {
                                    if (mh.TryGetProperty("hits", out var hitsEl))
                                        hitCount = hitsEl.GetInt32();
                                    if (mh.TryGetProperty("eachHitMulti", out var ehm))
                                        eachHitMulti = ehm.GetDecimal();
                                }
                            }
                            catch { }
                        }

                        for (int hitIdx = 0; hitIdx < hitCount; hitIdx++)
                        {
                            decimal thisMultiplier = playerMultiplier * (eachHitMulti > 0 ? eachHitMulti : 1.0m);

                            ExecuteActionWithPassives(
                                playerStats, opponentStats, playerAction, thisMultiplier,
                                round, isPlayer: true, attackerGoesFirst: playerHasFirstStrike,
                                ref opponentFirstHitReceived,
                                activeSkill: playerAction != "BasicAttack" ? activeSkill : null,
                                ref opponentStunRoundsRemaining);

                            if (opponentStats.CurrentHp <= 0) break;
                        }
                    }
                    else
                    {
                        round.PlayerAction = "Stunned"; round.PlayerHit = false;
                        round.EventLog = AppendLog(round.EventLog, "Stun: Player skips");
                        if (playerStunRoundsRemaining > 0) playerStunRoundsRemaining--;
                    }

                    // ✅ YENİ: Player aksiyonu sonrası death check
                    if (opponentStats.CurrentHp <= 0)
                    {
                        round.PlayerHpRemaining = playerStats.CurrentHp;
                        round.OpponentHpRemaining = opponentStats.CurrentHp;
                        combatResult.Rounds.Add(round);
                        break;
                    }

                    opponentIsStunned = opponentStunRoundsRemaining > 0;

                    if (opponentStats.CurrentHp > 0)
                    {
                        if (!opponentIsStunned)
                        {
                            ExecuteActionWithPassives(
                                opponentStats, playerStats, opponentAction, opponentMultiplier,
                                round, isPlayer: false, attackerGoesFirst: !playerHasFirstStrike,
                                ref playerFirstHitReceived,
                                activeSkill: null,
                                ref playerStunRoundsRemaining);
                        }
                        else
                        {
                            round.OpponentAction = "Stunned"; round.OpponentHit = false;
                            round.EventLog = AppendLog(round.EventLog, "Stun: Opponent skips");
                            if (opponentStunRoundsRemaining > 0) opponentStunRoundsRemaining--;
                        }
                    }
                }
                else // opponent önce saldırır
                {
                    if (!opponentIsStunned)
                    {
                        ExecuteActionWithPassives(
                            opponentStats, playerStats, opponentAction, opponentMultiplier,
                            round, isPlayer: false, attackerGoesFirst: !playerHasFirstStrike,
                            ref playerFirstHitReceived,
                            activeSkill: null,
                            ref playerStunRoundsRemaining);
                    }
                    else
                    {
                        round.OpponentAction = "Stunned"; round.OpponentHit = false;
                        round.EventLog = AppendLog(round.EventLog, "Stun: Opponent skips");
                        if (opponentStunRoundsRemaining > 0) opponentStunRoundsRemaining--;
                    }

                    // ✅ YENİ: Opponent aksiyonu sonrası death check
                    if (playerStats.CurrentHp <= 0)
                    {
                        round.PlayerHpRemaining = playerStats.CurrentHp;
                        round.OpponentHpRemaining = opponentStats.CurrentHp;
                        combatResult.Rounds.Add(round);
                        break;
                    }

                    playerIsStunned = playerStunRoundsRemaining > 0;

                    if (playerStats.CurrentHp > 0)
                    {
                        if (!playerIsStunned)
                        {
                            // ── MultiHit kontrolü ──────────────────────────
                            int hitCount2 = 1;
                            decimal eachHitMulti2 = 1.0m;
                            if (playerAction != "BasicAttack"
                                && activeSkill?.EffectJson != null
                                && activeSkill.EffectJson != "{}")
                            {
                                try
                                {
                                    var jdoc2 = System.Text.Json.JsonDocument.Parse(activeSkill.EffectJson);
                                    if (jdoc2.RootElement.TryGetProperty("multiHit", out var mh2))
                                    {
                                        if (mh2.TryGetProperty("hits", out var hitsEl2))
                                            hitCount2 = hitsEl2.GetInt32();
                                        if (mh2.TryGetProperty("eachHitMulti", out var ehm2))
                                            eachHitMulti2 = ehm2.GetDecimal();
                                    }
                                }
                                catch { }
                            }

                            for (int hitIdx2 = 0; hitIdx2 < hitCount2; hitIdx2++)
                            {
                                decimal thisMultiplier2 = playerMultiplier * (eachHitMulti2 > 0 ? eachHitMulti2 : 1.0m);

                                ExecuteActionWithPassives(
                                    playerStats, opponentStats, playerAction, thisMultiplier2,
                                    round, isPlayer: true, attackerGoesFirst: playerHasFirstStrike,
                                    ref opponentFirstHitReceived,
                                    activeSkill: playerAction != "BasicAttack" ? activeSkill : null,
                                    ref opponentStunRoundsRemaining);

                                if (opponentStats.CurrentHp <= 0) break;
                            }
                        }
                        else
                        {
                            round.PlayerAction = "Stunned"; round.PlayerHit = false;
                            round.EventLog = AppendLog(round.EventLog, "Stun: Player skips");
                            if (playerStunRoundsRemaining > 0) playerStunRoundsRemaining--;
                        }
                    }
                }

                round.PlayerHpRemaining = playerStats.CurrentHp;
                round.OpponentHpRemaining = opponentStats.CurrentHp;
                combatResult.Rounds.Add(round);

                if (playerSkillCooldown > 0) playerSkillCooldown--;
                if (opponentSkillCooldown > 0) opponentSkillCooldown--;
                roundNumber++;
            }

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


        // YARDIMCI: Aksiyon calistir
        private void ExecuteActionWithPassives(
            CharacterStats attacker,
            CharacterStats defender,
            string actionName,
            decimal multiplier,
            CombatRound round,
            bool isPlayer,
            bool attackerGoesFirst,
            ref bool defenderFirstHitReceived,
            SkillDefinition? activeSkill,
            ref int defenderStunRoundsRemaining)
        {
            var pPb = attacker.PassiveBonuses;
            var dPb = defender.PassiveBonuses;

            decimal bonusMult = 1.0m;
            if (attackerGoesFirst) bonusMult *= pPb.FirstStrikeDamageMult;

            decimal defenderHpRatio = defender.MaxHp > 0 ? defender.CurrentHp / defender.MaxHp : 0m;
            if (defenderHpRatio < pPb.ExecutionThreshold && pPb.ExecutionWindowMult > 1.0m)
                bonusMult *= pPb.ExecutionWindowMult;

            var markEffect = defender.StatusEffects.FirstOrDefault(e => e.Type == "Mark");
            if (markEffect != null) bonusMult *= markEffect.DamageTakenMult;

            var chargeEffect = attacker.StatusEffects
                .FirstOrDefault(e => e.Type == "StaticCharge" || e.Type == "AshArmor");
            if (chargeEffect != null)
            {
                bonusMult *= chargeEffect.DamageBonusMult;
                chargeEffect.ChargesRemaining--;
                if (chargeEffect.ChargesRemaining <= 0)
                    attacker.StatusEffects.Remove(chargeEffect);
            }

            var action = _damageCalculator.CalculateAttack(
                attacker, defender, actionName, multiplier * bonusMult);

            // ✅ OnHit efektleri: sadece "hit" ile bağla (damage 0 olsa bile proc edebilir)
            if (action.DidHit)
            {
                if (activeSkill != null
                    && !string.IsNullOrEmpty(activeSkill.EffectJson)
                    && activeSkill.EffectJson != "{}")
                {
                    SkillEffectApplier.ApplyOnHit(
                        attacker, defender, activeSkill.EffectJson, attacker.BaseDamage);

                    // ✅ Stun status'ü geldi mi? Sayaç set et (max ile).
                    var stunEffect = defender.StatusEffects
                        .FirstOrDefault(e => e.Type == "Stun" && e.RemainingRounds > 0);

                    if (stunEffect != null)
                    {
                        defenderStunRoundsRemaining = Math.Max(defenderStunRoundsRemaining, stunEffect.RemainingRounds);
                        round.EventLog = AppendLog(round.EventLog,
                            $"⚡ STUN queued! (stunRounds={defenderStunRoundsRemaining})");
                    }
                }
            }

            // Taken multiplier / mitigation sadece gerçek hasar varsa uygulanır
            if (action.DidHit && action.FinalDamage > 0)
            {
                decimal takenMult = 1.0m;

                if (!defenderFirstHitReceived && dPb.FirstHitDamageTakenMult < 1.0m)
                {
                    takenMult *= dPb.FirstHitDamageTakenMult;
                    defenderFirstHitReceived = true;
                }
                if (defenderHpRatio < dPb.LowHpThreshold && dPb.LowHpDamageTakenMult < 1.0m)
                    takenMult *= dPb.LowHpDamageTakenMult;
                if (action.WasBlocked && dPb.BlockSuccessDamageTakenMult < 1.0m)
                    takenMult *= dPb.BlockSuccessDamageTakenMult;

                action.FinalDamage *= takenMult;
            }

            defender.CurrentHp -= action.FinalDamage;

            // ✅ Lifesteal — sadece hit olursa ve gerçek hasar varsa
            if (action.DidHit && action.FinalDamage > 0 && pPb.LifeSteal > 0)
            {
                decimal healAmount = action.FinalDamage * pPb.LifeSteal;
                attacker.CurrentHp = Math.Min(attacker.MaxHp, attacker.CurrentHp + healAmount);
                round.EventLog = AppendLog(round.EventLog,
                    $"💚 Lifesteal +{healAmount:F1} HP");
            }

            // ✅ Flat Elemental Damage — ayrı hit, Resist mitigation uygulanır, Armor uygulanmaz
            decimal totalElementalDmg = 0m;

            if (action.DidHit)  // sadece hit olduysa elemental de çarpar
            {
                void ApplyFlat(decimal flatDmg, string dmgType)
                {
                    if (flatDmg <= 0) return;
                    decimal resist = dmgType switch
                    {
                        "Fire" => defender.FireResist,
                        "Cold" => defender.ColdResist,
                        "Lightning" => defender.LightningResist,
                        "Chaos" => defender.ChaosResist,
                        _ => 0m
                    };
                    // Crit elemental'a da uygulanır
                    decimal dmg = flatDmg * (action.DidCrit ? attacker.CritMultiplier : 1m);
                    // Ward absorbs non-physical
                    decimal afterResist = dmg * (1 - resist);
                    if (defender.Ward > 0)
                    {
                        decimal wa = Math.Min(defender.Ward, afterResist);
                        afterResist -= wa;
                        if (isPlayer) round.PlayerWardAbsorbed += wa;
                        else round.OpponentWardAbsorbed += wa;
                    }
                    totalElementalDmg += afterResist;
                }

                ApplyFlat(attacker.FlatFireDamage, "Fire");
                ApplyFlat(attacker.FlatColdDamage, "Cold");
                ApplyFlat(attacker.FlatLightningDamage, "Lightning");
                ApplyFlat(attacker.FlatChaosDamage, "Chaos");

                if (totalElementalDmg > 0)
                {
                    defender.CurrentHp -= totalElementalDmg;
                    if (isPlayer) round.PlayerDamage += totalElementalDmg;
                    else round.OpponentDamage += totalElementalDmg;

                    round.EventLog = AppendLog(round.EventLog,
                        $"⚡ Elemental: -{totalElementalDmg:F1}");
                }
            }

            // ✅ Ward absorbed değerini round'a yaz
            if (isPlayer)
            {
                round.PlayerWardAbsorbed += action.WardAbsorbed;
            }
            else
            {
                round.OpponentWardAbsorbed += action.WardAbsorbed;
            }

            if (isPlayer)
            {
                round.PlayerAction = actionName;
                round.PlayerHit = round.PlayerHit || action.DidHit;      // ✅ en az biri hit olduysa true
                round.PlayerCrit = round.PlayerCrit || action.DidCrit;   // ✅ en az biri crit olduysa true
                round.PlayerDamage += action.FinalDamage;                // ✅ = yerine += (topla)
                round.PlayerBlocked = round.PlayerBlocked || action.WasBlocked;
            }
            else
            {
                round.OpponentAction = actionName;
                round.OpponentHit = round.OpponentHit || action.DidHit;
                round.OpponentCrit = round.OpponentCrit || action.DidCrit;
                round.OpponentDamage += action.FinalDamage;              // ✅ += 
                round.OpponentBlocked = round.OpponentBlocked || action.WasBlocked;
            }
        }


        private void ApplySuddenDeath(CharacterStats player, CharacterStats opponent, int stacks)
        {
            player.CurrentHp -= _damageCalculator.CalculateSuddenDeathDamage(player.MaxHp, stacks);
            opponent.CurrentHp -= _damageCalculator.CalculateSuddenDeathDamage(opponent.MaxHp, stacks);
        }

        private async Task<CharacterStats> BuildStatsFromRun(Run run)
        {
            var baseStat = await _context.BaseStatDefinitions
                .FirstOrDefaultAsync(b => b.Race == run.Race)
                ?? throw new InvalidOperationException($"BaseStatDefinition not found for race: {run.Race}");

            var runWithData = await _context.Runs
                .Include(r => r.Equipment).ThenInclude(e => e.Weapon)
                    .ThenInclude(ri => ri!.Item).ThenInclude(i => i.Affixes).ThenInclude(a => a.AffixDefinition)
                .Include(r => r.Equipment).ThenInclude(e => e.Offhand)
                    .ThenInclude(ri => ri!.Item).ThenInclude(i => i.Affixes).ThenInclude(a => a.AffixDefinition)
                .Include(r => r.Equipment).ThenInclude(e => e.Armor)
                    .ThenInclude(ri => ri!.Item).ThenInclude(i => i.Affixes).ThenInclude(a => a.AffixDefinition)
                .Include(r => r.Equipment).ThenInclude(e => e.Belt)
                    .ThenInclude(ri => ri!.Item).ThenInclude(i => i.Affixes).ThenInclude(a => a.AffixDefinition)
                .Include(r => r.AllocatedPassives)
                .FirstOrDefaultAsync(r => r.Id == run.Id)
                ?? run;

            var allocatedNodeIds = runWithData.AllocatedPassives.Select(p => p.NodeId).ToHashSet();
            List<PassiveNodeDefinition> allocatedDefs;

            if (allocatedNodeIds.Count > 0)
            {
                var allNodes = await _context.PassiveNodeDefinitions.AsNoTracking().ToListAsync();
                allocatedDefs = allNodes.Where(nd => allocatedNodeIds.Contains(nd.NodeId)).ToList();
            }
            else allocatedDefs = new List<PassiveNodeDefinition>();

            return _statCalculator.CalculateRunStats(runWithData, baseStat, allocatedDefs);
        }

        private static int ApplyPassiveCooldownReduction(int baseCooldown, PassiveBonuses pb)
        {
            if (pb.CooldownReduction <= 0) return baseCooldown;
            if (baseCooldown < (int)pb.CooldownReductionThreshold) return baseCooldown;
            return Math.Max(2, baseCooldown - (int)pb.CooldownReduction);
        }

        private static CharacterStats BuildStatsFromMonster(PveMonster monster)
        {
            return new CharacterStats
            {
                MaxHp = monster.MaxHp,
                CurrentHp = monster.MaxHp,
                MaxMana = 0,
                CurrentMana = 0,
                MeleeDamage = monster.BaseDamage,
                RangedDamage = monster.BaseDamage,
                SpellDamage = monster.BaseDamage,
                IncreasedDamage = 1.0m,
                Armor = monster.Armor,
                IncreasedArmor = 1.0m,
                Evasion = monster.Evasion,
                IncreasedEvasion = 1.0m,
                Ward = monster.Ward,
                IncreasedWard = 1.0m,
                Accuracy = monster.Accuracy,
                IncreasedAccuracy = 1.0m,
                CritChance = monster.CritChance,
                CritMultiplier = monster.CritMultiplier,
                BlockChance = monster.BlockChance,
                BlockReduction = 0.35m,
                Initiative = monster.Initiative,
                FireResist = monster.ResistFire,
                ColdResist = monster.ResistCold,
                LightningResist = monster.ResistLightning,
                ChaosResist = monster.ResistChaos,
                ArmorPenetration = 0m
            };
        }

        private static string AppendLog(string existing, string entry)
        {
            if (string.IsNullOrEmpty(entry)) return existing;
            return string.IsNullOrEmpty(existing) ? entry : existing + " | " + entry;
        }
    }
}
