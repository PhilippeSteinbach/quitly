namespace Quitly.Api.Domain.Entities;

public sealed class Relapse
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }

    public Guid HabitId { get; set; }

    public DateTimeOffset OccurredAt { get; set; }

    public string? ContextNote { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public User? User { get; set; }

    public Habit? Habit { get; set; }

    public ICollection<RecoveryPlanStep> RecoveryPlanSteps { get; set; } = new List<RecoveryPlanStep>();
}
