using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AtlasRPG.Core.Entities.Combat;

namespace AtlasRPG.Core.Entities.Runs
{
    public class RunTurn : BaseEntity
    {
        public Guid RunId { get; set; }
        public Run Run { get; set; } = null!;

        public int TurnNumber { get; set; }
        public bool IsPvp { get; set; }
        public bool IsCompleted { get; set; } = false;

        // Turn Result
        public bool? IsVictory { get; set; }
        public int GoldEarned { get; set; } = 0;
        public int PvpPointsEarned { get; set; } = 0;

        // Selected Loadout
        public Guid? SelectedWeaponId { get; set; }
        public Guid? SelectedActiveSkillId { get; set; }

        // Combat Reference
        public Guid? CombatResultId { get; set; }
        public CombatResult? CombatResult { get; set; }

        public DateTime? CompletedAt { get; set; }
    }
}
