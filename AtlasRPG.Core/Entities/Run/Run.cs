using AtlasRPG.Core.Entities.Identity;
using AtlasRPG.Core.Entities.Items;
using AtlasRPG.Core.Entities.Player;
using AtlasRPG.Core.Enums;

namespace AtlasRPG.Core.Entities.Runs
{
    public class Run : BaseEntity
    {
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser User { get; set; } = null!;

        // Run Configuration
        public RaceType Race { get; set; }
        public ClassType Class { get; set; }
        public string RunHash { get; set; } = Guid.NewGuid().ToString("N"); // For matchmaking

        // Run State
        public int CurrentTurn { get; set; } = 1;
        public int CurrentLevel { get; set; } = 1;
        public int RemainingLives { get; set; } = 3;
        public int Gold { get; set; } = 50;
        public int PvpPoints { get; set; } = 0;
        public bool IsActive { get; set; } = true;
        public bool HasWoundDebuff { get; set; } = false;

        // Stat Points
        public int AvailableStatPoints { get; set; } = 0;
        public int AvailableSkillPoints { get; set; } = 0;

        // Allocated Stats (başlangıç: her stat 5)
        public int Strength { get; set; } = 5;
        public int Dexterity { get; set; } = 5;
        public int Agility { get; set; } = 5;
        public int Intelligence { get; set; } = 5;
        public int Vitality { get; set; } = 5;
        public int Wisdom { get; set; } = 5;
        public int Luck { get; set; } = 5;

        // Navigation
        public ICollection<RunTurn> Turns { get; set; } = new List<RunTurn>();
        public ICollection<RunItem> Inventory { get; set; } = new List<RunItem>();
        public ICollection<RunPassiveNode> AllocatedPassives { get; set; } = new List<RunPassiveNode>();
        public RunEquipment? Equipment { get; set; }

        public DateTime? CompletedAt { get; set; }
    }
}