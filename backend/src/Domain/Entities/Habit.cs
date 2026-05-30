using Quitly.Api.Domain.Enums;

namespace Quitly.Api.Domain.Entities;

public sealed class Habit
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }

    public HabitCategory Category { get; set; }

    public HabitMode Mode { get; set; }

    public string Title { get; set; } = string.Empty;

    public bool Active { get; set; } = true;

    public DateOnly StartedOn { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public User? User { get; set; }

    public ICollection<CheckIn> CheckIns { get; set; } = new List<CheckIn>();

    public ICollection<Relapse> Relapses { get; set; } = new List<Relapse>();
}
