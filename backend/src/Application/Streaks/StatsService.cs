using Microsoft.EntityFrameworkCore;
using Quitly.Api.Domain.Calculation;
using Quitly.Api.Infrastructure.Security;
using Quitly.Api.Persistence;

namespace Quitly.Api.Application.Streaks;

/// <summary>
/// Delegates month and year statistics to the pure <see cref="StreakCalculator"/> functions.
/// </summary>
public sealed class StatsService(QuitlyDbContext dbContext, ICurrentUserAccessor currentUserAccessor)
{
    public async Task<MonthStatsDto> GetMonthStatsAsync(
        Guid habitId,
        int year,
        int month,
        CancellationToken cancellationToken)
    {
        var (startedAt, relapseTimes, timezone) = await LoadContextAsync(habitId, cancellationToken);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var stats = StreakCalculator.CalculateMonthStats(year, month, timezone, startedAt, relapseTimes, today);
        return new MonthStatsDto(year, month, stats.AbstinentDays, stats.RelevantDays, stats.RelapseCount, stats.IsCurrentMonth);
    }

    public async Task<YearStatsDto> GetYearStatsAsync(
        Guid habitId,
        int year,
        CancellationToken cancellationToken)
    {
        var (startedAt, relapseTimes, timezone) = await LoadContextAsync(habitId, cancellationToken);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var stats = StreakCalculator.CalculateYearStats(year, timezone, startedAt, relapseTimes, today);
        var months = stats.Months.Select((m, i) =>
            new MonthStatsDto(year, i + 1, m.AbstinentDays, m.RelevantDays, m.RelapseCount, m.IsCurrentMonth)).ToArray();
        return new YearStatsDto(year, stats.TotalAbstinentDays, stats.TotalRelevantDays, months);
    }

    private async Task<(DateTimeOffset startedAt, IReadOnlyList<DateTimeOffset> relapseTimes, string timezone)> LoadContextAsync(
        Guid habitId,
        CancellationToken cancellationToken)
    {
        var userId = currentUserAccessor.GetRequiredUserId();

        var habit = await dbContext.Habits
            .AsNoTracking()
            .SingleOrDefaultAsync(h => h.Id == habitId && h.UserId == userId, cancellationToken)
            ?? throw new ArgumentException("Habit not found.");

        var startedAt = habit.StartedAt
            ?? new DateTimeOffset(habit.StartedOn.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);

        var timezone = (await dbContext.Users
            .AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => u.Timezone)
            .SingleAsync(cancellationToken)) ?? "UTC";

        var relapseTimes = await dbContext.Relapses
            .AsNoTracking()
            .Where(r => r.HabitId == habitId && r.UserId == userId)
            .Select(r => r.OccurredAt)
            .ToListAsync(cancellationToken);

        return (startedAt, relapseTimes, timezone);
    }
}

public sealed record MonthStatsDto(int Year, int Month, int AbstinentDays, int RelevantDays, int RelapseCount, bool IsCurrentMonth);
public sealed record YearStatsDto(int Year, int TotalAbstinentDays, int TotalRelevantDays, IReadOnlyList<MonthStatsDto> Months);
