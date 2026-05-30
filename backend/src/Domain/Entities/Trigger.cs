namespace Quitly.Api.Domain.Entities;

public sealed class Trigger
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Code { get; set; } = string.Empty;

    public string Label { get; set; } = string.Empty;

    public bool Active { get; set; } = true;

    public ICollection<CheckInTrigger> CheckInTriggers { get; set; } = new List<CheckInTrigger>();
}
