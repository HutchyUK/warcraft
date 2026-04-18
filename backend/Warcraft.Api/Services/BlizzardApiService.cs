using System.Net.Http.Headers;
using System.Text.Json;

namespace Warcraft.Api.Services;

public class BlizzardCharacterResult
{
    public List<BlizzardCharacter> Characters { get; init; } = [];
    public bool ApiFailed { get; init; }
}

public record BlizzardCharacter(
    string Name,
    string Realm,
    string Class,
    int Level,
    string? Spec,
    string? AvatarUrl,
    string? BlizzardId
);

public class BlizzardApiService(IHttpClientFactory httpClientFactory, ILogger<BlizzardApiService> logger)
{
    public async Task<BlizzardCharacterResult> GetRetailCharactersAsync(
        string accessToken, string region)
    {
        var regionLower = region.ToLowerInvariant();
        var baseUrl = regionLower == "eu"
            ? "https://eu.api.blizzard.com"
            : "https://us.api.blizzard.com";
        var namespace_ = $"profile-{regionLower}";

        var url = $"{baseUrl}/profile/user/wow?namespace={namespace_}&locale=en_US";

        try
        {
            var client = httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await client.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning(
                    "Blizzard Retail API returned {StatusCode} for namespace {Namespace}. " +
                    "Falling back to manual character entry.",
                    response.StatusCode, namespace_);
                return new BlizzardCharacterResult { ApiFailed = true };
            }

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            if (!doc.RootElement.TryGetProperty("wow_accounts", out var accounts))
                return new BlizzardCharacterResult { ApiFailed = true };

            var characters = new List<BlizzardCharacter>();

            foreach (var account in accounts.EnumerateArray())
            {
                if (!account.TryGetProperty("characters", out var chars)) continue;

                foreach (var ch in chars.EnumerateArray())
                {
                    // Exclude Classic characters — the retail namespace still includes them
                    if (!IsRetailCharacter(ch)) continue;

                    characters.Add(new BlizzardCharacter(
                        Name: ch.TryGetProperty("name", out var n) ? n.GetString() ?? "" : "",
                        Realm: ch.TryGetProperty("realm", out var r)
                            ? r.TryGetProperty("name", out var rn) ? rn.GetString() ?? "" : ""
                            : "",
                        Class: ch.TryGetProperty("playable_class", out var pc)
                            ? pc.TryGetProperty("name", out var cn) ? cn.GetString() ?? "" : ""
                            : "",
                        Level: ch.TryGetProperty("level", out var l) ? l.GetInt32() : 0,
                        Spec: null,    // Not available in list endpoint
                        AvatarUrl: null, // Requires separate media endpoint call
                        BlizzardId: ch.TryGetProperty("id", out var id) ? id.GetInt64().ToString() : null
                    ));
                }
            }

            return new BlizzardCharacterResult { Characters = characters };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to fetch Blizzard Retail characters");
            return new BlizzardCharacterResult { ApiFailed = true };
        }
    }

    private static bool IsRetailCharacter(JsonElement character)
    {
        // Exclude Classic characters (Classic Era, Anniversary, etc.)
        if (character.TryGetProperty("game_version", out var gv))
        {
            var type = gv.TryGetProperty("type", out var t) ? t.GetString() : null;
            return type is not ("CLASSIC" or "CLASSIC_ERA");
        }

        // Exclude if realm slug contains "classic"
        if (character.TryGetProperty("realm", out var realm) &&
            realm.TryGetProperty("slug", out var slug))
        {
            return !slug.GetString()?.Contains("classic", StringComparison.OrdinalIgnoreCase) ?? true;
        }

        // No game_version field → assume retail
        return true;
    }
}
