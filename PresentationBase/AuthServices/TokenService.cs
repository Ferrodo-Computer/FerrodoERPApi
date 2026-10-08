using Application.ViewModels;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using PresentationBase.AuthServices.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Application.Services
{
    public class TokenService : ITokenService
    {
        readonly IConfiguration _config;

        public TokenService(IConfiguration config)
        {
            _config = config;
        }

        public async Task<TokenPair> IssueToken(string login)
        {
            TokenPair newTokens = new TokenPair();
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, login),
            };
            var token = CreateToken(claims);
            var refreshToken = Generate();
            var hash = Hash(refreshToken);

            return new TokenPair() { AccessToken = new JwtSecurityTokenHandler().WriteToken(token), RefreshToken = refreshToken, RefreshTokenValidDate = DateTime.UtcNow.AddDays(14) };
        }

        private JwtSecurityToken CreateToken(Claim[] claims)
        {
            var secretKey = new SymmetricSecurityKey(
                Encoding.ASCII.GetBytes(
                    _config["Authentication:SecretKey"]));

            var signingCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

            int logOutMinutes = Convert.ToInt32(15); //czas z parametru
            var expires = DateTime.UtcNow.AddMinutes(logOutMinutes + 4);

            var accessClaims = claims.Append(new Claim("typ", "access"));

            return new JwtSecurityToken(
                    issuer: _config["Authentication:Issuer"],
                    audience: _config["Authentication:Audience"],
                    claims: accessClaims,
                    notBefore: DateTime.UtcNow,
                    expires: DateTime.UtcNow.AddMinutes(logOutMinutes + 4),
                    signingCredentials: signingCredentials
                );
        }

        private string Generate()
        {
            var bytes = RandomNumberGenerator.GetBytes(64);
            return Convert.ToBase64String(bytes);
        }

        private string Hash(string token)
        {
            return Convert.ToBase64String(
                SHA256.HashData(Encoding.UTF8.GetBytes(token)));
        }
    }


}
