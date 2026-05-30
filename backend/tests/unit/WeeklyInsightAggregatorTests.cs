using FluentAssertions;
using Quitly.Api.Application.Insights;
using Quitly.Api.Domain.Enums;

namespace Quitly.UnitTests;

public sealed class WeeklyInsightAggregatorTests
{
    [Theory]
    [InlineData(1, InsightConfidence.Low)]
    [InlineData(3, InsightConfidence.Medium)]
    [InlineData(5, InsightConfidence.High)]
    public void CalculateConfidence_ReturnsExpectedBand(int checkInCount, InsightConfidence expected)
    {
        WeeklyInsightService.CalculateConfidence(checkInCount).Should().Be(expected);
    }
}
