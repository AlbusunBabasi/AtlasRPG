// AtlasRPG.Application/Services/ShopService.cs
using AtlasRPG.Core.Entities.Runs;
using AtlasRPG.Core.Entities.Items;
using AtlasRPG.Core.Enums;
using AtlasRPG.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AtlasRPG.Application.Services
{
    public class ShopService
    {
        private readonly ApplicationDbContext _context;
        private readonly ItemGenerationService _itemGenerator;
        private readonly Random _random;

        public ShopService(
            ApplicationDbContext context,
            ItemGenerationService itemGenerator)
        {
            _context = context;
            _itemGenerator = itemGenerator;
            _random = new Random();
        }

        // Eski imza: GenerateShopInventory(int currentTurn)
        // YENİ imza: Run nesnesi alır — cache için
        public async Task<List<Item>> GenerateShopInventory(Run run)
        {
            // ✅ Aynı turn için cache'den dön
            if (run.ShopLastGeneratedTurn == run.CurrentTurn
                && !string.IsNullOrEmpty(run.ShopItemIdsJson)
                && run.ShopItemIdsJson != "[]")
            {
                var cachedIds = System.Text.Json.JsonSerializer.Deserialize<List<Guid>>(run.ShopItemIdsJson)
                                ?? new List<Guid>();

                if (cachedIds.Any())
                {
                    // ✅ Contains yerine tek tek yükle — SQL CTE hatasını önler
                    var cachedItems = new List<Item>();
                    foreach (var cid in cachedIds)
                    {
                        var item = await _context.Items
                            .Include(i => i.Affixes)
                                .ThenInclude(a => a.AffixDefinition)
                            .FirstOrDefaultAsync(i => i.Id == cid);

                        if (item != null)
                            cachedItems.Add(item);
                    }

                    if (cachedItems.Count > 0)
                        return cachedItems;
                }
            }

            // ✅ Yeni shop üret
            var shopItems = new List<Item>();
            for (int i = 0; i < 5; i++)
            {
                var rarity = DetermineShopRarity(run.CurrentTurn);
                var slot = GetRandomSlot();
                var item = await _itemGenerator.GenerateShopItem(slot, run.CurrentTurn, rarity);

                // Affixleri yükle
                await _context.Entry(item)
                    .Collection(x => x.Affixes)
                    .Query()
                    .Include(a => a.AffixDefinition)
                    .LoadAsync();

                shopItems.Add(item);
            }

            // ✅ Cache'e kaydet
            run.ShopLastGeneratedTurn = run.CurrentTurn;
            run.ShopItemIdsJson = System.Text.Json.JsonSerializer.Serialize(
                shopItems.Select(i => i.Id).ToList());
            await _context.SaveChangesAsync();

            return shopItems;
        }

        private ItemRarity DetermineShopRarity(int turn)
        {
            double roll = _random.NextDouble() * 100;

            if (turn >= 16)
            {
                // Turn 16-20: 25% Normal, 50% Magic, 25% Rare
                if (roll < 25) return ItemRarity.Normal;
                if (roll < 75) return ItemRarity.Magic;
                return ItemRarity.Rare;
            }
            else if (turn >= 10)
            {
                // Turn 10-14: 40% Normal, 45% Magic, 15% Rare
                if (roll < 40) return ItemRarity.Normal;
                if (roll < 85) return ItemRarity.Magic;
                return ItemRarity.Rare;
            }
            else if (turn == 9 || turn == 15)
            {
                // Turn 9, 15: 45% Normal, 45% Magic, 10% Rare
                if (roll < 45) return ItemRarity.Normal;
                if (roll < 90) return ItemRarity.Magic;
                return ItemRarity.Rare;
            }
            else if (turn >= 4)
            {
                // Turn 4-8: 60% Normal, 35% Magic, 5% Rare
                if (roll < 60) return ItemRarity.Normal;
                if (roll < 95) return ItemRarity.Magic;
                return ItemRarity.Rare;
            }
            else
            {
                // Turn 1-3: 75% Normal, 25% Magic, 0% Rare
                if (roll < 75) return ItemRarity.Normal;
                return ItemRarity.Magic;
            }
        }

        private ItemSlot GetRandomSlot()
        {
            var slots = new[] { ItemSlot.Weapon, ItemSlot.Offhand, ItemSlot.Armor, ItemSlot.Belt };
            return slots[_random.Next(slots.Length)];
        }

        public int CalculateItemPrice(Item item)
        {
            int basePrice = 8 + (4 * item.ItemLevel);

            decimal rarityMultiplier = item.Rarity switch
            {
                ItemRarity.Normal => 1.0m,
                ItemRarity.Magic => 1.8m,
                ItemRarity.Rare => 3.0m,
                _ => 1.0m
            };

            return (int)(basePrice * rarityMultiplier);
        }

        public int CalculateSellPrice(Item item)
        {
            int buyPrice = CalculateItemPrice(item);
            return (int)(buyPrice * 0.25m);
        }

        public int CalculateRerollCost(Item item)
        {
            return item.Rarity switch
            {
                ItemRarity.Normal => 6 + (2 * item.ItemLevel),
                ItemRarity.Magic => 10 + (3 * item.ItemLevel),
                ItemRarity.Rare => 18 + (5 * item.ItemLevel),
                _ => 10
            };
        }

        public int CalculateUpgradeCost(Item item)
        {
            return item.Rarity switch
            {
                ItemRarity.Normal => 35 + (6 * item.ItemLevel), // Normal -> Magic
                ItemRarity.Magic => 70 + (10 * item.ItemLevel), // Magic -> Rare
                _ => int.MaxValue // Can't upgrade Rare
            };
        }

        // AtlasRPG.Application/Services/ShopService.cs
        public async Task<bool> BuyItem(Guid runId, Guid itemId)
        {
            var run = await _context.Runs.FindAsync(runId);

            // ✅ Contains yok, direkt FindAsync / FirstOrDefaultAsync
            var item = await _context.Items
                .FirstOrDefaultAsync(i => i.Id == itemId);

            if (run == null || item == null)
                return false;

            int price = CalculateItemPrice(item);

            if (run.Gold < price)
                return false;

            run.Gold -= price;

            var runItem = new RunItem
            {
                RunId = run.Id,
                ItemId = item.Id,
                AcquiredAtTurn = run.CurrentTurn,
                IsEquipped = false
            };

            _context.RunItems.Add(runItem);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> SellItem(Guid runId, Guid runItemId)
        {
            var runItem = await _context.RunItems
                .Include(ri => ri.Item).ThenInclude(i => i.Affixes).ThenInclude(a => a.AffixDefinition)
                .Include(ri => ri.Run)
                .FirstOrDefaultAsync(ri => ri.Id == runItemId && ri.RunId == runId);

            if (runItem == null || runItem.IsEquipped)
                return false;

            int sellPrice = CalculateSellPrice(runItem.Item);

            runItem.Run.Gold += sellPrice;

            _context.RunItems.Remove(runItem);
            await _context.SaveChangesAsync();

            return true;
        }

        // ShopService.cs - RerollItem
        public async Task<bool> RerollItem(Guid runId, Guid runItemId)
        {
            var runItem = await _context.RunItems
                .Include(ri => ri.Item).ThenInclude(i => i.Affixes)
                .Include(ri => ri.Run)
                .FirstOrDefaultAsync(ri => ri.Id == runItemId && ri.RunId == runId);

            if (runItem == null) return false;

            int cost = CalculateRerollCost(runItem.Item);
            if (runItem.Run.Gold < cost) return false;

            runItem.Run.Gold -= cost;

            // ⭐ Önce gold düşüşünü kaydet
            await _context.SaveChangesAsync();

            // Sonra affix'leri kaldır ve kaydet
            _context.ItemAffixes.RemoveRange(runItem.Item.Affixes);
            await _context.SaveChangesAsync(); // ⭐ Ayrı batch

            // Yeni affix'leri oluştur ve kaydet
            await _itemGenerator.GenerateAffixes(runItem.Item, runItem.Item.Rarity, runItem.Item.ItemLevel);
            await _context.SaveChangesAsync(); // ⭐ Ayrı batch

            return true;
        }

        // ShopService.cs - UpgradeItem
        public async Task<bool> UpgradeItem(Guid runId, Guid runItemId)
        {
            var runItem = await _context.RunItems
                .Include(ri => ri.Item).ThenInclude(i => i.Affixes)
                .Include(ri => ri.Run)
                .FirstOrDefaultAsync(ri => ri.Id == runItemId && ri.RunId == runId);

            if (runItem == null || runItem.Item.Rarity == ItemRarity.Rare) return false;

            int cost = CalculateUpgradeCost(runItem.Item);
            if (runItem.Run.Gold < cost) return false;

            runItem.Run.Gold -= cost;

            // Hedef rarity ve affix sayısı
            ItemRarity newRarity = runItem.Item.Rarity == ItemRarity.Normal
                ? ItemRarity.Magic
                : ItemRarity.Rare;

            int targetAffixCount = newRarity switch
            {
                ItemRarity.Magic => 2,
                ItemRarity.Rare => 3,
                _ => 1
            };

            // ✅ Sadece eksik kadar affix ekle (mevcut korunur)
            int currentCount = runItem.Item.Affixes.Count;
            int toAdd = targetAffixCount - currentCount;

            runItem.Item.Rarity = newRarity;
            await _context.SaveChangesAsync();

            if (toAdd > 0)
            {
                // Mevcut affix key'leri — duplicate engeli için
                var existingKeys = runItem.Item.Affixes
                    .Select(a => a.AffixDefinition?.AffixKey ?? "")
                    .ToHashSet();

                await _itemGenerator.GenerateAffixesWithCount(
                    runItem.Item, toAdd, runItem.Item.ItemLevel, existingKeys);
                await _context.SaveChangesAsync();
            }

            return true;
        }
    }
}
