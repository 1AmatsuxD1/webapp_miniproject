using Microsoft.AspNetCore.Mvc;
using webapp_miniproject.Models;

namespace webapp_miniproject.Controllers;

public class GameGroupController : Controller
{
    private readonly Supabase.Client _supabase;

    public GameGroupController(Supabase.Client supabase)
    {
        _supabase = supabase;
    }

    private async Task<List<GameInfo>> FetchGameInfosWithGroups()
    {
        var gamesResponse = await _supabase.From<GameInfo>().Get();
        var gameGroupsResponse = await _supabase.From<GameGroupInfo>().Get();

        var groups = gameGroupsResponse.Models;

        foreach (var game in gamesResponse.Models)
        {
            game.GameGroupInfos = groups
                .Where(g => g.GameId == game.Id)
                .ToList();
        }

        return gamesResponse.Models.OrderBy(g => g.Name).ToList();
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var gameInfos = await FetchGameInfosWithGroups();
        return View(gameInfos);
    }

    [HttpGet]
    public async Task<IActionResult> IndexPartial(string? q, int? gameId)
    {
        var gameInfos = await FetchGameInfosWithGroups();

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

        var filtered = gameInfos.Where(g => g.GameGroupInfos.Any()).ToList();
        return PartialView("_GameGroups", filtered);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var groupResponse = await _supabase
            .From<GameGroupInfo>()
            .Where(g => g.Id == id)
            .Get();

        var gameGroup = groupResponse.Models.FirstOrDefault();
        if (gameGroup is null) return NotFound();

        var gameResponse = await _supabase
            .From<GameInfo>()
            .Where(g => g.Id == gameGroup.GameId)
            .Get();

        gameGroup.Game = gameResponse.Models.FirstOrDefault();

        return View(gameGroup);
    }
}