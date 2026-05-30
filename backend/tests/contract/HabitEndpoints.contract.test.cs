using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Quitly.ContractTests;

public sealed class HabitEndpointsContractTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public HabitEndpointsContractTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetHabit_RequiresAuthentication()
    {
        using var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v1/habit");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
