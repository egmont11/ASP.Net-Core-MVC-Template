using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using TemplateWeb.Data;
using TemplateWeb.Models;
using TemplateWeb.Models.DbModels;
using TemplateWeb.Services;

namespace WebTests;

[TestClass]
public class AuthServiceTests
{
    private AppDbContext _context = null!;
    private Mock<ILogger<AuthService>> _loggerMock = null!;
    private Mock<IHttpContextAccessor> _httpContextAccessorMock = null!;
    private AuthService _authService = null!;

    [TestInitialize]
    public void Initialize()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);
        _loggerMock = new Mock<ILogger<AuthService>>();
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _authService = new AuthService(_context, _loggerMock.Object, _httpContextAccessorMock.Object);
    }

    [TestCleanup]
    public void Cleanup()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [TestMethod]
    public async Task RegisterAsync_ShouldReturnSuccess_WhenUserIsNew()
    {
        // Arrange
        var model = new AuthRegisterViewModel
        {
            UserName = "testuser",
            Email = "test@example.com",
            Password = "password123",
            ConfirmPassword = "password123"
        };

        // Act
        var result = await _authService.RegisterAsync(model);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.IsNotNull(result.Data);
        Assert.AreEqual("testuser", result.Data.UserName);
        Assert.AreEqual("test@example.com", result.Data.Email);
        
        var userInDb = await _context.Users.FirstOrDefaultAsync(u => u.UserName == "testuser");
        Assert.IsNotNull(userInDb);
    }

    [TestMethod]
    public async Task RegisterAsync_ShouldReturnFailure_WhenUserAlreadyExists()
    {
        // Arrange
        var existingUser = new UserModel { UserName = "existing", Email = "existing@example.com", Password = "hashedpassword" };
        _context.Users.Add(existingUser);
        await _context.SaveChangesAsync();

        var model = new AuthRegisterViewModel
        {
            UserName = "existing",
            Email = "new@example.com",
            Password = "password123",
            ConfirmPassword = "password123"
        };

        // Act
        var result = await _authService.RegisterAsync(model);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual("Username or Email already exists.", result.Message);
    }

    [TestMethod]
    public async Task FindUserByEmailOrUsernameAsync_ShouldReturnUser_WhenUsernameMatches()
    {
        // Arrange
        var user = new UserModel { UserName = "testuser", Email = "test@example.com", Password = "password" };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _authService.FindUserByEmailOrUsernameAsync("TESTUSER");

        // Assert
        Assert.IsTrue(result.Success);
        Assert.IsNotNull(result.Data);
        Assert.AreEqual("testuser", result.Data.UserName);
    }

    [TestMethod]
    public async Task FindUserByEmailOrUsernameAsync_ShouldReturnFailure_WhenUserNotFound()
    {
        // Act
        var result = await _authService.FindUserByEmailOrUsernameAsync("nonexistent");

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual("User not found.", result.Message);
    }
}
