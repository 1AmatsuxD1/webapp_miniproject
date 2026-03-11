using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using webapp_miniproject.Models;

namespace webapp_miniproject.Controllers;

public class AccountController : Controller
{
    private readonly Supabase.Client _supabase;

    public AccountController(Supabase.Client supabase)
    {
        _supabase = supabase;
    }

    // -- Login ----------------------------------------------------------------

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewBag.ReturnUrl = returnUrl;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(string username, string password, string? returnUrl = null)
    {
        var response = await _supabase
            .From<UserInfo>()
            .Where(u => u.Username == username && u.Password == password)
            .Get();
    
        var user = response.Models.FirstOrDefault();

        // No user matched
        if (user is null)
        {
            ViewBag.Error = "Incorrect username or password.";
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        await SignInCookie(user.Id.ToString(), user.Username);

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction("Index", "Home");
    }

    // -- Sign up ----------------------------------------------------------------

    [HttpGet]
    public IActionResult SignUp() => View();

    [HttpPost]
    public async Task<IActionResult> SignUp(string username, string password, string confirmPassword)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            ViewBag.Error = "Username is required.";
            return View();
        }

        if (password != confirmPassword)
        {
            ViewBag.Error = "Passwords do not match.";
            return View();
        }

        // Check if username is taken taken
        var existing = await _supabase
            .From<UserInfo>()
            .Where(u => u.Username == username)
            .Get();

        if (existing.Models.Any())
        {
            ViewBag.Error = "Username is already taken.";
            return View();
        }

        var newUser = new UserInfo
        {
            Username = username,
            Password = password
        };

        var response = await _supabase.From<UserInfo>().Insert(newUser);
        var createdUser = response.Models.FirstOrDefault();
    
        if (createdUser is null)
        {
            ViewBag.Error = "Sign up failed. Please try again.";
            return View();
        }

        await SignInCookie(createdUser.Id.ToString(), createdUser.Username);
        return RedirectToAction("Index", "Home");
    }

    // -- Logout ----------------------------------------------------------------

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        var userId = CurrentUserId;
        if (userId is null)
        {
            return RedirectToAction(nameof(Login));
        }

        var (model, profileLoaded) = await BuildProfileViewModelAsync(userId.Value);
        if (model is null)
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Login));
        }

        model.IsCurrentUser = true;

        if (!profileLoaded)
        {
            TempData["ProfileWarning"] = "Profile table is not ready yet. You can still view account actions below.";
        }

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> UserProfile(int id)
    {
        if (CurrentUserId == id)
        {
            return RedirectToAction(nameof(Profile));
        }

        var (model, profileLoaded) = await BuildProfileViewModelAsync(id);
        if (model is null)
        {
            return NotFound();
        }

        model.IsCurrentUser = false;

        if (!profileLoaded)
        {
            ViewBag.Error = "This player's profile details are not available yet.";
        }

        return View("Profile", model);
    }

    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(ProfileViewModel model)
    {
        var userId = CurrentUserId;
        if (userId is null)
        {
            return RedirectToAction(nameof(Login));
        }

        var userResponse = await _supabase
            .From<UserInfo>()
            .Where(u => u.Id == userId.Value)
            .Get();

        var user = userResponse.Models.FirstOrDefault();
        if (user is null)
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(nameof(Login));
        }

        model.UserId = user.Id;
        model.Username = user.Username;
        model.IsCurrentUser = true;

        if (string.IsNullOrWhiteSpace(model.DisplayName))
        {
            model.DisplayName = user.Username;
        }

        if (!string.IsNullOrWhiteSpace(model.AvatarUrl))
        {
            var isValid = Uri.TryCreate(model.AvatarUrl, UriKind.Absolute, out var parsed)
                          && (parsed.Scheme == Uri.UriSchemeHttp || parsed.Scheme == Uri.UriSchemeHttps);
            if (!isValid)
            {
                ModelState.AddModelError(nameof(model.AvatarUrl), "Avatar URL must be a valid http/https URL.");
            }
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var profileResponse = await _supabase
                .From<UserProfileInfo>()
                .Where(p => p.UserId == userId.Value)
                .Get();

            var profile = profileResponse.Models.FirstOrDefault();

            if (profile is null)
            {
                await _supabase
                    .From<UserProfileInfo>()
                    .Insert(new UserProfileInfo
                    {
                        UserId = userId.Value,
                        DisplayName = model.DisplayName,
                        Bio = model.Bio,
                        AvatarUrl = model.AvatarUrl,
                        FavoriteGame = model.FavoriteGame,
                        UpdatedAt = DateTime.UtcNow
                    });
            }
            else
            {
                profile.DisplayName = model.DisplayName;
                profile.Bio = model.Bio;
                profile.AvatarUrl = model.AvatarUrl;
                profile.FavoriteGame = model.FavoriteGame;
                profile.UpdatedAt = DateTime.UtcNow;

                await _supabase
                    .From<UserProfileInfo>()
                    .Update(profile);
            }

            TempData["ProfileSuccess"] = "Profile saved.";
            return RedirectToAction(nameof(Profile));
        }
        catch
        {
            ViewBag.Error = "Could not save profile. Ensure Supabase table 'UserProfile' exists with expected columns.";
            return View(model);
        }
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    // -- Delete Account ----------------------------------------------------------------

    [Authorize]
    [HttpGet]
    public IActionResult DeleteAccount() => View();

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> DeleteAccount(string confirm)
    {
        if (confirm != "DELETE")
        {
            ViewBag.Error = "Please type DELETE to confirm.";
            return View();
        }

        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        await _supabase
            .From<UserInfo>()
            .Where(u => u.Id == userId)
            .Delete();
        
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    // -- Helper ----------------------------------------------------------------

    private async Task SignInCookie(string userId, string email)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Name, email),
            new(ClaimTypes.Email, email),
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity));
    }

    private async Task<(ProfileViewModel? Model, bool ProfileLoaded)> BuildProfileViewModelAsync(int userId)
    {
        var userResponse = await _supabase
            .From<UserInfo>()
            .Where(u => u.Id == userId)
            .Get();

        var user = userResponse.Models.FirstOrDefault();
        if (user is null)
        {
            return (null, true);
        }

        var model = new ProfileViewModel
        {
            UserId = user.Id,
            Username = user.Username,
            DisplayName = user.Username
        };

        try
        {
            var profileResponse = await _supabase
                .From<UserProfileInfo>()
                .Where(p => p.UserId == userId)
                .Get();

            var profile = profileResponse.Models.FirstOrDefault();
            if (profile is not null)
            {
                model.DisplayName = string.IsNullOrWhiteSpace(profile.DisplayName) ? user.Username : profile.DisplayName;
                model.Bio = profile.Bio;
                model.AvatarUrl = profile.AvatarUrl;
                model.FavoriteGame = profile.FavoriteGame;
            }

            return (model, true);
        }
        catch
        {
            return (model, false);
        }
    }

    private int? CurrentUserId =>
        int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;
}