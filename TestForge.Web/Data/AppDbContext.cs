using Microsoft.EntityFrameworkCore;
using TestForge.Web.Models;

namespace TestForge.Web.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options)
    : DbContext(options)
{
    public DbSet<TaskItem> Tasks => Set<TaskItem>();
}
