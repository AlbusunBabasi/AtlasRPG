// AtlasRPG.Web/Controllers/ShopController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AtlasRPG.Application.Services;
using AtlasRPG.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

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

        // AtlasRPG.Web/Controllers/ShopController.cs
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

            // Shop item'larını generate et
            var shopItems = await _shopService.GenerateShopInventory(run.CurrentTurn);

            // ✅ Contains YOK — her item için affixleri ayrı ayrı yükle
            foreach (var item in shopItems)
            {
                await _context.Entry(item)
                    .Collection(i => i.Affixes)
                    .Query()
                    .Include(a => a.AffixDefinition)
                    .LoadAsync();
            }

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

            return RedirectToAction("Index", new { runId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upgrade(Guid runId, Guid runItemId)
        {
            var success = await _shopService.UpgradeItem(runId, runItemId);

            TempData[success ? "Success" : "Error"] = success
                ? "Item upgraded!"
                : "Not enough gold or already max rarity!";

            return RedirectToAction("Index", new { runId });
        }
    }
}
