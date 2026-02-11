using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AtlasRPG.Core.Enums;

namespace AtlasRPG.Core.Entities.Matchmaking
{
    public class Snapshot : BaseEntity
    {
        public string SnapshotHash { get; set; } = Guid.NewGuid().ToString("N");
        public string PlayerIdHash { get; set; } = string.Empty; // Anonymized
        public string RunIdHash { get; set; } = string.Empty;

        public int TurnIndex { get; set; }
        public string MatchSeed { get; set; } = string.Empty;

        // Character Info
        public RaceType Race { get; set; }
        public ClassType Class { get; set; }
        public int Level { get; set; }

        // Stats Snapshot (JSON veya ayrı kolonlar)
        public string StatsJson { get; set; } = string.Empty; // Serialized stats

        // Loadout
        public WeaponType SelectedWeapon { get; set; }
        public string SelectedActiveSkillId { get; set; } = string.Empty;

        // Power Metrics
        public decimal PowerScore { get; set; }
        public decimal StructuralScore { get; set; }
        public int PowerBand { get; set; }

        public bool IsValid { get; set; } = true;
        public int TimesSelected { get; set; } = 0;
        public DateTime? LastSelectedAt { get; set; }
    }
}
