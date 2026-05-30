using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Quitly.Api.Domain.Entities;
using System.Text.Json;

namespace Quitly.Api.Persistence;

public sealed class QuitlyDbContext(DbContextOptions<QuitlyDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    public DbSet<Habit> Habits => Set<Habit>();

    public DbSet<CheckIn> CheckIns => Set<CheckIn>();

    public DbSet<Trigger> Triggers => Set<Trigger>();

    public DbSet<CheckInTrigger> CheckInTriggers => Set<CheckInTrigger>();

    public DbSet<Streak> Streaks => Set<Streak>();

    public DbSet<Reminder> Reminders => Set<Reminder>();

    public DbSet<WeeklyInsight> WeeklyInsights => Set<WeeklyInsight>();

    public DbSet<Relapse> Relapses => Set<Relapse>();

    public DbSet<RecoveryPlanStep> RecoveryPlanSteps => Set<RecoveryPlanStep>();

    public DbSet<Intervention> Interventions => Set<Intervention>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var stringListConverter = new ValueConverter<List<string>, string>(
            value => JsonSerializer.Serialize(value, (JsonSerializerOptions?)null),
            value => JsonSerializer.Deserialize<List<string>>(value, (JsonSerializerOptions?)null) ?? new List<string>());

        var dictionaryConverter = new ValueConverter<Dictionary<string, int>, string>(
            value => JsonSerializer.Serialize(value, (JsonSerializerOptions?)null),
            value => JsonSerializer.Deserialize<Dictionary<string, int>>(value, (JsonSerializerOptions?)null) ?? new Dictionary<string, int>());

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(item => item.Id);
            entity.HasIndex(item => item.Email).IsUnique();
            entity.Property(item => item.Email).HasMaxLength(320);
            entity.Property(item => item.PasswordHash).HasMaxLength(512);
            entity.Property(item => item.Timezone).HasMaxLength(120);
            entity.Property(item => item.Locale).HasMaxLength(20);
        });

        modelBuilder.Entity<Habit>(entity =>
        {
            entity.ToTable("habits");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.Title).HasMaxLength(120);
            entity.HasOne(item => item.User)
                .WithMany(item => item.Habits)
                .HasForeignKey(item => item.UserId);
            entity.HasIndex(item => new { item.UserId, item.Active })
                .HasFilter("\"Active\" = true")
                .IsUnique();
        });

        modelBuilder.Entity<CheckIn>(entity =>
        {
            entity.ToTable("check_ins");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.Note).HasMaxLength(500);
            entity.HasIndex(item => new { item.UserId, item.Day }).IsUnique();
            entity.HasOne(item => item.User)
                .WithMany(item => item.CheckIns)
                .HasForeignKey(item => item.UserId);
            entity.HasOne(item => item.Habit)
                .WithMany(item => item.CheckIns)
                .HasForeignKey(item => item.HabitId);
        });

        modelBuilder.Entity<Trigger>(entity =>
        {
            entity.ToTable("triggers");
            entity.HasKey(item => item.Id);
            entity.HasIndex(item => item.Code).IsUnique();
            entity.Property(item => item.Code).HasMaxLength(80);
            entity.Property(item => item.Label).HasMaxLength(120);
        });

        modelBuilder.Entity<CheckInTrigger>(entity =>
        {
            entity.ToTable("check_in_triggers");
            entity.HasKey(item => new { item.CheckInId, item.TriggerId });
            entity.HasOne(item => item.CheckIn)
                .WithMany(item => item.CheckInTriggers)
                .HasForeignKey(item => item.CheckInId);
            entity.HasOne(item => item.Trigger)
                .WithMany(item => item.CheckInTriggers)
                .HasForeignKey(item => item.TriggerId);
        });

        modelBuilder.Entity<Streak>(entity =>
        {
            entity.ToTable("streaks");
            entity.HasKey(item => item.UserId);
            entity.HasOne(item => item.User)
                .WithOne(item => item.Streak)
                .HasForeignKey<Streak>(item => item.UserId);
        });

        modelBuilder.Entity<Reminder>(entity =>
        {
            entity.ToTable("reminders");
            entity.HasKey(item => item.UserId);
            entity.HasOne(item => item.User)
                .WithOne(item => item.Reminder)
                .HasForeignKey<Reminder>(item => item.UserId);
        });

        modelBuilder.Entity<WeeklyInsight>(entity =>
        {
            entity.ToTable("weekly_insights");
            entity.HasKey(item => item.Id);
            entity.HasIndex(item => new { item.UserId, item.WeekStart }).IsUnique();
            entity.Property(item => item.TopTriggers).HasConversion(stringListConverter).HasColumnType("jsonb");
            entity.Property(item => item.MoodTrend).HasConversion(dictionaryConverter).HasColumnType("jsonb");
            entity.Property(item => item.SummaryText).HasMaxLength(1000);
        });

        modelBuilder.Entity<Relapse>(entity =>
        {
            entity.ToTable("relapses");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.ContextNote).HasMaxLength(500);
            entity.HasOne(item => item.User).WithMany().HasForeignKey(item => item.UserId);
            entity.HasOne(item => item.Habit).WithMany(item => item.Relapses).HasForeignKey(item => item.HabitId);
        });

        modelBuilder.Entity<RecoveryPlanStep>(entity =>
        {
            entity.ToTable("recovery_plan_steps");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.StepText).HasMaxLength(300);
            entity.HasOne(item => item.Relapse)
                .WithMany(item => item.RecoveryPlanSteps)
                .HasForeignKey(item => item.RelapseId);
            entity.HasOne(item => item.User)
                .WithMany()
                .HasForeignKey(item => item.UserId);
        });

        modelBuilder.Entity<Intervention>(entity =>
        {
            entity.ToTable("interventions");
            entity.HasKey(item => item.Id);
            entity.Property(item => item.Kind).HasMaxLength(60);
            entity.Property(item => item.Payload).HasColumnType("jsonb");
            entity.HasIndex(item => new { item.UserId, item.Kind });
        });
    }
}
