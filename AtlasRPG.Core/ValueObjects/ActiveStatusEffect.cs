namespace AtlasRPG.Core.ValueObjects
{
    public class ActiveStatusEffect
    {
        public string Type { get; set; } = string.Empty; // "Bleed", "Burn", "Poison", "Stun", etc.
        public int RemainingRounds { get; set; }
        public int MaxStacks { get; set; } = 1;
        public int CurrentStacks { get; set; } = 1;
        public bool RefreshDuration { get; set; } = true;

        // DOT parametreleri
        public decimal TickValue { get; set; } = 0m;        // Sabit tick (Bleed: AttackerBaseDamage * 0.25)
        public decimal TickPercent { get; set; } = 0m;      // % tick (Poison: 0.04 * CurrentHP)
        public bool WardAbsorbs { get; set; } = false;

        // Debuff parametreleri
        public decimal AccuracyMult { get; set; } = 1.0m;   // AshCloud
        public decimal LightningResistReduction { get; set; } = 0m; // Paralyze
        public decimal DamageTakenMult { get; set; } = 1.0m; // Mark

        // Buff parametreleri
        public decimal EvasionBonus { get; set; } = 0m;
        public decimal CritBonus { get; set; } = 0m;
        public decimal DamageBonusMult { get; set; } = 1.0m; // StaticCharge, AshArmor
        public int ChargesRemaining { get; set; } = 0;       // Charge-based buffs
    }
}