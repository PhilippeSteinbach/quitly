using FluentAssertions;
using System.Net;

namespace Quitly.IntegrationTests;

/// <summary>
/// T054: Rate-limiting test for GET /time endpoint.
///
/// The plan requires ≥ 60 requests/min to GET /time → 429 Too Many Requests.
/// This test fires 65 rapid requests and verifies at least one 429 is returned.
///
/// Marked Skip because it requires a running API instance.
/// Run manually: dotnet test --filter "FullyQualifiedName~RateLimitTests"
/// </summary>
[Trait("Category", "LoadTest")]
public sealed class RateLimitTests
{
    private const string ApiBase = "http://localhost:5000";

    [Fact(Skip = "Requires running API on port 5000 — run manually")]
    public async Task GetTime_Over60RequestsPerMinute_Returns429()
    {
        using var client = new HttpClient { BaseAddress = new Uri(ApiBase) };

        // Fire 65 requests as fast as possible (well within 1 minute window)
        var tasks = Enumerable.Range(0, 65).Select(_ => client.GetAsync("/time"));
        var responses = await Task.WhenAll(tasks);

        var statusCodes = responses.Select(r => r.StatusCode).ToList();
        statusCodes.Should().Contain(
            HttpStatusCode.TooManyRequests,
            because: "more than 60 requests/min to GET /time must be rate-limited (429)"
        );
    }
}
