using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TemplateWeb.Entities;
using TemplateWeb.Models;

namespace TemplateWeb.Areas.Admin.Controllers;

public class UsersController : AdminBaseController
{
    private readonly UserManager<UserEntity> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public UsersController(UserManager<UserEntity> userManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<IActionResult> Index()
    {
        var allRoles = _roleManager.Roles.Select(r => r.Name!).ToList();
        var users = _userManager.Users.ToList();
        var viewModels = new List<UserWithRolesViewModel>();
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            viewModels.Add(new UserWithRolesViewModel { UserEntity = user, Roles = roles, AllRoles = allRoles });
        }
        return View(viewModels);
    }

    public async Task<IActionResult> Details(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        var roles = await _userManager.GetRolesAsync(user);
        var allRoles = _roleManager.Roles.Select(r => r.Name!).ToList();
        return View(new UserWithRolesViewModel { UserEntity = user, Roles = roles, AllRoles = allRoles });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateRoles(string id, List<string> selectedRoles)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        var currentRoles = await _userManager.GetRolesAsync(user);
        await _userManager.RemoveFromRolesAsync(user, currentRoles);
        if (selectedRoles.Count > 0)
            await _userManager.AddToRolesAsync(user, selectedRoles);

        TempData["SuccessMessage"] = "Roles updated successfully.";
        return RedirectToAction(nameof(Details), new { id });
    }
}
