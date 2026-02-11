using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AtlasRPG.Core.Entities.Runs
{
    public class RunEquipment : BaseEntity
    {
        public Guid RunId { get; set; }
        public Run Run { get; set; } = null!;

        public Guid? WeaponId { get; set; }
        public RunItem? Weapon { get; set; }

        public Guid? OffhandId { get; set; }
        public RunItem? Offhand { get; set; }

        public Guid? ArmorId { get; set; }
        public RunItem? Armor { get; set; }

        public Guid? BeltId { get; set; }
        public RunItem? Belt { get; set; }
    }
}