using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SurePath.Markets.Models;
using System.ComponentModel.DataAnnotations;

namespace SurePath.Markets.Controllers;

public class AuthController : Controller
{
    private readonly SignInManager<ApplicationUser> _signIn;
    private readonly UserManager<ApplicationUser>   _users;

    public AuthController(
        SignInManager<ApplicationUser> signIn,
        UserManager<ApplicationUser>   users)
    {
        _signIn = signIn;
        _users  = users;
    }

    // ── GET /auth/login ───────────────────────────────────────────────────────
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        // Only skip the login page for real accounts — demo users should be
        // able to sign in with actual credentials without being bounced away.
        if (_signIn.IsSignedIn(User) && !User.IsInRole("Demo"))
            return RedirectToAction("Journal", "Trade");

        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    // ── POST /auth/login ──────────────────────────────────────────────────────
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (!ModelState.IsValid)
            return View(model);

        var result = await _signIn.PasswordSignInAsync(
            model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);

        if (result.Succeeded)
            return LocalRedirect(returnUrl ?? "/trade/analytics");

        ModelState.AddModelError(string.Empty, "Invalid email or password.");
        return View(model);
    }

    // ── POST /auth/demo ───────────────────────────────────────────────────────
    // Signs in automatically as the seeded demo user — no password required.
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Demo()
    {
        var demoUser = await _users.FindByEmailAsync("demo@spe.com");

        if (demoUser is null)
            return RedirectToAction("Login");

        await _signIn.SignInAsync(demoUser, isPersistent: false);
        return RedirectToAction("Analytics", "Trade");
    }

    // ── POST /auth/logout ─────────────────────────────────────────────────────
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signIn.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }
}

// ── View model ────────────────────────────────────────────────────────────────
public class LoginViewModel
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required, DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; }
}
