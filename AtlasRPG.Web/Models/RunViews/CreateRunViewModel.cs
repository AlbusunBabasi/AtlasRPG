// AtlasRPG.Web/Models/Run/CreateRunViewModel.cs
using AtlasRPG.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace AtlasRPG.Web.Models.RunViews
{
    public class CreateRunViewModel
    {
        [Required(ErrorMessage = "Please select a race")]
        public RaceType? Race { get; set; }

        [Required(ErrorMessage = "Please select a class")]
        public ClassType? Class { get; set; }
    }
}