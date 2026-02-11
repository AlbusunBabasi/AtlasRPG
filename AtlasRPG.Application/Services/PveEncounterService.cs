// AtlasRPG.Application/Services/PveEncounterService.cs
using AtlasRPG.Core.Enums;

namespace AtlasRPG.Application.Services
{
    public class PveMonster
    {
        public string Name { get; set; } = "";
        public int TurnNumber { get; set; }

        // Combat stats
        public decimal MaxHp { get; set; }
        public decimal CurrentHp { get; set; }
        public decimal BaseDamage { get; set; }
        public decimal Armor { get; set; }
        public decimal Evasion { get; set; }
        public decimal Accuracy { get; set; }
        public decimal CritChance { get; set; }
        public decimal CritMultiplier { get; set; }
        public decimal Ward { get; set; }
        public decimal BlockChance { get; set; }
        public decimal Initiative { get; set; }

        // Resistances
        public decimal ResistFire { get; set; }
        public decimal ResistCold { get; set; }
        public decimal ResistLightning { get; set; }
        public decimal ResistChaos { get; set; }

        // PVE has no active skill (basic attack only)
        public string SkillName => "Basic Attack";
        public decimal SkillMultiplier => 1.0m;
        public int SkillManaCost => 0;
        public int SkillCooldown => 0;
    }

    public class PveEncounterService
    {
        private readonly Random _random = new Random();

        private static readonly string[] MonsterNames = new[]
        {
            "Goblin Raider", "Forest Wolf", "Bandit Scout",
            "Stone Golem", "Dark Cultist", "Orc Brute",
            "Shadow Wraith", "Iron Guardian", "Chaos Fiend",
            "Ancient Colossus"
        };

        public PveMonster GenerateMonster(int turnNumber, int playerLevel)
        {
            // Turn 1-3: Kolay (ısınma)
            // Turn 9: Orta
            // Turn 15: Zor
            float difficultyMultiplier = turnNumber switch
            {
                <= 3 => 0.65f,   // %65 güç — kolay isınma
                9 => 0.80f,      // %80 güç — orta
                15 => 0.95f,     // %95 güç — zor
                _ => 0.70f
            };

            // Base values scaled to turn
            decimal baseHp = 60 + (turnNumber * 15);
            decimal baseDmg = 5 + (turnNumber * 2);
            decimal baseArmor = 10 + (turnNumber * 3);

            // Slight random variance ±10%
            float variance = 0.9f + (float)(_random.NextDouble() * 0.2);

            var monster = new PveMonster
            {
                Name = MonsterNames[Math.Min(turnNumber - 1, MonsterNames.Length - 1)],
                TurnNumber = turnNumber,

                MaxHp = Math.Round(baseHp * (decimal)difficultyMultiplier * (decimal)variance),
                BaseDamage = Math.Round(baseDmg * (decimal)difficultyMultiplier * (decimal)variance),
                Armor = Math.Round(baseArmor * (decimal)difficultyMultiplier),

                // Düşük kaçınma/isabet — monsters daha çok hasar alır
                Evasion = turnNumber * 1.5m,
                Accuracy = 60 + (turnNumber * 3),

                // Düşük crit — tahmin edilebilir savaş
                CritChance = 0.05m,
                CritMultiplier = 1.5m,

                // No block, no ward
                BlockChance = 0m,
                Ward = 0m,

                // Orta initiative — oyuncu genelde ilk vurur
                Initiative = 8 + turnNumber,

                // Zayıf resistances
                ResistFire = 0.05m,
                ResistCold = 0.05m,
                ResistLightning = 0.05m,
                ResistChaos = 0.0m
            };

            monster.CurrentHp = monster.MaxHp;
            return monster;
        }
    }
}