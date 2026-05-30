using Microsoft.EntityFrameworkCore;
using Quitly.Api.Domain.Calculation;
using Quitly.Api.Infrastructure.Security;
using Quitly.Api.Persistence;

namespace Quitly.Api.Application.Streaks;

/// <summary>
/// Returns per-day status for a calendar month, with optional decrypted context notes
/// for relapse days. Constitution I (Privacy by Default): context notes are only
/// decrypted on explicit request and scoped per-user.
/// </summary>
public sealed class CalendarService(
    QuitlyDbContext dbContext,
    ICurrentUserAccessor currentUserAccessor,
    FieldEncryptor fieldEncryptor)
{
    public async Task<CalendarMonthDto> GetMonthAsync(
        Guid habitId,
        int year,
        int month,
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

        var relapses = await dbContext.Relapses
            .AsNoTracking()
            .Where(r => r.HabitId == habitId && r.UserId == userId)
            .ToListAsync(cancellationToken);

        var relapseTimes = relapses.Select(r => r.OccurredAt).ToArray();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var days = new List<CalendarDayDto>();
        var daysInMonth = DateTime.DaysInMonth(year, month);

        for (var d = 1; d <= daysInMonth; d++)
        {
            var date = new DateOnly(year, month, d);
            var status = StreakCalculator.CalculateDayStatus(date, timezone, startedAt, relapseTimes, today);

            // Gather decrypted notes for relapse days
            IReadOnlyList<string>? notes = null;
            if (status == StreakCalculator.DayStatus.Relapse)
            {
                var dateStr = date.ToString("yyyy-MM-dd");
                var relapseNotes = relapses
                    .Where(r =>
                    {
                        var tz = TimeZoneInfo.FindSystemTimeZoneById(timezone);
                        var localDate = DateOnly.FromDateTime(
                            TimeZoneInfo.ConvertTimeFromUtc(r.OccurredAt.UtcDateTime, tz));
                        return localDate == date && r.ContextNoteEncrypted is not null;
                    })
                    .Select(r =>
                    {
                        try { return fieldEncryptor.Decrypt(r.ContextNoteEncrypted!, userId); }
                        catch { return null; }
                    })
                    .OfType<string>()
                    .ToArray();

                if (relapseNotes.Length > 0)
                    notes = relapseNotes;
            }

            days.Add(new CalendarDayDto(date, MapStatus(status), notes));
        }

        return new CalendarMonthDto(year, month, days);
    }

    private static string MapStatus(StreakCalculator.DayStatus status) => status switch
    {
        StreakCalculator.DayStatus.Abstinent => "abstinent",
        StreakCalculator.DayStatus.Relapse   => "relapse",
        StreakCalculator.DayStatus.Paused    => "paused",
        _                                    => "neutral",
    };
}

public sealed record CalendarMonthDto(int Year, int Month, IReadOnlyList<CalendarDayDto> Days);
public sealed record CalendarDayDto(DateOnly Date, string Status, IReadOnlyList<string>? Notes);
