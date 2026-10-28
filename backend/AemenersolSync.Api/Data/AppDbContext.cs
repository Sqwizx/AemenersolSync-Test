using Microsoft.EntityFrameworkCore;
using AemenersolSync.Api.Models;

namespace AemenersolSync.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Platform> Platforms => Set<Platform>();
    public DbSet<Well> Wells => Set<Well>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Platform>()
            .HasMany(p => p.Wells)
            .WithOne(w => w.Platform)
            .HasForeignKey(w => w.PlatformId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
