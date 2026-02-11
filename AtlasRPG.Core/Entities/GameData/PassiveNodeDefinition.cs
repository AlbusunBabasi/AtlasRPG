using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace AtlasRPG.Core.Entities.GameData
{
    public class PassiveNodeDefinition : BaseEntity
    {
        public string NodeId { get; set; } = string.Empty; // "N01", "N02"
        public string DisplayName { get; set; } = string.Empty;
        public string NodeType { get; set; } = string.Empty; // "Minor", "Notable", "Keystone"

        public int RequiredLevel { get; set; }
        public string PrerequisiteNodeIds { get; set; } = string.Empty; // Comma-separated

        public string EffectJson { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}