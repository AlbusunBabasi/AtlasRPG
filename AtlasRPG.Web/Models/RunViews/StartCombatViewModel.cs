using System.ComponentModel.DataAnnotations;

namespace AtlasRPG.Web.Models.RunViews
{
    public class StartCombatViewModel
    {
        [Required(ErrorMessage = "Please select a skill")]
        public Guid SelectedSkillId { get; set; }

        public Guid TurnId { get; set; }
    }
}