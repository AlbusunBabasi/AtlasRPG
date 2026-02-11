// AtlasRPG.Application/Services/DamageCalculatorService.cs
using AtlasRPG.Core.ValueObjects;
using System;

namespace AtlasRPG.Application.Services
{
    public class DamageCalculatorService
    {
        private readonly Random _random = new Random();

        public CombatAction CalculateAttack(
            CharacterStats attacker,
            CharacterStats defender,
            string actionName,
            decimal skillMultiplier,
            string damageType = "Physical")
        {
            var action = new CombatAction
            {
                ActionName = actionName,
                IsSkill = skillMultiplier > 1.0m,
                DamageMultiplier = skillMultiplier,
                DamageType = damageType
            };

            // 1. Hit Check
            decimal hitChance = CalculateHitChance(attacker.Accuracy, defender.Evasion);
            action.DidHit = _random.NextDouble() < (double)hitChance;

            if (!action.DidHit)
            {
                action.FinalDamage = 0;
                return action;
            }

            // 2. Crit Check
            action.DidCrit = _random.NextDouble() < (double)attacker.CritChance;

            // 3. Base Damage Calculation
            decimal baseDamage = attacker.BaseDamage;

            // Add stat scaling based on damage type
            if (damageType == "Physical")
            {
                baseDamage = attacker.MeleeDamage; // Already includes STR scaling
            }
            else if (damageType == "Ranged")
            {
                baseDamage = attacker.RangedDamage; // Already includes DEX scaling
            }
            else // Elemental/Spell
            {
                baseDamage = attacker.SpellDamage; // Already includes INT scaling
            }

            // 4. Apply skill multiplier
            baseDamage *= skillMultiplier;

            // 5. Random variance (-10% to +10%)
            decimal variance = (decimal)(_random.NextDouble() * 0.2 - 0.1); // -0.1 to +0.1
            baseDamage *= (1 + variance);

            // 6. Apply crit multiplier
            if (action.DidCrit)
            {
                baseDamage *= attacker.CritMultiplier;
            }

            action.RawDamage = baseDamage;

            // 7. Apply Mitigation
            decimal damageAfterMitigation = ApplyMitigation(baseDamage, defender, damageType);

            // 8. Block Check
            action.WasBlocked = _random.NextDouble() < (double)defender.BlockChance;
            if (action.WasBlocked)
            {
                damageAfterMitigation *= (1 - defender.BlockReduction);
            }

            // 9. Ward Absorption (only for non-Physical)
            if (damageType != "Physical" && defender.Ward > 0)
            {
                decimal wardAbsorbed = Math.Min(defender.Ward, damageAfterMitigation);
                damageAfterMitigation -= wardAbsorbed;
                // Note: Ward değişimi combat state'de tutulmalı
            }

            action.FinalDamage = Math.Max(0, damageAfterMitigation);

            return action;
        }

        private decimal CalculateHitChance(decimal accuracy, decimal evasion)
        {
            decimal hitChance = accuracy / (accuracy + evasion);
            return Math.Clamp(hitChance, 0.05m, 0.95m);
        }

        private decimal ApplyMitigation(decimal damage, CharacterStats defender, string damageType)
        {
            if (damageType == "Physical")
            {
                // Armor mitigation
                decimal armorMit = defender.Armor / (defender.Armor + 100);
                return damage * (1 - armorMit);
            }
            else
            {
                // Elemental resistance
                decimal resist = damageType switch
                {
                    "Fire" => defender.FireResist,
                    "Cold" => defender.ColdResist,
                    "Lightning" => defender.LightningResist,
                    "Chaos" => defender.ChaosResist,
                    _ => 0m
                };

                return damage * (1 - resist);
            }
        }

        public decimal CalculateSuddenDeathDamage(decimal maxHp, int stacks)
        {
            return maxHp * 0.05m * stacks;
        }
    }
}