using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AtlasRPG.Core.Entities.Items;

namespace AtlasRPG.Core.Entities.Runs
{
    public class RunItem : BaseEntity
    {
        public Guid RunId { get; set; }
        public Run Run { get; set; } = null!;

        public Guid ItemId { get; set; }
        public Item Item { get; set; } = null!;

        public int AcquiredAtTurn { get; set; }
        public bool IsEquipped { get; set; } = false;
    }
}
