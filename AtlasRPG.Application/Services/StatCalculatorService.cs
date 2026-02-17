// AtlasRPG.Application/Services/StatCalculatorService.cs
using AtlasRPG.Core.Entities.Runs;
using AtlasRPG.Core.Entities.Items;
using AtlasRPG.Core.Entities.GameData;
using AtlasRPG.Core.ValueObjects;
using AtlasRPG.Core.Enums;

namespace AtlasRPG.Application.Services
{
    public class StatCalculatorService
    {
        // ───────────────────────────────────────────────
        // CONVENIENCE: Run + BaseStatDefinition verilince
        // equipment ve affixleri otomatik toplar
        // ───────────────────────────────────────────────
        public CharacterStats CalculateRunStats(
            Run run,
            BaseStatDefinition baseStat,
            IEnumerable<PassiveNodeDefinition>? allocatedNodes = null)
        {
            var equipment = run.Equipment;

            var allAffixes = new List<ItemAffix>();
            if (equipment?.Weapon?.Item?.Affixes != null)
                allAffixes.AddRange(equipment.Weapon.Item.Affixes);
            if (equipment?.Offhand?.Item?.Affixes != null)
                allAffixes.AddRange(equipment.Offhand.Item.Affixes);
            if (equipment?.Armor?.Item?.Affixes != null)
                allAffixes.AddRange(equipment.Armor.Item.Affixes);
            if (equipment?.Belt?.Item?.Affixes != null)
                allAffixes.AddRange(equipment.Belt.Item.Affixes);

            // Equipped weapon type string (passive silah bonus'ları için)
            string? weaponType = equipment?.Weapon?.Item?.WeaponType?.ToString();

            var passiveBonuses = allocatedNodes != null
                ? PassiveNodeApplier.Build(allocatedNodes)
                : new PassiveBonuses();

            return CalculateFinalStats(run, baseStat, equipment, allAffixes, passiveBonuses, weaponType);
        }

        // ───────────────────────────────────────────────
        // CORE: Tüm parametreler dışarıdan verilir
        // ───────────────────────────────────────────────
        public CharacterStats CalculateFinalStats(
            Run run,
            BaseStatDefinition baseStat,
            RunEquipment? equipment,
            List<ItemAffix> allAffixes,
            PassiveBonuses? passiveBonuses = null,
            string? equippedWeaponType = null)
        {
            var stats = new CharacterStats();


            // 1. Primary Stats
            decimal woundMultiplier = (run.HasWoundDebuff && run.Race != RaceType.Undead)
                ? 0.97m
                : 1.0m;  // ✅ Undead wound'ı ignore eder

            stats.Strength = (int)(run.Strength * woundMultiplier);
            stats.Dexterity = (int)(run.Dexterity * woundMultiplier);
            stats.Agility = (int)(run.Agility * woundMultiplier);
            stats.Intelligence = (int)(run.Intelligence * woundMultiplier);
            stats.Vitality = (int)(run.Vitality * woundMultiplier);
            stats.Wisdom = (int)(run.Wisdom * woundMultiplier);
            stats.Luck = (int)(run.Luck * woundMultiplier);

            // 2. MaxHP / MaxMana
            stats.MaxHp = baseStat.BaseHp + (stats.Strength * 2) + (stats.Vitality * 4);
            stats.MaxMana = baseStat.BaseMana + (stats.Intelligence * 2) + (stats.Wisdom * 4);

            // 3. Multiplier affixlerini önce uygula (IncreasedDamage, IncreasedArmor vs.)
            ApplyAffixMultipliers(stats, allAffixes);

            // 4. MaxHP % affixleri multiplier SONRASI uygula
            decimal hpPctBonus = GetAffixSum(allAffixes, "MaxHPPct");
            if (hpPctBonus > 0)
                stats.MaxHp = (int)(stats.MaxHp * (1 + hpPctBonus));

            decimal manaPctBonus = GetAffixSum(allAffixes, "MaxManaPct");
            if (manaPctBonus > 0)
                stats.MaxMana = (int)(stats.MaxMana * (1 + manaPctBonus));

            // 5. Weapon base stats
            decimal weaponBaseDamage = GetWeaponBaseDamage(equipment);
            decimal weaponAttackSpeed = GetWeaponAttackSpeed(equipment);

            stats.BaseDamage = weaponBaseDamage;

            // 6. Melee / Ranged / Spell Damage
            stats.MeleeDamage = (weaponBaseDamage + stats.Strength * 2) * stats.IncreasedDamage;
            stats.RangedDamage = (weaponBaseDamage + stats.Dexterity * 2) * stats.IncreasedDamage;
            stats.SpellDamage = (weaponBaseDamage + stats.Intelligence * 2) * stats.IncreasedDamage;

            // 7. Defense
            decimal baseArmor = GetItemBaseValue(equipment?.Armor?.Item, i => i.BaseArmor);
            decimal baseEvasion = GetItemBaseValue(equipment?.Armor?.Item, i => i.BaseEvasion);
            decimal baseWard = GetItemBaseValue(equipment?.Offhand?.Item, i => i.BaseWard)
                                + GetItemBaseValue(equipment?.Armor?.Item, i => i.BaseWard);

            decimal armorAffixBonus = GetAffixSumForSlots(allAffixes, "ArmorPct", equipment, ItemSlot.Armor);
            decimal evasionAffixBonus = GetAffixSumForSlots(allAffixes, "EvasionPct", equipment, ItemSlot.Armor);
            decimal wardAffixBonus = GetAffixSum(allAffixes, "WardPct");

            stats.Armor = (baseArmor + Math.Max(0, stats.Vitality - 5) * 2) * (1 + armorAffixBonus) * stats.IncreasedArmor;
            stats.Evasion = (baseEvasion + Math.Max(0, stats.Agility - 5) * 2) * (1 + evasionAffixBonus) * stats.IncreasedEvasion;
            stats.Ward = (baseWard + Math.Max(0, stats.Wisdom - 5) * 2) * (1 + wardAffixBonus) * stats.IncreasedWard;

            // 8. Accuracy / Hit
            decimal gearAccPct = GetAffixSum(allAffixes, "AccuracyPct");
            stats.Accuracy = (100m + stats.Dexterity * 2) * (1 + gearAccPct) * stats.IncreasedAccuracy;

            // 9. Crit
            decimal gearCritChance = GetAffixSum(allAffixes, "CritChanceFlat");
            stats.CritChance = Math.Min(0.40m,
                (0.05m + stats.Dexterity * 0.002m + gearCritChance) * stats.IncreasedCritChance);

            decimal gearCritMulti = GetAffixSum(allAffixes, "CritMultiFlat");
            stats.CritMultiplier = Math.Min(2.50m, 1.5m + gearCritMulti);

            // 10. Block
            decimal shieldBaseBlock = GetItemBaseValue(equipment?.Offhand?.Item, i => i.BaseBlockChance);
            decimal gearBlockFlat = GetAffixSum(allAffixes, "BlockChanceFlat");

            stats.BlockChance = Math.Min(0.40m,
                (shieldBaseBlock + stats.Vitality * 0.002m + gearBlockFlat) * stats.IncreasedBlockChance);

            stats.BlockReduction = Math.Min(0.60m, 0.35m);

            // 11. Resistances (base race + affix + ResistAll)
            decimal resistAllBonus = GetAffixSum(allAffixes, "ResistAll");

            stats.FireResist = Math.Clamp(
                baseStat.BaseFireResist
                + GetAffixSum(allAffixes, "ResistFire")
                + resistAllBonus, -0.50m, 0.75m);

            stats.ColdResist = Math.Clamp(
                baseStat.BaseColdResist
                + GetAffixSum(allAffixes, "ResistCold")
                + resistAllBonus, -0.50m, 0.75m);

            stats.LightningResist = Math.Clamp(
                baseStat.BaseLightningResist
                + GetAffixSum(allAffixes, "ResistLightning")
                + resistAllBonus, -0.50m, 0.75m);

            stats.ChaosResist = Math.Clamp(
                baseStat.BaseChaosResist
                + GetAffixSum(allAffixes, "ResistChaos")
                + resistAllBonus, -0.50m, 0.75m);

            // 12. Initiative
            decimal gearInitiative = GetAffixSum(allAffixes, "InitiativeFlat");
            decimal attackSpeedPct = GetAffixSum(allAffixes, "AttackSpeedPct");
            stats.Initiative = weaponAttackSpeed * (1 + attackSpeedPct)
                               + stats.Agility * 2
                               + gearInitiative;

            // 13. ArmorPenetration
            stats.ArmorPenetration = GetAffixSum(allAffixes, "ArmorPenPct");


            var pb = passiveBonuses ?? new PassiveBonuses();

            // 15a. MaxHP & MaxMana percent bonus
            if (pb.MaxHpPercent != 0m)
                stats.MaxHp *= (1 + pb.MaxHpPercent);
            if (pb.MaxManaPercent != 0m)
                stats.MaxMana *= (1 + pb.MaxManaPercent);

            // 15b. Initiative flat bonus
            stats.Initiative += pb.InitiativeFlat;

            // 15c. IncreasedDamage: global + weapon-specific
            decimal weaponDmgBonus = equippedWeaponType != null
                ? pb.GetWeaponDamageBonus(equippedWeaponType)
                : 0m;
            stats.IncreasedDamage += pb.IncreasedDamageGlobal + weaponDmgBonus;

            // 15d. IncreasedAccuracy global
            stats.IncreasedAccuracy += pb.IncreasedAccuracyGlobal;
            // Accuracy'yi yeniden hesapla (IncreasedAccuracy değişti)
            {
                decimal gearAccPct2 = GetAffixSum(allAffixes, "AccuracyPct");
                stats.Accuracy = (100m + stats.Dexterity * 2)
                                 * (1 + gearAccPct2)
                                 * stats.IncreasedAccuracy
                                 * pb.AccuracyMult;   // AccuracyMult (N07 Clean Hit)
            }

            // 15e. CritChance (additive, cap %40 yeniden uygula)
            if (pb.CritChanceFlat != 0m)
            {
                decimal gearCritChance2 = GetAffixSum(allAffixes, "CritChanceFlat");
                stats.CritChance = Math.Min(0.40m,
                    (0.05m + stats.Dexterity * 0.002m + gearCritChance2 + pb.CritChanceFlat)
                    * stats.IncreasedCritChance);
            }

            // 15f. CritMultiplier bonus
            if (pb.CritMultiBonus != 0m)
                stats.CritMultiplier = Math.Min(2.50m, stats.CritMultiplier + pb.CritMultiBonus);

            // 15g. Armor / Evasion / Ward multiplier bonus
            stats.IncreasedArmor += pb.IncreasedArmor;
            stats.IncreasedEvasion += pb.IncreasedEvasion;
            stats.IncreasedWard += pb.IncreasedWard;

            // Armor / Evasion / Ward'u yeniden hesapla
            {
                decimal baseArm2 = GetItemBaseValue(equipment?.Armor?.Item, i => i.BaseArmor);
                decimal gearArm2 = GetAffixSumForSlots(allAffixes, "ArmorPct", equipment, ItemSlot.Armor);
                stats.Armor = baseArm2 * (1 + gearArm2) * stats.IncreasedArmor;

                decimal baseEv2 = GetItemBaseValue(equipment?.Armor?.Item, i => i.BaseEvasion);
                decimal gearEv2 = GetAffixSumForSlots(allAffixes, "EvasionPct", equipment, ItemSlot.Armor);
                stats.Evasion = baseEv2 * (1 + gearEv2) * stats.IncreasedEvasion;

                decimal baseWard2 = GetItemBaseValue(equipment?.Offhand?.Item, i => i.BaseWard);
                decimal gearWard2 = GetAffixSumForSlots(allAffixes, "WardPct", equipment, ItemSlot.Belt);
                stats.Ward = (stats.Wisdom * 2 + baseWard2) * (1 + gearWard2) * stats.IncreasedWard;
            }

            // 15h. BlockChance
            if (pb.IncreasedBlockChance != 0m)
            {
                decimal shieldBaseBlock2 = GetItemBaseValue(equipment?.Offhand?.Item, i => i.BaseBlockChance);
                decimal gearBlockFlat2 = GetAffixSum(allAffixes, "BlockChanceFlat");
                decimal blockCap = pb.IncreasedBlockChance > 0 ? 0.45m : 0.40m; // Duelist keystone cap
                stats.BlockChance = Math.Min(blockCap,
                    (shieldBaseBlock2 + stats.Vitality * 0.002m + gearBlockFlat2)
                    * (stats.IncreasedBlockChance + pb.IncreasedBlockChance));
            }

            // 15i. ArmorPenetration
            stats.ArmorPenetration += pb.ArmorPenetration;

            // ════════════════════════════════════════════════════════
            // RACIAL ABILITIES
            // ════════════════════════════════════════════════════════
            // ════════════════════════════════════════════════════════
            // RACIAL ABILITIES
            // ════════════════════════════════════════════════════════
            switch (run.Race)
            {
                case RaceType.Human:
                    // Adaptive Growth: her tur +1 stat point → RunService'te uygulanır
                    // Trader's Instinct: shop fiyatı %5 indirim → ShopService'te uygulanır
                    break;

                case RaceType.Dwarf:
                    // Stonehide: +5% Armor
                    stats.Armor *= 1.05m;
                    // Steadfast: Debuff süresi -1 round
                    pb.DebuffDurationReduction = Math.Max(pb.DebuffDurationReduction, 1);
                    break;

                case RaceType.Orc:
                    // Savage Sustain: %1 Lifesteal
                    pb.LifeSteal += 0.01m;
                    // Bloodlust: PVP galibiyetinden sonra o tur +%15 hasar
                    if (run.LastTurnWasVictory && run.LastTurnWasPvp)
                        pb.IncreasedDamageGlobal += 0.15m;
                    break;

                case RaceType.Undead:
                    // Void-Touched: +%15 Chaos Resist
                    stats.ChaosResist = Math.Clamp(stats.ChaosResist + 0.15m, -0.50m, 0.75m);
                    // Unfeeling: Wound işlemez → başta woundMultiplier zaten 1.0m yapıldı
                    break;

                case RaceType.Drakoid:
                    // Scaled Hide: +%5 All Resist
                    stats.FireResist = Math.Clamp(stats.FireResist + 0.05m, -0.50m, 0.75m);
                    stats.ColdResist = Math.Clamp(stats.ColdResist + 0.05m, -0.50m, 0.75m);
                    stats.LightningResist = Math.Clamp(stats.LightningResist + 0.05m, -0.50m, 0.75m);
                    stats.ChaosResist = Math.Clamp(stats.ChaosResist + 0.05m, -0.50m, 0.75m);
                    // Draconic Core: combat-time'da kullanılır
                    pb.DraconicCoreActive = true;
                    break;
            }

            // 15j. Passive combat bonuses'ı sakla (CombatService okuyacak)
            stats.PassiveBonuses = pb;

            // 16. Current HP/Mana = Max (turn başı) — eski adım 14 artık 16
            stats.CurrentHp = stats.MaxHp;
            stats.CurrentMana = stats.MaxMana;

            // 17. Flat Elemental Damage (sadece weapon affixlerinden)
            stats.FlatFireDamage = GetAffixSum(allAffixes, "FlatFireDamage");
            stats.FlatColdDamage = GetAffixSum(allAffixes, "FlatColdDamage");
            stats.FlatLightningDamage = GetAffixSum(allAffixes, "FlatLightningDamage");
            stats.FlatChaosDamage = GetAffixSum(allAffixes, "FlatChaosDamage");

            return stats;
        }

        // ─────────────────────────────────────────
        //  PRIVATE HELPERS
        // ─────────────────────────────────────────

        private void ApplyAffixMultipliers(CharacterStats stats, List<ItemAffix> affixes)
        {
            foreach (var affix in affixes)
            {
                switch (affix.AffixDefinition.AffixKey)
                {
                    case "DamagePct":
                        stats.IncreasedDamage += affix.RolledValue;
                        break;
                    case "ArmorPct":
                        // Slot bazlı uygulanıyor, burada IncreasedArmor multiplier değil
                        stats.IncreasedArmor += affix.RolledValue;
                        break;
                    case "EvasionPct":
                        stats.IncreasedEvasion += affix.RolledValue;
                        break;
                    case "WardPct":
                        stats.IncreasedWard += affix.RolledValue;
                        break;
                    case "AttackSpeedPct":
                        // Initiative hesabında kullanılıyor
                        break;
                        // MaxHPPct ve MaxManaPct ayrı hesaplanıyor (adım 4)
                }
            }
        }

        private decimal GetWeaponBaseDamage(RunEquipment? equipment)
        {
            if (equipment?.Weapon?.Item == null) return 5m; // bare-hand fallback
            return equipment.Weapon.Item.BaseDamage;
        }

        private decimal GetWeaponAttackSpeed(RunEquipment? equipment)
        {
            if (equipment?.Weapon?.Item == null) return 5m;
            return equipment.Weapon.Item.BaseAttackSpeed;
        }

        // Tek bir item'dan belirtilen property'yi güvenle al
        private decimal GetItemBaseValue(
            AtlasRPG.Core.Entities.Items.Item? item,
            Func<AtlasRPG.Core.Entities.Items.Item, decimal> selector)
        {
            if (item == null) return 0m;
            return selector(item);
        }

        // Sadece belirli slot'tan gelen affix toplamını al (ArmorPct / EvasionPct için)
        private decimal GetAffixSumForSlots(
            List<ItemAffix> affixes,
            string affixKey,
            RunEquipment? equipment,
            ItemSlot slot)
        {
            // Tüm affixlerden sadece bu key'e ait olanlar zaten
            // slot-allowlist ile generate edildiği için direkt sum yeterli
            return GetAffixSum(affixes, affixKey);
        }

        private decimal GetAffixSum(List<ItemAffix> affixes, string affixKey)
        {
            return affixes
                .Where(a => a.AffixDefinition?.AffixKey == affixKey)
                .Sum(a => a.RolledValue);
        }
    }
}
