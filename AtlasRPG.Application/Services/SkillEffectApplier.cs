using System.Text.Json;
using AtlasRPG.Core.ValueObjects;

namespace AtlasRPG.Application.Services
{
    /// <summary>
    /// SkillDefinition.EffectJson'ını parse eder ve CharacterStats'a uygular.
    /// </summary>
    public static class SkillEffectApplier
    {
        private static readonly Random _rng = new();

        // ── OnUse: skill cast anında tetiklenir (hit olmadan) ───────
        public static void ApplyOnUse(
            CharacterStats caster,
            CharacterStats target,
            string effectJson,
            bool actingFirst)
        {
            if (string.IsNullOrWhiteSpace(effectJson) || effectJson == "{}")
                return;

            try
            {
                var doc = JsonDocument.Parse(effectJson);
                if (!doc.RootElement.TryGetProperty("onUse", out var onUse))
                    return;

                ApplyOnUseEffects(caster, target, onUse, actingFirst);
            }
            catch { /* Geçersiz JSON sessizce atla */ }
        }

        // ── OnHit: saldırı isabet ettiğinde tetiklenir ──────────────
        public static void ApplyOnHit(
            CharacterStats attacker,
            CharacterStats target,
            string effectJson,
            decimal attackerBaseDamage)
        {
            if (string.IsNullOrWhiteSpace(effectJson) || effectJson == "{}")
                return;

            try
            {
                var doc = JsonDocument.Parse(effectJson);
                if (!doc.RootElement.TryGetProperty("onHit", out var onHit))
                    return;

                ApplyOnHitEffects(attacker, target, onHit, attackerBaseDamage);
            }
            catch { }
        }

        // ── Round başı DOT tick ──────────────────────────────────────
        public static decimal TickStatusEffects(CharacterStats character)
        {
            decimal totalDamage = 0m;
            var toRemove = new List<ActiveStatusEffect>();

            foreach (var effect in character.StatusEffects)
            {
                switch (effect.Type)
                {
                    case "Bleed":
                        // True damage — armor/resist geçmez
                        totalDamage += effect.TickValue * effect.CurrentStacks;
                        break;

                    case "Poison":
                        // True damage — % of current HP
                        totalDamage += character.CurrentHp * effect.TickPercent;
                        break;

                    case "Burn":
                        // FireResist uygulanır, Ward absorbs
                        decimal burnTick = character.CurrentHp * 0.03m * effect.CurrentStacks;
                        burnTick *= (1m - character.FireResist);
                        if (effect.WardAbsorbs && character.Ward > 0)
                        {
                            decimal absorbed = Math.Min(character.Ward, burnTick);
                            character.Ward -= absorbed;
                            burnTick -= absorbed;
                        }
                        totalDamage += burnTick;
                        break;
                }

                effect.RemainingRounds--;
                if (effect.RemainingRounds <= 0)
                    toRemove.Add(effect);
            }

            foreach (var e in toRemove)
                character.StatusEffects.Remove(e);

            // Stun bir round sonra sıfırlanır
            character.IsStunned = false;

            return totalDamage;
        }

        // ────────────────────────────────────────────────────────────
        // PRIVATE HELPERS
        // ────────────────────────────────────────────────────────────

        private static void ApplyOnUseEffects(
            CharacterStats caster, CharacterStats target,
            JsonElement onUse, bool actingFirst)
        {
            foreach (var prop in onUse.EnumerateObject())
            {
                switch (prop.Name)
                {
                    case "critBonus":
                        // BackStab: bu saldırıya özel, CombatService tarafında
                        // CharacterStats'a geçici olarak eklenir
                        caster.CritChance = Math.Min(0.40m,
                            caster.CritChance + prop.Value.GetDecimal());
                        break;

                    case "armorPenetration":
                        caster.ArmorPenetration += prop.Value.GetDecimal();
                        break;

                    case "damageBonus":
                        // Condition kontrolü
                        if (onUse.TryGetProperty("condition", out var cond))
                        {
                            string condStr = cond.GetString() ?? "";
                            if (condStr == "isActingFirst" && !actingFirst)
                                break;
                        }
                        // Geçici damage mult ekle (tek saldırı için)
                        caster.IncreasedDamage *= prop.Value.GetDecimal();
                        break;

                    case "applySwordDance":
                        ApplyBuff(caster, "SwordDance", prop.Value);
                        break;

                    case "applyCounterStance":
                        ApplyBuff(caster, "CounterStance", prop.Value);
                        break;

                    case "applyColossusStance":
                        ApplyBuff(caster, "ColossusStance", prop.Value,
                            b => {
                                caster.IncreasedDamage *= 1.20m;
                                caster.IncreasedArmor *= 1.20m;
                            });
                        break;

                    case "applyShadowStep":
                        ApplyBuff(caster, "ShadowStep", prop.Value,
                            b => caster.Evasion *= 1.40m);
                        break;

                    case "applyHunterFocus":
                        var hf = prop.Value;
                        caster.Accuracy *= hf.TryGetProperty("accuracyMult", out var am)
                            ? am.GetDecimal() : 1.20m;
                        caster.CritChance = Math.Min(0.40m,
                            caster.CritChance + (hf.TryGetProperty("critBonus", out var cb)
                            ? cb.GetDecimal() : 0.10m));
                        break;

                    case "applyStaticCharge":
                    case "applyAshArmor":
                        // Charge-based — CombatService'te round bazında işlenir
                        int charges = prop.Value.TryGetProperty("charge", out var ch)
                            ? ch.GetInt32() : 3;
                        decimal dmgMult = prop.Value.TryGetProperty("damageMult", out var dm)
                            ? dm.GetDecimal() : 1.20m;
                        AddOrRefreshStatus(caster.StatusEffects, new ActiveStatusEffect
                        {
                            Type = prop.Name == "applyStaticCharge" ? "StaticCharge" : "AshArmor",
                            RemainingRounds = 99, // Charges bitince biter
                            ChargesRemaining = charges,
                            DamageBonusMult = dmgMult,
                            MaxStacks = 1
                        });
                        break;
                }
            }
        }

        private static void ApplyOnHitEffects(
            CharacterStats attacker, CharacterStats target,
            JsonElement onHit, decimal attackerBaseDamage)
        {
            foreach (var prop in onHit.EnumerateObject())
            {
                var data = prop.Value;

                switch (prop.Name)
                {
                    case "applyBleed":
                        TryApplyDotEffect(target, "Bleed", data, 0,
                            tickValue: attackerBaseDamage * 0.25m,
                            wardAbsorbs: false,
                            condition: () => CheckHitCondition(data, target));
                        break;

                    case "applyPoison":
                        TryApplyDotEffect(target, "Poison", data,
                            tickPercent: data.TryGetProperty("tickPercent", out var tp)
                                ? tp.GetDecimal() : 0.04m,
                            wardAbsorbs: false);
                        break;

                    case "applyBurn":
                        TryApplyDotEffect(target, "Burn", data, 0,
                            wardAbsorbs: true);
                        break;

                    case "applyStun":
                        double stunChance = data.TryGetProperty("chance", out var sc)
                            ? sc.GetDouble() : 1.0;
                        if (_rng.NextDouble() < stunChance)
                            target.IsStunned = true;
                        break;

                    case "applyArmorShred":
                        // Armor geçici olarak azalt
                        decimal shredAmount = target.Armor * 0.15m;
                        target.Armor = Math.Max(0, target.Armor - shredAmount);
                        AddOrRefreshStatus(target.StatusEffects, new ActiveStatusEffect
                        {
                            Type = "ArmorShred",
                            RemainingRounds = data.TryGetProperty("duration", out var asd)
                                ? asd.GetInt32() : 2,
                            MaxStacks = 1
                        });
                        break;

                    case "applyMark":
                        AddOrRefreshStatus(target.StatusEffects, new ActiveStatusEffect
                        {
                            Type = "Mark",
                            RemainingRounds = data.TryGetProperty("duration", out var md)
                                ? md.GetInt32() : 3,
                            DamageTakenMult = 1.12m,
                            MaxStacks = 1
                        });
                        break;

                    case "applyAshCloud":
                        decimal accMult = data.TryGetProperty("accuracyMult", out var acm)
                            ? acm.GetDecimal() : 0.85m;
                        target.Accuracy *= accMult;
                        AddOrRefreshStatus(target.StatusEffects, new ActiveStatusEffect
                        {
                            Type = "AshCloud",
                            RemainingRounds = data.TryGetProperty("duration", out var acd)
                                ? acd.GetInt32() : 2,
                            MaxStacks = 1
                        });
                        break;

                    case "applyParalyze":
                        decimal resistReduction = data.TryGetProperty("resistReduction", out var rr)
                            ? rr.GetDecimal() : 0.20m;
                        target.LightningResist -= resistReduction;
                        AddOrRefreshStatus(target.StatusEffects, new ActiveStatusEffect
                        {
                            Type = "Paralyze",
                            RemainingRounds = data.TryGetProperty("duration", out var pd)
                                ? pd.GetInt32() : 2,
                            MaxStacks = 1
                        });
                        break;

                    case "bonusDamageIfMarked":
                        // Mark varsa damage bonus — CombatService'te hasar hesaplanırken kontrol edilir
                        break;
                }
            }
        }

        private static void TryApplyDotEffect(
            CharacterStats target, string type,
            JsonElement data, decimal tickPercent = 0m,
            decimal tickValue = 0m,
            bool wardAbsorbs = false,
            Func<bool>? condition = null)
        {
            if (condition != null && !condition())
                return;

            double chance = data.TryGetProperty("chance", out var c) ? c.GetDouble() : 1.0;
            if (_rng.NextDouble() > chance)
                return;

            int duration = data.TryGetProperty("duration", out var d) ? d.GetInt32() : 2;
            int maxStacks = data.TryGetProperty("maxStacks", out var ms) ? ms.GetInt32() : 1;

            AddOrRefreshStatus(target.StatusEffects, new ActiveStatusEffect
            {
                Type = type,
                RemainingRounds = duration,
                MaxStacks = maxStacks,
                TickValue = tickValue,
                TickPercent = tickPercent,
                WardAbsorbs = wardAbsorbs,
                RefreshDuration = true
            });
        }

        private static bool CheckHitCondition(JsonElement data, CharacterStats target)
        {
            if (!data.TryGetProperty("condition", out var cond))
                return true;

            string condStr = cond.GetString() ?? "";
            if (condStr == "targetHPPercent<0.50")
                return target.MaxHp > 0 && (target.CurrentHp / target.MaxHp) < 0.50m;

            return true;
        }

        private static void ApplyBuff(
            CharacterStats caster, string type,
            JsonElement data,
            Action<ActiveStatusEffect>? onApply = null)
        {
            int dur = data.TryGetProperty("duration", out var d) ? d.GetInt32() : 2;
            decimal evBonus = data.TryGetProperty("evasionBonus", out var eb) ? eb.GetDecimal() : 0m;
            decimal crBonus = data.TryGetProperty("critBonus", out var cb) ? cb.GetDecimal() : 0m;

            if (evBonus > 0) caster.Evasion += caster.Evasion * evBonus;
            if (crBonus > 0) caster.CritChance = Math.Min(0.40m, caster.CritChance + crBonus);

            var effect = new ActiveStatusEffect
            {
                Type = type,
                RemainingRounds = dur,
                MaxStacks = 1
            };

            onApply?.Invoke(effect);
            AddOrRefreshStatus(caster.StatusEffects, effect);
        }

        private static void AddOrRefreshStatus(
            List<ActiveStatusEffect> effects, ActiveStatusEffect newEffect)
        {
            var existing = effects.FirstOrDefault(e => e.Type == newEffect.Type);
            if (existing == null)
            {
                effects.Add(newEffect);
                return;
            }

            // Refresh veya stack
            if (existing.CurrentStacks < existing.MaxStacks)
                existing.CurrentStacks++;
            else
                existing.RemainingRounds = newEffect.RemainingRounds; // duration refresh
        }
    }
}
