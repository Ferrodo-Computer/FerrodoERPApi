using Application.ViewModels;
using Asp.Versioning;
using FerrodoERPApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using PresentationBase.AuthServices.Interfaces;
using System.Net;

namespace FerrodoERPApi.Controllers
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    public class AuthenticationController : ControllerBase
    {
        readonly ILogger<AuthenticationController> _logger;
        readonly ITokenService _tokenService;

        public AuthenticationController(ILogger<AuthenticationController> logger, ITokenService tokenService)
        {
            _logger = logger;
            _tokenService = tokenService;
        }

        [AllowAnonymous]
        [EnableRateLimiting("auth-strict")]
        [HttpPost("logIn")]
        public async Task<IActionResult> Authenticate([FromBody] string login)
        {
            try
            {
                _logger.LogInformation("Próba logowania do programu użytkownika: {Login}", login);

                var tokens = await _tokenService.IssueToken(login);
                AddRefreshTokenCookie(tokens);

                return Ok(tokens.AccessToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Błąd podczas logowania użytkownika: {Login}", login);
                return StatusCode(500, ex.Message);
            }
        }

        private void AddRefreshTokenCookie(TokenPair tokens)
        {
            Response.Cookies.Append(
                "refresh_token",
                tokens.RefreshToken,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Expires = DateTimeOffset.UtcNow.AddDays(14),
                    Path = "/api/v1/Authentication"
                });
        }
    }
}
