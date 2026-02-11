// AtlasRPG.Web/Models/RunViews/PassiveTreeViewModel.cs

namespace AtlasRPG.Web.Models.RunViews
{
    public class PassiveTreeViewModel
    {
        public Guid RunId { get; set; }
        public int AvailablePoints { get; set; }
        public int CurrentLevel { get; set; }
        public List<PassiveNodeViewModel> Nodes { get; set; } = new();
        public HashSet<string> AllocatedNodeIds { get; set; } = new();
    }

    public class PassiveNodeViewModel
    {
        public string NodeId { get; set; } = string.Empty;       // "N01"
        public string DisplayName { get; set; } = string.Empty;
        public string NodeType { get; set; } = string.Empty;     // Minor | Notable | Keystone
        public string Description { get; set; } = string.Empty;
        public int RequiredLevel { get; set; }
        public List<string> PrerequisiteNodeIds { get; set; } = new();
        public string EffectJson { get; set; } = string.Empty;

        // Computed helpers
        public bool IsAllocated { get; set; }

        /// <summary>
        /// Level met AND at least one prereq allocated (or no prereqs) AND points available
        /// </summary>
        public bool IsAllocatable { get; set; }
    }
}
