using Microsoft.EntityFrameworkCore;
using TaskManagerAPI.Models;

namespace TaskManagerAPI.Data;

/// <summary>
/// The Entity Framework database context for the application.
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// This represents the 'TaskItems' table in our database.
    /// </summary>
    public DbSet<TaskItem> TaskItems { get; set; }
}
