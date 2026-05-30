namespace Quitly.Api.Domain.Entities;

public sealed class CheckInTrigger
{
    public Guid CheckInId { get; set; }

    public Guid TriggerId { get; set; }

    public CheckIn? CheckIn { get; set; }

    public Trigger? Trigger { get; set; }
}
