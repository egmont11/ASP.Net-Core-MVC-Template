using Microsoft.EntityFrameworkCore;
using TemplateWeb.Models.DbModels;

namespace TemplateWeb.Data;

public static class DataSeeder
{
    public static async Task SeedAdminUser(AppDbContext context)
    {
        // Only run if no users exist
        if (await context.Users.AnyAsync())
        {
            return;
        }

        var admin = new UserModel
        {
            UserName = "admin",
            Email = "admin@template.com",
            Password = BCrypt.Net.BCrypt.HashPassword("Admin123!"), // You should change this on first login
            Role = UserRole.Admin
        };

        await context.Users.AddAsync(admin);
        await context.SaveChangesAsync();
    }
}
