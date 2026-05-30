using Microsoft.EntityFrameworkCore;
using Quitly.Api.Domain.Entities;
using Quitly.Api.Domain.Enums;
using Quitly.Api.Infrastructure.Security;
using Quitly.Api.Persistence;

namespace Quitly.Api.Application.Streaks;

public sealed class StreakService(QuitlyDbContext dbContext, ICurrentUserAccessor currentUserAccessor)
{
    public async Task<Streak> GetOrUpdateAsync(CancellationToken cancellationToken)
    {
        var userId = currentUserAccessor.GetRequiredUserId();
        var checkIns = await dbContext.CheckIns
            .AsNoTracking()
            .Where(item => item.UserId == userId)
            .OrderBy(item => item.Day)
            .ToListAsync(cancellationToken);

        var snapshot = CalculateSnapshot(checkIns);
        var streak = await dbContext.Streaks.SingleOrDefaultAsync(item => item.UserId == userId, cancellationToken);

        if (streak is null)
        {
            streak = new Streak { UserId = userId };
            dbContext.Streaks.Add(streak);
        }

        streak.CurrentStreakDays = snapshot.CurrentStreakDays;
        streak.LastAbstinentDay = snapshot.LastAbstinentDay;
        streak.LastNonAbstinentDay = snapshot.LastNonAbstinentDay;
        streak.UpdatedAt = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        return streak;
    }

    public static StreakSnapshot CalculateSnapshot(IEnumerable<CheckIn> checkIns)
    {
        var ordered = checkIns.OrderBy(item => item.Day).ToList();
        if (ordered.Count == 0)
        {
            return new StreakSnapshot(0, null, null);
        }

        var lastAbstinentDay = ordered.LastOrDefault(item => item.Status == CheckInStatus.Abstinent)?.Day;
        var lastNonAbstinentDay = ordered.LastOrDefault(item => item.Status == CheckInStatus.NonAbstinent)?.Day;
        var latest = ordered[^1];

        if (latest.Status != CheckInStatus.Abstinent)
        {
            return new StreakSnapshot(0, lastAbstinentDay, lastNonAbstinentDay);
        }

        var current = latest.Day;
        var streakDays = 0;

        for (var index = ordered.Count - 1; index >= 0; index--)
        {
            var candidate = ordered[index];
            if (candidate.Day != current || candidate.Status != CheckInStatus.Abstinent)
            {
                break;
            }

            streakDays++;
            current = current.AddDays(-1);
        }

        return new StreakSnapshot(streakDays, lastAbstinentDay, lastNonAbstinentDay);
    }
}

public sealed record StreakSnapshot(int CurrentStreakDays, DateOnly? LastAbstinentDay, DateOnly? LastNonAbstinentDay);
