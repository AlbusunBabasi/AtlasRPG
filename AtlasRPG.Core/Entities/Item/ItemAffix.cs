using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AtlasRPG.Core.Entities.Items
{
    public class ItemAffix : BaseEntity
    {
        public Guid ItemId { get; set; }
        public Item Item { get; set; } = null!;

        public Guid AffixDefinitionId { get; set; }
        public AffixDefinition AffixDefinition { get; set; } = null!;

        public int Tier { get; set; } // 1, 2, or 3
        public decimal RolledValue { get; set; }
    }
}
