// AtlasRPG.Application/Services/RunService.cs
using AtlasRPG.Core.Entities.Runs;
using AtlasRPG.Core.Entities.Items;
using AtlasRPG.Core.Entities.Identity;
using AtlasRPG.Core.Entities.GameData;
using AtlasRPG.Core.Enums;
using AtlasRPG.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AtlasRPG.Application.Services
{
    public class RunService
    {
        private readonly ApplicationDbContext _context;
        private readonly ItemGenerationService _itemGenerator;

        public RunService(
            ApplicationDbContext context,
            ItemGenerationService itemGenerator)
        {
            _context = context;
            _itemGenerator = itemGenerator;
        }

        public async Task<Run> CreateNewRun(string userId, RaceType race, ClassType classType)
        {
            // ⭐ ÖNCE KULLANICI VARLIĞINI KONTROL ET
            var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
            if (!userExists)
            {
                throw new InvalidOperationException($"User with ID '{userId}' not found in database.");
            }

            // 1. Check if user has active run
            var activeRun = await _context.Runs
                .FirstOrDefaultAsync(r => r.UserId == userId && r.IsActive);

            if (activeRun != null)
            {
                throw new Exception("You already have an active run. Complete or abandon it first.");
            }

            // 2. Create new run
            var run = new Run
            {
                UserId = userId,
                Race = race,
                Class = classType,
                RunHash = Guid.NewGuid().ToString("N"),
                CurrentTurn = 1,
                CurrentLevel = 1,
                RemainingLives = 3,
                Gold = 50,
                PvpPoints = 0,
                IsActive = true,
                HasWoundDebuff = false,
                AvailableStatPoints = 0,
                AvailableSkillPoints = 0,
                // Starting stats: 5 each
                Strength = 5,
                Dexterity = 5,
                Agility = 5,
                Intelligence = 5,
                Vitality = 5,
                Wisdom = 5,
                Luck = 5
            };

            _context.Runs.Add(run);
            await _context.SaveChangesAsync();

            // 3. Generate starter equipment
            await GenerateStarterEquipment(run);

            // 4. Create Turn 1 (PVE)
            await CreateTurn(run, 1, isPvp: false);

            await _context.SaveChangesAsync();

            return run;
        }

        private async Task GenerateStarterEquipment(Run run)
        {
            // Generate 4 starter items (Normal rarity, ilvl 1)
            var weapon = await _itemGenerator.GenerateItem(ItemSlot.Weapon, 1, ItemRarity.Normal, run.Class);
            var armor = await _itemGenerator.GenerateItem(ItemSlot.Armor, 1, ItemRarity.Normal, run.Class);
            var belt = await _itemGenerator.GenerateItem(ItemSlot.Belt, 1, ItemRarity.Normal, run.Class);

            // Add to inventory
            var weaponRunItem = new RunItem
            {
                RunId = run.Id,
                ItemId = weapon.Id,
                AcquiredAtTurn = 0, // Starter gear
                IsEquipped = true
            };

            var armorRunItem = new RunItem
            {
                RunId = run.Id,
                ItemId = armor.Id,
                AcquiredAtTurn = 0,
                IsEquipped = true
            };

            var beltRunItem = new RunItem
            {
                RunId = run.Id,
                ItemId = belt.Id,
                AcquiredAtTurn = 0,
                IsEquipped = true
            };

            _context.RunItems.AddRange(weaponRunItem, armorRunItem, beltRunItem);

            // Create equipment setup
            var equipment = new RunEquipment
            {
                RunId = run.Id,
                WeaponId = weaponRunItem.Id,
                ArmorId = armorRunItem.Id,
                BeltId = beltRunItem.Id
            };

            _context.RunEquipments.Add(equipment);
        }

        private async Task CreateTurn(Run run, int turnNumber, bool isPvp)
        {
            var turn = new RunTurn
            {
                RunId = run.Id,
                TurnNumber = turnNumber,
                IsPvp = isPvp,
                IsCompleted = false
            };

            _context.RunTurns.Add(turn);
        }

        public async Task<RunTurn?> GetCurrentTurn(Guid runId)
        {
            return await _context.RunTurns
                .Where(t => t.RunId == runId && !t.IsCompleted)
                .OrderBy(t => t.TurnNumber)
                .FirstOrDefaultAsync();
        }

        // RunService.cs — CompleteTurn metodunu güncelle

        public async Task<bool> CompleteTurn(Guid turnId, bool isVictory)
        {
            var turn = await _context.RunTurns
                .Include(t => t.Run)
                .FirstOrDefaultAsync(t => t.Id == turnId);

            if (turn == null || turn.IsCompleted)
                return false;

            var run = turn.Run;

            turn.IsCompleted = true;
            turn.IsVictory = isVictory;
            turn.CompletedAt = DateTime.UtcNow;

            if (isVictory)
            {
                // ✅ Her kazanışta wound temizlenir
                run.HasWoundDebuff = false;

                // Gold ödülü
                int goldEarned = CalculateGoldReward(turn.TurnNumber, turn.IsPvp, isVictory: true);
                run.Gold += goldEarned;
                turn.GoldEarned = goldEarned;

                // PVP puanı
                if (turn.IsPvp)
                {
                    run.PvpPoints++;
                    turn.PvpPointsEarned = 1;
                }
            }
            else // Yenilgi
            {
                run.RemainingLives--;

                if (turn.IsPvp)
                {
                    // ✅ PVP yenilgisi: wound debuff + az gold
                    run.HasWoundDebuff = true;
                    int lostGold = CalculateGoldReward(turn.TurnNumber, isPvp: true, isVictory: false);
                    run.Gold += lostGold; // Yenilse de biraz gold alır
                    turn.GoldEarned = lostGold;
                }
                else
                {
                    // ✅ PVE yenilgisi: wound ETKİLENMEZ, gold YOK
                    // (HasWoundDebuff değişmez — zaten design gereği)
                    turn.GoldEarned = 0;
                }
            }

            // ✅ Her turn sonunda level atla (kazansın kaybetsin)
            run.CurrentLevel++;
            run.AvailableStatPoints += 2;
            run.AvailableSkillPoints += 1;

            // Run bitiş kontrolü
            if (run.RemainingLives <= 0)
            {
                run.IsActive = false;
                run.CompletedAt = DateTime.UtcNow;
                // ❌ Erken return — sonraki turn oluşturma
                await _context.SaveChangesAsync();
                return true;
            }

            if (run.CurrentTurn >= 20)
            {
                run.IsActive = false;
                run.CompletedAt = DateTime.UtcNow;
            }
            else
            {
                run.CurrentTurn++;
                bool nextIsPvp = DetermineTurnType(run.CurrentTurn);
                await CreateTurn(run, run.CurrentTurn, nextIsPvp);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        private int CalculateGoldReward(int turnNumber, bool isPvp, bool isVictory)
        {
            if (isPvp)
            {
                return isVictory ? (10 + 4 * turnNumber) : (4 + 2 * turnNumber);
            }
            else
            {
                return isVictory ? (12 + 3 * turnNumber) : 0;
            }
        }

        private bool DetermineTurnType(int turnNumber)
        {
            // Turn 1-3: PVE
            if (turnNumber >= 1 && turnNumber <= 3)
                return false;

            // Turn 4-8: PVP
            if (turnNumber >= 4 && turnNumber <= 8)
                return true;

            // Turn 9: PVE
            if (turnNumber == 9)
                return false;

            // Turn 10-14: PVP
            if (turnNumber >= 10 && turnNumber <= 14)
                return true;

            // Turn 15: PVE
            if (turnNumber == 15)
                return false;

            // Turn 16-20: PVP
            return true;
        }
    }
}