// AtlasRPG.Core/ValueObjects/CombatAction.cs
namespace AtlasRPG.Core.ValueObjects
{
    public class CombatAction
    {
        public string ActionName { get; set; } = string.Empty;
        public bool IsSkill { get; set; }
        public decimal DamageMultiplier { get; set; } = 1.0m;
        public int ManaCost { get; set; }
        public bool DidHit { get; set; }
        public bool DidCrit { get; set; }
        public bool WasBlocked { get; set; }
        public decimal RawDamage { get; set; }
        public decimal FinalDamage { get; set; }
        public string DamageType { get; set; } = "Physical"; // Physical, Fire, Cold, Lightning, Chaos
        public decimal WardAbsorbed { get; set; } = 0m;
    }
}
