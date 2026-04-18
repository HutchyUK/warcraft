using Microsoft.EntityFrameworkCore;
using Warcraft.Api.Models;

namespace Warcraft.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Character> Characters => Set<Character>();
    public DbSet<WeeklyTask> WeeklyTasks => Set<WeeklyTask>();
    public DbSet<DailyTask> DailyTasks => Set<DailyTask>();
    public DbSet<MythicPlusRun> MythicPlusRuns => Set<MythicPlusRun>();
    public DbSet<ProfessionCooldown> ProfessionCooldowns => Set<ProfessionCooldown>();
    public DbSet<GearSlot> GearSlots => Set<GearSlot>();

    // Content template tables (managed via admin UI, seeded from content.json on first run)
    public DbSet<RaidTemplate> RaidTemplates => Set<RaidTemplate>();
    public DbSet<DungeonTemplate> DungeonTemplates => Set<DungeonTemplate>();
    public DbSet<WeeklyQuestTemplate> WeeklyQuestTemplates => Set<WeeklyQuestTemplate>();
    public DbSet<ProfessionCdTemplate> ProfessionCdTemplates => Set<ProfessionCdTemplate>();

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

        modelBuilder.Entity<MythicPlusRun>()
            .HasIndex(r => new { r.CharacterId, r.WeekStart });

        modelBuilder.Entity<RaidTemplate>()
            .HasIndex(t => t.Key).IsUnique();

        modelBuilder.Entity<DungeonTemplate>()
            .HasIndex(t => t.Key).IsUnique();

        modelBuilder.Entity<WeeklyQuestTemplate>()
            .HasIndex(t => t.Key).IsUnique();

        modelBuilder.Entity<ProfessionCdTemplate>()
            .HasIndex(t => t.Key).IsUnique();
    }
}
