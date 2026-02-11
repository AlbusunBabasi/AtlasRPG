using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AtlasRPG.Core.Entities.Runs;

namespace AtlasRPG.Core.Entities.Player
{
    public class Player : BaseEntity
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;

        // Profile Stats (persist across runs)
        public int ProfileLevel { get; set; } = 1;
        public int TotalGold { get; set; } = 0;
        public int TotalWins { get; set; } = 0;
        public int TotalLosses { get; set; } = 0;

        // Navigation
        public ICollection<Run> Runs { get; set; } = new List<Run>();
    }
}
