// AtlasRPG.Web/Controllers/TestController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AtlasRPG.Application.Services;
using AtlasRPG.Core.Entities.Runs;
using AtlasRPG.Core.Entities.GameData;
using AtlasRPG.Core.Enums;
using AtlasRPG.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AtlasRPG.Web.Controllers
{
    [Authorize]
    public class TestController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly CombatService _combatService;

        public TestController(
            ApplicationDbContext context,
            CombatService combatService)
        {
            _context = context;
            _combatService = combatService;
        }

        public async Task<IActionResult> CombatSimulation()
        {
            // Create two test runs
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var run1 = new Run
            {
                UserId = userId,
                Race = RaceType.Human,
                Class = ClassType.Warrior,
                Strength = 10,
                Vitality = 8,
                Dexterity = 5,
                Agility = 5,
                Intelligence = 5,
                Wisdom = 5,
                Luck = 5
            };

            var run2 = new Run
            {
                UserId = userId,
                Race = RaceType.Orc,
                Class = ClassType.Berserker,
                Strength = 12,
                Vitality = 6,
                Dexterity = 5,
                Agility = 5,
                Intelligence = 5,
                Wisdom = 5,
                Luck = 5
            };

            _context.Runs.Add(run1);
            _context.Runs.Add(run2);
            await _context.SaveChangesAsync();

            // Get a basic skill
            var basicSkill = await _context.SkillDefinitions.FirstOrDefaultAsync();

            if (basicSkill == null)
            {
                return Content("No skills found in database. Please run seed data.");
            }

            // Simulate combat
            var result = await _combatService.SimulateCombat(run1, run2, basicSkill.Id);

            // Display results
            ViewBag.Result = result;

            return View();
        }
    }
}