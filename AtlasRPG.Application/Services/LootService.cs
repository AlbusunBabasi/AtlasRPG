// ============================================================
// AtlasRPG.Application/Services/LootService.cs
// ============================================================
// PVE loot sistemi:
//   - Win: 2 item drop, Loss: 1 item drop
//   - PVP: item drop YOK
//   - ilvl = turnNumber
//   - Rarity eğrisi turn bazlı
//   - Slot: Weapon/Offhand/Armor/Belt ağırlıklı random
// ============================================================

using AtlasRPG.Core.Entities.Items;
using AtlasRPG.Core.Entities.Runs;
using AtlasRPG.Core.Enums;
using AtlasRPG.Infrastructure.Data;

namespace AtlasRPG.Application.Services
{
    public class LootService
    {
        private readonly ApplicationDbContext _context;
        private readonly ItemGenerationService _itemGen;
        private readonly Random _random;

        public LootService(
            ApplicationDbContext context,
            ItemGenerationService itemGen)
        {
            _context = context;
            _itemGen = itemGen;
            _random = new Random();
        }

        // ────────────────────────────────────────────────────────────
        // ANA METOD: PVE turn sonrası loot üret ve envantere ekle
        // Dönen liste: yeni eklenen RunItem'lar (view'de göstermek için)
        // ────────────────────────────────────────────────────────────
        public async Task<List<RunItem>> GeneratePveLoot(
            Run run,
            int turnNumber,
            bool isVictory)
        {
            int dropCount = isVictory ? 2 : 1;
            var droppedItems = new List<RunItem>();

            for (int i = 0; i < dropCount; i++)
            {
                var slot = RollSlot(run.Class);
                var rarity = RollRarity(turnNumber);
                var item = await _itemGen.GenerateItem(slot, turnNumber, rarity, run.Class);

                var runItem = new RunItem
                {
                    RunId = run.Id,
                    ItemId = item.Id,
                    AcquiredAtTurn = turnNumber,
                    IsEquipped = false,
                };

                _context.RunItems.Add(runItem);
                droppedItems.Add(runItem);
            }

            await _context.SaveChangesAsync();

            // Navigate property'yi doldur (view için)
            foreach (var ri in droppedItems)
            {
                await _context.Entry(ri)
                    .Reference(x => x.Item)
                    .LoadAsync();
                await _context.Entry(ri.Item)
                    .Collection(x => x.Affixes)
                    .LoadAsync();
                foreach (var affix in ri.Item.Affixes)
                {
                    await _context.Entry(affix)
                        .Reference(x => x.AffixDefinition)
                        .LoadAsync();
                }
            }

            return droppedItems;
        }

        // ────────────────────────────────────────────────────────────
        // RARITY TABLOSU (tasarım dokümanı: PVE Loot Tablosu)
        //
        //  Turn  | Normal | Magic | Rare
        //  1–3   |   70%  |  30%  |  0%
        //  4–8   |   62%  |  33%  |  5%   ← interpolated
        //  9     |   55%  |  40%  |  5%
        //  10–14 |   48%  |  42%  | 10%   ← interpolated
        //  15    |   40%  |  45%  | 15%
        //  16–20 |   33%  |  47%  | 20%   ← extrapolated
        // ────────────────────────────────────────────────────────────
        private ItemRarity RollRarity(int turn)
        {
            var (normal, magic, _) = turn switch
            {
                <= 3 => (70, 30, 0),
                <= 8 => (62, 33, 5),
                9 => (55, 40, 5),
                <= 14 => (48, 42, 10),
                15 => (40, 45, 15),
                _ => (33, 47, 20),
            };

            int roll = _random.Next(100);
            if (roll < normal) return ItemRarity.Normal;
            if (roll < normal + magic) return ItemRarity.Magic;
            return ItemRarity.Rare;
        }

        // ────────────────────────────────────────────────────────────
        // SLOT SEÇİMİ — class bazlı ağırlık
        // ────────────────────────────────────────────────────────────
        private ItemSlot RollSlot(ClassType classType)
        {
            // Genel ağırlıklar: Weapon 30, Offhand 20, Armor 30, Belt 20
            // Bazı class'lar için weapon ağırlığı biraz artar
            bool isSpellClass = classType is ClassType.Mage;


            // [Weapon, Offhand, Armor, Belt] kümülatif ağırlıklar
            int[] weights = isSpellClass
                ? [30, 25, 28, 17]   // Caster: offhand (focus/wand) biraz favori
                : [32, 18, 32, 18];  // Melee/Ranged: weapon+armor önde

            int roll = _random.Next(100);
            int cumulative = 0;

            if (roll < (cumulative += weights[0])) return ItemSlot.Weapon;
            if (roll < (cumulative += weights[1])) return ItemSlot.Offhand;
            if (roll < (cumulative += weights[2])) return ItemSlot.Armor;
            return ItemSlot.Belt;
        }
    }

    // ────────────────────────────────────────────────────────────────
    // DTO — CombatResult view'ine geçmek için
    // ────────────────────────────────────────────────────────────────
    public class LootDropResult
    {
        public bool IsPveLoot { get; set; }
        public bool IsVictory { get; set; }
        public List<LootItemDto> Items { get; set; } = new();
    }

    public class LootItemDto
    {
        public Guid RunItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public string Slot { get; set; } = string.Empty;
        public string Rarity { get; set; } = string.Empty;
        public int ItemLevel { get; set; }
        public string? WeaponType { get; set; }
        public List<string> AffixSummaries { get; set; } = new();

        public static LootItemDto FromRunItem(RunItem ri)
        {
            var item = ri.Item;
            var dto = new LootItemDto
            {
                RunItemId = ri.Id,
                ItemName = item.WeaponType.HasValue
                    ? $"{item.WeaponType} ({item.Rarity})"
                    : $"{item.Slot} ({item.Rarity})",
                Slot = item.Slot.ToString(),
                Rarity = item.Rarity.ToString(),
                ItemLevel = item.ItemLevel,
                WeaponType = item.WeaponType?.ToString(),
            };

            foreach (var affix in item.Affixes ?? [])
            {
                string summary = affix.AffixDefinition?.DisplayName ?? "?";
                string val = affix.RolledValue switch
                {
                    >= 0.1m and < 2m => $"{(affix.RolledValue * 100):F0}%",
                    _ => $"+{affix.RolledValue:F1}",
                };
                dto.AffixSummaries.Add($"{summary} {val} <span class='tier-badge t{affix.Tier}'>T{affix.Tier}</span>");
            }

            return dto;
        }
    }
}
