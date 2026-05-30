namespace Quitly.Api.Domain.Entities;

public sealed class User
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public string Timezone { get; set; } = "UTC";

    public string Locale { get; set; } = "en";

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public ICollection<Habit> Habits { get; set; } = new List<Habit>();

    public ICollection<CheckIn> CheckIns { get; set; } = new List<CheckIn>();

    public Reminder? Reminder { get; set; }
}
