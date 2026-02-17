// AtlasRPG.Web/Controllers/ShopController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AtlasRPG.Application.Services;
using AtlasRPG.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AtlasRPG.Web.Controllers
{
    [Authorize]
    public class ShopController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ShopService _shopService;

        public ShopController(
            ApplicationDbContext context,
            ShopService shopService)
        {
            _context = context;
            _shopService = shopService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(Guid runId)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            var run = await _context.Runs
                .Include(r => r.Inventory)
                    .ThenInclude(i => i.Item)
                    .ThenInclude(i => i.Affixes)
                    .ThenInclude(a => a.AffixDefinition)
                .AsSplitQuery()
                .FirstOrDefaultAsync(r => r.Id == runId && r.UserId == userId && r.IsActive);

            if (run == null)
                return NotFound();

            // ✅ run nesnesini geçir — cache çalışsın
            var shopItems = await _shopService.GenerateShopInventory(run);

            ViewBag.Run = run;
            ViewBag.ShopItems = shopItems;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Buy(Guid runId, Guid itemId)
        {
            var success = await _shopService.BuyItem(runId, itemId);

            TempData[success ? "Success" : "Error"] = success
                ? "Item purchased successfully!"
                : "Not enough gold!";

            return RedirectToAction("TurnHub", "Run", new { id = runId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Sell(Guid runId, Guid runItemId)
        {
            var success = await _shopService.SellItem(runId, runItemId);

            TempData[success ? "Success" : "Error"] = success
                ? "Item sold!"
                : "Cannot sell equipped item.";

            return RedirectToAction("TurnHub", "Run", new { id = runId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reroll(Guid runId, Guid runItemId)
        {
            var success = await _shopService.RerollItem(runId, runItemId);

            TempData[success ? "Success" : "Error"] = success
                ? "Affixes rerolled!"
                : "Not enough gold!";

            return RedirectToAction("Upgrade", new { runId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upgrade(Guid runId, Guid runItemId)
        {
            var success = await _shopService.UpgradeItem(runId, runItemId);

            TempData[success ? "Success" : "Error"] = success
                ? "Item upgraded!"
                : "Not enough gold or already max rarity!";

            return RedirectToAction("Upgrade", new { runId });
        }

        [HttpGet]
        public async Task<IActionResult> Upgrade(Guid runId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var run = await _context.Runs
                .Include(r => r.Inventory)
                    .ThenInclude(ri => ri.Item)
                    .ThenInclude(i => i.Affixes)
                    .ThenInclude(a => a.AffixDefinition)
                .FirstOrDefaultAsync(r => r.Id == runId && r.UserId == userId && r.IsActive);

            if (run == null) return NotFound();

            ViewBag.Run = run;
            return View(run.Inventory
                .Where(ri => !ri.IsEquipped)   // sadece çantadakiler upgrade edilebilir
                .Select(ri => ri)
                .ToList());
        }
    }
}
