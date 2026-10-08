using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace FerrodoERPApi.StartUpConfig;

public static class AuthConfig
{
    public static void AddAuthServices(this WebApplicationBuilder builder)
    {
        string? secretKey = builder.Configuration.GetValue<string>("Authentication:SecretKey");

        if (string.IsNullOrWhiteSpace(secretKey))
        {
            throw new InvalidOperationException("Brak klucza Authentication:SecretKey w konfiguracji");
        }

        // Domyślnie każdy endpoint wymaga zalogowania - wyjątki oznaczamy [AllowAnonymous]
        builder.Services.AddAuthorization(opts =>
        {
            opts.FallbackPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
        });

        builder.Services.AddAuthentication("Bearer")
            .AddJwtBearer(opts =>
            {
                opts.TokenValidationParameters = new()
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration.GetValue<string>("Authentication:Issuer"),
                    ValidAudience = builder.Configuration.GetValue<string>("Authentication:Audience"),
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secretKey)),
                    ClockSkew = TimeSpan.FromMinutes(1)
                };
            });
    }
}
