using System.ComponentModel.DataAnnotations.Schema;

public class GameInfo
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public List<GameGroupInfo> GameGroupInfos { get; set; } = new();
}

public class GameGroupInfo
{
    public int Id { get; set; }
    public int GameId { get; set; }

    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string? ImageUrl { get; set; }

    [ForeignKey("GameId")]
    public GameInfo? Game { get; set; }
}