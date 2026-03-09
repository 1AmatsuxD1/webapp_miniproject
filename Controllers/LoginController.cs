using Microsoft.AspNetCore.Mvc;

namespace webapp_miniproject.Controllers;

public class LoginController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return View("_login");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Index(string email, string password, bool rememberMe = false)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            ModelState.AddModelError(string.Empty, "Email and password are required.");
            return View("_login");
        }

        TempData["LoginMessage"] = $"Login attempt for {email}.";
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult CreateAccount()
    {
        return View("_createacc");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult CreateAccount(string email, string password, string confirmPassword)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(confirmPassword))
        {
            ModelState.AddModelError(string.Empty, "Email, password, and confirm password are required.");
            return View("_createacc");
        }

        if (!string.Equals(password, confirmPassword, StringComparison.Ordinal))
        {
            ModelState.AddModelError(string.Empty, "Passwords do not match.");
            return View("_createacc");
        }

        TempData["CreateAccountMessage"] = $"Account creation request received for {email}.";
        return RedirectToAction(nameof(CreateAccount));
    }

    [HttpGet]
    public IActionResult Profile()
    {
        return View("_profile");
    }
}
