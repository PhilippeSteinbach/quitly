namespace Quitly.Api.Domain.Calculation;

/// <summary>
/// Pure static calculation functions for streak and abstinence logic.
/// No side effects, no DI. Identical behaviour mirrored in frontend/src/lib/streak-calc/index.ts.
/// </summary>
public static class StreakCalculator
{
    // ── Types ────────────────────────────────────────────────────────────────

    public enum DayStatus
    {
        Abstinent,
        Relapse,
        /// <summary>
        /// MVP stub: DayStatus.Paused is never returned until Feature 005 (pause/archive) is implemented.
        /// This value is reserved so callers can compile against it today.
        /// </summary>
        Paused,
        Neutral
    }

    public sealed record MonthStats(
        int AbstinentDays,
        int RelevantDays,
        int RelapseCount,
        bool IsCurrentMonth);

    public sealed record YearStats(
        int TotalAbstinentDays,
        int TotalRelevantDays,
        IReadOnlyList<MonthStats> Months);

    // ── Functions ────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns abstinence duration in UTC seconds since the last relapse (or startedAt if none).
    /// </summary>
    public static long CalculateCurrentSeconds(
        DateTimeOffset startedAt,
        IEnumerable<DateTimeOffset> relapseTimes,
        DateTimeOffset now)
    {
        // Anchor = latest valid relapse or startedAt, whichever is more recent
        var anchor = relapseTimes
            .Where(r => r >= startedAt)
            .Concat([startedAt])
            .Max();

        return Math.Max(0L, (long)(now - anchor).TotalSeconds);
    }

    /// <summary>
    /// Returns the abstinence status of a local calendar date.
    /// Tagesgrenzen are computed in the user's local timezone via TimeZoneInfo.
    /// </summary>
    /// <param name="today">
    /// The local calendar date considered "today". When null, defaults to the
    /// current UTC instant converted to the given timezone — useful in production;
    /// pass an explicit value in tests to avoid coupling to the system clock.
    /// </param>
    public static DayStatus CalculateDayStatus(
        DateOnly date,
        string timezone,
        DateTimeOffset startedAt,
        IEnumerable<DateTimeOffset> relapseTimes,
        DateOnly? today = null)
    {
        var tz = ResolveTimeZone(timezone);
        var startedAtLocal = ToLocalDate(startedAt, tz);

        if (date < startedAtLocal)
            return DayStatus.Neutral;

        var todayLocal = today ?? ToLocalDate(DateTimeOffset.UtcNow, tz);
        if (date > todayLocal)
            return DayStatus.Neutral;

        // Paused: not implemented in MVP (Feature 005 not yet available)
        // DayStatus.Paused is never returned for MVP inputs

        var hasRelapse = relapseTimes.Any(r => ToLocalDate(r, tz) == date);
        return hasRelapse ? DayStatus.Relapse : DayStatus.Abstinent;
    }

    /// <summary>
    /// Returns monthly abstinence statistics.
    /// Denominator for the current month is capped to today (not full month length).
    /// </summary>
    public static MonthStats CalculateMonthStats(
        int year,
        int month,
        string timezone,
        DateTimeOffset startedAt,
        IEnumerable<DateTimeOffset> relapseTimes,
        DateOnly today)
    {
        var tz = ResolveTimeZone(timezone);
        var relapsesArr = relapseTimes.ToArray();

        var monthFirst = new DateOnly(year, month, 1);
        var monthLast = new DateOnly(year, month, DateTime.DaysInMonth(year, month));

        var startedAtLocal = ToLocalDate(startedAt, tz);
        var firstRelevant = startedAtLocal > monthFirst ? startedAtLocal : monthFirst;

        var isCurrentMonth = today.Year == year && today.Month == month;
        var lastRelevant = (isCurrentMonth && today < monthLast) ? today : monthLast;

        if (firstRelevant > lastRelevant)
            return new MonthStats(0, 0, 0, isCurrentMonth);

        var abstinentDays = 0;
        var relevantDays = 0;
        var d = firstRelevant;

        while (d <= lastRelevant)
        {
            relevantDays++;
            var status = CalculateDayStatus(d, timezone, startedAt, relapsesArr, today);
            if (status == DayStatus.Abstinent)
                abstinentDays++;
            d = d.AddDays(1);
        }

        var relapseCount = relapsesArr
            .Count(r => ToLocalDate(r, tz) >= firstRelevant && ToLocalDate(r, tz) <= lastRelevant);

        return new MonthStats(abstinentDays, relevantDays, relapseCount, isCurrentMonth);
    }

    /// <summary>
    /// Returns yearly statistics — one MonthStats per calendar month (months 1–12).
    /// </summary>
    public static YearStats CalculateYearStats(
        int year,
        string timezone,
        DateTimeOffset startedAt,
        IEnumerable<DateTimeOffset> relapseTimes,
        DateOnly today)
    {
        var relapsesArr = relapseTimes.ToArray();
        var months = Enumerable.Range(1, 12)
            .Select(m => CalculateMonthStats(year, m, timezone, startedAt, relapsesArr, today))
            .ToArray();

        return new YearStats(
            TotalAbstinentDays: months.Sum(m => m.AbstinentDays),
            TotalRelevantDays: months.Sum(m => m.RelevantDays),
            Months: months);
    }

    /// <summary>
    /// Returns true when the device clock appears to have been set back.
    /// Only negative deviation (offlineDeltaMs &lt; serverDeltaMs) exceeding the 5-minute
    /// threshold (300 000 ms) triggers a correction toast (Constitution II — non-shaming).
    /// Positive deviation (device ran fast) is silently corrected — no user notification.
    /// </summary>
    public static bool DetectManipulation(long offlineDeltaMs, long serverDeltaMs)
        => serverDeltaMs - offlineDeltaMs > 300_000L;

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static TimeZoneInfo ResolveTimeZone(string timezone)
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById(timezone);
        }
        catch (TimeZoneNotFoundException)
        {
            // Fall back to UTC for unknown timezone identifiers
            return TimeZoneInfo.Utc;
        }
    }

    private static DateOnly ToLocalDate(DateTimeOffset utc, TimeZoneInfo tz)
        => DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(utc.UtcDateTime, tz));
}
