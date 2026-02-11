using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AtlasRPG.Core.Enums;

namespace AtlasRPG.Core.Entities.GameData
{
    public class BaseStatDefinition : BaseEntity
    {
        public RaceType? Race { get; set; }
        public ClassType? Class { get; set; }

        // Base Stats
        public int BaseHp { get; set; }
        public int BaseMana { get; set; }

        // Base Resistances
        public decimal BaseFireResist { get; set; } = 0;
        public decimal BaseColdResist { get; set; } = 0;
        public decimal BaseLightningResist { get; set; } = 0;
        public decimal BaseChaosResist { get; set; } = 0;

        public string Notes { get; set; } = string.Empty;
    }
}
