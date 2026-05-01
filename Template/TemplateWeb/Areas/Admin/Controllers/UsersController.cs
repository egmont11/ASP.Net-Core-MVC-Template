using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TemplateWeb.Entities;
using TemplateWeb.Models;

namespace TemplateWeb.Areas.Admin.Controllers;

public class UsersController : AdminBaseController
{
    private readonly UserManager<UserEntity> _userManager;

    public UsersController(UserManager<UserEntity> userManager)
    {
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        var users = _userManager.Users.ToList();
        var viewModels = new List<UserWithRolesViewModel>();
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            viewModels.Add(new UserWithRolesViewModel { UserEntity = user, Roles = roles });
        }
        return View(viewModels);
    }

    public async Task<IActionResult> Details(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        var roles = await _userManager.GetRolesAsync(user);
        return View(new UserWithRolesViewModel { UserEntity = user, Roles = roles });
    }
}
