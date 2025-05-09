
using AuthService.Repositories.Interfaces;
using AuthService.Data;
using AuthService.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using SharedLibrary.Auth.Middleware;
using SharedLibrary.Auth.Configuration;
using System.Text.Json.Serialization;
using AuthService.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Connect DB
builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlServerOptions => sqlServerOptions.EnableRetryOnFailure()
    )
);

// Add logging
builder.Services.AddLogging();

// Register Services
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IOAuthService, AuthService.Services.OAuthService>();
builder.Services.AddScoped<IAuthService, AuthService.Services.AuthService>();
builder.Services.AddScoped<ITokenService, AuthService.Services.TokenService>();
builder.Services.AddScoped<IEmailService, AuthService.Services.EmailService>();

builder.Services.AddSingleton<IRabbitMqConnection, RabbitMqConnection>();
builder.Services.AddScoped<IAccountProducer, AccountProducer>();

// Configure JWT
var authConfig = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

builder.Services.AddSharedJwtConfig(authConfig);

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
}); ;

builder.Services.AddOpenApi();

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(8081, listenOptions =>
    {
        listenOptions.UseHttps("/https/aspnetapp.pfx", "Hau2004@");
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}


app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.UseMiddleware<RoleMiddleware>();

app.MapControllers();

app.MapGet("/", () => "Running Auth-Service...").AllowAnonymous();

app.Run();
