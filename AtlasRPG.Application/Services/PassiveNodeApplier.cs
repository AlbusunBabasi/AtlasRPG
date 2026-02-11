// AtlasRPG.Application/Services/PassiveNodeApplier.cs
using System.Text.Json;
using AtlasRPG.Core.Entities.GameData;
using AtlasRPG.Core.ValueObjects;

namespace AtlasRPG.Application.Services
{
    /// <summary>
    /// Allocated passive node definitionlarının EffectJson'larını parse eder,
    /// toplu PassiveBonuses nesnesi üretir.
    /// </summary>
    public static class PassiveNodeApplier
    {
        private static readonly JsonSerializerOptions _jsonOpts = new()
        {
            PropertyNameCaseInsensitive = true
        };

        // ──────────────────────────────────────────────────────────
        // ANA ENTRY POINT
        // Bir Run'ın allocated node listesini (definition'larıyla birlikte)
        // alıp PassiveBonuses döner.
        // ──────────────────────────────────────────────────────────
        public static PassiveBonuses Build(IEnumerable<PassiveNodeDefinition> allocatedDefs)
        {
            var pb = new PassiveBonuses();

            foreach (var node in allocatedDefs)
            {
                if (string.IsNullOrWhiteSpace(node.EffectJson) || node.EffectJson == "{}")
                    continue;

                try
                {
                    var doc = JsonDocument.Parse(node.EffectJson);
                    ApplyNode(pb, doc.RootElement);
                }
                catch
                {
                    // Geçersiz JSON'u sessizce atla — üretim log'una düşür
                }
            }

            return pb;
        }

        // ──────────────────────────────────────────────────────────
        // TEKİL NODE APPLY
        // ──────────────────────────────────────────────────────────
        private static void ApplyNode(PassiveBonuses pb, JsonElement root)
        {
            // weaponType varsa silaha özel hasar bonusu ayrı izlenir
            string? weaponType = TryGetString(root, "weaponType");

            foreach (var prop in root.EnumerateObject())
            {
                switch (prop.Name)
                {
                    // ── Stat-time ─────────────────────────────────
                    case "maxHPPercent":
                        pb.MaxHpPercent += prop.Value.GetDecimal();
                        break;

                    case "maxManaPercent":
                        pb.MaxManaPercent += prop.Value.GetDecimal();
                        break;

                    case "initiative":
                        pb.InitiativeFlat += prop.Value.GetDecimal();
                        break;

                    case "increasedDamage":
                        decimal dmgBonus = prop.Value.GetDecimal();
                        if (weaponType != null)
                            AddWeaponDamage(pb, weaponType, dmgBonus);
                        else
                            pb.IncreasedDamageGlobal += dmgBonus;
                        break;

                    case "increasedAccuracy":
                        pb.IncreasedAccuracyGlobal += prop.Value.GetDecimal();
                        break;

                    case "increasedCritChance":
                        pb.CritChanceFlat += prop.Value.GetDecimal();
                        break;

                    case "accuracyMult":
                        // Multiplicative — birden fazla node alırsa çarpılır
                        pb.AccuracyMult *= prop.Value.GetDecimal();
                        break;

                    case "critMultiBonus":
                        pb.CritMultiBonus += prop.Value.GetDecimal();
                        break;

                    case "increasedArmor":
                        pb.IncreasedArmor += prop.Value.GetDecimal();
                        break;

                    case "increasedEvasion":
                        pb.IncreasedEvasion += prop.Value.GetDecimal();
                        break;

                    case "increasedWard":
                        pb.IncreasedWard += prop.Value.GetDecimal();
                        break;

                    case "increasedBlockChance":
                        pb.IncreasedBlockChance += prop.Value.GetDecimal();
                        break;

                    case "armorPenetration":
                        pb.ArmorPenetration += prop.Value.GetDecimal();
                        break;

                    // ── Combat-time ───────────────────────────────
                    case "manaCostReduction":
                        pb.ManaCostReduction += prop.Value.GetDecimal();
                        break;

                    case "firstSkillManaCostMult":
                        // Multiplicative (birden fazla alınırsa çarpılır)
                        pb.FirstSkillManaCostMult *= prop.Value.GetDecimal();
                        break;

                    case "cooldownReduction":
                        pb.CooldownReduction += prop.Value.GetDecimal();
                        // cdThreshold en düşüğü korur (birden fazla node varsa)
                        if (TryGetDecimal(root, "cdThreshold", out decimal threshold))
                            pb.CooldownReductionThreshold = Math.Min(pb.CooldownReductionThreshold, threshold);
                        break;

                    case "firstHitDamageTakenMult":
                        // Multiplicative
                        pb.FirstHitDamageTakenMult *= prop.Value.GetDecimal();
                        break;

                    case "lowHPDamageTakenMult":
                        pb.LowHpDamageTakenMult *= prop.Value.GetDecimal();
                        if (TryGetDecimal(root, "hpThreshold", out decimal lpThresh))
                            pb.LowHpThreshold = Math.Max(pb.LowHpThreshold, lpThresh);
                        break;

                    case "damageBonusLowHP":
                        pb.ExecutionWindowMult *= prop.Value.GetDecimal();
                        if (TryGetDecimal(root, "hpThreshold", out decimal ewThresh))
                            pb.ExecutionThreshold = Math.Max(pb.ExecutionThreshold, ewThresh);
                        break;

                    case "markDurationBonus":
                        pb.MarkDurationBonus += prop.Value.GetInt32();
                        break;

                    case "damageVsMarked":
                        pb.DamageVsMarked *= prop.Value.GetDecimal();
                        break;

                    case "markEffectBonus":
                        pb.MarkEffectBonus += prop.Value.GetDecimal();
                        break;

                    case "firstStrikeDamage":
                        pb.FirstStrikeDamageMult *= prop.Value.GetDecimal();
                        break;

                    case "poisonDurationBonus":
                        pb.PoisonDurationBonus += prop.Value.GetInt32();
                        break;

                    case "bleedDurationBonus":
                        pb.BleedDurationBonus += prop.Value.GetInt32();
                        break;

                    case "blockSuccessDamageTakenMult":
                        pb.BlockSuccessDamageTakenMult *= prop.Value.GetDecimal();
                        break;

                    case "riposteAfterBlockMult":
                        pb.RiposteAfterBlockMult *= prop.Value.GetDecimal();
                        break;

                    case "buffDurationBonus":
                        pb.BuffDurationBonus2H += prop.Value.GetInt32();
                        break;

                    case "activeSkillMultBonus":
                        pb.ActiveSkillMult2HBonus += prop.Value.GetDecimal();
                        break;

                    case "stunChance":
                        pb.StunChanceBonus += prop.Value.GetDecimal();
                        break;

                    case "lightningResistShredDurationBonus":
                        pb.LightningResistShredDurationBonus += prop.Value.GetInt32();
                        break;

                    case "stunDamageTakenBonus":
                        pb.StunDamageTakenBonus *= prop.Value.GetDecimal();
                        if (TryGetInt(root, "stunDuration", out int stunDur))
                            pb.StunDamageTakenDuration = Math.Max(pb.StunDamageTakenDuration, stunDur);
                        break;

                    case "chargeBonus":
                        pb.ChargeBonus += prop.Value.GetInt32();
                        break;

                    case "accuracyDebuffDurationBonus":
                        pb.AccuracyDebuffDurationBonus += prop.Value.GetInt32();
                        break;

                    case "burnMaxStacksBonus":
                        pb.BurnMaxStacksBonus += prop.Value.GetInt32();
                        break;

                    case "burnTickMult":
                        pb.BurnTickMult *= prop.Value.GetDecimal();
                        break;

                        // Ignore meta-keys (weaponType, hpThreshold, cdThreshold, globalCap, etc.)
                        // handled inline above
                }
            }
        }

        // ──────────────────────────────────────────────────────────
        // HELPERS
        // ──────────────────────────────────────────────────────────
        private static void AddWeaponDamage(PassiveBonuses pb, string weaponType, decimal bonus)
        {
            switch (weaponType)
            {
                case "Bow": pb.IncreasedDamageBow += bonus; break;
                case "Dagger": pb.IncreasedDamageDagger += bonus; break;
                case "1HSword": pb.IncreasedDamage1HSword += bonus; break;
                case "2HSword": pb.IncreasedDamage2HSword += bonus; break;
                case "Wand": pb.IncreasedDamageWand += bonus; break;
                case "Staff": pb.IncreasedDamageStaff += bonus; break;
                default: pb.IncreasedDamageGlobal += bonus; break;
            }
        }

        private static string? TryGetString(JsonElement root, string key)
        {
            if (root.TryGetProperty(key, out var el) && el.ValueKind == JsonValueKind.String)
                return el.GetString();
            return null;
        }

        private static bool TryGetDecimal(JsonElement root, string key, out decimal value)
        {
            if (root.TryGetProperty(key, out var el) && el.TryGetDecimal(out value))
                return true;
            value = 0m;
            return false;
        }

        private static bool TryGetInt(JsonElement root, string key, out int value)
        {
            if (root.TryGetProperty(key, out var el) && el.TryGetInt32(out value))
                return true;
            value = 0;
            return false;
        }
    }
}
