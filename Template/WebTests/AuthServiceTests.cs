using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TemplateWeb.Data;
using TemplateWeb.Entities;

namespace WebTests;

[TestClass]
public class AuthServiceTests
{
    private ServiceProvider _serviceProvider = null!;
    private UserManager<UserEntity> _userManager = null!;

    [TestInitialize]
    public void Initialize()
    {
        var services = new ServiceCollection();

        services.AddLogging();

        services.AddDbContext<AppDbContext>(options =>
            options.UseInMemoryDatabase(Guid.NewGuid().ToString()));

        services.AddIdentity<UserEntity, IdentityRole>()
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

        _serviceProvider = services.BuildServiceProvider();
        _userManager = _serviceProvider.GetRequiredService<UserManager<UserEntity>>();
    }

    [TestCleanup]
    public void Cleanup()
    {
        _serviceProvider.Dispose();
    }

    [TestMethod]
    public async Task CreateAsync_ShouldReturnSuccess_WhenUserIsNew()
    {
        // Arrange
        var user = new UserEntity { UserName = "testuser", Email = "test@example.com" };

        // Act
        var result = await _userManager.CreateAsync(user, "Password123!");

        // Assert
        Assert.IsTrue(result.Succeeded);

        var userInDb = await _userManager.FindByNameAsync("testuser");
        Assert.IsNotNull(userInDb);
        Assert.AreEqual("test@example.com", userInDb.Email);
    }

    [TestMethod]
    public async Task CreateAsync_ShouldReturnFailure_WhenUsernameAlreadyExists()
    {
        // Arrange
        var existing = new UserEntity { UserName = "existing", Email = "existing@example.com" };
        await _userManager.CreateAsync(existing, "Password123!");

        var duplicate = new UserEntity { UserName = "existing", Email = "other@example.com" };

        // Act
        var result = await _userManager.CreateAsync(duplicate, "Password123!");

        // Assert
        Assert.IsFalse(result.Succeeded);
        Assert.IsTrue(result.Errors.Any(e => e.Code == "DuplicateUserName"));
    }

    [TestMethod]
    public async Task FindByNameAsync_ShouldReturnUser_CaseInsensitive()
    {
        // Arrange
        var user = new UserEntity { UserName = "testuser", Email = "test@example.com" };
        await _userManager.CreateAsync(user, "Password123!");

        // Act — Identity normalizes usernames, so lookup is always case-insensitive
        var found = await _userManager.FindByNameAsync("TESTUSER");

        // Assert
        Assert.IsNotNull(found);
        Assert.AreEqual("testuser", found.UserName);
    }

    [TestMethod]
    public async Task FindByNameAsync_ShouldReturnNull_WhenUserNotFound()
    {
        // Act
        var result = await _userManager.FindByNameAsync("nonexistent");

        // Assert
        Assert.IsNull(result);
    }
}
