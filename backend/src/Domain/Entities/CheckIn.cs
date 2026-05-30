using Quitly.Api.Domain.Enums;

namespace Quitly.Api.Domain.Entities;

public sealed class CheckIn
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }

    public Guid HabitId { get; set; }

    public DateOnly Day { get; set; }

    public CheckInStatus Status { get; set; }

    public MoodLevel? Mood { get; set; }

    public string? Note { get; set; }

    public CheckInSource Source { get; set; } = CheckInSource.Manual;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public User? User { get; set; }

    public Habit? Habit { get; set; }

    public ICollection<CheckInTrigger> CheckInTriggers { get; set; } = new List<CheckInTrigger>();
}
