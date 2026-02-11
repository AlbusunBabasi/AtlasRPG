using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AtlasRPG.Core.Entities.Items
{
    public class AffixDefinition : BaseEntity
    {
        public string AffixKey { get; set; } = string.Empty; // "DamagePct", "AccuracyPct"
        public string DisplayName { get; set; } = string.Empty; // "% Damage"
        public string AllowedSlots { get; set; } = string.Empty; // "Weapon,Offhand"

        // Tier Ranges
        public decimal Tier1Min { get; set; }
        public decimal Tier1Max { get; set; }
        public decimal Tier2Min { get; set; }
        public decimal Tier2Max { get; set; }
        public decimal Tier3Min { get; set; }
        public decimal Tier3Max { get; set; }

        public bool IsPercentage { get; set; } = false;
    }
}