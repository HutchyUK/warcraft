using System.Net;
using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Warcraft.Api.Services;

namespace Warcraft.Api.Tests.Unit;

public class BlizzardApiServiceTests
{
    private static BlizzardApiService BuildService(HttpMessageHandler handler)
    {
        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(f => f.CreateClient(It.IsAny<string>()))
               .Returns(new HttpClient(handler));
        return new BlizzardApiService(factory.Object, NullLogger<BlizzardApiService>.Instance);
    }

    private static FakeMessageHandler RespondWith(HttpStatusCode status, string body = "")
        => new(status, body);

    // --- Non-2xx response ---

    [Fact]
    public async Task NonSuccessStatus_ReturnsApiFailed()
    {
        var svc = BuildService(RespondWith(HttpStatusCode.Forbidden));
        var result = await svc.GetClassicCharactersAsync("token", "US");
        result.ApiFailed.Should().BeTrue();
        result.Characters.Should().BeEmpty();
    }

    [Fact]
    public async Task NotFoundStatus_ReturnsApiFailed()
    {
        var svc = BuildService(RespondWith(HttpStatusCode.NotFound));
        var result = await svc.GetClassicCharactersAsync("token", "US");
        result.ApiFailed.Should().BeTrue();
    }

    // --- Missing wow_accounts field ---

    [Fact]
    public async Task MissingWowAccounts_ReturnsApiFailed()
    {
        var json = """{"id":123,"battletag":"Test#1234"}""";
        var svc = BuildService(RespondWith(HttpStatusCode.OK, json));
        var result = await svc.GetClassicCharactersAsync("token", "US");
        result.ApiFailed.Should().BeTrue();
    }

    // --- Filtering by game_version ---

    [Fact]
    public async Task MixedGameVersions_ReturnsOnlyClassicCharacters()
    {
        var json = """
        {
          "wow_accounts": [{
            "characters": [
              {
                "name": "ClassicChar",
                "level": 70,
                "realm": { "name": "Faerlina", "slug": "faerlina-classic" },
                "playable_class": { "name": "Warrior" },
                "id": 1,
                "game_version": { "type": "CLASSIC" }
              },
              {
                "name": "RetailChar",
                "level": 70,
                "realm": { "name": "Stormrage", "slug": "stormrage" },
                "playable_class": { "name": "Mage" },
                "id": 2,
                "game_version": { "type": "RETAIL" }
              }
            ]
          }]
        }
        """;
        var svc = BuildService(RespondWith(HttpStatusCode.OK, json));
        var result = await svc.GetClassicCharactersAsync("token", "US");
        result.ApiFailed.Should().BeFalse();
        result.Characters.Should().HaveCount(1);
        result.Characters[0].Name.Should().Be("ClassicChar");
        result.Characters[0].Class.Should().Be("Warrior");
    }

    // --- Realm slug fallback ---

    [Fact]
    public async Task NoGameVersionField_FallsBackToRealmSlug_IncludesClassicSlug()
    {
        var json = """
        {
          "wow_accounts": [{
            "characters": [{
              "name": "SlugFallback",
              "level": 60,
              "realm": { "name": "Old Blanchy", "slug": "old-blanchy-classic" },
              "playable_class": { "name": "Druid" },
              "id": 3
            }]
          }]
        }
        """;
        var svc = BuildService(RespondWith(HttpStatusCode.OK, json));
        var result = await svc.GetClassicCharactersAsync("token", "US");
        result.ApiFailed.Should().BeFalse();
        result.Characters.Should().HaveCount(1);
        result.Characters[0].Name.Should().Be("SlugFallback");
    }

    [Fact]
    public async Task NoGameVersionField_NonClassicSlug_ExcludesCharacter()
    {
        var json = """
        {
          "wow_accounts": [{
            "characters": [{
              "name": "RetailOnly",
              "level": 80,
              "realm": { "name": "Stormrage", "slug": "stormrage" },
              "playable_class": { "name": "Paladin" },
              "id": 4
            }]
          }]
        }
        """;
        var svc = BuildService(RespondWith(HttpStatusCode.OK, json));
        var result = await svc.GetClassicCharactersAsync("token", "US");
        result.ApiFailed.Should().BeFalse();
        result.Characters.Should().BeEmpty();
    }

    // --- HTTP exception ---

    [Fact]
    public async Task HttpClientThrows_ReturnsApiFailed_NoException()
    {
        var svc = BuildService(new ThrowingHandler());
        var act = async () => await svc.GetClassicCharactersAsync("token", "US");
        await act.Should().NotThrowAsync();
        var result = await svc.GetClassicCharactersAsync("token", "US");
        result.ApiFailed.Should().BeTrue();
    }

    // --- Classic Era type ---

    [Fact]
    public async Task ClassicEraType_IsIncluded()
    {
        var json = """
        {
          "wow_accounts": [{
            "characters": [{
              "name": "EraChar",
              "level": 60,
              "realm": { "name": "Mankrik", "slug": "mankrik" },
              "playable_class": { "name": "Shaman" },
              "id": 5,
              "game_version": { "type": "CLASSIC_ERA" }
            }]
          }]
        }
        """;
        var svc = BuildService(RespondWith(HttpStatusCode.OK, json));
        var result = await svc.GetClassicCharactersAsync("token", "US");
        result.Characters.Should().HaveCount(1);
        result.Characters[0].Name.Should().Be("EraChar");
    }
}

// --- Test helpers ---

internal sealed class FakeMessageHandler(HttpStatusCode status, string body) : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var response = new HttpResponseMessage(status)
        {
            Content = new StringContent(body, Encoding.UTF8, "application/json")
        };
        return Task.FromResult(response);
    }
}

internal sealed class ThrowingHandler : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
        => throw new HttpRequestException("Simulated network failure");
}
