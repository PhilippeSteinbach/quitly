using Microsoft.EntityFrameworkCore;
using Quitly.Api.Domain.Entities;
using Quitly.Api.Domain.Enums;
using Quitly.Api.Infrastructure.Security;
using Quitly.Api.Persistence;

namespace Quitly.Api.Application.Insights;

public sealed class WeeklyInsightService(QuitlyDbContext dbContext, ICurrentUserAccessor currentUserAccessor)
{
    public async Task<WeeklyInsight> GetOrGenerateAsync(DateOnly? weekStart, CancellationToken cancellationToken)
    {
        var userId = currentUserAccessor.GetRequiredUserId();
        var targetWeekStart = weekStart ?? StartOfWeek(DateOnly.FromDateTime(DateTime.UtcNow));

        var existing = await dbContext.WeeklyInsights.SingleOrDefaultAsync(
            item => item.UserId == userId && item.WeekStart == targetWeekStart,
            cancellationToken);

        var end = targetWeekStart.AddDays(6);
        var checkIns = await dbContext.CheckIns
            .AsNoTracking()
            .Include(item => item.CheckInTriggers)
            .ThenInclude(item => item.Trigger)
            .Where(item => item.UserId == userId && item.Day >= targetWeekStart && item.Day <= end)
            .ToListAsync(cancellationToken);

        var insight = existing ?? new WeeklyInsight { UserId = userId, WeekStart = targetWeekStart };
        insight.CheckInCount = checkIns.Count;
        insight.AbstinentDays = checkIns.Count(item => item.Status == CheckInStatus.Abstinent);
        insight.TopTriggers = checkIns
            .SelectMany(item => item.CheckInTriggers)
            .Select(item => item.Trigger?.Code)
            .Where(code => !string.IsNullOrWhiteSpace(code))
            .GroupBy(code => code!)
            .OrderByDescending(group => group.Count())
            .Take(3)
            .Select(group => group.Key)
            .ToList();
        insight.MoodTrend = checkIns
            .Where(item => item.Mood is not null)
            .GroupBy(item => item.Mood!.Value.ToString().ToLowerInvariant())
            .ToDictionary(group => group.Key, group => group.Count());
        insight.Confidence = CalculateConfidence(checkIns.Count);
        insight.SummaryText = CreateSummary(insight);
        insight.CreatedAt = DateTimeOffset.UtcNow;

        if (existing is null)
        {
            dbContext.WeeklyInsights.Add(insight);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return insight;
    }

    public static InsightConfidence CalculateConfidence(int checkInCount) => checkInCount switch
    {
        >= 5 => InsightConfidence.High,
        >= 3 => InsightConfidence.Medium,
        _ => InsightConfidence.Low
    };

    private static string CreateSummary(WeeklyInsight insight)
    {
        if (insight.CheckInCount == 0)
        {
            return "No check-ins yet this week. One short entry is enough to restart the pattern view.";
        }

        var triggerText = insight.TopTriggers.Count > 0
            ? $" Frequent triggers: {string.Join(", ", insight.TopTriggers)}."
            : string.Empty;

        return $"You logged {insight.CheckInCount} check-ins and {insight.AbstinentDays} abstinent days this week.{triggerText}";
    }

    private static DateOnly StartOfWeek(DateOnly date)
    {
        var current = date;
        while (current.DayOfWeek != DayOfWeek.Monday)
        {
            current = current.AddDays(-1);
        }

        return current;
    }
}
