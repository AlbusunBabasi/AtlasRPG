using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AtlasRPG.Core.Entities.Runs
{
    public class RunPassiveNode : BaseEntity
    {
        public Guid RunId { get; set; }
        public Run Run { get; set; } = null!;

        public string NodeId { get; set; } = string.Empty; // e.g., "N01", "N02"
        public int AllocatedAtLevel { get; set; }
    }
}
