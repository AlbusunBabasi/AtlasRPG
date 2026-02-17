// AtlasRPG.Application/Services/ItemGenerationService.cs
using AtlasRPG.Core.Entities.Items;
using AtlasRPG.Core.Enums;
using AtlasRPG.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AtlasRPG.Application.Services
{
    public class ItemGenerationService
    {
        private readonly ApplicationDbContext _context;
        private readonly Random _random;

        public ItemGenerationService(ApplicationDbContext context)
        {
            _context = context;
            _random = new Random();
        }

        // ─── Ana üretim metodu ───────────────────────────────────────────
        public async Task<Item> GenerateItem(
            ItemSlot slot,
            int itemLevel,
            ItemRarity rarity,
            ClassType classType)
        {
            var item = new Item
            {
                Slot = slot,
                Rarity = rarity,
                ItemLevel = itemLevel
            };

            switch (slot)
            {
                case ItemSlot.Weapon:
                    SetWeaponStats(item, classType, itemLevel);
                    break;

                case ItemSlot.Offhand:
                    SetOffhandStats(item, classType, itemLevel);
                    break;

                case ItemSlot.Armor:
                    SetArmorStats(item, classType, itemLevel);
                    break;

                case ItemSlot.Belt:
                    // Belt: sadece affix, base stat yok
                    break;
            }

            _context.Items.Add(item);
            await _context.SaveChangesAsync();

            await GenerateAffixes(item, rarity, itemLevel);

            return item;
        }

        // ─── Shop için: sınıf kısıtı olmadan rastgele item ──────────────
        // ShopService hardcoded ClassType.Warrior kullanıyordu,
        // artık bu method çağrılınca random weapon type üretilecek.
        public async Task<Item> GenerateShopItem(
            ItemSlot slot,
            int itemLevel,
            ItemRarity rarity)
        {
            // Shop'ta tüm sınıfların itemleri çıkabilir
            var allClasses = new[] {
                ClassType.Warrior, ClassType.Rogue, ClassType.Archer,
                ClassType.Mage, ClassType.Berserker
            };
            var randomClass = allClasses[_random.Next(allClasses.Length)];
            return await GenerateItem(slot, itemLevel, rarity, randomClass);
        }

        // ════════════════════════════════════════════════════════════════
        // WEAPON
        // ════════════════════════════════════════════════════════════════
        private void SetWeaponStats(Item item, ClassType classType, int ilvl)
        {
            item.WeaponType = DetermineWeaponType(classType);

            // Base damage — 2H silahlar biraz daha güçlü
            bool is2H = item.WeaponType is
                WeaponType.TwoHandSword or WeaponType.Staff;
            bool isRanged = item.WeaponType is WeaponType.Bow;
            bool isSpell = item.WeaponType is WeaponType.Wand or WeaponType.Staff;

            decimal damageMultiplier = is2H ? 1.35m : 1.0m;
            item.BaseDamage = (10 + ilvl * 2) * damageMultiplier;

            // Attack speed — dagger hızlı, 2h yavaş
            item.BaseAttackSpeed = item.WeaponType switch
            {
                WeaponType.Dagger => 7m,
                WeaponType.OneHandSword => 5m,
                WeaponType.Wand => 5m,
                WeaponType.Bow => 4m,
                WeaponType.TwoHandSword => 3m,
                WeaponType.Staff => 3m,
                _ => 5m
            };

            // Base crit — dagger en yüksek
            item.BaseCritChance = item.WeaponType switch
            {
                WeaponType.Dagger => 0.08m,
                WeaponType.OneHandSword => 0.05m,
                WeaponType.Bow => 0.06m,
                WeaponType.Wand => 0.04m,
                WeaponType.TwoHandSword => 0.04m,
                WeaponType.Staff => 0.03m,
                _ => 0.05m
            };
        }

        // ════════════════════════════════════════════════════════════════
        // OFFHAND — Shield / Quiver / Focus
        // ════════════════════════════════════════════════════════════════
        private void SetOffhandStats(Item item, ClassType classType, int ilvl)
        {
            item.OffhandType = DetermineOffhandType(classType);

            switch (item.OffhandType)
            {
                case OffhandType.Shield:
                    // Yüksek block + biraz armor
                    item.BaseBlockChance = 0.15m + ilvl * 0.005m;
                    item.BaseArmor = 3 + ilvl;
                    break;

                case OffhandType.Quiver:
                    // Evasion + hafif armor (Archer)
                    item.BaseEvasion = 4 + ilvl;
                    break;

                case OffhandType.Focus:
                    // Ward (Mage)
                    item.BaseWard = 3 + ilvl;
                    break;
            }
        }

        // ════════════════════════════════════════════════════════════════
        // ARMOR — HeavyArmor / EvasionArmor / WardArmor
        // ════════════════════════════════════════════════════════════════
        private void SetArmorStats(Item item, ClassType classType, int ilvl)
        {
            item.ArmorType = DetermineArmorType(classType);

            switch (item.ArmorType)
            {
                case ArmorType.HeavyArmor:
                    // Zırhlı: yüksek armor, düşük evasion, sıfır ward
                    item.BaseArmor = 8 + ilvl * 2;
                    item.BaseEvasion = 2 + ilvl / 2;
                    item.BaseWard = 0;
                    break;

                case ArmorType.EvasionArmor:
                    // Hafif: düşük armor, yüksek evasion, sıfır ward
                    item.BaseArmor = 2 + ilvl / 2;
                    item.BaseEvasion = 7 + ilvl * 2;
                    item.BaseWard = 0;
                    break;

                case ArmorType.WardArmor:
                    // Büyücü: düşük armor+evasion, yüksek ward
                    item.BaseArmor = 1 + ilvl / 3;
                    item.BaseEvasion = 1 + ilvl / 3;
                    item.BaseWard = 6 + ilvl * 2;
                    break;
            }
        }

        // ════════════════════════════════════════════════════════════════
        // SUBTYPESİ BELİRLE
        // ════════════════════════════════════════════════════════════════

        private WeaponType DetermineWeaponType(ClassType classType)
        {
            return classType switch
            {
                ClassType.Warrior => _random.Next(2) == 0
                                         ? WeaponType.OneHandSword
                                         : WeaponType.TwoHandSword,

                ClassType.Rogue => _random.Next(3) == 0
                                         ? WeaponType.OneHandSword
                                         : WeaponType.Dagger,

                ClassType.Archer => WeaponType.Bow,

                ClassType.Mage => _random.Next(2) == 0
                                         ? WeaponType.Staff
                                         : WeaponType.Wand,

                ClassType.Berserker => WeaponType.TwoHandSword,

                _ => WeaponType.OneHandSword
            };
        }

        private OffhandType DetermineOffhandType(ClassType classType)
        {
            return classType switch
            {
                ClassType.Warrior => OffhandType.Shield,
                ClassType.Berserker => OffhandType.Shield,
                ClassType.Archer => OffhandType.Quiver,
                ClassType.Mage => OffhandType.Focus,
                ClassType.Rogue => _random.Next(2) == 0
                                         ? OffhandType.Shield
                                         : OffhandType.Focus,
                _ => OffhandType.Shield
            };
        }

        private ArmorType DetermineArmorType(ClassType classType)
        {
            return classType switch
            {
                ClassType.Warrior => ArmorType.HeavyArmor,
                ClassType.Berserker => ArmorType.HeavyArmor,
                ClassType.Archer => ArmorType.EvasionArmor,
                ClassType.Rogue => ArmorType.EvasionArmor,
                ClassType.Mage => ArmorType.WardArmor,
                _ => ArmorType.HeavyArmor
            };
        }

        // ════════════════════════════════════════════════════════════════
        // AFFIX ÜRETİMİ
        // ════════════════════════════════════════════════════════════════
        public async Task GenerateAffixes(Item item, ItemRarity rarity, int itemLevel)
        {
            int affixCount = rarity switch
            {
                ItemRarity.Rare => 3,
                ItemRarity.Magic => 2,
                _ => 1  // Normal
            };

            // Slot + subtype bilgisiyle affix havuzunu belirle
            string slotKey = GetSlotKey(item);
            // Hem spesifik (Shield/Quiver/Focus) hem de genel "Offhand" affixleri dahil et
            var eligibleAffixes = await _context.AffixDefinitions
                .Where(a => a.AllowedSlots.Contains(slotKey)
                         || (item.Slot == ItemSlot.Offhand && a.AllowedSlots.Contains("Offhand")))
                .ToListAsync();

            if (!eligibleAffixes.Any()) return;

            var usedKeys = new HashSet<string>();

            for (int i = 0; i < affixCount; i++)
            {
                var available = eligibleAffixes
                    .Where(a => !usedKeys.Contains(a.AffixKey))
                    .ToList();

                if (!available.Any()) break;

                var chosen = available[_random.Next(available.Count)];
                usedKeys.Add(chosen.AffixKey);

                // Tier roll: T1=65%, T2=25%, T3=10%
                int tierRoll = _random.Next(100);
                int tier;
                decimal min, max;

                if (tierRoll < 65)
                {
                    tier = 1; min = chosen.Tier1Min; max = chosen.Tier1Max;
                }
                else if (tierRoll < 90)
                {
                    tier = 2; min = chosen.Tier2Min; max = chosen.Tier2Max;
                }
                else
                {
                    tier = 3; min = chosen.Tier3Min; max = chosen.Tier3Max;
                }

                decimal range = max - min;
                decimal rolled = range <= 0
                    ? min
                    : min + (decimal)(_random.NextDouble()) * range;

                rolled = Math.Round(rolled, 4);

                _context.ItemAffixes.Add(new ItemAffix
                {
                    ItemId = item.Id,
                    AffixDefinitionId = chosen.Id,
                    Tier = tier,
                    RolledValue = rolled
                });
            }

            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Belirli sayıda, mevcut key'leri hariç tutarak affix ekler (Upgrade için).
        /// </summary>
        public async Task GenerateAffixesWithCount(
            Item item,
            int count,
            int itemLevel,
            HashSet<string> existingKeys)
        {
            string slotKey = GetSlotKey(item);

            var eligibleAffixes = await _context.AffixDefinitions
                .Where(a => a.AllowedSlots.Contains(slotKey))
                .ToListAsync();

            for (int i = 0; i < count; i++)
            {
                var available = eligibleAffixes
                    .Where(a => !existingKeys.Contains(a.AffixKey))
                    .ToList();

                if (!available.Any()) break;

                var chosen = available[_random.Next(available.Count)];
                existingKeys.Add(chosen.AffixKey);

                int tierRoll = _random.Next(100);
                int tier;
                decimal min, max;

                if (tierRoll < 65) { tier = 1; min = chosen.Tier1Min; max = chosen.Tier1Max; }
                else if (tierRoll < 90) { tier = 2; min = chosen.Tier2Min; max = chosen.Tier2Max; }
                else { tier = 3; min = chosen.Tier3Min; max = chosen.Tier3Max; }

                decimal range = max - min;
                decimal rolled = range <= 0 ? min : min + (decimal)(_random.NextDouble()) * range;
                rolled = Math.Round(rolled, 4);

                _context.ItemAffixes.Add(new ItemAffix
                {
                    ItemId = item.Id,
                    AffixDefinitionId = chosen.Id,
                    Tier = tier,
                    RolledValue = rolled
                });
            }

            await _context.SaveChangesAsync();
        }

        // Affix filtreleme için slot anahtarı
        // AffixDefinition.AllowedSlots: "Weapon", "Offhand", "Armor", "Belt"
        // + subtype ekstraları: "Offhand(Shield)", "Offhand(Quiver)", "Offhand(Focus)"
        private string GetSlotKey(Item item)
        {
            return item.Slot switch
            {
                ItemSlot.Weapon => "Weapon",
                ItemSlot.Armor => "Armor",
                ItemSlot.Belt => "Belt",
                ItemSlot.Offhand => item.OffhandType switch
                {
                    OffhandType.Shield => "Shield",
                    OffhandType.Quiver => "Quiver",
                    OffhandType.Focus => "Focus",
                    _ => "Offhand"
                },
                _ => "Offhand"
            };
        }

    }
}
