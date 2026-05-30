using FluentAssertions;
using Quitly.Api.Application.Streaks;
using Quitly.Api.Domain.Entities;
using Quitly.Api.Domain.Enums;

namespace Quitly.UnitTests;

public sealed class StreakCalculatorTests
{
    [Fact]
    public void CalculateSnapshot_ReturnsConsecutiveAbstinentDays()
    {
        var checkIns = new[]
        {
            new CheckIn { Day = new DateOnly(2026, 5, 25), Status = CheckInStatus.Abstinent },
            new CheckIn { Day = new DateOnly(2026, 5, 26), Status = CheckInStatus.Abstinent },
            new CheckIn { Day = new DateOnly(2026, 5, 27), Status = CheckInStatus.Abstinent }
        };

        var snapshot = StreakService.CalculateSnapshot(checkIns);

        snapshot.CurrentStreakDays.Should().Be(3);
        snapshot.LastAbstinentDay.Should().Be(new DateOnly(2026, 5, 27));
    }

    [Fact]
    public void CalculateSnapshot_ResetsOnNonAbstinentDay()
    {
        var checkIns = new[]
        {
            new CheckIn { Day = new DateOnly(2026, 5, 26), Status = CheckInStatus.Abstinent },
            new CheckIn { Day = new DateOnly(2026, 5, 27), Status = CheckInStatus.NonAbstinent }
        };

        var snapshot = StreakService.CalculateSnapshot(checkIns);

        snapshot.CurrentStreakDays.Should().Be(0);
        snapshot.LastNonAbstinentDay.Should().Be(new DateOnly(2026, 5, 27));
    }
}
