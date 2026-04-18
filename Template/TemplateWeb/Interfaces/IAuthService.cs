using TemplateWeb.Models;
using TemplateWeb.Models.DbModels;

namespace TemplateWeb.Interfaces;

public interface IAuthService
{
    Task<ServiceResult<UserModel>> FindUserByEmailOrUsernameAsync(string emailOrUsername);
    Task<ServiceResult> AuthenticateAsync(UserModel user, bool rememberMe);
    Task<ServiceResult<UserModel>> RegisterAsync(AuthRegisterViewModel model);
    Task LogoutAsync();
    Task<ServiceResult> DeleteUserAsync(UserModel user);
}
