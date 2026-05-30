namespace Quitly.Api.Domain.Entities;

public sealed class Intervention
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }

    public Guid? HabitId { get; set; }

    public string Kind { get; set; } = string.Empty;

    public string Payload { get; set; } = "{}";

    public bool Enabled { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
