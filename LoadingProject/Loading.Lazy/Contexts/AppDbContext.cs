
using Loading.Lazy.Entities;
using Microsoft.EntityFrameworkCore;

namespace Loading.Lazy.Contexts;
public class AppDbContext:DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options):base(options)
    {
        
    }

    public DbSet<Student> Students { get; set; }
    public DbSet<Group> Groups { get; set; }
}
