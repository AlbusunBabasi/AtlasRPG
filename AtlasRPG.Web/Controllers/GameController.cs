// AtlasRPG.Web/Controllers/GameController.cs - Dashboard'ı güncelle

using AtlasRPG.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class GameController : Controller
{
    private readonly ApplicationDbContext _context;

    public GameController(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Dashboard()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        // Get active run
        var activeRun = await _context.Runs
            .FirstOrDefaultAsync(r => r.UserId == userId && r.IsActive);

        // Get completed runs
        var completedRuns = await _context.Runs
            .Where(r => r.UserId == userId && !r.IsActive)
            .OrderByDescending(r => r.CompletedAt)
            .Take(5)
            .ToListAsync();

        ViewBag.ActiveRun = activeRun;
        ViewBag.CompletedRuns = completedRuns;

        return View();
    }

    public IActionResult NewRun()
    {
        return RedirectToAction("Create", "Run");
    }

    public IActionResult Leaderboard()
    {
        // TODO: Leaderboard logic
        return View();
    }
}