namespace webapp_miniproject.Contracts;

public class CreateGameGroupRequest
{
    public int GameId { get; set; }
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string? ImageUrl { get; set; }
}