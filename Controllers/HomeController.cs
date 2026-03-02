using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using webapp_miniproject.Models;

namespace webapp_miniproject.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly GameGroupDbContext _gameGroupDb;

    public HomeController(GameGroupDbContext gameGroupDb, ILogger<HomeController> logger)
    {
        _gameGroupDb = gameGroupDb;
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> FindGroup()
    {
        var gameInfos = await _gameGroupDb.GameInfos
            .Include(g => g.GameGroupInfos)
            .OrderBy(g => g.Name)
            .ToListAsync();
        return View(gameInfos);
    }

    [HttpGet]
    public async Task<IActionResult> FindGroupPartial(string? q, int? gameId)
    {
        var gameInfos = await _gameGroupDb.GameInfos
            .Include(g => g.GameGroupInfos)
            .OrderBy(g => g.Name)
            .ToListAsync();
        
        foreach (var gameInfo in gameInfos)
        {
            gameInfo.GameGroupInfos = gameInfo.GameGroupInfos
                .Where(g =>
                    (gameId == null || g.GameId == gameId) &&
                    (
                        string.IsNullOrWhiteSpace(q) ||
                        (g.Title?.Contains(q, StringComparison.OrdinalIgnoreCase) ?? false) ||
                        (g.Description?.Contains(q, StringComparison.OrdinalIgnoreCase) ?? false)
                    )
                )
                .ToList();
        }

        var filtered = gameInfos
            .Where(g => g.GameGroupInfos.Any())
            .ToList();

        return PartialView("_GameGroups", filtered);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
