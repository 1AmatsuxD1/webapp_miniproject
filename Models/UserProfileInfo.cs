using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace webapp_miniproject.Models;

[Table("UserProfile")]
public class UserProfileInfo : BaseModel
{
    [PrimaryKey("id", false)]
    public int Id { get; set; }

    [Column("user_id")]
    public int UserId { get; set; }

    [Column("display_name")]
    public string DisplayName { get; set; } = "";

    [Column("bio")]
    public string Bio { get; set; } = "";

    [Column("avatar_url")]
    public string AvatarUrl { get; set; } = "";

    [Column("favorite_game")]
    public string FavoriteGame { get; set; } = "";

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; }
}
