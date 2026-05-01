using dotenv.net;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TemplateWeb.Data;
using TemplateWeb.Entities;

namespace TemplateWeb;

public class Program
{
    public static void Main(string[] args)
    {
        DotEnv.Load();
        var builder = WebApplication.CreateBuilder(args);

        // gets the connection string from configuration
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                               ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));

        builder.Services.AddControllersWithViews();

        // Configure Identity — password requirements are read from appsettings.json "Identity:Password"
        var pwSection = builder.Configuration.GetSection("Identity:Password");
        builder.Services.AddIdentity<UserEntity, IdentityRole>(options =>
            {
                options.Password.RequiredLength         = pwSection.GetValue<int>("RequiredLength", 6);
                options.Password.RequireDigit           = pwSection.GetValue<bool>("RequireDigit", true);
                options.Password.RequireLowercase       = pwSection.GetValue<bool>("RequireLowercase", true);
                options.Password.RequireUppercase       = pwSection.GetValue<bool>("RequireUppercase", true);
                options.Password.RequireNonAlphanumeric = pwSection.GetValue<bool>("RequireNonAlphanumeric", false);
                options.Password.RequiredUniqueChars    = pwSection.GetValue<int>("RequiredUniqueChars", 1);
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

        builder.Services.ConfigureApplicationCookie(options =>
        {
            options.LoginPath = "/Auth/Login";
            options.AccessDeniedPath = "/Auth/AccessDenied";
            options.ExpireTimeSpan = TimeSpan.FromHours(2);
        });

        var app = builder.Build();

        // Apply migrations and seed on startup
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            if (db.Database.GetPendingMigrations().Any())
            {
                db.Database.Migrate();
            }

            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<UserEntity>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            DataSeeder.SeedAsync(userManager, roleManager).GetAwaiter().GetResult();
        }

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
        }

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapStaticAssets();
        app.MapControllerRoute(
            name: "areas",
            pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

        app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
            .WithStaticAssets();

        app.Run();
    }
}
