using System.Threading.RateLimiting;

namespace FerrodoERPApi.StartUpConfig;

public static class SecurityConfig
{
    public static void RegisterSecurityServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            //Bardziej szczelna wersja, dla krytycznych endpointów jak logowanie
            options.AddPolicy("auth-strict", context =>
            {
                var ip = context.Connection.RemoteIpAddress?.ToString() ?? "nieznany";

                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: ip,
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 10,                // dopuszczalna ilość zapytań dla jednego adresu IP
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0,
                        AutoReplenishment = true
                    });
            });

            options.OnRejected = async (context, token) =>
            {
                var httpContext = context.HttpContext;
                var logger = httpContext.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("RateLimiter");

                logger.LogWarning(
                    "Minutowy limit połączeń przekroczony. IP={IP}, Endpoint={Endpoint}",
                    httpContext.Connection.RemoteIpAddress?.ToString() ?? "nieznany",
                    httpContext.Request.Path);

                //RFC-compliant Retry-After (seconds)
                httpContext.Response.Headers["Retry-After"] = TimeSpan.FromMinutes(1).TotalSeconds.ToString();
                httpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;

                await httpContext.Response.WriteAsync("Zbyt wiele zapytań", token);
            };
        });
    }
}
