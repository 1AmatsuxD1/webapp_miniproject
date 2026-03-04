namespace webapp_miniproject.Contracts;

public class GameGroupResponse
{
    public int Id { get; set; }
    public int GameId { get; set; }
    public string GameName { get; set; } = "";
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string? ImageUrl { get; set; }
    public DateTime CreatedAt { get; set; }
}