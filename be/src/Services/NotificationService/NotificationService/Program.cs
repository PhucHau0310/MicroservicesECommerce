using NotificationService.Application.Interfaces;
using NotificationService.Application.Messaging;
using NotificationService.Application.Services;
using NotificationService.Data;
using NotificationService.Domain.Interfaces;
using NotificationService.Helper;
using NotificationService.Infrastructure.Hubs;
using NotificationService.Infrastructure.Messaging;
using NotificationService.Infrastructure.Persistence;
using SharedLibrary.Auth.Configuration;
using SharedLibrary.Auth.Middleware;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Add logging
builder.Services.AddLogging();

builder.Services.AddSingleton<RedisHelper>();
builder.Services.AddSingleton<MongoDbContext>();
builder.Services.AddSingleton<NotificationHub>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<INotificationService, NotificationService.Application.Services.NotificationService>();

builder.Services.AddSingleton<IRabbitMqConnection, RabbitMqConnection>();
builder.Services.AddSingleton<IOrderPlacedConsumer, OrderPlacedConsumer>();
builder.Services.AddSingleton<IPaymentConsumer, PaymentConsumer>();
builder.Services.AddSingleton<IAccountConsumer, AccountConsumer>();
builder.Services.AddHostedService<ConsumerHostedService>();

// Register SignalR
builder.Services.AddSignalR((options) =>
{
    // Cho phép WebSocket không SSL trong môi trường dev
    options.EnableDetailedErrors = true;
    options.MaximumReceiveMessageSize = 64 * 1024; // 64 KB
});

builder.Services.AddSharedJwtConfig(builder.Configuration);

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
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

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.UseMiddleware<RoleMiddleware>();

app.UseEndpoints(endpoints =>
{
    endpoints.MapHub<NotificationHub>("/notificationhub");
    app.MapControllers();
});

//app.MapControllers();

app.Run();
