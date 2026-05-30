using FsCheck;
using FsCheck.Fluent;
using FsCheck.Xunit;
using FluentAssertions;
using Quitly.Api.Domain.Calculation;

namespace Quitly.UnitTests;

/// <summary>
/// Property-based tests for StreakCalculator using FsCheck (SC-002, SC-003, SC-008, FR-018).
/// These tests generate random inputs to verify invariant properties hold across all scenarios.
/// </summary>
public sealed class StreakCalculatorPropertyTests
{
    private static readonly DateTimeOffset BaseStart = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
    private const string Utc = "UTC";

    // ── Property 1: CalculateCurrentSeconds is never negative ────────────────

    [Property(MaxTest = 300)]
    public Property CalculateCurrentSeconds_NeverNegative()
    {
        var arbPair = ArbStartAndNow();
        return Prop.ForAll(arbPair, pair =>
        {
            var (startedAt, now) = pair;
            var result = StreakCalculator.CalculateCurrentSeconds(startedAt, [], now);
            return result >= 0;
        });
    }

    // ── Property 2: Adding an offset to every timestamp preserves seconds ────

    [Property(MaxTest = 200)]
    public Property CalculateCurrentSeconds_UniformTimeShift_PreservesSeconds()
    {
        var arbPair = ArbStartAndNow();
        var arbShift = ArbShiftSeconds();
        return Prop.ForAll(Gen.Zip(arbPair.Generator, arbShift.Generator).ToArbitrary(), item =>
        {
            var ((startedAt, now), shiftSec) = item;
            var shift = TimeSpan.FromSeconds(shiftSec);
            var relapseTimes = Array.Empty<DateTimeOffset>();

            var baseline = StreakCalculator.CalculateCurrentSeconds(startedAt, relapseTimes, now);
            var shifted  = StreakCalculator.CalculateCurrentSeconds(
                startedAt + shift, relapseTimes, now + shift);

            return baseline == shifted;
        });
    }

    // ── Property 3: Adding a relapse at midpoint reduces streak ──────────────

    [Property(MaxTest = 200)]
    public Property AddingRelapse_NeverIncreasesStreak()
    {
        var arbPair = ArbStartAndNow();
        return Prop.ForAll(arbPair, pair =>
        {
            var (startedAt, now) = pair;
            if (now <= startedAt.AddSeconds(2)) return true; // skip degenerate

            var midpoint = startedAt + TimeSpan.FromSeconds((long)((now - startedAt).TotalSeconds / 2));

            var withoutRelapse = StreakCalculator.CalculateCurrentSeconds(startedAt, [], now);
            var withRelapse    = StreakCalculator.CalculateCurrentSeconds(startedAt, [midpoint], now);

            return withRelapse <= withoutRelapse;
        });
    }

    // ── Property 4: MonthStats.RelevantDays ≤ days-in-month ─────────────────

    [Property(MaxTest = 300)]
    public Property MonthStats_RelevantDays_NeverExceedsDaysInMonth()
    {
        var arbYM = ArbYearMonth();
        return Prop.ForAll(arbYM, ym =>
        {
            var (year, month) = ym;
            var daysInMonth = DateTime.DaysInMonth(year, month);
            var today   = new DateOnly(year, month, daysInMonth);
            var started = new DateTimeOffset(year, month, 1, 0, 0, 0, TimeSpan.Zero);

            var stats = StreakCalculator.CalculateMonthStats(year, month, Utc, started, [], today);
            return stats.RelevantDays <= daysInMonth;
        });
    }

    // ── Property 5: DetectManipulation threshold is exactly > 300 000 ms ─────

    [Property(MaxTest = 500)]
    public Property DetectManipulation_MatchesExpectedThreshold()
    {
        var arbOffline = ArbDeltaMs();
        var arbServer  = ArbDeltaMs();
        return Prop.ForAll(Gen.Zip(arbOffline.Generator, arbServer.Generator).ToArbitrary(), pair =>
        {
            var (offline, server) = pair;
            var result   = StreakCalculator.DetectManipulation(offline, server);
            var expected = server - offline > 300_000L;
            return result == expected;
        });
    }

    // ── Explicit edge-case facts ──────────────────────────────────────────────

    [Fact]
    public void LeapYearFeb2028_Has29RelevantDays()
    {
        // 2028 is a leap year (SC-003)
        var started = new DateTimeOffset(2028, 2, 1, 0, 0, 0, TimeSpan.Zero);
        var today   = new DateOnly(2028, 2, 29);
        var stats   = StreakCalculator.CalculateMonthStats(2028, 2, Utc, started, [], today);
        stats.RelevantDays.Should().Be(29);
    }

    [Fact]
    public void DstSpringForward_BerlinMarch2026_NoMissingHour()
    {
        // DST spring-forward: 2026-03-29 clocks go 02:00 → 03:00 in Europe/Berlin.
        // Streak should not be reset; no "missing" hour counted as relapse.
        var started = new DateTimeOffset(2026, 3, 1, 0, 0, 0, TimeSpan.Zero);
        var relapseTimes = Array.Empty<DateTimeOffset>();
        var today = new DateOnly(2026, 3, 29);

        var stats = StreakCalculator.CalculateMonthStats(2026, 3, "Europe/Berlin", started, relapseTimes, today);
        stats.AbstinentDays.Should().Be(29); // all days abstinent, including the DST transition day
        stats.RelevantDays.Should().Be(29);
    }

    [Fact]
    public void DstFallBack_BerlinOctober2026_NoDuplicateDay()
    {
        // DST fall-back: 2026-10-25 clocks go 03:00 → 02:00 in Europe/Berlin.
        // The 25th still appears exactly once in stats (no double-counting).
        var started = new DateTimeOffset(2026, 10, 1, 0, 0, 0, TimeSpan.Zero);
        var today   = new DateOnly(2026, 10, 25);

        // Pass today explicitly so CalculateDayStatus doesn't reject future dates
        var stats = StreakCalculator.CalculateMonthStats(2026, 10, "Europe/Berlin", started, [], today);
        stats.RelevantDays.Should().Be(25);
        stats.AbstinentDays.Should().Be(25);
    }

    // ── Generators ───────────────────────────────────────────────────────────

    private static Arbitrary<(DateTimeOffset start, DateTimeOffset now)> ArbStartAndNow()
    {
        return Gen.Zip(
            Gen.Choose(0, 86400 * 365).Select(s => BaseStart.AddSeconds(s)),
            Gen.Choose(0, 86400 * 365).Select(s => BaseStart.AddSeconds(s)))
        .Where(pair => pair.Item2 >= pair.Item1)
        .Select(pair => (pair.Item1, pair.Item2))
        .ToArbitrary();
    }

    private static Arbitrary<long> ArbShiftSeconds()
        => Gen.Choose(-86400, 86400).Select(v => (long)v).ToArbitrary();

    private static Arbitrary<(int year, int month)> ArbYearMonth()
        => Gen.Zip(Gen.Choose(2025, 2030), Gen.Choose(1, 12))
              .Select(p => (p.Item1, p.Item2))
              .ToArbitrary();

    private static Arbitrary<long> ArbDeltaMs()
        => Gen.Choose(0, 999_999).Select(v => (long)v).ToArbitrary();
}

