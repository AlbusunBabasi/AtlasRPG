// AtlasRPG.Infrastructure/Data/DbInitializer.cs
using AtlasRPG.Core.Entities.GameData;
using AtlasRPG.Core.Entities.Items;
using AtlasRPG.Core.Enums;

namespace AtlasRPG.Infrastructure.Data
{
    public static class DbInitializer
    {
        public static void SeedGameData(ApplicationDbContext context)
        {
            // 1. Base Stats
            SeedBaseStats(context);

            // 2. Affix Definitions
            SeedAffixDefinitions(context);

            // 3. Skills (birkaç örnek)
            SeedSkills(context);

            // 4. Passive Nodes (birkaç örnek)
            SeedPassiveNodes(context);

            context.SaveChanges();
        }

        private static void SeedBaseStats(ApplicationDbContext context)
        {
            if (context.BaseStatDefinitions.Any()) return;

            var baseStats = new List<BaseStatDefinition>
            {
                // Human
                new BaseStatDefinition
                {
                    Race = RaceType.Human,
                    Class = null,
                    BaseHp = 100,
                    BaseMana = 50,
                    Notes = "Human base (all classes)"
                },
                // Dwarf
                new BaseStatDefinition
                {
                    Race = RaceType.Dwarf,
                    Class = null,
                    BaseHp = 120,
                    BaseMana = 40,
                    Notes = "Dwarf base - tankier"
                },
                // Orc
                new BaseStatDefinition
                {
                    Race = RaceType.Orc,
                    Class = null,
                    BaseHp = 110,
                    BaseMana = 40,
                    Notes = "Orc base - aggressive"
                },
                // Undead
                new BaseStatDefinition
                {
                    Race = RaceType.Undead,
                    Class = null,
                    BaseHp = 90,
                    BaseMana = 60,
                    BaseChaosResist = 0.15m,
                    Notes = "Undead base - chaos resist"
                },
                // Drakoid
                new BaseStatDefinition
                {
                    Race = RaceType.Drakoid,
                    Class = null,
                    BaseHp = 100,
                    BaseMana = 55,
                    BaseFireResist = 0.05m,
                    BaseColdResist = 0.05m,
                    BaseLightningResist = 0.05m,
                    Notes = "Drakoid base - all resist"
                }
            };

            context.BaseStatDefinitions.AddRange(baseStats);
        }

        private static void SeedAffixDefinitions(ApplicationDbContext context)
        {
            if (context.AffixDefinitions.Any()) return;

            var affixes = new List<AffixDefinition>
            {
                // Damage %
                new AffixDefinition
                {
                    AffixKey = "DamagePct",
                    DisplayName = "% Damage",
                    AllowedSlots = "Weapon",
                    Tier1Min = 0.04m, Tier1Max = 0.07m,
                    Tier2Min = 0.08m, Tier2Max = 0.12m,
                    Tier3Min = 0.13m, Tier3Max = 0.18m,
                    IsPercentage = true
                },
                // Attack Speed %
                new AffixDefinition
                {
                    AffixKey = "AttackSpeedPct",
                    DisplayName = "% Attack Speed",
                    AllowedSlots = "Weapon,Offhand",
                    Tier1Min = 0.03m, Tier1Max = 0.05m,
                    Tier2Min = 0.06m, Tier2Max = 0.09m,
                    Tier3Min = 0.10m, Tier3Max = 0.14m,
                    IsPercentage = true
                },
                // Crit Chance
                new AffixDefinition
                {
                    AffixKey = "CritChanceFlat",
                    DisplayName = "% Critical Strike Chance",
                    AllowedSlots = "Weapon",
                    Tier1Min = 0.01m, Tier1Max = 0.02m,
                    Tier2Min = 0.03m, Tier2Max = 0.04m,
                    Tier3Min = 0.05m, Tier3Max = 0.06m,
                    IsPercentage = true
                },
                // Max HP %
                new AffixDefinition
                {
                    AffixKey = "MaxHPPct",
                    DisplayName = "% Maximum Life",
                    AllowedSlots = "Armor,Offhand,Belt",
                    Tier1Min = 0.03m, Tier1Max = 0.05m,
                    Tier2Min = 0.06m, Tier2Max = 0.09m,
                    Tier3Min = 0.10m, Tier3Max = 0.14m,
                    IsPercentage = true
                },
                // Armor %
                new AffixDefinition
                {
                    AffixKey = "ArmorPct",
                    DisplayName = "% Armor",
                    AllowedSlots = "Offhand,Armor",
                    Tier1Min = 0.05m, Tier1Max = 0.08m,
                    Tier2Min = 0.09m, Tier2Max = 0.13m,
                    Tier3Min = 0.14m, Tier3Max = 0.20m,
                    IsPercentage = true
                },
                // Fire Resist
                new AffixDefinition
                {
                    AffixKey = "ResistFire",
                    DisplayName = "% Fire Resistance",
                    AllowedSlots = "Armor,Belt,Offhand",
                    Tier1Min = 0.10m, Tier1Max = 0.15m,
                    Tier2Min = 0.16m, Tier2Max = 0.22m,
                    Tier3Min = 0.23m, Tier3Max = 0.30m,
                    IsPercentage = true
                },
                // Cold Resist
                new AffixDefinition
                {
                    AffixKey = "ResistCold",
                    DisplayName = "% Cold Resistance",
                    AllowedSlots = "Armor,Belt,Offhand",
                    Tier1Min = 0.10m, Tier1Max = 0.15m,
                    Tier2Min = 0.16m, Tier2Max = 0.22m,
                    Tier3Min = 0.23m, Tier3Max = 0.30m,
                    IsPercentage = true
                },
                // Flat Fire Damage
                new AffixDefinition
                {
                    AffixKey = "FlatFireDamage",
                    DisplayName = "Fire Damage",
                    AllowedSlots = "Weapon",
                    Tier1Min = 1m, Tier1Max = 2m,
                    Tier2Min = 3m, Tier2Max = 4m,
                    Tier3Min = 5m, Tier3Max = 6m,
                    IsPercentage = false
                },
                
                new AffixDefinition
                {
                    AffixKey = "AccuracyPct",
                    DisplayName = "% Accuracy",
                    AllowedSlots = "Weapon, Offhand",
                    Tier1Min = 0.05m, Tier1Max = 0.08m,
                    Tier2Min = 0.09m, Tier2Max = 0.13m,
                    Tier3Min = 0.14m, Tier3Max = 0.20m,
                    IsPercentage = true
                },

                new AffixDefinition
                {
                    AffixKey = "CritMultiFlat",
                    DisplayName = "% Critical Multiplier",
                    AllowedSlots = "Weapon",
                    Tier1Min = 0.03m, Tier1Max = 0.05m,
                    Tier2Min = 0.06m, Tier2Max = 0.9m,
                    Tier3Min = 0.10m, Tier3Max = 0.14m,
                    IsPercentage = true
                },

                new AffixDefinition
                {
                    AffixKey = "ArmorPenPct",
                    DisplayName = "% Armor Penetration",
                    AllowedSlots = "Weapon",
                    Tier1Min = 0.05m, Tier1Max = 0.08m,
                    Tier2Min = 0.09m, Tier2Max = 0.13m,
                    Tier3Min = 0.14m, Tier3Max = 0.20m,
                    IsPercentage = true
                },

                new AffixDefinition
                {
                    AffixKey = "FlatColdDamage",
                    DisplayName = "Cold Damage",
                    AllowedSlots = "Weapon",
                    Tier1Min = 1m, Tier1Max = 2m,
                    Tier2Min = 3m, Tier2Max = 4m,
                    Tier3Min = 5m, Tier3Max = 6m,
                    IsPercentage = false
                },

                new AffixDefinition
                {
                    AffixKey = "FlatLightningDamage",
                    DisplayName = "Lightning Damage",
                    AllowedSlots = "Weapon",
                    Tier1Min = 1m, Tier1Max = 2m,
                    Tier2Min = 3m, Tier2Max = 4m,
                    Tier3Min = 5m, Tier3Max = 6m,
                    IsPercentage = false
                },

                new AffixDefinition
                {
                    AffixKey = "FlatChaosDamage",
                    DisplayName = "Chaos Damage",
                    AllowedSlots = "Weapon",
                    Tier1Min = 1m, Tier1Max = 1m,
                    Tier2Min = 2m, Tier2Max = 3m,
                    Tier3Min = 4m, Tier3Max = 5m,
                    IsPercentage = false
                },

                new AffixDefinition
                {
                    AffixKey = "BlockChanceFlat",
                    DisplayName = "% Block Chance",
                    AllowedSlots = "Offhand",
                    Tier1Min = 0.03m, Tier1Max = 0.05m,
                    Tier2Min = 0.06m, Tier2Max = 0.08m,
                    Tier3Min = 0.09m, Tier3Max = 0.12m,
                    IsPercentage = true
                },

                new AffixDefinition
                {
                    AffixKey = "WardPct",
                    DisplayName = "% Ward",
                    AllowedSlots = "Offhand, Armor, Belt",
                    Tier1Min = 0.06m, Tier1Max = 0.10m,
                    Tier2Min = 0.11m, Tier2Max = 0.16m,
                    Tier3Min = 0.17m, Tier3Max = 0.24m,
                    IsPercentage = true
                },

                new AffixDefinition
                {
                    AffixKey = "EvasionPct",
                    DisplayName = "% Evasion",
                    AllowedSlots = "Offhand, Armor, Belt",
                    Tier1Min = 0.05m, Tier1Max = 0.08m,
                    Tier2Min = 0.09m, Tier2Max = 0.13m,
                    Tier3Min = 0.14m, Tier3Max = 0.20m,
                    IsPercentage = true
                },

                new AffixDefinition
                {
                    AffixKey = "InitiativeFlat",
                    DisplayName = "Initiative",
                    AllowedSlots = "Offhand, Belt",
                    Tier1Min = 1m, Tier1Max = 2m,
                    Tier2Min = 3m, Tier2Max = 4m,
                    Tier3Min = 5m, Tier3Max = 6m,
                    IsPercentage = false
                },

                new AffixDefinition
                {
                    AffixKey = "MaxManaPct",
                    DisplayName = "% Max Mana",
                    AllowedSlots = "Offhand, Armor, Belt",
                    Tier1Min = 0.06m, Tier1Max = 0.10m,
                    Tier2Min = 0.11m, Tier2Max = 0.16m,
                    Tier3Min = 0.17m, Tier3Max = 0.24m,
                    IsPercentage = true
                },

                new AffixDefinition
                {
                    AffixKey = "ResistLightning",
                    DisplayName = "% Lightning Resistance",
                    AllowedSlots = "Offhand, Armor, Belt",
                    Tier1Min = 0.10m, Tier1Max = 0.15m,
                    Tier2Min = 0.16m, Tier2Max = 0.22m,
                    Tier3Min = 0.23m, Tier3Max = 0.30m,
                    IsPercentage = true
                },

                new AffixDefinition
                {
                    AffixKey = "ResistChaos",
                    DisplayName = "% Chaos Resistance",
                    AllowedSlots = "Offhand, Belt",
                    Tier1Min = 0.08m, Tier1Max = 0.12m,
                    Tier2Min = 0.13m, Tier2Max = 0.18m,
                    Tier3Min = 0.19m, Tier3Max = 0.25m,
                    IsPercentage = true
                },

                new AffixDefinition
                {
                    AffixKey = "ResistAll",
                    DisplayName = "% All Resistance",
                    AllowedSlots = "Belt",
                    Tier1Min = 0.03m, Tier1Max = 0.07m,
                    Tier2Min = 0.08m, Tier2Max = 0.10m,
                    Tier3Min = 0.11m, Tier3Max = 0.14m,
                    IsPercentage = true
                },
            };

            context.AffixDefinitions.AddRange(affixes);
        }

        private static void SeedSkills(ApplicationDbContext context)
        {
            if (context.SkillDefinitions.Any()) return;

            var skills = new List<SkillDefinition>
            {
               // ============================================
                // 1H SWORD SKILLS
                // ============================================

                new SkillDefinition
                {
                    SkillId = "QuickSlash",
                    DisplayName = "Quick Slash",
                    WeaponType = WeaponType.OneHandSword,
                    Multiplier = 1.10m,
                    ManaCost = 0,
                    Cooldown = 1,
                    RequiredLevel = 1,
                    Description = "Fast attack with 20% chance to apply Bleed for 2 turns",
                    EffectJson = "{\"onHit\":{\"applyBleed\":{\"chance\":0.20,\"duration\":2,\"maxStacks\":1,\"refreshDuration\":true}}}"
                },

                new SkillDefinition
                {
                    SkillId = "WeakPoint",
                    DisplayName = "Weak Point",
                    WeaponType = WeaponType.OneHandSword,
                    Multiplier = 0.85m,
                    ManaCost = 4,
                    Cooldown = 3,
                    RequiredLevel = 3,
                    Description = "Apply ArmorShred debuff for 2 turns",
                    EffectJson = "{\"onHit\":{\"applyArmorShred\":{\"chance\":1.0,\"duration\":2,\"maxStacks\":1,\"refreshDuration\":true}}}"
                },

                new SkillDefinition
                {
                    SkillId = "CounterMove",
                    DisplayName = "Counter Move",
                    WeaponType = WeaponType.OneHandSword,
                    Multiplier = 0.70m,
                    ManaCost = 5,
                    Cooldown = 2,
                    RequiredLevel = 5,
                    Description = "Enter counter stance for 2 turns. Next successful block grants Riposte (+20% melee damage)",
                    EffectJson = "{\"onUse\":{\"applyCounterStance\":{\"duration\":2,\"riposteDuration\":1,\"riposteDamageBonus\":0.20}}}"
                },

                new SkillDefinition
                {
                    SkillId = "CrossStrike",
                    DisplayName = "Cross Strike",
                    WeaponType = WeaponType.OneHandSword,
                    Multiplier = 1.50m,
                    ManaCost = 3,
                    Cooldown = 3,
                    RequiredLevel = 7,
                    Description = "Guaranteed Bleed for 2 turns if target HP < 50%",
                    EffectJson = "{\"onHit\":{\"applyBleed\":{\"chance\":1.0,\"duration\":2,\"maxStacks\":1,\"refreshDuration\":true,\"condition\":\"targetHPPercent<0.50\"}}}"
                },

                new SkillDefinition
                {
                    SkillId = "SwordDance",
                    DisplayName = "Sword Dance",
                    WeaponType = WeaponType.OneHandSword,
                    Multiplier = 0.70m,
                    ManaCost = 6,
                    Cooldown = 3,
                    RequiredLevel = 9,
                    Description = "Gain +15% Evasion and +10% Crit Chance for 2 turns",
                    EffectJson = "{\"onUse\":{\"applySwordDance\":{\"duration\":2,\"evasionBonus\":0.15,\"critBonus\":0.10}}}"
                },

                // ============================================
                // DAGGER SKILLS
                // ============================================

                new SkillDefinition
                {
                    SkillId = "BackStab",
                    DisplayName = "Back Stab",
                    WeaponType = WeaponType.Dagger,
                    Multiplier = 0.85m,
                    ManaCost = 0,
                    Cooldown = 0,
                    RequiredLevel = 1,
                    Description = "+20% crit chance for this attack",
                    EffectJson = "{\"onUse\":{\"critBonus\":0.20}}"
                },

                new SkillDefinition
                {
                    SkillId = "RapidSlash",
                    DisplayName = "Rapid Slash",
                    WeaponType = WeaponType.Dagger,
                    Multiplier = 1.50m,
                    ManaCost = 10,
                    Cooldown = 2,
                    RequiredLevel = 3,
                    Description = "Deal 150% bonus damage if acting first in combat",
                    EffectJson = "{\"onUse\":{\"damageBonus\":1.50,\"condition\":\"isActingFirst\"}}"
                },

                new SkillDefinition
                {
                    SkillId = "VenomousStrike",
                    DisplayName = "Venomous Strike",
                    WeaponType = WeaponType.Dagger,
                    Multiplier = 0.80m,
                    ManaCost = 7,
                    Cooldown = 2,
                    RequiredLevel = 2,
                    Description = "Apply Poison (4% of target's current HP per tick for 3 turns)",
                    EffectJson = "{\"onHit\":{\"applyPoison\":{\"tickPercent\":0.04,\"duration\":3,\"maxStacks\":1,\"refreshDuration\":true}}}"
                },

                new SkillDefinition
                {
                    SkillId = "ShadowStep",
                    DisplayName = "Shadow Step",
                    WeaponType = WeaponType.Dagger,
                    Multiplier = 0.00m,
                    ManaCost = 5,
                    Cooldown = 2,
                    RequiredLevel = 2,
                    Description = "+40% Evasion for 1 turn",
                    EffectJson = "{\"onUse\":{\"applyShadowStep\":{\"duration\":1,\"evasionMult\":1.40}}}"
                },

                new SkillDefinition
                {
                    SkillId = "Execution",
                    DisplayName = "Execution",
                    WeaponType = WeaponType.Dagger,
                    Multiplier = 1.30m,
                    ManaCost = 18,
                    Cooldown = 4,
                    RequiredLevel = 6,
                    Description = "Deal 200% damage if target HP < 30%",
                    EffectJson = "{\"onUse\":{\"damageBonus\":2.0,\"condition\":\"targetHPPercent<0.30\"}}"
                },

                // ============================================
                // 2H SWORD SKILLS
                // ============================================

                new SkillDefinition
                {
                    SkillId = "Cleave",
                    DisplayName = "Cleave",
                    WeaponType = WeaponType.TwoHandSword,
                    Multiplier = 1.35m,
                    ManaCost = 2,
                    Cooldown = 0,
                    RequiredLevel = 1,
                    Description = "Powerful slash attack",
                    EffectJson = "{}"
                },

                new SkillDefinition
                {
                    SkillId = "ExecutionersStrike",
                    DisplayName = "Executioner's Strike",
                    WeaponType = WeaponType.TwoHandSword,
                    Multiplier = 1.10m,
                    ManaCost = 8,
                    Cooldown = 2,
                    RequiredLevel = 3,
                    Description = "Strike twice, each hit can trigger effects separately",
                    EffectJson = "{\"multiHit\":{\"hits\":2,\"eachHitMulti\":1.00}}"
                },

                new SkillDefinition
                {
                    SkillId = "Sunder",
                    DisplayName = "Sunder",
                    WeaponType = WeaponType.TwoHandSword,
                    Multiplier = 1.35m,
                    ManaCost = 12,
                    Cooldown = 2,
                    RequiredLevel = 4,
                    Description = "40% chance to apply Bleed for 3 turns",
                    EffectJson = "{\"onHit\":{\"applyBleed\":{\"chance\":0.40,\"duration\":3,\"maxStacks\":1,\"refreshDuration\":true}}}"
                },

                new SkillDefinition
                {
                    SkillId = "ColossusStance",
                    DisplayName = "Colossus Stance",
                    WeaponType = WeaponType.TwoHandSword,
                    Multiplier = 0.00m,
                    ManaCost = 10,
                    Cooldown = 3,
                    RequiredLevel = 5,
                    Description = "+20% Damage and +20% Armor for 3 turns",
                    EffectJson = "{\"onUse\":{\"applyColossusStance\":{\"duration\":3,\"damageMult\":1.20,\"armorMult\":1.20}}}"
                },

                new SkillDefinition
                {
                    SkillId = "BarbariansWrath",
                    DisplayName = "Barbarian's Wrath",
                    WeaponType = WeaponType.TwoHandSword,
                    Multiplier = 1.60m,
                    ManaCost = 18,
                    Cooldown = 3,
                    RequiredLevel = 7,
                    Description = "Ignore 40% armor, +20% crit multiplier (max 250%)",
                    EffectJson = "{\"onUse\":{\"armorPenetration\":0.40,\"critMultiBonus\":0.20,\"critMultiCap\":2.50}}"
                },

                // ============================================
                // BOW SKILLS
                // ============================================

                new SkillDefinition
                {
                    SkillId = "AimedShot",
                    DisplayName = "Aimed Shot",
                    WeaponType = WeaponType.Bow,
                    Multiplier = 1.35m,
                    ManaCost = 7,
                    Cooldown = 1,
                    RequiredLevel = 1,
                    Description = "+20% accuracy and +15% crit chance",
                    EffectJson = "{\"onUse\":{\"accuracyMult\":1.20,\"critBonus\":0.15}}"
                },

                new SkillDefinition
                {
                    SkillId = "ArmorPiercingArrow",
                    DisplayName = "Armor-Piercing Arrow",
                    WeaponType = WeaponType.Bow,
                    Multiplier = 1.25m,
                    ManaCost = 6,
                    Cooldown = 1,
                    RequiredLevel = 3,
                    Description = "Apply Mark for 3 turns. Deal 125% damage if target already has Mark",
                    EffectJson = "{\"onHit\":{\"applyMark\":{\"duration\":3},\"bonusDamageIfMarked\":1.25}}"
                },

                new SkillDefinition
                {
                    SkillId = "BleedingShot",
                    DisplayName = "Bleeding Shot",
                    WeaponType = WeaponType.Bow,
                    Multiplier = 1.50m,
                    ManaCost = 3,
                    Cooldown = 0,
                    RequiredLevel = 5,
                    Description = "Guaranteed Bleed for 2 turns",
                    EffectJson = "{\"onHit\":{\"applyBleed\":{\"chance\":1.0,\"duration\":2,\"maxStacks\":1,\"refreshDuration\":true}}}"
                },

                new SkillDefinition
                {
                    SkillId = "HuntersFocus",
                    DisplayName = "Hunter's Focus",
                    WeaponType = WeaponType.Bow,
                    Multiplier = 0.00m,
                    ManaCost = 5,
                    Cooldown = 2,
                    RequiredLevel = 7,
                    Description = "+20% Accuracy and +10% Crit Chance for 3 turns",
                    EffectJson = "{\"onUse\":{\"applyHunterFocus\":{\"duration\":3,\"accuracyMult\":1.20,\"critBonus\":0.10,\"maxStacks\":1,\"refreshDuration\":true}}}"
                },

                new SkillDefinition
                {
                    SkillId = "HeavyArrow",
                    DisplayName = "Heavy Arrow",
                    WeaponType = WeaponType.Bow,
                    Multiplier = 1.85m,
                    ManaCost = 14,
                    Cooldown = 4,
                    RequiredLevel = 9,
                    Description = "Ignore 40% of target's armor",
                    EffectJson = "{\"onUse\":{\"armorPenetration\":0.40}}"
                },

                // ============================================
                // WAND SKILLS
                // ============================================

                new SkillDefinition
                {
                    SkillId = "Spark",
                    DisplayName = "Spark",
                    WeaponType = WeaponType.Wand,
                    Multiplier = 1.10m,
                    ManaCost = 3,
                    Cooldown = 0,
                    RequiredLevel = 1,
                    Description = "Basic lightning attack",
                    EffectJson = "{}"
                },

                new SkillDefinition
                {
                    SkillId = "LightningBolt",
                    DisplayName = "Lightning Bolt",
                    WeaponType = WeaponType.Wand,
                    Multiplier = 1.40m,
                    ManaCost = 9,
                    Cooldown = 1,
                    RequiredLevel = 2,
                    Description = "Powerful lightning strike",
                    EffectJson = "{}"
                },

                new SkillDefinition
                {
                    SkillId = "StaticCharge",
                    DisplayName = "Static Charge",
                    WeaponType = WeaponType.Wand,
                    Multiplier = 0.00m,
                    ManaCost = 6,
                    Cooldown = 3,
                    RequiredLevel = 4,
                    Description = "+20% Final Damage for 3 charges",
                    EffectJson = "{\"onUse\":{\"applyStaticCharge\":{\"charges\":3,\"damageMult\":1.20,\"maxStacks\":1,\"refreshDuration\":true}}}"
                },

                new SkillDefinition
                {
                    SkillId = "Paralyze",
                    DisplayName = "Paralyze",
                    WeaponType = WeaponType.Wand,
                    Multiplier = 0.85m,
                    ManaCost = 8,
                    Cooldown = 2,
                    RequiredLevel = 3,
                    Description = "Reduce Lightning Resistance by 20% for 2 turns. 35% chance to Stun for 1 turn",
                    EffectJson = "{\"onHit\":{\"applyParalyze\":{\"chance\":1.0,\"duration\":2,\"resistReduction\":0.20,\"maxStacks\":1,\"refreshDuration\":true},\"applyStun\":{\"chance\":0.35,\"duration\":1,\"maxStacks\":1,\"refreshDuration\":true}}}"
                },

                new SkillDefinition
                {
                    SkillId = "ChainLightning",
                    DisplayName = "Chain Lightning",
                    WeaponType = WeaponType.Wand,
                    Multiplier = 1.45m,
                    ManaCost = 16,
                    Cooldown = 3,
                    RequiredLevel = 6,
                    Description = "Lightning chains to hit twice",
                    EffectJson = "{\"multiHit\":{\"hits\":2}}"
                },

                // ============================================
                // STAFF SKILLS
                // ============================================

                new SkillDefinition
                {
                    SkillId = "AshStrike",
                    DisplayName = "Ash Strike",
                    WeaponType = WeaponType.Staff,
                    Multiplier = 1.10m,
                    ManaCost = 5,
                    Cooldown = 0,
                    RequiredLevel = 1,
                    Description = "Apply Ash Cloud debuff for 2 turns (-15% accuracy)",
                    EffectJson = "{\"onHit\":{\"applyAshCloud\":{\"chance\":1.0,\"duration\":2,\"accuracyMult\":0.85,\"maxStacks\":1,\"refreshDuration\":true}}}"
                },

                new SkillDefinition
                {
                    SkillId = "DimFlame",
                    DisplayName = "Dim Flame",
                    WeaponType = WeaponType.Staff,
                    Multiplier = 1.15m,
                    ManaCost = 7,
                    Cooldown = 1,
                    RequiredLevel = 2,
                    Description = "50% chance to apply Burn for 3 turns (stacks up to 3)",
                    EffectJson = "{\"onHit\":{\"applyBurn\":{\"chance\":0.5,\"duration\":3,\"maxStacks\":3,\"refreshDuration\":true}}}"
                },

                new SkillDefinition
                {
                    SkillId = "AshArmor",
                    DisplayName = "Ash Armor",
                    WeaponType = WeaponType.Staff,
                    Multiplier = 0.00m,
                    ManaCost = 6,
                    Cooldown = 3,
                    RequiredLevel = 5,
                    Description = "+30% Final Damage for 3 charges",
                    EffectJson = "{\"onUse\":{\"applyAshArmor\":{\"charges\":3,\"damageMult\":1.30,\"maxStacks\":1,\"refreshDuration\":true}}}"
                },

                new SkillDefinition
                {
                    SkillId = "FireBall",
                    DisplayName = "FireBall",
                    WeaponType = WeaponType.Staff,
                    Multiplier = 1.40m,
                    ManaCost = 6,
                    Cooldown = 1,
                    RequiredLevel = 4,
                    Description = "30% chance to apply Burn for 3 turns (stacks up to 3)",
                    EffectJson = "{\"onHit\":{\"applyBurn\":{\"chance\":0.3,\"duration\":3,\"maxStacks\":3,\"refreshDuration\":true}}}"
                },

                new SkillDefinition
                {
                    SkillId = "Combustion",
                    DisplayName = "Combustion",
                    WeaponType = WeaponType.Staff,
                    Multiplier = 1.10m,
                    ManaCost = 20,
                    Cooldown = 5,
                    RequiredLevel = 7,
                    Description = "Apply 2 stacks of Burn for 3 turns (stacks up to 3)",
                    EffectJson = "{\"onHit\":{\"applyBurn\":{\"chance\":1.0,\"duration\":3,\"stacks\":2,\"maxStacks\":3,\"refreshDuration\":true}}}"
                }
            };

            context.SkillDefinitions.AddRange(skills);
        }

        private static void SeedPassiveNodes(ApplicationDbContext context)
        {
            if (context.PassiveNodeDefinitions.Any()) return;

            var nodes = new List<PassiveNodeDefinition>
            {
                // ============================================
                // MERKEZ + GENEL KOLLAR (18 nodes)
                // ============================================

                // (1) Merkez
                new PassiveNodeDefinition
                {
                    NodeId = "N01",
                    DisplayName = "Core Start",
                    NodeType = "Minor",
                    RequiredLevel = 1,
                    PrerequisiteNodeIds = "",
                    Description = "Starting node",
                    EffectJson = "{}"
                },

                // (2) Offense (N02–N07)
                new PassiveNodeDefinition
                {
                    NodeId = "N02",
                    DisplayName = "Offense Path",
                    NodeType = "Minor",
                    RequiredLevel = 1,
                    PrerequisiteNodeIds = "N01",
                    Description = "+3% Damage",
                    EffectJson = "{\"increasedDamage\":0.03}"
                },

                new PassiveNodeDefinition
                {
                    NodeId = "N03",
                    DisplayName = "Precision I",
                    NodeType = "Minor",
                    RequiredLevel = 1,
                    PrerequisiteNodeIds = "N02",
                    Description = "+4% Accuracy",
                    EffectJson = "{\"increasedAccuracy\":0.04}"
                },

                new PassiveNodeDefinition
                {
                    NodeId = "N04",
                    DisplayName = "Crit I",
                    NodeType = "Minor",
                    RequiredLevel = 5,
                    PrerequisiteNodeIds = "N03",
                    Description = "+2% Crit Chance (additive, cap 40%)",
                    EffectJson = "{\"increasedCritChance\":0.02}"
                },

                new PassiveNodeDefinition
                {
                    NodeId = "N05",
                    DisplayName = "Brutality",
                    NodeType = "Minor",
                    RequiredLevel = 10,
                    PrerequisiteNodeIds = "N04",
                    Description = "+6% Damage",
                    EffectJson = "{\"increasedDamage\":0.06}"
                },

                new PassiveNodeDefinition
                {
                    NodeId = "N06",
                    DisplayName = "Execution Window",
                    NodeType = "Minor",
                    RequiredLevel = 15,
                    PrerequisiteNodeIds = "N05",
                    Description = "+6% Damage when target HP < 35%",
                    EffectJson = "{\"damageBonusLowHP\":1.06,\"hpThreshold\":0.35}"
                },

                new PassiveNodeDefinition
                {
                    NodeId = "N07",
                    DisplayName = "Clean Hit",
                    NodeType = "Notable",
                    RequiredLevel = 10,
                    PrerequisiteNodeIds = "N04",
                    Description = "+10% Accuracy multiplier",
                    EffectJson = "{\"accuracyMult\":1.10}"
                },

                // (3) Defense (N08–N13)
                new PassiveNodeDefinition
                {
                    NodeId = "N08",
                    DisplayName = "Defense Path",
                    NodeType = "Minor",
                    RequiredLevel = 1,
                    PrerequisiteNodeIds = "N01",
                    Description = "+3% Max HP",
                    EffectJson = "{\"maxHPPercent\":0.03}"
                },

                new PassiveNodeDefinition
                {
                    NodeId = "N09",
                    DisplayName = "Plating I",
                    NodeType = "Minor",
                    RequiredLevel = 1,
                    PrerequisiteNodeIds = "N08",
                    Description = "+5% Armor",
                    EffectJson = "{\"increasedArmor\":0.05}"
                },

                new PassiveNodeDefinition
                {
                    NodeId = "N10",
                    DisplayName = "Evasion I",
                    NodeType = "Minor",
                    RequiredLevel = 5,
                    PrerequisiteNodeIds = "N09",
                    Description = "+5% Evasion",
                    EffectJson = "{\"increasedEvasion\":0.05}"
                },

                new PassiveNodeDefinition
                {
                    NodeId = "N11",
                    DisplayName = "Ward I",
                    NodeType = "Minor",
                    RequiredLevel = 10,
                    PrerequisiteNodeIds = "N10",
                    Description = "+6% Ward",
                    EffectJson = "{\"increasedWard\":0.06}"
                },

                new PassiveNodeDefinition
                {
                    NodeId = "N12",
                    DisplayName = "Brace",
                    NodeType = "Notable",
                    RequiredLevel = 10,
                    PrerequisiteNodeIds = "N10",
                    Description = "First hit taken in turn: -6% damage taken",
                    EffectJson = "{\"firstHitDamageTakenMult\":0.94}"
                },

                new PassiveNodeDefinition
                {
                    NodeId = "N13",
                    DisplayName = "Unyielding",
                    NodeType = "Notable",
                    RequiredLevel = 15,
                    PrerequisiteNodeIds = "N11",
                    Description = "When HP < 25%: -8% damage taken",
                    EffectJson = "{\"lowHPDamageTakenMult\":0.92,\"hpThreshold\":0.25}"
                },

                // (4) Utility (N14–N19)
                new PassiveNodeDefinition
                {
                    NodeId = "N14",
                    DisplayName = "Utility Path",
                    NodeType = "Minor",
                    RequiredLevel = 1,
                    PrerequisiteNodeIds = "N01",
                    Description = "+2 Initiative",
                    EffectJson = "{\"initiative\":2}"
                },

                new PassiveNodeDefinition
                {
                    NodeId = "N15",
                    DisplayName = "Tempo I",
                    NodeType = "Minor",
                    RequiredLevel = 1,
                    PrerequisiteNodeIds = "N14",
                    Description = "+3 Initiative",
                    EffectJson = "{\"initiative\":3}"
                },

                new PassiveNodeDefinition
                {
                    NodeId = "N16",
                    DisplayName = "Mana Reserve",
                    NodeType = "Minor",
                    RequiredLevel = 5,
                    PrerequisiteNodeIds = "N15",
                    Description = "+6% Max Mana",
                    EffectJson = "{\"maxManaPercent\":0.06}"
                },

                new PassiveNodeDefinition
                {
                    NodeId = "N17",
                    DisplayName = "Efficient Casting I",
                    NodeType = "Minor",
                    RequiredLevel = 10,
                    PrerequisiteNodeIds = "N16",
                    Description = "-1 Mana Cost (min 0)",
                    EffectJson = "{\"manaCostReduction\":1}"
                },

                new PassiveNodeDefinition
                {
                    NodeId = "N18",
                    DisplayName = "First Tempo",
                    NodeType = "Notable",
                    RequiredLevel = 10,
                    PrerequisiteNodeIds = "N16",
                    Description = "First skill cast in turn: 25% less mana cost",
                    EffectJson = "{\"firstSkillManaCostMult\":0.75}"
                },

                new PassiveNodeDefinition
                {
                    NodeId = "N19",
                    DisplayName = "Fast Hands",
                    NodeType = "Notable",
                    RequiredLevel = 15,
                    PrerequisiteNodeIds = "N17",
                    Description = "Skills with CD ≥ 3: -1 Cooldown (min 2)",
                    EffectJson = "{\"cooldownReduction\":1,\"minCooldown\":2,\"cdThreshold\":3}"
                },

                // ============================================
                // SİLAH KOLLARI (42 nodes)
                // ============================================

                // (1) Bow (N20–N26)
                new PassiveNodeDefinition
                {
                    NodeId = "N20",
                    DisplayName = "Bow Path",
                    NodeType = "Minor",
                    RequiredLevel = 1,
                    PrerequisiteNodeIds = "N01",
                    Description = "+4% Accuracy",
                    EffectJson = "{\"increasedAccuracy\":0.04}"
                },

                new PassiveNodeDefinition
                {
                    NodeId = "N21",
                    DisplayName = "Bow Tempo",
                    NodeType = "Minor",
                    RequiredLevel = 5,
                    PrerequisiteNodeIds = "N20",
                    Description = "+3 Initiative",
                    EffectJson = "{\"initiative\":3}"
                },

                new PassiveNodeDefinition
                {
                    NodeId = "N22",
                    DisplayName = "Bow Damage I",
                    NodeType = "Minor",
                    RequiredLevel = 5,
                    PrerequisiteNodeIds = "N21",
                    Description = "+4% Damage with Bow",
                    EffectJson = "{\"increasedDamage\":0.04,\"weaponType\":\"Bow\"}"
                },

                new PassiveNodeDefinition
                {
                    NodeId = "N23",
                    DisplayName = "Mark Specialist",
                    NodeType = "Notable",
                    RequiredLevel = 10,
                    PrerequisiteNodeIds = "N22",
                    Description = "Mark duration +1 round",
                    EffectJson = "{\"markDurationBonus\":1}"
                },

                new PassiveNodeDefinition
                {
                    NodeId = "N24",
                    DisplayName = "Bow Crit",
                    NodeType = "Minor",
                    RequiredLevel = 10,
                    PrerequisiteNodeIds = "N23",
                    Description = "+2% Crit Chance",
                    EffectJson = "{\"increasedCritChance\":0.02}"
                },

                new PassiveNodeDefinition
                {
                    NodeId = "N25",
                    DisplayName = "Hunted",
                    NodeType = "Notable",
                    RequiredLevel = 15,
                    PrerequisiteNodeIds = "N24",
                    Description = "+6% Damage vs Marked targets",
                    EffectJson = "{\"damageVsMarked\":1.06}"
                },

                new PassiveNodeDefinition
                {
                    NodeId = "N26",
                    DisplayName = "Deadeye",
                    NodeType = "Keystone",
                    RequiredLevel = 18,
                    PrerequisiteNodeIds = "N25",
                    Description = "+10% Mark effect bonus, -6% Max HP",
                    EffectJson = "{\"markEffectBonus\":0.10,\"maxHPPercent\":-0.06}"
                },

                // (2) Dagger (N27–N33)
                new PassiveNodeDefinition
                {
                    NodeId = "N27",
                    DisplayName = "Dagger Path",
                    NodeType = "Minor",
                    RequiredLevel = 1,
                    PrerequisiteNodeIds = "N01",
                    Description = "+2% Crit Chance",
                    EffectJson = "{\"increasedCritChance\":0.02}"
                },

                new PassiveNodeDefinition
                {
                    NodeId = "N28",
                    DisplayName = "Dagger Tempo",
                    NodeType = "Minor",
                    RequiredLevel = 5,
                    PrerequisiteNodeIds = "N27",
                    Description = "+4 Initiative",
                    EffectJson = "{\"initiative\":4}"
                },

                new PassiveNodeDefinition
                {
                    NodeId = "N29",
                    DisplayName = "Dagger Damage I",
                    NodeType = "Minor",
                    RequiredLevel = 5,
                    PrerequisiteNodeIds = "N28",
                    Description = "+4% Damage with Dagger",
                    EffectJson = "{\"increasedDamage\":0.04,\"weaponType\":\"Dagger\"}"
                },

                new PassiveNodeDefinition
                {
                    NodeId = "N30",
                    DisplayName = "First Blood",
                    NodeType = "Notable",
                    RequiredLevel = 10,
                    PrerequisiteNodeIds = "N29",
                    Description = "+6% Damage if you strike first in combat",
                    EffectJson = "{\"firstStrikeDamage\":1.06}"
                },

                new PassiveNodeDefinition
                {
                    NodeId = "N31",
                    DisplayName = "Poison Edge",
                    NodeType = "Minor",
                    RequiredLevel = 10,
                    PrerequisiteNodeIds = "N30",
                    Description = "Poison duration +1 round",
                    EffectJson = "{\"poisonDurationBonus\":1}"
                },

                new PassiveNodeDefinition
                {
                    NodeId = "N32",
                    DisplayName = "Critical Aim",
                    NodeType = "Notable",
                    RequiredLevel = 15,
                    PrerequisiteNodeIds = "N31",
                    Description = "+5% Crit Multiplier",
                    EffectJson = "{\"critMultiBonus\":0.05}"
                },

                new PassiveNodeDefinition
                {
                    NodeId = "N33",
                    DisplayName = "Assassin",
                    NodeType = "Keystone",
                    RequiredLevel = 18,
                    PrerequisiteNodeIds = "N32",
                    Description = "First crit in turn: +15% Crit Multiplier, -10% Armor",
                    EffectJson = "{\"firstCritMultiBonus\":0.15,\"increasedArmor\":-0.10}"
                },

                // (3) 1H Sword (N34–N40)
                new PassiveNodeDefinition
                {
                    NodeId = "N34",
                    DisplayName = "1H Path",
                    NodeType = "Minor",
                    RequiredLevel = 1,
                    PrerequisiteNodeIds = "N01",
                    Description = "+4% Armor",
                    EffectJson = "{\"increasedArmor\":0.04}"
                },

                new PassiveNodeDefinition
                {
                    NodeId = "N35",
                    DisplayName = "Block I",
                    NodeType = "Minor",
                    RequiredLevel = 5,
                    PrerequisiteNodeIds = "N34",
                    Description = "+5% Block Chance (cap 40%)",
                    EffectJson = "{\"increasedBlockChance\":0.05}"
                },

                new PassiveNodeDefinition
                {
                    NodeId = "N36",
                    DisplayName = "1H Damage I",
                    NodeType = "Minor",
                    RequiredLevel = 5,
                    PrerequisiteNodeIds = "N35",
                    Description = "+4% Damage with 1H Sword",
                    EffectJson = "{\"increasedDamage\":0.04,\"weaponType\":\"OneHandSword\"}"
                },

                new PassiveNodeDefinition
                {
                    NodeId = "N37",
                    DisplayName = "Riposte Drill",
                    NodeType = "Notable",
                    RequiredLevel = 10,
                    PrerequisiteNodeIds = "N36",
                    Description = "After Block: next attack +8% damage (once per turn)",
                    EffectJson = "{\"riposteDamage\":1.08}"
                },

                new PassiveNodeDefinition
                {
                    NodeId = "N38",
                    DisplayName = "Bleed Mastery",
                    NodeType = "Minor",
                    RequiredLevel = 10,
                    PrerequisiteNodeIds = "N37",
                    Description = "Bleed duration +1 round",
                    EffectJson = "{\"bleedDurationBonus\":1}"
                },

                new PassiveNodeDefinition
                {
                    NodeId = "N39",
                    DisplayName = "Guarded",
                    NodeType = "Notable",
                    RequiredLevel = 15,
                    PrerequisiteNodeIds = "N38",
                    Description = "On successful Block: -8% damage taken from that hit",
                    EffectJson = "{\"blockedHitDamageTakenMult\":0.92}"
                },

                new PassiveNodeDefinition
                {
                    NodeId = "N40",
                    DisplayName = "Duelist",
                    NodeType = "Keystone",
                    RequiredLevel = 18,
                    PrerequisiteNodeIds = "N39",
                    Description = "Block cap +5% (max 45%), -12% Evasion",
                    EffectJson = "{\"blockCapBonus\":0.05,\"increasedEvasion\":-0.12}"
                },

                // (4) 2H Sword (N41–N47)
                new PassiveNodeDefinition
                {
                    NodeId = "N41",
                    DisplayName = "2H Path",
                    NodeType = "Minor",
                    RequiredLevel = 1,
                    PrerequisiteNodeIds = "N01",
                    Description = "+4% Damage",
                    EffectJson = "{\"increasedDamage\":0.04}"
                },

                new PassiveNodeDefinition
                {
                    NodeId = "N42",
                    DisplayName = "2H Power",
                    NodeType = "Minor",
                    RequiredLevel = 5,
                    PrerequisiteNodeIds = "N41",
                    Description = "+5% Crit Multiplier",
                    EffectJson = "{\"critMultiBonus\":0.05}"
                },

                new PassiveNodeDefinition
                {
                    NodeId = "N43",
                    DisplayName = "2H Tempo",
                    NodeType = "Minor",
                    RequiredLevel = 5,
                    PrerequisiteNodeIds = "N42",
                    Description = "+2 Initiative",
                    EffectJson = "{\"initiative\":2}"
                },

                new PassiveNodeDefinition
                {
                    NodeId = "N44",
                    DisplayName = "Heavy Hands",
                    NodeType = "Notable",
                    RequiredLevel = 10,
                    PrerequisiteNodeIds = "N43",
                    Description = "+3% skill multiplier for 2H skills",
                    EffectJson = "{\"activeSkillMult\":0.03,\"weaponType\":\"TwoHandSword\"}"
                },

                new PassiveNodeDefinition
                {
                    NodeId = "N45",
                    DisplayName = "Armor Breaker",
                    NodeType = "Minor",
                    RequiredLevel = 10,
                    PrerequisiteNodeIds = "N44",
                    Description = "+5% Armor Penetration",
                    EffectJson = "{\"armorPenetration\":0.05}"
                },

                new PassiveNodeDefinition
                {
                    NodeId = "N46",
                    DisplayName = "Colossus Discipline",
                    NodeType = "Notable",
                    RequiredLevel = 15,
                    PrerequisiteNodeIds = "N45",
                    Description = "2H buff duration +1 round",
                    EffectJson = "{\"buffDurationBonus\":1,\"weaponType\":\"TwoHandSword\"}"
                },

                new PassiveNodeDefinition
                {
                    NodeId = "N47",
                    DisplayName = "Juggernaut",
                    NodeType = "Keystone",
                    RequiredLevel = 18,
                    PrerequisiteNodeIds = "N46",
                    Description = "+18% Armor, -6 Initiative",
                    EffectJson = "{\"increasedArmor\":0.18,\"initiative\":-6}"
                },

                // (5) Wand (N48–N54)
                new PassiveNodeDefinition
                {
                    NodeId = "N48",
                    DisplayName = "Wand Path",
                    NodeType = "Minor",
                    RequiredLevel = 1,
                    PrerequisiteNodeIds = "N01",
                    Description = "+3% Accuracy",
                    EffectJson = "{\"increasedAccuracy\":0.03}"
                },

                new PassiveNodeDefinition
                {
                    NodeId = "N49",
                    DisplayName = "Lightning I",
                    NodeType = "Minor",
                    RequiredLevel = 5,
                    PrerequisiteNodeIds = "N48",
                    Description = "+4% Damage with Wand",
                    EffectJson = "{\"increasedDamage\":0.04,\"weaponType\":\"Wand\"}"
                },

                new PassiveNodeDefinition
                {
                    NodeId = "N50",
                    DisplayName = "Shock Tempo",
                    NodeType = "Minor",
                    RequiredLevel = 5,
                    PrerequisiteNodeIds = "N49",
                    Description = "Wand skills with CD ≥ 3: -1 Cooldown (min 2)",
                    EffectJson = "{\"cooldownReduction\":1,\"minCooldown\":2,\"cdThreshold\":3,\"weaponType\":\"Wand\"}"
                },

                new PassiveNodeDefinition
                {
                    NodeId = "N51",
                    DisplayName = "Static Charges",
                    NodeType = "Notable",
                    RequiredLevel = 10,
                    PrerequisiteNodeIds = "N50",
                    Description = "Charge-based buff skills: +1 charge",
                    EffectJson = "{\"chargeBonus\":1}"
                },

                new PassiveNodeDefinition
                {
                    NodeId = "N52",
                    DisplayName = "Control I",
                    NodeType = "Minor",
                    RequiredLevel = 10,
                    PrerequisiteNodeIds = "N51",
                    Description = "+5% Stun chance (cap 50%)",
                    EffectJson = "{\"stunChance\":0.05}"
                },

                new PassiveNodeDefinition
                {
                    NodeId = "N53",
                    DisplayName = "Conductive",
                    NodeType = "Notable",
                    RequiredLevel = 15,
                    PrerequisiteNodeIds = "N52",
                    Description = "Lightning Resist shred duration +1 round",
                    EffectJson = "{\"lightningResistShredDurationBonus\":1}"
                },

                new PassiveNodeDefinition
                {
                    NodeId = "N54",
                    DisplayName = "Stormcaller",
                    NodeType = "Keystone",
                    RequiredLevel = 18,
                    PrerequisiteNodeIds = "N53",
                    Description = "On Stun: target takes +6% damage for 1 round, -8% Max Mana",
                    EffectJson = "{\"stunDamageTakenBonus\":1.06,\"stunDuration\":1,\"maxManaPercent\":-0.08}"
                },

                // (6) Staff (N55–N60)
                new PassiveNodeDefinition
                {
                    NodeId = "N55",
                    DisplayName = "Staff Path",
                    NodeType = "Minor",
                    RequiredLevel = 1,
                    PrerequisiteNodeIds = "N01",
                    Description = "+4% Max Mana",
                    EffectJson = "{\"maxManaPercent\":0.04}"
                },

                new PassiveNodeDefinition
                {
                    NodeId = "N56",
                    DisplayName = "Fire I",
                    NodeType = "Minor",
                    RequiredLevel = 5,
                    PrerequisiteNodeIds = "N55",
                    Description = "+4% Damage with Staff",
                    EffectJson = "{\"increasedDamage\":0.04,\"weaponType\":\"Staff\"}"
                },

                new PassiveNodeDefinition
                {
                    NodeId = "N57",
                    DisplayName = "Ash Control",
                    NodeType = "Minor",
                    RequiredLevel = 5,
                    PrerequisiteNodeIds = "N56",
                    Description = "Accuracy debuff duration +1 round",
                    EffectJson = "{\"accuracyDebuffDurationBonus\":1}"
                },

                new PassiveNodeDefinition
                {
                    NodeId = "N58",
                    DisplayName = "Burn Expert",
                    NodeType = "Notable",
                    RequiredLevel = 10,
                    PrerequisiteNodeIds = "N57",
                    Description = "Burn max stacks +1 (global cap: 4)",
                    EffectJson = "{\"burnMaxStacksBonus\":1,\"globalCap\":4}"
                },

                new PassiveNodeDefinition
                {
                    NodeId = "N59",
                    DisplayName = "Staff Crit",
                    NodeType = "Minor",
                    RequiredLevel = 10,
                    PrerequisiteNodeIds = "N58",
                    Description = "+2% Crit Chance",
                    EffectJson = "{\"increasedCritChance\":0.02}"
                },

                new PassiveNodeDefinition
                {
                    NodeId = "N60",
                    DisplayName = "Pyromancer",
                    NodeType = "Keystone",
                    RequiredLevel = 18,
                    PrerequisiteNodeIds = "N59",
                    Description = "Burn tick damage +15%, -10% Evasion",
                    EffectJson = "{\"burnTickMult\":1.15,\"increasedEvasion\":-0.10}"
                }
            };

            context.PassiveNodeDefinitions.AddRange(nodes);
        }
    }
}