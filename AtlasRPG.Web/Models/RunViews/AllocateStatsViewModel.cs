// AtlasRPG.Web/Models/Run/AllocateStatsViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace AtlasRPG.Web.Models.RunViews
{
    public class AllocateStatsViewModel
    {
        public Guid RunId { get; set; }
        public int AvailablePoints { get; set; }

        [Range(0, 100)]
        public int StrengthToAdd { get; set; } = 0;

        [Range(0, 100)]
        public int DexterityToAdd { get; set; } = 0;

        [Range(0, 100)]
        public int AgilityToAdd { get; set; } = 0;

        [Range(0, 100)]
        public int IntelligenceToAdd { get; set; } = 0;

        [Range(0, 100)]
        public int VitalityToAdd { get; set; } = 0;

        [Range(0, 100)]
        public int WisdomToAdd { get; set; } = 0;

        [Range(0, 100)]
        public int LuckToAdd { get; set; } = 0;

        // Current stats
        public int CurrentStrength { get; set; }
        public int CurrentDexterity { get; set; }
        public int CurrentAgility { get; set; }
        public int CurrentIntelligence { get; set; }
        public int CurrentVitality { get; set; }
        public int CurrentWisdom { get; set; }
        public int CurrentLuck { get; set; }
    }
}