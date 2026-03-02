using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace webapp_miniproject.Controllers;

public class GameGroupController : Controller
{
    private readonly GameGroupDbContext _db;

    public GameGroupController(GameGroupDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Details(int id)
    {
        var group = await _db.GameGroupInfos
            .Include(g => g.Game)
            .FirstOrDefaultAsync(g => g.Id == id);

        if (group == null) return NotFound();

        return View(group);
    }
}