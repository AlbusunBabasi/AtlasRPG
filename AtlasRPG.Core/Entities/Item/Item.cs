using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AtlasRPG.Core.Enums;

namespace AtlasRPG.Core.Entities.Items
{
    public class Item : BaseEntity
    {
        public ItemSlot Slot { get; set; }
        public ItemRarity Rarity { get; set; }
        public int ItemLevel { get; set; }
        public WeaponType? WeaponType { get; set; } // Null if not weapon

        // Weapon Base Stats
        public decimal BaseAttackSpeed { get; set; } = 0;
        public decimal BaseDamage { get; set; } = 0;
        public decimal BaseCritChance { get; set; } = 0;

        // Armor Base Stats
        public decimal BaseArmor { get; set; } = 0;
        public decimal BaseEvasion { get; set; } = 0;
        public decimal BaseWard { get; set; } = 0;
        public decimal BaseBlockChance { get; set; } = 0;

        // Navigation
        public ICollection<ItemAffix> Affixes { get; set; } = new List<ItemAffix>();
    }
}