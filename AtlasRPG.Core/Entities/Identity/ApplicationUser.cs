using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace AtlasRPG.Core.Entities.Identity
{
    public class ApplicationUser : IdentityUser
    {
        // Extra profile fields
        public string? DisplayName { get; set; }
        public int ProfileLevel { get; set; } = 1;
        public int TotalGold { get; set; } = 0;
        public int TotalWins { get; set; } = 0;
        public int TotalLosses { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLoginAt { get; set; }
    }
}