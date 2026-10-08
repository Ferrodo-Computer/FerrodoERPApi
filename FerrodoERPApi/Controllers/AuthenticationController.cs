using Application.ViewModels;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using PresentationBase.AuthServices.Interfaces;
using System.Security.Authentication;

namespace FerrodoERPApi.Controllers
{
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [ApiVersion("1.0")]
    public class AuthenticationController : ControllerBase
    {
        readonly ILogger<AuthenticationController> _logger;
        readonly ITokenService _tokenService;
        readonly string _login;

        public AuthenticationController(ILogger<AuthenticationController> logger, ITokenService tokenService)
        {
            _logger = logger;
            _tokenService = tokenService;
            _login = _tokenService.ReadLoginFromClaims();
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

        [HttpPost("logOut")]
        [EnableRateLimiting("auth-strict")]
        public async Task<IActionResult> LogOut()
        {
            try
            {                
                var refreshToken = Request.Cookies["refresh_token"];                

                DeleteRefreshTokenCookie();

                _logger.LogInformation("Udane wylogowanie użytkownika {UserId}", _login);

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Logout failed");
                return StatusCode(500);
            }
        }

        //[AllowAnonymous]
        //[EnableRateLimiting("auth-strict")]
        //[HttpPost("refreshToken")]
        //public async Task<IActionResult> RefreshToken(CancellationToken cancellationToken)
        //{
        //    try
        //    {
        //        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

        //        var refreshToken = Request.Cookies["refresh_token"];
        //        if (string.IsNullOrEmpty(refreshToken))
        //            return Unauthorized();

        //        var tokens = await _tokenService.RefreshToken(refreshToken, ipAddress);
               
        //        AddRefreshTokenCookie(tokens);

        //        _logger.LogInformation("Udana próba odświeżenia tokena dla tokenu: " + refreshToken);

        //        return Ok(tokens.AccessToken);
        //    }
        //    catch (AuthenticationException ex)
        //    {
        //        _logger.LogError(ex.Message);
        //        return Unauthorized("Brak autoryzacji!!");
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex.Message);
        //        return StatusCode(500, ex.Message);
        //    }
        //}

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

        private void DeleteRefreshTokenCookie()
        {
            Response.Cookies.Append(
                "refresh_token",
                "",
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Expires = DateTimeOffset.UtcNow.AddDays(-1),
                    Path = "/api/v1/Authentication"
                });
        }
    }
}
