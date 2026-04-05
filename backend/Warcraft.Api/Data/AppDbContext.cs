using Microsoft.EntityFrameworkCore;
using Warcraft.Api.Models;

namespace Warcraft.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Character> Characters => Set<Character>();
    public DbSet<WeeklyTask> WeeklyTasks => Set<WeeklyTask>();
    public DbSet<DailyTask> DailyTasks => Set<DailyTask>();
    public DbSet<ProfessionCooldown> ProfessionCooldowns => Set<ProfessionCooldown>();
    public DbSet<GearSlot> GearSlots => Set<GearSlot>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WeeklyTask>()
            .HasIndex(t => new { t.CharacterId, t.TaskKey, t.WeekStart })
            .IsUnique();

        modelBuilder.Entity<DailyTask>()
            .HasIndex(t => new { t.CharacterId, t.TaskKey, t.DayStart })
            .IsUnique();

        modelBuilder.Entity<ProfessionCooldown>()
            .HasIndex(t => new { t.CharacterId, t.CdKey })
            .IsUnique();
    }
}
