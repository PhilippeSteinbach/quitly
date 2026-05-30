using FluentAssertions;
using Quitly.Api.Domain.Calculation;

namespace Quitly.UnitTests;

public sealed class StreakCalculatorTests
{
    private static readonly DateTimeOffset StartedAt = new DateTimeOffset(2026, 1, 15, 9, 30, 0, TimeSpan.Zero);
    private const string Utc = "UTC";
    private const string Berlin = "Europe/Berlin";

    // ── CalculateCurrentSeconds ──────────────────────────────────────────────

    [Fact]
    public void CalculateCurrentSeconds_NoRelapses_ReturnsDeltaFromStart()
    {
        var now = StartedAt.AddSeconds(3600);
        var result = StreakCalculator.CalculateCurrentSeconds(StartedAt, [], now);
        result.Should().Be(3600);
    }

    [Fact]
    public void CalculateCurrentSeconds_WithOneRelapse_ReturnsDeltaFromRelapse()
    {
        var relapse = StartedAt.AddDays(5);
        var now = relapse.AddSeconds(7200);
        var result = StreakCalculator.CalculateCurrentSeconds(StartedAt, [relapse], now);
        result.Should().Be(7200);
    }

    [Fact]
    public void CalculateCurrentSeconds_MultipleRelapses_UsesLatest()
    {
        var r1 = StartedAt.AddDays(1);
        var r2 = StartedAt.AddDays(3);
        var now = r2.AddSeconds(1000);
        var result = StreakCalculator.CalculateCurrentSeconds(StartedAt, [r1, r2], now);
        result.Should().Be(1000);
    }

    [Fact]
    public void CalculateCurrentSeconds_RelapseBeforeStart_Ignored()
    {
        var relapseBefore = StartedAt.AddDays(-1);
        var now = StartedAt.AddSeconds(500);
        var result = StreakCalculator.CalculateCurrentSeconds(StartedAt, [relapseBefore], now);
        result.Should().Be(500);
    }

    [Fact]
    public void CalculateCurrentSeconds_NowEqualsStart_ReturnsZero()
    {
        var result = StreakCalculator.CalculateCurrentSeconds(StartedAt, [], StartedAt);
        result.Should().Be(0);
    }

    // ── CalculateDayStatus ───────────────────────────────────────────────────

    [Fact]
    public void CalculateDayStatus_BeforeStart_ReturnsNeutral()
    {
        var date = new DateOnly(2026, 1, 14); // one day before StartedAt
        var status = StreakCalculator.CalculateDayStatus(date, Utc, StartedAt, []);
        status.Should().Be(StreakCalculator.DayStatus.Neutral);
    }

    [Fact]
    public void CalculateDayStatus_StartDay_NoRelapse_ReturnsAbstinent()
    {
        var date = new DateOnly(2026, 1, 15); // same local date as StartedAt in UTC
        var status = StreakCalculator.CalculateDayStatus(date, Utc, StartedAt, []);
        status.Should().Be(StreakCalculator.DayStatus.Abstinent);
    }

    [Fact]
    public void CalculateDayStatus_DayWithRelapse_ReturnsRelapse()
    {
        var date = new DateOnly(2026, 2, 1);
        var relapse = new DateTimeOffset(2026, 2, 1, 14, 0, 0, TimeSpan.Zero);
        var status = StreakCalculator.CalculateDayStatus(date, Utc, StartedAt, [relapse]);
        status.Should().Be(StreakCalculator.DayStatus.Relapse);
    }

    [Fact]
    public void CalculateDayStatus_RelapseExactlyAtMidnightUtc_BelongsToNewDay()
    {
        // Relapse at 00:00:00 UTC on Feb 1 → belongs to Feb 1, not Jan 31
        var date = new DateOnly(2026, 2, 1);
        var midnight = new DateTimeOffset(2026, 2, 1, 0, 0, 0, TimeSpan.Zero);
        var status = StreakCalculator.CalculateDayStatus(date, Utc, StartedAt, [midnight]);
        status.Should().Be(StreakCalculator.DayStatus.Relapse);

        var dayBefore = new DateOnly(2026, 1, 31);
        var statusBefore = StreakCalculator.CalculateDayStatus(dayBefore, Utc, StartedAt, [midnight]);
        statusBefore.Should().Be(StreakCalculator.DayStatus.Abstinent);
    }

    [Fact]
    public void CalculateDayStatus_MultipleRelapsesSameDay_StillRelapse()
    {
        var date = new DateOnly(2026, 3, 10);
        var r1 = new DateTimeOffset(2026, 3, 10, 8, 0, 0, TimeSpan.Zero);
        var r2 = new DateTimeOffset(2026, 3, 10, 20, 0, 0, TimeSpan.Zero);
        var status = StreakCalculator.CalculateDayStatus(date, Utc, StartedAt, [r1, r2]);
        status.Should().Be(StreakCalculator.DayStatus.Relapse);
    }

    [Fact]
    public void CalculateDayStatus_BerlinTimezone_MidnightUtcMapsToPreviousDay()
    {
        // UTC+2 in summer: 22:00 UTC on June 30 = 00:00 July 1 in Europe/Berlin
        var started = new DateTimeOffset(2026, 6, 1, 0, 0, 0, TimeSpan.Zero);
        var relapseUtc = new DateTimeOffset(2026, 6, 30, 22, 0, 0, TimeSpan.Zero); // 00:00 July 1 Berlin
        var dateJuly1 = new DateOnly(2026, 7, 1);
        var dateJune30 = new DateOnly(2026, 6, 30);
        // Pass today explicitly so future-date guard does not interfere
        var today = new DateOnly(2026, 7, 1);

        var statusJuly1 = StreakCalculator.CalculateDayStatus(dateJuly1, Berlin, started, [relapseUtc], today);
        var statusJune30 = StreakCalculator.CalculateDayStatus(dateJune30, Berlin, started, [relapseUtc], today);

        statusJuly1.Should().Be(StreakCalculator.DayStatus.Relapse);
        statusJune30.Should().Be(StreakCalculator.DayStatus.Abstinent);
    }

    // ── CalculateMonthStats ──────────────────────────────────────────────────

    [Fact]
    public void CalculateMonthStats_FullAbstinentMonth_ReturnsAllDaysAbstinent()
    {
        var started = new DateTimeOffset(2026, 2, 1, 0, 0, 0, TimeSpan.Zero);
        var today = new DateOnly(2026, 2, 28);
        var stats = StreakCalculator.CalculateMonthStats(2026, 2, Utc, started, [], today);
        stats.RelevantDays.Should().Be(28); // Feb 2026 has 28 days
        stats.AbstinentDays.Should().Be(28);
        stats.RelapseCount.Should().Be(0);
    }

    [Fact]
    public void CalculateMonthStats_StartMidMonth_NumeratorCapped()
    {
        // Habit started on May 15; month stats for May 2026
        var started = new DateTimeOffset(2026, 5, 15, 9, 0, 0, TimeSpan.Zero);
        var today = new DateOnly(2026, 5, 31); // completed month
        var stats = StreakCalculator.CalculateMonthStats(2026, 5, Utc, started, [], today);
        stats.RelevantDays.Should().Be(17); // May 15..31 inclusive = 17 days
    }

    [Fact]
    public void CalculateMonthStats_CurrentMonth_DenominatorCappedAtToday()
    {
        var started = new DateTimeOffset(2026, 5, 1, 0, 0, 0, TimeSpan.Zero);
        var today = new DateOnly(2026, 5, 10); // only 10 days have passed
        var stats = StreakCalculator.CalculateMonthStats(2026, 5, Utc, started, [], today);
        stats.RelevantDays.Should().Be(10);
        stats.IsCurrentMonth.Should().BeTrue();
    }

    [Fact]
    public void CalculateMonthStats_BeforeStart_ReturnsZeros()
    {
        var started = new DateTimeOffset(2026, 6, 1, 0, 0, 0, TimeSpan.Zero);
        var today = new DateOnly(2026, 6, 30);
        var stats = StreakCalculator.CalculateMonthStats(2026, 5, Utc, started, [], today);
        stats.RelevantDays.Should().Be(0);
        stats.AbstinentDays.Should().Be(0);
    }

    [Fact]
    public void CalculateMonthStats_LeapYearFeb_Has29Days()
    {
        var started = new DateTimeOffset(2028, 2, 1, 0, 0, 0, TimeSpan.Zero);
        var today = new DateOnly(2028, 2, 29); // 2028 is a leap year
        var stats = StreakCalculator.CalculateMonthStats(2028, 2, Utc, started, [], today);
        stats.RelevantDays.Should().Be(29);
    }

    [Fact]
    public void CalculateMonthStats_WithRelapses_CountsCorrectly()
    {
        var started = new DateTimeOffset(2026, 5, 1, 0, 0, 0, TimeSpan.Zero);
        var today = new DateOnly(2026, 5, 31);
        var relapses = new[]
        {
            new DateTimeOffset(2026, 5, 5, 10, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 5, 10, 10, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 5, 10, 18, 0, 0, TimeSpan.Zero), // same day as above
        };
        var stats = StreakCalculator.CalculateMonthStats(2026, 5, Utc, started, relapses, today);
        stats.RelevantDays.Should().Be(31);
        stats.AbstinentDays.Should().Be(29); // 31 - 2 relapse days
        stats.RelapseCount.Should().Be(3);   // 3 individual relapse events
    }

    // ── CalculateYearStats ───────────────────────────────────────────────────

    [Fact]
    public void CalculateYearStats_Returns12Months()
    {
        var started = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var today = new DateOnly(2026, 12, 31);
        var stats = StreakCalculator.CalculateYearStats(2026, Utc, started, [], today);
        stats.Months.Should().HaveCount(12);
    }

    [Fact]
    public void CalculateYearStats_TotalsAggregateCorrectly()
    {
        var started = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var today = new DateOnly(2026, 12, 31);
        var stats = StreakCalculator.CalculateYearStats(2026, Utc, started, [], today);
        stats.TotalAbstinentDays.Should().Be(stats.Months.Sum(m => m.AbstinentDays));
        stats.TotalRelevantDays.Should().Be(stats.Months.Sum(m => m.RelevantDays));
    }

    // ── DetectManipulation ───────────────────────────────────────────────────

    [Fact]
    public void DetectManipulation_NegativeDeviationAboveThreshold_ReturnsTrue()
    {
        // Device measured 100s offline, server says 500s elapsed → device was behind by 400s (> 5 min)
        var result = StreakCalculator.DetectManipulation(
            offlineDeltaMs: 100_000L,
            serverDeltaMs: 500_000L);
        result.Should().BeTrue();
    }

    [Fact]
    public void DetectManipulation_NegativeDeviationBelowThreshold_ReturnsFalse()
    {
        var result = StreakCalculator.DetectManipulation(
            offlineDeltaMs: 490_000L,
            serverDeltaMs: 500_000L); // only 10s difference
        result.Should().BeFalse();
    }

    [Fact]
    public void DetectManipulation_PositiveDeviation_ReturnsFalse()
    {
        // Device measured MORE time than server (device ran fast) → silent correction, no toast
        var result = StreakCalculator.DetectManipulation(
            offlineDeltaMs: 600_000L,
            serverDeltaMs: 500_000L);
        result.Should().BeFalse();
    }

    [Fact]
    public void DetectManipulation_ExactlyAtThreshold_ReturnsFalse()
    {
        // 300 000 ms difference is not strictly greater than threshold
        var result = StreakCalculator.DetectManipulation(
            offlineDeltaMs: 200_000L,
            serverDeltaMs: 500_000L); // diff = 300_000 exactly
        result.Should().BeFalse();
    }
}

