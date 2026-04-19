using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.EntityFrameworkCore;
using Warcraft.Api.Data;
using Warcraft.Api.Models;
using Warcraft.Api.Services;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;
var frontendUrl = config["FrontendUrl"] ?? "/";

// Database — try config first, fall back to DATABASE_URL env var (Railway default)
var connectionString = config.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
    if (!string.IsNullOrWhiteSpace(databaseUrl))
    {
        // Convert postgresql://user:pass@host/db?sslmode=require to Npgsql format
        var uri = new Uri(databaseUrl);
        var colonIdx = uri.UserInfo.IndexOf(':');
        var username = uri.UserInfo[..colonIdx];
        var password = Uri.UnescapeDataString(uri.UserInfo[(colonIdx + 1)..]);
        var port = uri.Port > 0 ? uri.Port : 5432;
        connectionString =
            $"Host={uri.Host};Port={port};Database={uri.AbsolutePath.TrimStart('/')}" +
            $";Username={username};Password={password};SSL Mode=Require" +
            $";Trust Server Certificate=true";
    }
}

Console.WriteLine($"[Startup] Connection string present: {!string.IsNullOrWhiteSpace(connectionString)}");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// HTTP client factory (for BlizzardApiService)
builder.Services.AddHttpClient();
builder.Services.AddScoped<BlizzardApiService>();
builder.Services.AddScoped<ContentSeeder>();

// CORS — allow Next.js frontend with credentials
var allowedOrigins = config.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? ["http://localhost:3000"];

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});

// Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = "Blizzard";
})
.AddCookie(options =>
{
    options.Cookie.Name = "warcraft_session";
    options.Cookie.SameSite = SameSiteMode.None;   // Required for cross-origin (Vercel + Railway)
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromDays(30);
    options.SlidingExpiration = true;
    options.Events.OnRedirectToLogin = ctx =>
    {
        ctx.Response.StatusCode = 401;
        return Task.CompletedTask;
    };
})
.AddOAuth("Blizzard", options =>
{
    options.ClientId = config["Blizzard:ClientId"]!;
    options.ClientSecret = config["Blizzard:ClientSecret"]!;
    options.CallbackPath = "/api/auth/callback";

    options.AuthorizationEndpoint = "https://oauth.battle.net/authorize";
    options.TokenEndpoint = "https://oauth.battle.net/token";
    options.UserInformationEndpoint = "https://us.battle.net/oauth/userinfo";

    options.Scope.Add("openid");
    options.Scope.Add("wow.profile");

    options.SaveTokens = true;

    options.ClaimActions.MapJsonKey("sub", "sub");
    options.ClaimActions.MapJsonKey("battle_tag", "battletag");

    options.Events = new OAuthEvents
    {
        OnCreatingTicket = async context =>
        {
            // Fetch user info from Blizzard
            var request = new HttpRequestMessage(
                HttpMethod.Get, context.Options.UserInformationEndpoint);
            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", context.AccessToken);

            var response = await context.Backchannel.SendAsync(request,
                HttpCompletionOption.ResponseHeadersRead, context.HttpContext.RequestAborted);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            using var userDoc = JsonDocument.Parse(json);
            context.RunClaimActions(userDoc.RootElement);

            var blizzardId = userDoc.RootElement.TryGetProperty("sub", out var sub)
                ? sub.GetString() ?? ""
                : "";
            var battleTag = userDoc.RootElement.TryGetProperty("battletag", out var bt)
                ? bt.GetString() ?? ""
                : "";

            // Upsert user in DB
            var db = context.HttpContext.RequestServices.GetRequiredService<AppDbContext>();
            var dbUser = await db.Users.FirstOrDefaultAsync(u => u.BlizzardAccountId == blizzardId);

            if (dbUser == null)
            {
                dbUser = new User
                {
                    BlizzardAccountId = blizzardId,
                    BattleTag = battleTag,
                };
                db.Users.Add(dbUser);
                await db.SaveChangesAsync();
            }
            else
            {
                dbUser.BattleTag = battleTag;
                dbUser.UpdatedAt = DateTime.UtcNow;
                await db.SaveChangesAsync();
            }

            // Add internal user ID and access token to claims
            context.Identity?.AddClaim(new Claim("user_id", dbUser.Id.ToString()));
            context.Identity?.AddClaim(new Claim("battle_tag", battleTag));
            context.Identity?.AddClaim(
                new Claim("blizzard_access_token", context.AccessToken ?? ""));
        },

        OnRemoteFailure = context =>
        {
            context.HandleResponse();
            context.Response.Redirect($"{frontendUrl}?error=auth_failed");
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();
builder.Services.AddControllers();

// Respect Railway's PORT env var
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://+:{port}");

var app = builder.Build();

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Auto-run migrations and seed content on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();

    var seeder = scope.ServiceProvider.GetRequiredService<ContentSeeder>();
    await seeder.SeedAsync();
}

app.Run();

// Required for WebApplicationFactory<Program> in integration tests
public partial class Program { }
