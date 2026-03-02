using Microsoft.EntityFrameworkCore;

public class GameGroupDbContext : DbContext
{
    public DbSet<GameGroupInfo> GameGroupInfos { get; set; }
    public DbSet<GameInfo> GameInfos { get; set; }

    public GameGroupDbContext(DbContextOptions<GameGroupDbContext> options) : base(options) { }
}