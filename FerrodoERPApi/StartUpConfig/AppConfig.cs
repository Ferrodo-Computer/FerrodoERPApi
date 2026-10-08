namespace FerrodoERPApi.StartUpConfig;

public static class AppConfig
{
    public static WebApplication ConfigureApp(this WebApplication app)
    {
        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                foreach (var description in app.DescribeApiVersions())
                {
                    options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());
                }
            });
        }

        // ---------------------------
        // MIDDLEWARE ORDER
        // ---------------------------

        app.UseHttpsRedirection();

        app.UseRouting();

        app.UseAuthentication();

        app.UseAuthorization();

        app.UseRateLimiter();

        app.MapControllers();

        return app;
    }
}
