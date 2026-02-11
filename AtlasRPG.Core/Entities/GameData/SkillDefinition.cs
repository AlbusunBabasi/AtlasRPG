using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// AtlasRPG.Core/Entities/GameData/SkillDefinition.cs
using AtlasRPG.Core.Enums;

namespace AtlasRPG.Core.Entities.GameData
{
    public class SkillDefinition : BaseEntity
    {
        public string SkillId { get; set; } = string.Empty; // "QuickSlash", "BackStab"
        public string DisplayName { get; set; } = string.Empty;
        public WeaponType WeaponType { get; set; }
        //public EffectType EffectType { get; set; }

        public decimal Multiplier { get; set; }
        public int ManaCost { get; set; }
        public int Cooldown { get; set; }
        public int RequiredLevel { get; set; } = 1;



        public string EffectJson { get; set; } = string.Empty; // JSON serialized effects
        public string Description { get; set; } = string.Empty;
    }
}
