using Microsoft.EntityFrameworkCore;
using test_app.Models;

namespace test_app.Services
{
  public class ApplicationContext : DbContext
  {
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Post> Posts { get; set; } = null!;
    public const string connectionString = "Host=localhost;Port=5433;Database=testtaskdb;Username=postgres;Password=postgres";

    public ApplicationContext()
    : base()
    {
      Database.EnsureCreated();
    }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
      optionsBuilder.UseNpgsql(connectionString);
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
    }
  }
}
