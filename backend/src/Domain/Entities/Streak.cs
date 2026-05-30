namespace Quitly.Api.Domain.Entities;

public sealed class Streak
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Foreign key to Habit. Unique — one streak row per habit.</summary>
    public Guid HabitId { get; set; }

    /// <summary>Current streak length in UTC seconds (from latest relapse or startedAt).</summary>
    public long CurrentStreakSeconds { get; set; }

    /// <summary>Server UTC epoch (ms) sent to clients at last sync — used for monotonic clock reference.</summary>
    public long LastServerUtcMs { get; set; }

    public DateTimeOffset LastSyncAt { get; set; } = DateTimeOffset.UtcNow;

    public Habit? Habit { get; set; }
}
