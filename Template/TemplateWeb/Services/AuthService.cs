using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using TemplateWeb.Data;
using TemplateWeb.Interfaces;
using TemplateWeb.Models;
using TemplateWeb.Models.DbModels;

namespace TemplateWeb.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly ILogger<AuthService> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;
    
    public AuthService(
        AppDbContext context, ILogger<AuthService> logger, IHttpContextAccessor httpContextAccessor)
    {
        _context = context; 
        _logger = logger; 
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<ServiceResult<UserModel>> FindUserByEmailOrUsernameAsync(string emailOrUsername)
    {
        if (string.IsNullOrWhiteSpace(emailOrUsername))
            return ServiceResult<UserModel>.Failure("Email or username is required.");

        var trimmed = emailOrUsername.Trim().ToLower();
        
        var user = await _context.Users.SingleOrDefaultAsync(u => 
            string.Equals(u.Email, trimmed) || 
            string.Equals(u.UserName, trimmed));

        return user == null ? ServiceResult<UserModel>.Failure("User not found.") : ServiceResult<UserModel>.Ok(user);
    }
    
    public async Task<ServiceResult> AuthenticateAsync(UserModel user, bool rememberMe)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.UserName),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role.ToString()),
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            _logger.LogError("HttpContext is null during authentication.");
            return ServiceResult.Failure("An internal error occurred.");
        }

        await httpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = rememberMe,
                ExpiresUtc = rememberMe
                    ? DateTimeOffset.UtcNow.AddDays(30)
                    : DateTimeOffset.UtcNow.AddHours(8)
            });

        return ServiceResult.Ok();
    }
    
    public async Task<ServiceResult<UserModel>> RegisterAsync(AuthRegisterViewModel viewModel)
    {
        var userNameTrimmed = viewModel.UserName.Trim();
        var emailTrimmed = viewModel.Email.Trim().ToLower();

        var existingUser = await _context.Users.AnyAsync(u => 
            string.Equals(u.UserName, userNameTrimmed) ||
            string.Equals(u.Email, emailTrimmed));

        if (existingUser)
        {
            return ServiceResult<UserModel>.Failure("Username or Email already exists.");
        }
        
        var user = new UserModel
        {
            UserName = userNameTrimmed,
            Email = emailTrimmed,
            Password = BCrypt.Net.BCrypt.HashPassword(viewModel.Password),
            Role = UserRole.User
        };
        
        try
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
            _logger.LogInformation("User {UserName} registered.", user.UserName);
            return ServiceResult<UserModel>.Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering user {UserName}", user.UserName);
            return ServiceResult<UserModel>.Failure("An error occurred while saving the user.");
        }
    }

    public async Task LogoutAsync()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext != null)
        {
            await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            _logger.LogInformation("User logged out.");
        }
    }

    public async Task<ServiceResult> DeleteUserAsync(UserModel user)
    {
        try
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            _logger.LogInformation("User {UserId} deleted.", user.Id);
            return ServiceResult.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user {UserId}", user.Id);
            return ServiceResult.Failure("An error occurred while deleting the user.");
        }
    }
}
