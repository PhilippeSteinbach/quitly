namespace Quitly.Api.Domain.Entities;

public sealed class Streak
{
    public Guid UserId { get; set; }

    public int CurrentStreakDays { get; set; }

    public DateOnly? LastAbstinentDay { get; set; }

    public DateOnly? LastNonAbstinentDay { get; set; }

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public User? User { get; set; }
}
