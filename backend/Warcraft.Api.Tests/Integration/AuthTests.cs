using FluentAssertions;
using Warcraft.Api.Tests.Integration.Infrastructure;

namespace Warcraft.Api.Tests.Integration;

/// <summary>
/// Verifies that [Authorize] endpoints reject unauthenticated requests with 401.
/// Uses UnauthenticatedFactory — no test auth handler is installed.
/// </summary>
public class AuthTests(UnauthenticatedFactory factory)
    : IClassFixture<UnauthenticatedFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Dashboard_Unauthenticated_Returns401()
    {
        var response = await _client.GetAsync("/api/tasks/dashboard/1");
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetCharacters_Unauthenticated_Returns401()
    {
        var response = await _client.GetAsync("/api/characters");
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GearNeeds_Unauthenticated_Returns401()
    {
        var response = await _client.GetAsync("/api/gear/needs");
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AuthMe_Unauthenticated_Returns401()
    {
        var response = await _client.GetAsync("/api/auth/me");
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
    }
}
