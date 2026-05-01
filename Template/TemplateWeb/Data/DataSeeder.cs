using Microsoft.AspNetCore.Identity;
using TemplateWeb.Entities;

namespace TemplateWeb.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(UserManager<UserEntity> userManager, RoleManager<IdentityRole> roleManager)
    {
        // Ensure roles exist
        foreach (var role in new[] { "Admin", "User" })
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        // Seed initial admin user if none exists
        if (!userManager.Users.Any())
        {
            var admin = new UserEntity
            {
                UserName = "admin",
                Email = "admin@template.com",
                EmailConfirmed = true,
            };

            // You should change this password on first login
            var result = await userManager.CreateAsync(admin, "Admin123!");
            if (result.Succeeded)
                await userManager.AddToRoleAsync(admin, "Admin");
        }
    }
}
