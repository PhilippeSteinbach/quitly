using Microsoft.EntityFrameworkCore;
using Quitly.Api.Domain.Calculation;
using Quitly.Api.Domain.Entities;
using Quitly.Api.Infrastructure.Security;
using Quitly.Api.Persistence;

namespace Quitly.Api.Application.Streaks;

public sealed class StreakService(QuitlyDbContext dbContext, ICurrentUserAccessor currentUserAccessor)
{
    /// <summary>
    /// Returns the current streak for the specified habit, recalculating and persisting
    /// it using UTC-second precision via <see cref="StreakCalculator"/>.
    /// </summary>
    public async Task<StreakDto> GetStreakAsync(Guid habitId, CancellationToken cancellationToken)
    {
        var userId = currentUserAccessor.GetRequiredUserId();

        var habit = await dbContext.Habits
            .AsNoTracking()
            .SingleOrDefaultAsync(h => h.Id == habitId && h.UserId == userId, cancellationToken)
            ?? throw new ArgumentException("Habit not found for current user.");

        var startedAt = habit.StartedAt
            ?? new DateTimeOffset(habit.StartedOn.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);

        var relapseTimes = await dbContext.Relapses
            .AsNoTracking()
            .Where(r => r.HabitId == habitId && r.UserId == userId)
            .Select(r => r.OccurredAt)
            .ToListAsync(cancellationToken);

        var now = DateTimeOffset.UtcNow;
        var currentSeconds = StreakCalculator.CalculateCurrentSeconds(startedAt, relapseTimes, now);
        var serverUtcMs = now.ToUnixTimeMilliseconds();

        // Upsert streak row
        var streak = await dbContext.Streaks
            .SingleOrDefaultAsync(s => s.HabitId == habitId, cancellationToken);

        if (streak is null)
        {
            streak = new Streak { HabitId = habitId };
            dbContext.Streaks.Add(streak);
        }

        streak.CurrentStreakSeconds = currentSeconds;
        streak.LastServerUtcMs = serverUtcMs;
        streak.LastSyncAt = now;

        await dbContext.SaveChangesAsync(cancellationToken);

        return new StreakDto(habitId, currentSeconds, serverUtcMs);
    }
}

/// <summary>Response DTO for the streak endpoint.</summary>
public sealed record StreakDto(Guid HabitId, long CurrentStreakSeconds, long ServerUtcMs);

