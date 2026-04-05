using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace Warcraft.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IConfiguration config) : ControllerBase
{
    [HttpGet("login")]
    public IActionResult Login()
    {
        var frontendUrl = config["FrontendUrl"] ?? "/";
        return Challenge(new AuthenticationProperties
        {
            RedirectUri = frontendUrl
        }, "Blizzard");
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Ok();
    }

    [HttpGet("me")]
    public IActionResult Me()
    {
        if (!User.Identity?.IsAuthenticated ?? true)
            return Unauthorized();

        return Ok(new
        {
            id = User.FindFirst("user_id")?.Value,
            battleTag = User.FindFirst("battle_tag")?.Value,
            isAuthenticated = true,
        });
    }
}
