// AtlasRPG.Web/Controllers/InventoryController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AtlasRPG.Application.Services;
using AtlasRPG.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AtlasRPG.Web.Controllers
{
    [Authorize]
    public class InventoryController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly InventoryService _inventoryService;

        public InventoryController(
            ApplicationDbContext context,
            InventoryService inventoryService)
        {
            _context = context;
            _inventoryService = inventoryService;
        }

        // GET: /Inventory/Index/{runId}
        [HttpGet]
        public async Task<IActionResult> Index(Guid runId)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            var run = await _context.Runs
                .Include(r => r.Inventory).ThenInclude(i => i.Item).ThenInclude(i => i.Affixes).ThenInclude(a => a.AffixDefinition)
                .Include(r => r.Equipment).ThenInclude(e => e.Weapon).ThenInclude(ri => ri.Item).ThenInclude(i => i.Affixes).ThenInclude(a => a.AffixDefinition)
                .Include(r => r.Equipment).ThenInclude(e => e.Offhand).ThenInclude(ri => ri.Item).ThenInclude(i => i.Affixes).ThenInclude(a => a.AffixDefinition)
                .Include(r => r.Equipment).ThenInclude(e => e.Armor).ThenInclude(ri => ri.Item).ThenInclude(i => i.Affixes).ThenInclude(a => a.AffixDefinition)
                .Include(r => r.Equipment).ThenInclude(e => e.Belt).ThenInclude(ri => ri.Item).ThenInclude(i => i.Affixes).ThenInclude(a => a.AffixDefinition)
                .FirstOrDefaultAsync(r => r.Id == runId && r.UserId == userId && r.IsActive);

            if (run == null)
                return NotFound();

            ViewBag.Run = run;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Equip(Guid runId, Guid runItemId)
        {
            var success = await _inventoryService.EquipItem(runId, runItemId);

            if (success)
                TempData["Success"] = "Item equipped successfully!";
            else
                TempData["Error"] = "Cannot equip: 2H weapon cannot use an offhand, or weapon/offhand type mismatch.";  // ✅

            return RedirectToAction("TurnHub", "Run", new { id = runId });
        }

        // POST: /Inventory/Unequip
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unequip(Guid runId, Guid runItemId)
        {
            var success = await _inventoryService.UnequipItem(runId, runItemId);

            if (success)
                TempData["Success"] = "Item unequipped successfully!";
            else
                TempData["Error"] = "Failed to unequip item";

            return RedirectToAction("TurnHub", "Run", new { id = runId });
        }

        // GET: /Inventory/StatPreview (AJAX)
        [HttpGet]
        public async Task<IActionResult> StatPreview(Guid runId, Guid runItemId)
        {
            var preview = await _inventoryService.GetStatPreview(runId, runItemId);
            return Json(preview);
        }
    }
}
