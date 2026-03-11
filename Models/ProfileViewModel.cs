using System.ComponentModel.DataAnnotations;

namespace webapp_miniproject.Models;

public class ProfileViewModel
{
    public int UserId { get; set; }

    public string Username { get; set; } = "";

    public bool IsCurrentUser { get; set; }

    [Required]
    [StringLength(50)]
    public string DisplayName { get; set; } = "";

    [StringLength(500)]
    public string Bio { get; set; } = "";

    [Url]
    [StringLength(350)]
    public string AvatarUrl { get; set; } = "";

    [StringLength(60)]
    public string FavoriteGame { get; set; } = "";
}
