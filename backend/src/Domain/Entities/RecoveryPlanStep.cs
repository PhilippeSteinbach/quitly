namespace Quitly.Api.Domain.Entities;

public sealed class RecoveryPlanStep
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid RelapseId { get; set; }

    public Guid UserId { get; set; }

    public string StepText { get; set; } = string.Empty;

    public int DueWithinHours { get; set; } = 24;

    public DateTimeOffset? CompletedAt { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public Relapse? Relapse { get; set; }

    public User? User { get; set; }
}
