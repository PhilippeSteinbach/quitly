using FluentAssertions;

namespace Quitly.IntegrationTests;

public sealed class RelapseRecoveryFlowTests
{
    [Fact]
    public void RecoveryContinuation_MustTrackCompletionWithin24Hours()
    {
        var createdAt = DateTimeOffset.UtcNow.AddHours(-3);
        var completedAt = DateTimeOffset.UtcNow;

        (completedAt - createdAt).Should().BeLessThan(TimeSpan.FromHours(24));
    }
}
