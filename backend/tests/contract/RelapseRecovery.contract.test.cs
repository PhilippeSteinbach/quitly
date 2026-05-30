using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Quitly.ContractTests;

public sealed class RelapseRecoveryContractTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public RelapseRecoveryContractTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task PostRelapse_RequiresAuthentication()
    {
        using var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/v1/relapse", new { occurredAt = DateTimeOffset.UtcNow, contextNote = "stress" });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
