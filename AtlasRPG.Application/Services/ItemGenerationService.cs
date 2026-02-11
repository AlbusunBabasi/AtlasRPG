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

            // Set base stats based on slot
            if (slot == ItemSlot.Weapon)
            {
                item.WeaponType = DetermineWeaponType(classType);
                item.BaseDamage = 10 + itemLevel * 2; // Simple scaling
                item.BaseAttackSpeed = 5m;
                item.BaseCritChance = 0.05m;
            }
            else if (slot == ItemSlot.Armor)
            {
                item.BaseArmor = 5 + itemLevel;
                item.BaseEvasion = 3 + itemLevel;
            }
            else if (slot == ItemSlot.Belt)
            {
                // Belt has no base stats, only affixes
            }

            _context.Items.Add(item);
            await _context.SaveChangesAsync();

            // Generate affixes
            await GenerateAffixes(item, rarity, itemLevel);

            return item;
        }

        private WeaponType DetermineWeaponType(ClassType classType)
        {
            return classType switch
            {
                ClassType.Warrior => WeaponType.OneHandSword,
                ClassType.Berserker => WeaponType.TwoHandSword,
                ClassType.Rogue => WeaponType.Dagger,
                ClassType.Archer => WeaponType.Bow,
                ClassType.Mage => WeaponType.Wand,
                _ => WeaponType.OneHandSword
            };
        }

        public async Task GenerateAffixes(Item item, ItemRarity rarity, int itemLevel)
        {
            int currentAffixCount = item.Affixes.Count;

            int targetAffixCount = rarity switch
            {
                ItemRarity.Normal => 1,
                ItemRarity.Magic => 2,
                ItemRarity.Rare => 3,
                _ => 1
            };

            int affixesToAdd = targetAffixCount - currentAffixCount;

            if (affixesToAdd <= 0)
            {
                if (currentAffixCount > 0)
                    affixesToAdd = targetAffixCount;
                else
                    return;
            }

            var slotName = item.Slot.ToString();

            // ✅ DÜZELTME: Contains() yerine EF.Functions.Like() kullan
            // EF Core 7+ string.Contains() -> COLLATE clause üretir, SQL Server'da hata verir
            var availableAffixes = await _context.AffixDefinitions
                .Where(a => EF.Functions.Like(a.AllowedSlots, $"%{slotName}%"))
                .ToListAsync();

            if (!availableAffixes.Any())
                return;

            var existingAffixIds = item.Affixes.Select(a => a.AffixDefinitionId).ToList();
            var selectableAffixes = availableAffixes
                .Where(a => !existingAffixIds.Contains(a.Id))
                .ToList();

            // Select random affixes
            var selectedAffixes = selectableAffixes
                .OrderBy(x => _random.Next())
                .Take(affixesToAdd)
                .ToList();

            foreach (var affixDef in selectedAffixes)
            {
                // Determine tier based on item level
                int tier = DetermineTier(itemLevel);

                // Roll value within tier range
                decimal rolledValue = tier switch
                {
                    1 => RollInRange(affixDef.Tier1Min, affixDef.Tier1Max),
                    2 => RollInRange(affixDef.Tier2Min, affixDef.Tier2Max),
                    3 => RollInRange(affixDef.Tier3Min, affixDef.Tier3Max),
                    _ => affixDef.Tier1Min
                };

                var itemAffix = new ItemAffix
                {
                    ItemId = item.Id,
                    AffixDefinitionId = affixDef.Id,
                    Tier = tier,
                    RolledValue = rolledValue
                };

                _context.ItemAffixes.Add(itemAffix);
            }

            await _context.SaveChangesAsync();
        }

        private int DetermineTier(int itemLevel)
        {
            double roll = _random.NextDouble();

            // Temel roll: T1 %65 / T2 %25 / T3 %10
            int rawTier;
            if (roll < 0.65) rawTier = 1;
            else if (roll < 0.90) rawTier = 2;
            else rawTier = 3;

            // ilvl kapısı — düşük level yüksek tier açamaz
            // Tier 2 için minimum ilvl: 8
            // Tier 3 için minimum ilvl: 15
            if (rawTier == 3 && itemLevel < 15)
                rawTier = itemLevel >= 8 ? 2 : 1;   // Geri düş: T3→T2, ya da T3→T1

            if (rawTier == 2 && itemLevel < 8)
                rawTier = 1;                          // T2→T1

            return rawTier;
        }

        private decimal RollInRange(decimal min, decimal max)
        {
            double range = (double)(max - min);
            double roll = _random.NextDouble() * range;
            return min + (decimal)roll;
        }
    }
}
