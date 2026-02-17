// AtlasRPG.Core/ValueObjects/CharacterStats.cs
namespace AtlasRPG.Core.ValueObjects
{
    public class CharacterStats
    {
        // HP / Mana
        public decimal MaxHp { get; set; }
        public decimal MaxMana { get; set; }
        public decimal CurrentHp { get; set; }
        public decimal CurrentMana { get; set; }

        // Primary Stats
        public int Strength { get; set; }
        public int Dexterity { get; set; }
        public int Agility { get; set; }
        public int Intelligence { get; set; }
        public int Vitality { get; set; }
        public int Wisdom { get; set; }
        public int Luck { get; set; }

        // Damage
        public decimal BaseDamage { get; set; }
        public decimal MeleeDamage { get; set; }
        public decimal RangedDamage { get; set; }
        public decimal SpellDamage { get; set; }

        // Defense
        public decimal Armor { get; set; }
        public decimal Evasion { get; set; }
        public decimal Ward { get; set; }

        // Hit / Crit / Block
        public decimal Accuracy { get; set; }
        public decimal CritChance { get; set; }
        public decimal CritMultiplier { get; set; }
        public decimal BlockChance { get; set; }
        public decimal BlockReduction { get; set; }

        // Resistances (ratio format: 0.20 = 20%)
        public decimal FireResist { get; set; }
        public decimal ColdResist { get; set; }
        public decimal LightningResist { get; set; }
        public decimal ChaosResist { get; set; }

        // Speed
        public decimal Initiative { get; set; }

        // Modifiers (increased/reduced)
        public decimal IncreasedDamage { get; set; } = 1.0m; // 1.0 = no modifier
        public decimal IncreasedArmor { get; set; } = 1.0m;
        public decimal IncreasedEvasion { get; set; } = 1.0m;
        public decimal IncreasedWard { get; set; } = 1.0m;
        public decimal IncreasedAccuracy { get; set; } = 1.0m;
        public decimal IncreasedCritChance { get; set; } = 1.0m;
        public decimal IncreasedBlockChance { get; set; } = 1.0m;
        public decimal ArmorPenetration { get; set; } = 0m;

        public PassiveBonuses PassiveBonuses { get; set; } = new();
        public List<ActiveStatusEffect> StatusEffects { get; set; } = new();
        public bool IsStunned { get; set; } = false;

        // Flat Elemental Damage (weapon affixlerinden gelir)
        public decimal FlatFireDamage { get; set; } = 0m;
        public decimal FlatColdDamage { get; set; } = 0m;
        public decimal FlatLightningDamage { get; set; } = 0m;
        public decimal FlatChaosDamage { get; set; } = 0m;
    }
}
