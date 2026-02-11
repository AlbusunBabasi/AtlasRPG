// ============================================================
// AtlasRPG.Application/Services/PowerScoreService.cs
// ============================================================
// Tasarım: PowerScore = 0.50*log(O) + 0.45*log(D) + 0.05*C
//
// O = OffenseScore — beklenen hasar
// D = DefenseScore — EHP
// C = ControlScore — max 0.08
// ============================================================

using AtlasRPG.Core.ValueObjects;
using AtlasRPG.Core.Entities.GameData;
using AtlasRPG.Core.Entities.Runs;
using AtlasRPG.Core.Enums;

namespace AtlasRPG.Application.Services
{
    public class PowerScoreService
    {
        // ── Proxy değerler (düşük popülasyon için sabit fallback) ────
        // Production'da DB medyanından hesaplanır; başlangıçta default.
        private const decimal DefaultEvasionProxy = 80m;
        private const decimal DefaultAccuracyProxy = 120m;

        // ── Uptime lookup table (CD x ManaBand) ─────────────────────
        // [cooldownRounds][manaCostBand] → uptime (0.35..0.65)
        // ManaBand: 0 = 0-4, 1 = 5-9, 2 = 10-14, 3 = 15-20, 4 = 20+
        private static readonly decimal[,] UptimeTable =
        {
            //  mana:  0     5     10    15    20+
            /* CD 0 */ { 0.65m, 0.60m, 0.55m, 0.50m, 0.45m },
            /* CD 1 */ { 0.60m, 0.55m, 0.50m, 0.45m, 0.40m },
            /* CD 2 */ { 0.55m, 0.50m, 0.45m, 0.40m, 0.37m },
            /* CD 3 */ { 0.48m, 0.45m, 0.42m, 0.38m, 0.35m },
            /* CD 4 */ { 0.42m, 0.40m, 0.38m, 0.36m, 0.35m },
            /* CD 5 */ { 0.38m, 0.37m, 0.36m, 0.35m, 0.35m },
        };

        // ────────────────────────────────────────────────────────────
        // ANA METOD
        // ────────────────────────────────────────────────────────────
        public PowerScoreResult Calculate(
            CharacterStats stats,
            SkillDefinition? activeSkill,
            decimal enemyEvasionProxy = DefaultEvasionProxy,
            decimal enemyAccuracyProxy = DefaultAccuracyProxy)
        {
            var result = new PowerScoreResult();

            // ── Offense ─────────────────────────────────────────────
            result.OffenseScore = CalculateOffense(stats, activeSkill, enemyEvasionProxy);

            // ── Defense ─────────────────────────────────────────────
            result.DefenseScore = CalculateDefense(stats, enemyAccuracyProxy);

            // ── Control ─────────────────────────────────────────────
            result.ControlScore = CalculateControl(stats, activeSkill);

            // ── Final PowerScore ────────────────────────────────────
            // PowerScore = 0.50*log(O) + 0.45*log(D) + 0.05*C
            double logO = result.OffenseScore > 0 ? Math.Log((double)result.OffenseScore) : 0;
            double logD = result.DefenseScore > 0 ? Math.Log((double)result.DefenseScore) : 0;

            result.PowerScore = (decimal)(0.50 * logO + 0.45 * logD) + 0.05m * result.ControlScore;

            return result;
        }

        // ────────────────────────────────────────────────────────────
        // StructuralScore (sandbag önleme)
        // slot ilvl + rarity + tier toplamı → normalize edilmiş 0..1
        // ────────────────────────────────────────────────────────────
        public decimal CalculateStructuralScore(
            RunEquipment? equipment,
            bool hasKeystone,
            decimal normalizationMedian = 50m)   // DB medyanından gelir
        {
            decimal raw = 0m;

            AddSlotScore(ref raw, equipment?.Weapon?.Item);
            AddSlotScore(ref raw, equipment?.Offhand?.Item);
            AddSlotScore(ref raw, equipment?.Armor?.Item);
            AddSlotScore(ref raw, equipment?.Belt?.Item);

            if (hasKeystone) raw += 5m;

            // Normalize: score / (2 * medyan) → yaklaşık 0..1 aralığı
            return normalizationMedian > 0
                ? Math.Min(1.5m, raw / (2m * normalizationMedian))
                : 0m;
        }

        // ────────────────────────────────────────────────────────────
        // BandScore = 0.80*PowerScore + 0.20*StructuralScoreNorm
        // ────────────────────────────────────────────────────────────
        public decimal CalculateBandScore(decimal powerScore, decimal structuralScoreNorm)
            => 0.80m * powerScore + 0.20m * structuralScoreNorm;

        // ────────────────────────────────────────────────────────────
        // Band bucket (0-4) — cutoffs dışarıdan verilir
        // ────────────────────────────────────────────────────────────
        public static int DetermineBand(decimal bandScore, BandCutoffs cutoffs)
        {
            if (bandScore < cutoffs.P20) return 0;
            if (bandScore < cutoffs.P40) return 1;
            if (bandScore < cutoffs.P60) return 2;
            if (bandScore < cutoffs.P80) return 3;
            return 4;
        }

        // ────────────────────────────────────────────────────────────
        // PRIVATE: Offense
        // ────────────────────────────────────────────────────────────
        private static decimal CalculateOffense(
            CharacterStats stats,
            SkillDefinition? activeSkill,
            decimal enemyEvasionProxy)
        {
            // HitChance = clamp(Acc/(Acc+EnemyEvasion), 0.10, 0.95)
            decimal totalAcc = stats.Accuracy > 0 ? stats.Accuracy : 1m;
            decimal hitChance = Math.Min(0.95m,
                Math.Max(0.10m, totalAcc / (totalAcc + enemyEvasionProxy)));

            // ExpectedCrit = 1 + CritChance * (CritMult - 1)
            decimal expectedCrit = 1m + stats.CritChance * (stats.CritMultiplier - 1m);

            // BaseDamage: en güçlü damage türü
            decimal baseDmg = Math.Max(Math.Max(stats.MeleeDamage, stats.RangedDamage), stats.SpellDamage);
            if (baseDmg <= 0) baseDmg = stats.BaseDamage > 0 ? stats.BaseDamage : 1m;

            // BaseHit = BaseDamage * HitChance * ExpectedCrit
            decimal baseHit = baseDmg * hitChance * expectedCrit;

            if (activeSkill == null)
                return baseHit;

            // SkillHit = BaseHit * ActiveSkillMult
            decimal skillHit = baseHit * activeSkill.Multiplier;

            // Uptime estimate
            decimal uptime = GetUptime(activeSkill.Cooldown, activeSkill.ManaCost);

            // O = BaseHit*(1-Uptime) + SkillHit*Uptime
            return baseHit * (1m - uptime) + skillHit * uptime;
        }

        // ────────────────────────────────────────────────────────────
        // PRIVATE: Defense
        // ────────────────────────────────────────────────────────────
        private static decimal CalculateDefense(
            CharacterStats stats,
            decimal enemyAccuracyProxy)
        {
            decimal maxHp = stats.MaxHp > 0 ? stats.MaxHp : 1m;

            // ArmorMit = Armor / (Armor + 100)
            decimal armorMit = stats.Armor / (stats.Armor + 100m);
            decimal ehpArmor = maxHp / Math.Max(0.01m, 1m - armorMit);

            // DodgeChanceProxy = clamp(Evasion/(Evasion+EnemyAccuracy), 0.05, 0.60)
            decimal dodge = Math.Min(0.60m,
                Math.Max(0.05m, stats.Evasion / (stats.Evasion + enemyAccuracyProxy)));

            decimal ehp = ehpArmor / Math.Max(0.01m, 1m - dodge);

            // Ward buffer
            ehp += stats.Ward;

            return ehp;
        }

        // ────────────────────────────────────────────────────────────
        // PRIVATE: Control (küçük, cap 0.08)
        // ────────────────────────────────────────────────────────────
        private static decimal CalculateControl(CharacterStats stats, SkillDefinition? skill)
        {
            decimal c = 0m;

            // ArmorPen
            if (stats.ArmorPenetration > 0) c += 0.02m;

            // Passive bonuses
            var pb = stats.PassiveBonuses;
            if (pb != null)
            {
                // DOT (bleed/poison/burn)
                if (pb.BleedDurationBonus > 0 || pb.PoisonDurationBonus > 0 || pb.BurnMaxStacksBonus > 0)
                    c += 0.02m;

                // Stun
                if (pb.StunChanceBonus > 0)
                    c += 0.03m * Math.Min(1m, pb.StunChanceBonus);

                // Mark
                if (pb.MarkDurationBonus > 0 || pb.MarkEffectBonus > 0)
                    c += 0.02m;
            }

            // Skill DOT / Stun effects (basit kontrol)
            //if (skill != null)
            //{
            //    if (skill.EffectType == EffectType.Poison)
            //        c += 0.02m;
            //    if (skill.EffectType == EffectType.Bleed)
            //        c += 0.02m;
            //    if (skill.EffectType == EffectType.Burn)
            //        c += 0.02m;
            //    if (skill.EffectType == EffectType.Stun)
            //        c += 0.03m;
            //    if (skill.EffectType == EffectType.Mark)
            //        c += 0.02m;
            //}

            return Math.Min(0.08m, c);
        }

        // ────────────────────────────────────────────────────────────
        // PRIVATE: Uptime lookup
        // ────────────────────────────────────────────────────────────
        private static decimal GetUptime(int cooldown, decimal manaCost)
        {
            int cdIdx = Math.Min(5, Math.Max(0, cooldown));
            int manaIdx = manaCost switch
            {
                < 5m => 0,
                < 10m => 1,
                < 15m => 2,
                < 20m => 3,
                _ => 4
            };
            return UptimeTable[cdIdx, manaIdx];
        }

        // ────────────────────────────────────────────────────────────
        // PRIVATE: StructuralScore slot katkısı
        // ────────────────────────────────────────────────────────────
        private static void AddSlotScore(ref decimal score, AtlasRPG.Core.Entities.Items.Item? item)
        {
            if (item == null) return;

            // ilvl katkısı (max 20 → 5 puan)
            score += item.ItemLevel * 0.25m;

            // Rarity: Normal=0, Magic=1, Rare=2
            score += (int)item.Rarity;

            // Affix tier puanları
            foreach (var affix in item.Affixes ?? new List<AtlasRPG.Core.Entities.Items.ItemAffix>())
            {
                score += affix.Tier switch
                {
                    1 => 1m,
                    2 => 2m,
                    3 => 3m,
                    _ => 0m
                };
            }
        }
    }

    // ────────────────────────────────────────────────────────────────
    // RESULT VALUE OBJECT
    // ────────────────────────────────────────────────────────────────
    public class PowerScoreResult
    {
        public decimal OffenseScore { get; set; }
        public decimal DefenseScore { get; set; }
        public decimal ControlScore { get; set; }
        public decimal PowerScore { get; set; }

        public override string ToString()
            => $"PS={PowerScore:F2}  O={OffenseScore:F1}  D={DefenseScore:F1}  C={ControlScore:F2}";
    }

    // ────────────────────────────────────────────────────────────────
    // BAND CUTOFFS (turn bazlı, DB'den veya cache'den gelir)
    // ────────────────────────────────────────────────────────────────
    public class BandCutoffs
    {
        public decimal P20 { get; set; }
        public decimal P40 { get; set; }
        public decimal P60 { get; set; }
        public decimal P80 { get; set; }

        // Smoothing: cutoff_new = 0.8*old + 0.2*calculated
        public BandCutoffs ApplySmoothing(BandCutoffs calculated)
            => new BandCutoffs
            {
                P20 = 0.8m * P20 + 0.2m * calculated.P20,
                P40 = 0.8m * P40 + 0.2m * calculated.P40,
                P60 = 0.8m * P60 + 0.2m * calculated.P60,
                P80 = 0.8m * P80 + 0.2m * calculated.P80,
            };

        /// <summary>Belirli bir turn için default başlangıç cutoffs</summary>
        public static BandCutoffs DefaultForTurn(int turnIndex)
        {
            // Turn ilerledikçe genel güç artar; cutoffs buna göre kayar.
            decimal baseOffset = turnIndex * 0.05m;
            return new BandCutoffs
            {
                P20 = 3.0m + baseOffset,
                P40 = 3.5m + baseOffset,
                P60 = 4.0m + baseOffset,
                P80 = 4.5m + baseOffset,
            };
        }
    }
}
