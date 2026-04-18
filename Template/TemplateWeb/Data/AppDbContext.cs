using Microsoft.EntityFrameworkCore;
using TemplateWeb.Models.DbModels;

namespace TemplateWeb.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }
    
    public DbSet<UserModel> Users { get; set; }
}
