using Quitly.Api.Domain.Enums;

namespace Quitly.Api.Domain.Entities;

public sealed class Reminder
{
    public Guid UserId { get; set; }

    public bool PassivePromptEnabled { get; set; } = true;

    public PromptTone PromptTone { get; set; } = PromptTone.Gentle;

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public User? User { get; set; }
}
