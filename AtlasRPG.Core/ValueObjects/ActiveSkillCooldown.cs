// AtlasRPG.Core/ValueObjects/ActiveSkillCooldown.cs
namespace AtlasRPG.Core.ValueObjects
{
    public class ActiveSkillCooldown
    {
        public Guid SkillId { get; set; }
        public int RemainingRounds { get; set; }
    }
}