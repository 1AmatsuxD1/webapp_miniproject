using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace webapp_miniproject.Models;

[Table("GameList")]
public class GameInfo : BaseModel
{
    [PrimaryKey("id", false)]
    public int Id { get; set; }

    [Column("name")]
    public string Name { get; set; } = "";
    
    public List<GameGroupInfo> GameGroupInfos { get; set; } = new();
}