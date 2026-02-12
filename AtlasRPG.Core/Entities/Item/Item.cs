// AtlasRPG.Core/Entities/Items/Item.cs
using AtlasRPG.Core.Enums;

namespace AtlasRPG.Core.Entities.Items
{
    public class Item : BaseEntity
    {
        public ItemSlot Slot { get; set; }
        public ItemRarity Rarity { get; set; }
        public int ItemLevel { get; set; }

        // Weapon
        public WeaponType? WeaponType { get; set; }

        // Offhand subtype (null unless Slot == Offhand)
        public OffhandType? OffhandType { get; set; }

        // Armor subtype (null unless Slot == Armor)
        public ArmorType? ArmorType { get; set; }

        // ── Weapon Base Stats ──
        public decimal BaseAttackSpeed { get; set; } = 0;
        public decimal BaseDamage { get; set; } = 0;
        public decimal BaseCritChance { get; set; } = 0;

        // ── Armor / Offhand Base Stats ──
        public decimal BaseArmor { get; set; } = 0;
        public decimal BaseEvasion { get; set; } = 0;
        public decimal BaseWard { get; set; } = 0;
        public decimal BaseBlockChance { get; set; } = 0;

        // Navigation
        public ICollection<ItemAffix> Affixes { get; set; } = new List<ItemAffix>();
    }
}
