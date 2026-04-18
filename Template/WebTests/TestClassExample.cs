using Microsoft.EntityFrameworkCore;
using TemplateWeb.Data;

namespace WebTests;

[TestClass]
public sealed class TestClassExample
{
    private AppDbContext _context; 
    // private Mock<ILogger<ExampleService>> _loggerMock;
    // private ExampleService _exampleService;
    
    [TestInitialize]
    public void Initialize()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new AppDbContext(options);
        // _exampleService = new ExampleService(_context);
        SeedDatabase();
    }
    
    private void SeedDatabase()
    {
        // var user1 = new UserModel { Id = 1, UserName = "User1", Email = "user1@example.com", Password = "password123" };
        // var user2 = new UserModel { Id = 2, UserName = "User2", Email = "user2@example.com", Password = "password123" };
        // var user3 = new UserModel { Id = 3, UserName = "User3", Email = "user3@example.com", Password = "password123" };
        // _context.Users.AddRange(user1, user2, user3);
        _context.SaveChanges();
    }
    
    [TestCleanup]
    public void Cleanup()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
    
    [TestMethod]
    public void ExampleTest()
    {
        Assert.IsTrue(true);
    }
}