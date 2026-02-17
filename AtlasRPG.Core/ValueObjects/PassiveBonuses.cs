// AtlasRPG.Core/ValueObjects/PassiveBonuses.cs
namespace AtlasRPG.Core.ValueObjects
{
    /// <summary>
    /// Oyuncunun alloklanan passive node'larından gelen tüm bonusları tutar.
    /// Stat-time ve combat-time effect'lerin ikisini de içerir.
    /// </summary>
    public class PassiveBonuses
    {
        // ────────────────────────────────────────────
        // STAT-TIME: StatCalculatorService'te uygulanır
        // ────────────────────────────────────────────

        /// <summary>MaxHP'ye eklenen yüzde toplamı (0.03 = +%3)</summary>
        public decimal MaxHpPercent { get; set; } = 0m;

        /// <summary>MaxMana'ya eklenen yüzde toplamı</summary>
        public decimal MaxManaPercent { get; set; } = 0m;

        /// <summary>Flat initiative bonus</summary>
        public decimal InitiativeFlat { get; set; } = 0m;

        /// <summary>IncreasedDamage'e additive eklenen (global, silah kısıtı yok)</summary>
        public decimal IncreasedDamageGlobal { get; set; } = 0m;

        // Silah tipine özel hasar bonusları
        public decimal IncreasedDamageBow { get; set; } = 0m;
        public decimal IncreasedDamageDagger { get; set; } = 0m;
        public decimal IncreasedDamage1HSword { get; set; } = 0m;
        public decimal IncreasedDamage2HSword { get; set; } = 0m;
        public decimal IncreasedDamageWand { get; set; } = 0m;
        public decimal IncreasedDamageStaff { get; set; } = 0m;

        /// <summary>IncreasedAccuracy'ye additive eklenen (global)</summary>
        public decimal IncreasedAccuracyGlobal { get; set; } = 0m;

        /// <summary>CritChance'e additive eklenen flat (%40 cap öncesi)</summary>
        public decimal CritChanceFlat { get; set; } = 0m;

        /// <summary>Accuracy'ye uygulanan çarpan (N07 Clean Hit → 1.10)</summary>
        public decimal AccuracyMult { get; set; } = 1.0m;

        /// <summary>CritMultiplier'a eklenen flat bonus</summary>
        public decimal CritMultiBonus { get; set; } = 0m;

        /// <summary>IncreasedArmor'a additive eklenen</summary>
        public decimal IncreasedArmor { get; set; } = 0m;

        /// <summary>IncreasedEvasion'a additive eklenen (negatif olabilir — Pyromancer gibi)</summary>
        public decimal IncreasedEvasion { get; set; } = 0m;

        /// <summary>IncreasedWard'a additive eklenen</summary>
        public decimal IncreasedWard { get; set; } = 0m;

        /// <summary>IncreasedBlockChance'e additive eklenen (Duelist: +5%)</summary>
        public decimal IncreasedBlockChance { get; set; } = 0m;

        /// <summary>ArmorPenetration'a eklenen (N45 Armor Breaker)</summary>
        public decimal ArmorPenetration { get; set; } = 0m;

        // ────────────────────────────────────────────
        // COMBAT-TIME: CombatService'te kullanılır
        // ────────────────────────────────────────────

        /// <summary>Her skill cast'te ManaCost'tan düşülen flat değer (min 0)</summary>
        public decimal ManaCostReduction { get; set; } = 0m;

        /// <summary>Turn'deki ilk skill cast ManaCost çarpanı (N18 First Tempo → 0.75)</summary>
        public decimal FirstSkillManaCostMult { get; set; } = 1.0m;

        /// <summary>CD≥CooldownReductionThreshold olan skill'lerin CD'sinden düşülen değer (N19/N50)</summary>
        public decimal CooldownReduction { get; set; } = 0m;

        /// <summary>Cooldown reduction'ın uygulandığı minimum CD eşiği</summary>
        public decimal CooldownReductionThreshold { get; set; } = 3m;

        /// <summary>Turn içinde ilk alınan hit'in DamageTaken çarpanı (N12 Brace → 0.94)</summary>
        public decimal FirstHitDamageTakenMult { get; set; } = 1.0m;

        /// <summary>HP &lt; LowHpThreshold iken DamageTaken çarpanı (N13 Unyielding → 0.92)</summary>
        public decimal LowHpDamageTakenMult { get; set; } = 1.0m;

        /// <summary>LowHpDamageTakenMult ve ExecutionWindow'ın HP eşiği</summary>
        public decimal LowHpThreshold { get; set; } = 0.25m;

        /// <summary>Hedef HP &lt; ExecutionThreshold iken Damage çarpanı (N06 Execution Window → 1.06)</summary>
        public decimal ExecutionWindowMult { get; set; } = 1.0m;

        /// <summary>ExecutionWindow'un HP eşiği (default 0.35)</summary>
        public decimal ExecutionThreshold { get; set; } = 0.35m;

        /// <summary>Mark duration'a eklenen round (N23 Mark Specialist)</summary>
        public int MarkDurationBonus { get; set; } = 0;

        /// <summary>Mark'lı hedefe verilen hasar çarpanı (N25 Hunted → 1.06)</summary>
        public decimal DamageVsMarked { get; set; } = 1.0m;

        /// <summary>Mark effect bonus (N26 Deadeye Keystone → +0.10)</summary>
        public decimal MarkEffectBonus { get; set; } = 0m;

        /// <summary>İlk vuran taraf ise damage çarpanı (N30 First Blood → 1.06)</summary>
        public decimal FirstStrikeDamageMult { get; set; } = 1.0m;

        /// <summary>Poison duration bonus round (N31 Poison Edge)</summary>
        public int PoisonDurationBonus { get; set; } = 0;

        /// <summary>Bleed duration bonus round (N38 Bleed Mastery)</summary>
        public int BleedDurationBonus { get; set; } = 0;

        /// <summary>Block başarılı ise o hit DamageTaken çarpanı (N39 Guarded → 0.92)</summary>
        public decimal BlockSuccessDamageTakenMult { get; set; } = 1.0m;

        /// <summary>Block sonrası sonraki saldırı Damage çarpanı (N37 Riposte, turn başına 1 kez)</summary>
        public decimal RiposteAfterBlockMult { get; set; } = 1.0m;

        /// <summary>2H buff süresine eklenen round (N46 Colossus Discipline)</summary>
        public int BuffDurationBonus2H { get; set; } = 0;

        /// <summary>2H skill ActiveSkillMult bonus (N44 Heavy Hands → +0.03)</summary>
        public decimal ActiveSkillMult2HBonus { get; set; } = 0m;

        /// <summary>Stun chance flat bonus (N52 Control I → +0.05)</summary>
        public decimal StunChanceBonus { get; set; } = 0m;

        /// <summary>Lightning Resist shred duration bonus round (N53 Conductive)</summary>
        public int LightningResistShredDurationBonus { get; set; } = 0;

        /// <summary>On Stun: hedefin DamageTaken çarpanı (N54 Stormcaller Keystone → 1.06)</summary>
        public decimal StunDamageTakenBonus { get; set; } = 1.0m;

        /// <summary>StunDamageTakenBonus'un süresi (round)</summary>
        public int StunDamageTakenDuration { get; set; } = 0;

        /// <summary>Charge-based skill'lere +N charge (N51 Static Charges)</summary>
        public int ChargeBonus { get; set; } = 0;

        /// <summary>Accuracy debuff duration bonus round (N57 Ash Control)</summary>
        public int AccuracyDebuffDurationBonus { get; set; } = 0;

        /// <summary>Burn max stack bonus (N58 Burn Expert → +1)</summary>
        public int BurnMaxStacksBonus { get; set; } = 0;

        /// <summary>Burn tick damage çarpanı (N60 Pyromancer Keystone → 1.15)</summary>
        public decimal BurnTickMult { get; set; } = 1.0m;

        public int DebuffDurationReduction { get; set; } = 0;  // Dwarf Steadfast
        public bool DraconicCoreActive { get; set; } = false;  // Drakoid Draconic Core

        // ────────────────────────────────────────────
        // HELPER: Weapon tipini string olarak al
        // ────────────────────────────────────────────
        public decimal GetWeaponDamageBonus(string weaponType) => weaponType switch
        {
            "Bow" => IncreasedDamageBow,
            "Dagger" => IncreasedDamageDagger,
            "1HSword" => IncreasedDamage1HSword,
            "2HSword" => IncreasedDamage2HSword,
            "Wand" => IncreasedDamageWand,
            "Staff" => IncreasedDamageStaff,
            _ => 0m
        };
    }
}
