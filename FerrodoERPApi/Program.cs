using FerrodoERPApi.StartUpConfig;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.RegisterStandardServices();

builder.RegisterSecurityServices();

builder.RegisterSwagger();

builder.AddAuthServices();

var app = builder.Build();

app.ConfigureApp();

app.Run();
