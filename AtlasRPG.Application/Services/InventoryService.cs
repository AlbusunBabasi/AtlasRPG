// AtlasRPG.Application/Services/InventoryService.cs
using AtlasRPG.Core.Entities.Runs;
using AtlasRPG.Core.Enums;
using AtlasRPG.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AtlasRPG.Application.Services
{
    public class InventoryService
    {
        private readonly ApplicationDbContext _context;
        private readonly StatCalculatorService _statCalculator;

        public InventoryService(
            ApplicationDbContext context,
            StatCalculatorService statCalculator)   // ← DI ile inject
        {
            _context = context;
            _statCalculator = statCalculator;
        }

        public async Task<bool> EquipItem(Guid runId, Guid runItemId)
        {
            var runItem = await _context.RunItems
                .Include(ri => ri.Item)
                .Include(ri => ri.Run)
                .FirstOrDefaultAsync(ri => ri.Id == runItemId && ri.RunId == runId);

            if (runItem == null || runItem.IsEquipped)
                return false;

            var equipment = await _context.RunEquipments
                .FirstOrDefaultAsync(e => e.RunId == runId);

            if (equipment == null)
            {
                equipment = new RunEquipment { RunId = runId };
                _context.RunEquipments.Add(equipment);
                await _context.SaveChangesAsync();
            }

            await UnequipSlot(runId, runItem.Item.Slot);

            bool equippingOffhand = runItem.Item.Slot == ItemSlot.Offhand;
            bool equippingWeapon = runItem.Item.Slot == ItemSlot.Weapon;

            if (equippingOffhand)
            {
                // Mevcut weapon 2H mı?
                var currentWeapon = equipment.Weapon?.Item;
                bool weaponIs2H = currentWeapon?.WeaponType is WeaponType.TwoHandSword
                               or WeaponType.Staff;
                if (weaponIs2H)
                    return false;  // 2H ile offhand takamazsın

                // Offhand, quiver ise bow gereklidir
                bool offhandIsQuiver = runItem.Item.OffhandType == OffhandType.Quiver;
                bool weaponIsBow = currentWeapon?.WeaponType == WeaponType.Bow;
                if (offhandIsQuiver && !weaponIsBow)
                    return false;  // Quiver sadece bow ile
            }

            if (equippingWeapon)
            {
                bool newWeaponIs2H = runItem.Item.WeaponType is WeaponType.TwoHandSword
                                  or WeaponType.Staff;
                if (newWeaponIs2H)
                {
                    // 2H takılınca mevcut offhand'ı unequip et
                    await UnequipSlot(runId, ItemSlot.Offhand);
                }
            }

            switch (runItem.Item.Slot)
            {
                case ItemSlot.Weapon: equipment.WeaponId = runItem.Id; break;
                case ItemSlot.Offhand: equipment.OffhandId = runItem.Id; break;
                case ItemSlot.Armor: equipment.ArmorId = runItem.Id; break;
                case ItemSlot.Belt: equipment.BeltId = runItem.Id; break;
            }

            runItem.IsEquipped = true;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UnequipItem(Guid runId, Guid runItemId)
        {
            var runItem = await _context.RunItems
                .Include(ri => ri.Item)
                .FirstOrDefaultAsync(ri => ri.Id == runItemId && ri.RunId == runId);

            if (runItem == null || !runItem.IsEquipped)
                return false;

            var equipment = await _context.RunEquipments
                .FirstOrDefaultAsync(e => e.RunId == runId);

            if (equipment == null)
                return false;

            switch (runItem.Item.Slot)
            {
                case ItemSlot.Weapon: equipment.WeaponId = null; break;
                case ItemSlot.Offhand: equipment.OffhandId = null; break;
                case ItemSlot.Armor: equipment.ArmorId = null; break;
                case ItemSlot.Belt: equipment.BeltId = null; break;
            }

            runItem.IsEquipped = false;
            await _context.SaveChangesAsync();
            return true;
        }

        private async Task UnequipSlot(Guid runId, ItemSlot slot)
        {
            var equipment = await _context.RunEquipments
                .FirstOrDefaultAsync(e => e.RunId == runId);

            if (equipment == null) return;

            Guid? currentItemId = slot switch
            {
                ItemSlot.Weapon => equipment.WeaponId,
                ItemSlot.Offhand => equipment.OffhandId,
                ItemSlot.Armor => equipment.ArmorId,
                ItemSlot.Belt => equipment.BeltId,
                _ => null
            };

            if (!currentItemId.HasValue) return;

            var currentRunItem = await _context.RunItems
                .FirstOrDefaultAsync(ri => ri.Id == currentItemId.Value);

            if (currentRunItem != null)
                currentRunItem.IsEquipped = false;

            switch (slot)
            {
                case ItemSlot.Weapon: equipment.WeaponId = null; break;
                case ItemSlot.Offhand: equipment.OffhandId = null; break;
                case ItemSlot.Armor: equipment.ArmorId = null; break;
                case ItemSlot.Belt: equipment.BeltId = null; break;
            }
        }

        public async Task<Dictionary<string, object>> GetStatPreview(Guid runId, Guid runItemId)
        {
            // Run'ı tüm equipment + affixlerle yükle
            var run = await _context.Runs
                .Include(r => r.Equipment)
                    .ThenInclude(e => e.Weapon)
                    .ThenInclude(ri => ri.Item)
                    .ThenInclude(i => i.Affixes)
                    .ThenInclude(a => a.AffixDefinition)
                .Include(r => r.Equipment)
                    .ThenInclude(e => e.Offhand)
                    .ThenInclude(ri => ri.Item)
                    .ThenInclude(i => i.Affixes)
                    .ThenInclude(a => a.AffixDefinition)
                .Include(r => r.Equipment)
                    .ThenInclude(e => e.Armor)
                    .ThenInclude(ri => ri.Item)
                    .ThenInclude(i => i.Affixes)
                    .ThenInclude(a => a.AffixDefinition)
                .Include(r => r.Equipment)
                    .ThenInclude(e => e.Belt)
                    .ThenInclude(ri => ri.Item)
                    .ThenInclude(i => i.Affixes)
                    .ThenInclude(a => a.AffixDefinition)
                    .AsSplitQuery()
                .FirstOrDefaultAsync(r => r.Id == runId);

            var newRunItem = await _context.RunItems
                .Include(ri => ri.Item)
                    .ThenInclude(i => i.Affixes)
                    .ThenInclude(a => a.AffixDefinition)
                .FirstOrDefaultAsync(ri => ri.Id == runItemId);

            if (run == null || newRunItem == null)
                return new Dictionary<string, object>();

            // Race'e göre BaseStatDefinition yükle
            var baseStat = await _context.BaseStatDefinitions
                .FirstOrDefaultAsync(b => b.Race == run.Race);

            if (baseStat == null)
                return new Dictionary<string, object>();

            // --- Mevcut stats ---
            var currentStats = _statCalculator.CalculateRunStats(run, baseStat);

            // --- Yeni stats (item geçici olarak swap edilir) ---
            RunItem? oldItem = null;

            switch (newRunItem.Item.Slot)
            {
                case ItemSlot.Weapon:
                    oldItem = run.Equipment?.Weapon;
                    if (run.Equipment != null) run.Equipment.Weapon = newRunItem;
                    break;
                case ItemSlot.Offhand:
                    oldItem = run.Equipment?.Offhand;
                    if (run.Equipment != null) run.Equipment.Offhand = newRunItem;
                    break;
                case ItemSlot.Armor:
                    oldItem = run.Equipment?.Armor;
                    if (run.Equipment != null) run.Equipment.Armor = newRunItem;
                    break;
                case ItemSlot.Belt:
                    oldItem = run.Equipment?.Belt;
                    if (run.Equipment != null) run.Equipment.Belt = newRunItem;
                    break;
            }

            var newStats = _statCalculator.CalculateRunStats(run, baseStat);

            // Eski item'ı geri koy (in-memory swap, DB'ye yazılmıyor)
            switch (newRunItem.Item.Slot)
            {
                case ItemSlot.Weapon:
                    if (run.Equipment != null) run.Equipment.Weapon = oldItem;
                    break;
                case ItemSlot.Offhand:
                    if (run.Equipment != null) run.Equipment.Offhand = oldItem;
                    break;
                case ItemSlot.Armor:
                    if (run.Equipment != null) run.Equipment.Armor = oldItem;
                    break;
                case ItemSlot.Belt:
                    if (run.Equipment != null) run.Equipment.Belt = oldItem;
                    break;
            }

            return new Dictionary<string, object>
            {
                ["currentMaxHp"] = currentStats.MaxHp,
                ["newMaxHp"] = newStats.MaxHp,
                ["currentMaxMana"] = currentStats.MaxMana,
                ["newMaxMana"] = newStats.MaxMana,
                ["currentBaseDamage"] = currentStats.BaseDamage,
                ["newBaseDamage"] = newStats.BaseDamage,
                ["currentArmor"] = currentStats.Armor,
                ["newArmor"] = newStats.Armor,
                ["currentEvasion"] = currentStats.Evasion,
                ["newEvasion"] = newStats.Evasion,
                ["currentCritChance"] = currentStats.CritChance,
                ["newCritChance"] = newStats.CritChance
            };
        }
    }
}
