using Quitly.Api.Domain.Enums;

namespace Quitly.Api.Domain.Entities;

public sealed class WeeklyInsight
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }

    public DateOnly WeekStart { get; set; }

    public int CheckInCount { get; set; }

    public int AbstinentDays { get; set; }

    public List<string> TopTriggers { get; set; } = new();

    public Dictionary<string, int> MoodTrend { get; set; } = new();

    public string SummaryText { get; set; } = string.Empty;

    public InsightConfidence Confidence { get; set; } = InsightConfidence.Low;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public User? User { get; set; }
}
