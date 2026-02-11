// AtlasRPG.Application/Services/BotService.cs
using AtlasRPG.Core.Entities.Runs;
using AtlasRPG.Core.Enums;
using AtlasRPG.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AtlasRPG.Application.Services
{
    public class BotService
    {
        private readonly ApplicationDbContext _context;
        private readonly ItemGenerationService _itemGenerator;
        private readonly Random _random;
        private string? _botUserId; // ⭐ Cache bot user ID

        public BotService(
            ApplicationDbContext context,
            ItemGenerationService itemGenerator)
        {
            _context = context;
            _itemGenerator = itemGenerator;
            _random = new Random();
        }

        // ⭐ Bot UserId'yi al (cache'lenmiş)
        private async Task<string> GetBotUserId()
        {
            if (_botUserId == null)
            {
                var botUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == "bot@system.internal");

                if (botUser == null)
                {
                    throw new InvalidOperationException(
                        "Bot user not found in database. Please ensure the system bot user is created during application startup.");
                }

                _botUserId = botUser.Id;
            }

            return _botUserId;
        }

        public async Task<Run> GeneratePveBot(int turnNumber, int playerLevel)
        {
            // ⭐ Gerçek bot UserId'yi al
            var botUserId = await GetBotUserId();

            // Random race and class
            var races = Enum.GetValues<RaceType>();
            var classes = Enum.GetValues<ClassType>();

            var race = races[_random.Next(races.Length)];
            var classType = classes[_random.Next(classes.Length)];

            // Create bot run (not persisted to user's runs)
            var bot = new Run
            {
                UserId = botUserId, // ✅ Gerçek UserId kullan
                Race = race,
                Class = classType,
                RunHash = Guid.NewGuid().ToString("N"),
                CurrentTurn = turnNumber,
                CurrentLevel = playerLevel, // Same level as player
                RemainingLives = 1,
                Gold = 0,
                IsActive = false, // Bots don't have "active" runs
                AvailableStatPoints = 0,
                AvailableSkillPoints = 0,
                PvpPoints = 0,
                HasWoundDebuff = false
            };

            // Allocate stats (slightly randomized)
            int totalStatPoints = (playerLevel - 1) * 2; // 2 per level
            DistributeStatPoints(bot, totalStatPoints, classType);

            _context.Runs.Add(bot);
            await _context.SaveChangesAsync();

            // Generate equipment scaled to turn
            await GenerateBotEquipment(bot, turnNumber);

            return bot;
        }

        private void DistributeStatPoints(Run bot, int points, ClassType classType)
        {
            // Base: 5 each
            bot.Strength = 5;
            bot.Dexterity = 5;
            bot.Agility = 5;
            bot.Intelligence = 5;
            bot.Vitality = 5;
            bot.Wisdom = 5;
            bot.Luck = 5;

            // Distribute based on class archetype
            for (int i = 0; i < points; i++)
            {
                switch (classType)
                {
                    case ClassType.Warrior:
                        if (i % 3 == 0) bot.Strength++;
                        else if (i % 3 == 1) bot.Vitality++;
                        else bot.Dexterity++;
                        break;

                    case ClassType.Berserker:
                        if (i % 2 == 0) bot.Strength++;
                        else bot.Vitality++;
                        break;

                    case ClassType.Rogue:
                        if (i % 3 == 0) bot.Dexterity++;
                        else if (i % 3 == 1) bot.Agility++;
                        else bot.Luck++;
                        break;

                    case ClassType.Archer:
                        if (i % 2 == 0) bot.Dexterity++;
                        else bot.Agility++;
                        break;

                    case ClassType.Mage:
                        if (i % 3 == 0) bot.Intelligence++;
                        else if (i % 3 == 1) bot.Wisdom++;
                        else bot.Vitality++;
                        break;
                }
            }
        }

        private async Task GenerateBotEquipment(Run bot, int turnNumber)
        {
            // Determine rarity based on turn
            ItemRarity rarity = turnNumber switch
            {
                <= 3 => ItemRarity.Normal,
                <= 8 => _random.NextDouble() < 0.7 ? ItemRarity.Normal : ItemRarity.Magic,
                <= 14 => _random.NextDouble() < 0.5 ? ItemRarity.Normal : ItemRarity.Magic,
                _ => _random.NextDouble() < 0.3 ? ItemRarity.Magic : ItemRarity.Rare
            };

            // Generate items
            var weapon = await _itemGenerator.GenerateItem(ItemSlot.Weapon, turnNumber, rarity, bot.Class);
            var armor = await _itemGenerator.GenerateItem(ItemSlot.Armor, turnNumber, rarity, bot.Class);
            var belt = await _itemGenerator.GenerateItem(ItemSlot.Belt, turnNumber, rarity, bot.Class);

            // Add to bot's inventory
            var weaponRunItem = new RunItem
            {
                RunId = bot.Id,
                ItemId = weapon.Id,
                AcquiredAtTurn = 0,
                IsEquipped = true
            };

            var armorRunItem = new RunItem
            {
                RunId = bot.Id,
                ItemId = armor.Id,
                AcquiredAtTurn = 0,
                IsEquipped = true
            };

            var beltRunItem = new RunItem
            {
                RunId = bot.Id,
                ItemId = belt.Id,
                AcquiredAtTurn = 0,
                IsEquipped = true
            };

            _context.RunItems.AddRange(weaponRunItem, armorRunItem, beltRunItem);

            // Create equipment
            var equipment = new RunEquipment
            {
                RunId = bot.Id,
                WeaponId = weaponRunItem.Id,
                ArmorId = armorRunItem.Id,
                BeltId = beltRunItem.Id
            };

            _context.RunEquipments.Add(equipment);
            await _context.SaveChangesAsync();
        }
    }
}
