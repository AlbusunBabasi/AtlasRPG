// AtlasRPG.Web/Helpers/ItemIconMapper.cs
// Migration gerektirecek değişikliklerle güncellendi.
// OffhandType ve ArmorType alanlarını kullanır.

using AtlasRPG.Core.Entities.Items;
using AtlasRPG.Core.Enums;

namespace AtlasRPG.Web.Helpers
{
    public static class ItemIconMapper
    {
        // ─── Ana metod: tam path döner (/images/items/...)
        public static string GetIconPath(Item item)
        {
            if (item.WeaponType.HasValue)
                return GetWeaponIconPath(item.WeaponType.Value);

            if (item.Slot == ItemSlot.Offhand && item.OffhandType.HasValue)
                return GetOffhandIconPath(item.OffhandType.Value);

            if (item.Slot == ItemSlot.Armor && item.ArmorType.HasValue)
                return GetArmorIconPath(item.ArmorType.Value);

            return GetSlotIconPath(item.Slot);
        }

        public static string GetWeaponIconPath(WeaponType weaponType) =>
            weaponType switch
            {
                WeaponType.OneHandSword => "/images/items/weapon-1hsword.svg",
                WeaponType.TwoHandSword => "/images/items/weapon-2hsword.svg",
                WeaponType.Dagger => "/images/items/weapon-dagger.svg",
                WeaponType.Bow => "/images/items/weapon-bow.svg",
                WeaponType.Wand => "/images/items/weapon-wand.svg",
                WeaponType.Staff => "/images/items/weapon-staff.svg",
                _ => "/images/items/weapon-1hsword.svg",
            };

        public static string GetOffhandIconPath(OffhandType offhandType) =>
            offhandType switch
            {
                OffhandType.Shield => "/images/items/offhand-shield.svg",
                OffhandType.Quiver => "/images/items/offhand-quiver.svg",
                OffhandType.Focus => "/images/items/offhand-focus.svg",
                _ => "/images/items/armor-offhand.svg",
            };

        public static string GetArmorIconPath(ArmorType armorType) =>
            armorType switch
            {
                ArmorType.HeavyArmor => "/images/items/armor-heavy.svg",
                ArmorType.EvasionArmor => "/images/items/armor-evasion.svg",
                ArmorType.WardArmor => "/images/items/armor-ward.svg",
                _ => "/images/items/armor-body.svg",
            };

        public static string GetSlotIconPath(ItemSlot slot) =>
            slot switch
            {
                ItemSlot.Armor => "/images/items/armor-body.svg",
                ItemSlot.Belt => "/images/items/armor-belt.svg",
                ItemSlot.Offhand => "/images/items/armor-offhand.svg",
                ItemSlot.Weapon => "/images/items/weapon-1hsword.svg",
                _ => "/images/items/misc-unknown.svg",
            };

        // ─── Fallback emoji
        public static string GetFallbackEmoji(Item item)
        {
            if (item.WeaponType.HasValue)
                return item.WeaponType.Value switch
                {
                    WeaponType.OneHandSword => "🗡️",
                    WeaponType.TwoHandSword => "⚔️",
                    WeaponType.Dagger => "🔪",
                    WeaponType.Bow => "🏹",
                    WeaponType.Wand => "🪄",
                    WeaponType.Staff => "🔮",
                    _ => "⚔️",
                };

            if (item.Slot == ItemSlot.Offhand && item.OffhandType.HasValue)
                return item.OffhandType.Value switch
                {
                    OffhandType.Shield => "🛡️",
                    OffhandType.Quiver => "🪶",
                    OffhandType.Focus => "💎",
                    _ => "🔰",
                };

            if (item.Slot == ItemSlot.Armor && item.ArmorType.HasValue)
                return item.ArmorType.Value switch
                {
                    ArmorType.HeavyArmor => "🪬",
                    ArmorType.EvasionArmor => "💨",
                    ArmorType.WardArmor => "🔵",
                    _ => "🛡️",
                };

            return item.Slot switch
            {
                ItemSlot.Belt => "🔰",
                _ => "📦",
            };
        }

        // ─── Display name
        public static string GetDisplayName(Item item)
        {
            string typeStr = GetTypeString(item);

            return item.Rarity switch
            {
                ItemRarity.Rare => RarePrefix(item, typeStr),
                ItemRarity.Magic => MagicPrefix(item, typeStr),
                _ => typeStr,
            };
        }

        // ─── Alt tip adı (slot badge için)
        public static string GetSubTypeName(Item item)
        {
            if (item.WeaponType.HasValue)
                return item.WeaponType.Value switch
                {
                    WeaponType.OneHandSword => "1H Sword",
                    WeaponType.TwoHandSword => "2H Sword",
                    WeaponType.Dagger => "Dagger",
                    WeaponType.Bow => "Bow",
                    WeaponType.Wand => "Wand",
                    WeaponType.Staff => "Staff",
                    _ => "Weapon",
                };

            if (item.Slot == ItemSlot.Offhand && item.OffhandType.HasValue)
                return item.OffhandType.Value switch
                {
                    OffhandType.Shield => "Shield",
                    OffhandType.Quiver => "Quiver",
                    OffhandType.Focus => "Focus",
                    _ => "Offhand",
                };

            if (item.Slot == ItemSlot.Armor && item.ArmorType.HasValue)
                return item.ArmorType.Value switch
                {
                    ArmorType.HeavyArmor => "Heavy Armour",
                    ArmorType.EvasionArmor => "Evasion Armour",
                    ArmorType.WardArmor => "Ward Armour",
                    _ => "Armour",
                };

            return item.Slot switch
            {
                ItemSlot.Belt => "Belt",
                _ => item.Slot.ToString(),
            };
        }

        // ─── Rarity label
        public static string GetRarityLabel(ItemRarity rarity) =>
            rarity switch
            {
                ItemRarity.Rare => "Rare",
                ItemRarity.Magic => "Magic",
                _ => "Normal",
            };

        // ─── CSS rarity class
        public static string GetRarityClass(ItemRarity rarity) =>
            rarity switch
            {
                ItemRarity.Rare => "rr",
                ItemRarity.Magic => "rm",
                _ => "rn",
            };

        // ─── Affix değerini doğru formatta göster
        // IsPercentage = true → 0.12 → "+12%"
        // IsPercentage = false → 5.0 → "+5"
        public static string FormatAffixValue(ItemAffix aff)
        {
            if (aff.AffixDefinition == null)
                return aff.RolledValue.ToString("F1");

            if (aff.AffixDefinition.IsPercentage)
                return $"+{(aff.RolledValue * 100):F0}%";

            // Flat değer — tam sayıysa integer göster
            return aff.RolledValue == Math.Floor(aff.RolledValue)
                ? $"+{(int)aff.RolledValue}"
                : $"+{aff.RolledValue:F1}";
        }
        public static string GetImplicitStats(Item item)
        {
            var parts = new List<string>();

            if (item.Slot == ItemSlot.Weapon)
            {
                if (item.BaseDamage > 0)
                    parts.Add($"⚔️ {item.BaseDamage:F0} dmg");
                if (item.BaseAttackSpeed > 0)
                    parts.Add($"⚡ {item.BaseAttackSpeed:F0} spd");
                if (item.BaseCritChance > 0)
                    parts.Add($"💥 {(item.BaseCritChance * 100):F0}% crit");
            }
            else if (item.Slot == ItemSlot.Offhand)
            {
                if (item.BaseBlockChance > 0)
                    parts.Add($"🛡️ {(item.BaseBlockChance * 100):F0}% block");
                if (item.BaseArmor > 0)
                    parts.Add($"🪬 {item.BaseArmor:F0} armor");
                if (item.BaseEvasion > 0)
                    parts.Add($"💨 {item.BaseEvasion:F0} evasion");
                if (item.BaseWard > 0)
                    parts.Add($"🔵 {item.BaseWard:F0} ward");
            }
            else if (item.Slot == ItemSlot.Armor)
            {
                if (item.BaseArmor > 0)
                    parts.Add($"🪬 {item.BaseArmor:F0} armor");
                if (item.BaseEvasion > 0)
                    parts.Add($"💨 {item.BaseEvasion:F0} evasion");
                if (item.BaseWard > 0)
                    parts.Add($"🔵 {item.BaseWard:F0} ward");
            }

            return parts.Any() ? string.Join("  ", parts) : string.Empty;
        }

        // ─── PRIVATE HELPERS ─────────────────────────────────────────

        private static string GetTypeString(Item item)
        {
            if (item.WeaponType.HasValue)
                return item.WeaponType.Value switch
                {
                    WeaponType.OneHandSword => "Sword",
                    WeaponType.TwoHandSword => "Greatsword",
                    WeaponType.Dagger => "Dagger",
                    WeaponType.Bow => "Bow",
                    WeaponType.Wand => "Wand",
                    WeaponType.Staff => "Staff",
                    _ => "Weapon",
                };

            if (item.Slot == ItemSlot.Offhand && item.OffhandType.HasValue)
                return item.OffhandType.Value switch
                {
                    OffhandType.Shield => "Shield",
                    OffhandType.Quiver => "Quiver",
                    OffhandType.Focus => "Focus",
                    _ => "Offhand",
                };

            if (item.Slot == ItemSlot.Armor && item.ArmorType.HasValue)
                return item.ArmorType.Value switch
                {
                    ArmorType.HeavyArmor => "Iron Plate",
                    ArmorType.EvasionArmor => "Leather Armour",
                    ArmorType.WardArmor => "Silk Robe",
                    _ => "Armour",
                };

            return item.Slot switch
            {
                ItemSlot.Belt => "Belt",
                _ => item.Slot.ToString(),
            };
        }

        private static string RarePrefix(Item item, string typeStr)
        {
            string[] prefixes = item.Slot switch
            {
                ItemSlot.Armor when item.ArmorType == ArmorType.HeavyArmor
                    => new[] { "Iron Fortress", "Shadow Plate", "Obsidian Shell", "Titan Guard" },
                ItemSlot.Armor when item.ArmorType == ArmorType.EvasionArmor
                    => new[] { "Veil of Night", "Ghost Step", "Whisper Hide", "Shadow Weave" },
                ItemSlot.Armor when item.ArmorType == ArmorType.WardArmor
                    => new[] { "Arcane Shroud", "Void Silk", "Ether Weave", "Mystic Robe" },
                ItemSlot.Offhand when item.OffhandType == OffhandType.Shield
                    => new[] { "Aegis of Dawn", "Iron Bulwark", "Ghost Barrier", "Titan Wall" },
                ItemSlot.Offhand when item.OffhandType == OffhandType.Quiver
                    => new[] { "Swiftflight", "Eagle Eye", "Storm Quiver", "Void Bolt" },
                ItemSlot.Offhand when item.OffhandType == OffhandType.Focus
                    => new[] { "Voidkeeper", "Arcane Lens", "Ether Core", "Soul Prism" },
                ItemSlot.Belt
                    => new[] { "Binding of Ash", "Dire Girdle", "Viper Coil", "Storm Lash" },
                _ => new[] { "Devastation", "Carnage", "Sorrow's Edge", "Void Fang" }
            };
            return prefixes[Math.Abs(item.Id.GetHashCode()) % prefixes.Length];
        }

        private static string MagicPrefix(Item item, string typeStr)
        {
            var prefixes = new[] {
                "Jagged", "Serrated", "Heavy", "Sharp",
                "Glinting", "Polished", "Burning", "Icy",
                "Arcane", "Hollow", "Runed"
            };
            string pre = prefixes[Math.Abs(item.Id.GetHashCode()) % prefixes.Length];
            return $"{pre} {typeStr}";
        }
    }
}

