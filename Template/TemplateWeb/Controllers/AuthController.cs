using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TemplateWeb.Interfaces;
using TemplateWeb.Models;

namespace TemplateWeb.Controllers;

public class AuthController : Controller
{
    private readonly ILogger<AuthController> _logger;
    private readonly IAuthService _authService;
    
    public AuthController(ILogger<AuthController> logger, IAuthService authService)
    {
        _logger = logger;
        _authService = authService;
    }
    
    #region HttpGet
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }
        return View(new AuthLoginViewModel { ReturnUrl = returnUrl });
    }
    
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Register(string? returnUrl)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }
        return View(new AuthRegisterViewModel { ReturnUrl = returnUrl });
    }

    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }
    #endregion
    
    #region httpPost
    [HttpPost] [AllowAnonymous] [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(AuthLoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }
        
        var userResult = await _authService.FindUserByEmailOrUsernameAsync(model.EmailOrUsername);
        if (!userResult.Success || userResult.Data == null)
        {
            ModelState.AddModelError(string.Empty, userResult.Message);
            return View(model);
        }

        var user = userResult.Data;
        if (!BCrypt.Net.BCrypt.Verify(model.Password, user.Password))
        {
            ModelState.AddModelError(string.Empty, "Invalid credentials.");
            return View(model);
        }
        
        var authResult = await _authService.AuthenticateAsync(user, model.RememberMe);
        if (!authResult.Success)
        {
            ModelState.AddModelError(string.Empty, authResult.Message);
            return View(model);
        }

        if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
        {
            return Redirect(model.ReturnUrl);
        }
        
        return RedirectToAction("Index", "Home");
    }

    [HttpPost] [AllowAnonymous] [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(AuthRegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }
        
        var result = await _authService.RegisterAsync(model);
        if (!result.Success)
        {
            ModelState.AddModelError(string.Empty, result.Message);
            return View(model);
        }

        if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
        {
            return Redirect(model.ReturnUrl);
        }
        
        return RedirectToAction("Login");
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _authService.LogoutAsync();
        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAccount()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim == null) return RedirectToAction("Login");

        if (!int.TryParse(userIdClaim.Value, out var userId))
        {
            return RedirectToAction("Index", "Home");
        }

        var userResult = await _authService.FindUserByEmailOrUsernameAsync(User.Identity?.Name ?? "");
        if (userResult is not { Success: true, Data: not null }) return RedirectToAction("Register");
        
        await _authService.DeleteUserAsync(userResult.Data);
        await _authService.LogoutAsync();

        return RedirectToAction("Register");
    }
    #endregion
}
