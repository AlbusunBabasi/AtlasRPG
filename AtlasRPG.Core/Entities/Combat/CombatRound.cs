using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AtlasRPG.Core.Entities.Combat
{
    public class CombatRound : BaseEntity
    {
        public Guid CombatResultId { get; set; }
        public CombatResult CombatResult { get; set; } = null!;

        public int RoundNumber { get; set; }

        // Player Action
        public string PlayerAction { get; set; } = string.Empty; // "BasicAttack" or skill name
        public bool PlayerHit { get; set; }
        public bool PlayerCrit { get; set; }
        public decimal PlayerDamage { get; set; }
        public bool PlayerBlocked { get; set; }

        // Opponent Action
        public string OpponentAction { get; set; } = string.Empty;
        public bool OpponentHit { get; set; }
        public bool OpponentCrit { get; set; }
        public decimal OpponentDamage { get; set; }
        public bool OpponentBlocked { get; set; }

        // Round End State
        public decimal PlayerHpRemaining { get; set; }
        public decimal OpponentHpRemaining { get; set; }
        public decimal PlayerWardAbsorbed { get; set; } = 0m;
        public decimal OpponentWardAbsorbed { get; set; } = 0m;
        public decimal PlayerDotDamage { get; set; } = 0m;   // round başında tick'lenen DOT
        public decimal OpponentDotDamage { get; set; } = 0m;
        public string ActivePlayerStatuses { get; set; } = string.Empty;   // "Bleed,Poison"
        public string ActiveOpponentStatuses { get; set; } = string.Empty;
        public decimal PlayerLifesteal { get; set; } = 0m;
        public decimal OpponentLifesteal { get; set; } = 0m;
        public decimal PlayerElementalDamage { get; set; } = 0m;
        public decimal OpponentElementalDamage { get; set; } = 0m;
        public string EventLog { get; set; } = string.Empty; // JSON or text summary
    }
}
