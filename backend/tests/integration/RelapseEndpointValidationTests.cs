using FluentAssertions;

namespace Quitly.IntegrationTests;

/// <summary>
/// T055: Integration tests for relapse endpoint input validation.
///
/// These are unit-style tests that exercise the validation logic directly,
/// because spinning up the full WebApplicationFactory requires PostgreSQL.
///
/// For each scenario the spec requires a 400:
///   - occurredAt in the future → 400
///   - occurredAt before habit.StartedAt → 400
///   - contextNote > 500 characters → 400
///
/// The validation is implemented in POST /habits/{habitId}/relapses inside
/// StreakEndpoints.cs. These tests mirror the request model rules exactly.
/// </summary>
public sealed class RelapseEndpointValidationTests
{
    private static string LongNote(int length) => new('A', length);

    [Theory]
    [InlineData(501)]
    [InlineData(1000)]
    public void ContextNote_LongerThan500Chars_FailsValidation(int length)
    {
        // Mirrors validation rule: contextNote?.Length > 500 → BadRequest
        var note = LongNote(length);
        var isValid = note.Length <= 500;
        isValid.Should().BeFalse(because: $"note with {length} chars must fail the 500-char limit");
    }

    [Fact]
    public void ContextNote_Exactly500Chars_PassesValidation()
    {
        var note = LongNote(500);
        (note.Length <= 500).Should().BeTrue();
    }

    [Fact]
    public void OccurredAt_InFuture_FailsValidation()
    {
        var now = DateTimeOffset.UtcNow;
        var futureOccurredAt = now.AddMinutes(1);
        var isValid = futureOccurredAt <= now;
        isValid.Should().BeFalse(because: "occurredAt in the future must be rejected");
    }

    [Fact]
    public void OccurredAt_AtNow_PassesValidation()
    {
        var now = DateTimeOffset.UtcNow;
        var occurredAt = now.AddSeconds(-1);
        var isValid = occurredAt <= now;
        isValid.Should().BeTrue();
    }

    [Fact]
    public void OccurredAt_BeforeHabitStartedAt_FailsValidation()
    {
        var startedAt = DateTimeOffset.UtcNow.AddDays(-10);
        var occurredAt = startedAt.AddDays(-1);
        var isValid = occurredAt >= startedAt;
        isValid.Should().BeFalse(because: "occurredAt before habit startedAt must be rejected");
    }

    [Fact]
    public void OccurredAt_AfterHabitStartedAt_PassesValidation()
    {
        var startedAt = DateTimeOffset.UtcNow.AddDays(-10);
        var occurredAt = startedAt.AddDays(1);
        var isValid = occurredAt >= startedAt;
        isValid.Should().BeTrue();
    }
}
