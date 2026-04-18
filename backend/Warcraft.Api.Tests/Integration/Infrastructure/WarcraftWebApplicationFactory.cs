using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Testcontainers.PostgreSql;
using Warcraft.Api.Data;

namespace Warcraft.Api.Tests.Integration.Infrastructure;

/// <summary>
/// Authenticated factory — every request runs as TestUserId = 1.
/// Spins up a real PostgreSQL container via Testcontainers.
/// Migrations run automatically on first startup (same as production).
/// </summary>
public class WarcraftWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _db = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .Build();

    public async Task InitializeAsync() => await _db.StartAsync();

    public new async Task DisposeAsync()
    {
        await _db.DisposeAsync();
        await base.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Provide dummy Blizzard config so the OAuth middleware doesn't throw on startup
        builder.UseSetting("Blizzard:ClientId", "test-client-id");
        builder.UseSetting("Blizzard:ClientSecret", "test-client-secret");
        builder.UseSetting("Blizzard:RedirectUri", "https://localhost/api/auth/callback");
        builder.UseSetting("FrontendUrl", "http://localhost:3000");

        builder.ConfigureServices(services =>
        {
            // Replace the real DB with the Testcontainers instance
            var descriptor = services.Single(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            services.Remove(descriptor);
            services.AddDbContext<AppDbContext>(opts =>
                opts.UseNpgsql(_db.GetConnectionString()));

            // Replace auth with a test handler that auto-authenticates as TestUserId = 1
            services.Configure<AuthenticationOptions>(opts =>
            {
                opts.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                opts.DefaultChallengeScheme = TestAuthHandler.SchemeName;
            });
            services.AddAuthentication(TestAuthHandler.SchemeName)
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                    TestAuthHandler.SchemeName, _ => { });
        });
    }
}

/// <summary>
/// Unauthenticated factory — no test auth handler, so [Authorize] endpoints return 401.
/// </summary>
public class UnauthenticatedFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _db = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .Build();

    public async Task InitializeAsync() => await _db.StartAsync();

    public new async Task DisposeAsync()
    {
        await _db.DisposeAsync();
        await base.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("Blizzard:ClientId", "test-client-id");
        builder.UseSetting("Blizzard:ClientSecret", "test-client-secret");
        builder.UseSetting("Blizzard:RedirectUri", "https://localhost/api/auth/callback");
        builder.UseSetting("FrontendUrl", "http://localhost:3000");

        builder.ConfigureServices(services =>
        {
            var descriptor = services.Single(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            services.Remove(descriptor);
            services.AddDbContext<AppDbContext>(opts =>
                opts.UseNpgsql(_db.GetConnectionString()));

            // Replace auth with a handler that always fails — [Authorize] returns 401, no redirect.
            services.Configure<AuthenticationOptions>(opts =>
            {
                opts.DefaultAuthenticateScheme = DenyAllHandler.SchemeName;
                opts.DefaultChallengeScheme = DenyAllHandler.SchemeName;
            });
            services.AddAuthentication(DenyAllHandler.SchemeName)
                .AddScheme<AuthenticationSchemeOptions, DenyAllHandler>(
                    DenyAllHandler.SchemeName, _ => { });
        });
    }
}

/// <summary>
/// Auth handler that always fails authentication and returns 401 on challenge.
/// Used by UnauthenticatedFactory to test that [Authorize] endpoints reject anonymous requests.
/// </summary>
internal sealed class DenyAllHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    internal const string SchemeName = "DenyAll";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        => Task.FromResult(AuthenticateResult.NoResult());

    protected override Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        Response.StatusCode = 401;
        return Task.CompletedTask;
    }
}
