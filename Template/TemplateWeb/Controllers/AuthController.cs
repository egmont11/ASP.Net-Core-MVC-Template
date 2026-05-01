using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TemplateWeb.Entities;
using TemplateWeb.Models;

namespace TemplateWeb.Controllers;

public class AuthController : Controller
{
    private readonly ILogger<AuthController> _logger;
    private readonly UserManager<UserEntity> _userManager;
    private readonly SignInManager<UserEntity> _signInManager;

    public AuthController(
        ILogger<AuthController> logger,
        UserManager<UserEntity> userManager,
        SignInManager<UserEntity> signInManager)
    {
        _logger = logger;
        _userManager = userManager;
        _signInManager = signInManager;
    }

    #region HttpGet

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Home");

        return View(new AuthLoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Register(string? returnUrl)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Home");

        return View(new AuthRegisterViewModel { ReturnUrl = returnUrl });
    }

    [HttpGet]
    public IActionResult AccessDenied() => View();

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Profile()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login");

        return View(new ProfileViewModel
        {
            UserName = user.UserName!,
            Email = user.Email!,
        });
    }

    #endregion

    #region HttpPost

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(AuthLoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        // Support login by email or username
        var user = model.EmailOrUsername.Contains('@')
            ? await _userManager.FindByEmailAsync(model.EmailOrUsername)
            : await _userManager.FindByNameAsync(model.EmailOrUsername);

        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "Invalid credentials.");
            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(
            user, model.Password, model.RememberMe, lockoutOnFailure: false);

        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, "Invalid credentials.");
            return View(model);
        }

        if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            return Redirect(model.ReturnUrl);

        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(AuthRegisterViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = new UserEntity
        {
            UserName = model.UserName.Trim(),
            Email = model.Email.Trim().ToLower(),
        };

        var result = await _userManager.CreateAsync(user, model.Password);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
            return View(model);
        }

        await _userManager.AddToRoleAsync(user, "User");
        _logger.LogInformation("User {UserName} registered.", user.UserName);

        if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            return Redirect(model.ReturnUrl);

        return RedirectToAction("Login");
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(ProfileViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login");

        // Update username
        if (user.UserName != model.UserName.Trim())
        {
            var setName = await _userManager.SetUserNameAsync(user, model.UserName.Trim());
            if (!setName.Succeeded)
            {
                foreach (var e in setName.Errors)
                    ModelState.AddModelError(string.Empty, e.Description);
                return View(model);
            }
        }

        // Update email
        if (user.Email != model.Email.Trim().ToLower())
        {
            var setEmail = await _userManager.SetEmailAsync(user, model.Email.Trim().ToLower());
            if (!setEmail.Succeeded)
            {
                foreach (var e in setEmail.Errors)
                    ModelState.AddModelError(string.Empty, e.Description);
                return View(model);
            }
        }

        // Change password (requires current password)
        if (!string.IsNullOrWhiteSpace(model.NewPassword))
        {
            if (string.IsNullOrWhiteSpace(model.CurrentPassword))
            {
                ModelState.AddModelError(nameof(model.CurrentPassword), "Current password is required to set a new password.");
                return View(model);
            }

            var changePassword = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (!changePassword.Succeeded)
            {
                foreach (var e in changePassword.Errors)
                    ModelState.AddModelError(string.Empty, e.Description);
                return View(model);
            }
        }

        // Refresh cookie so the user stays logged in after security-stamp changes
        await _signInManager.RefreshSignInAsync(user);

        TempData["SuccessMessage"] = "Profile updated successfully.";
        return RedirectToAction("Profile");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAccount()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login");

        await _signInManager.SignOutAsync();
        await _userManager.DeleteAsync(user);
        _logger.LogInformation("User {UserId} deleted their account.", user.Id);

        return RedirectToAction("Register");
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> DownloadData()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login");

        var roles = await _userManager.GetRolesAsync(user);

        var data = new
        {
            user.Id,
            user.UserName,
            user.Email,
            Roles = roles,
            ExportDate = DateTime.UtcNow,
        };

        var json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
        var bytes = Encoding.UTF8.GetBytes(json);

        return File(bytes, "application/json", $"user_data_{user.UserName}.json");
    }

    #endregion
}
