using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AtlasRPG.Core.Entities.Runs;


namespace AtlasRPG.Core.Entities.Combat
{
    public class CombatResult : BaseEntity
    {
        public Guid RunTurnId { get; set; }
        public RunTurn RunTurn { get; set; } = null!;

        // Matchmaking Info
        public string? OpponentSnapshotId { get; set; }
        public string? OpponentUsername { get; set; }
        public string MatchSeed { get; set; } = string.Empty;

        // Combat Outcome
        public bool IsVictory { get; set; }
        public int TotalRounds { get; set; }
        public bool WasSuddenDeath { get; set; } = false;
        public int SuddenDeathStacks { get; set; } = 0;

        // Player Stats
        public decimal PlayerTotalDamageDealt { get; set; }
        public decimal PlayerTotalDamageTaken { get; set; }
        public int PlayerCriticalHits { get; set; }

        // Opponent Stats
        public decimal OpponentTotalDamageDealt { get; set; }
        public decimal OpponentTotalDamageTaken { get; set; }
        public int OpponentCriticalHits { get; set; }

        // Navigation
        public ICollection<CombatRound> Rounds { get; set; } = new List<CombatRound>();
    }
}