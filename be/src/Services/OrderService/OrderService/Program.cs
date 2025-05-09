using OrderService.Services;
using OrderService.Data;
using OrderService.Repositories.Interfaces;
using SharedLibrary.Auth.Configuration;
using SharedLibrary.Auth.Middleware;
using System.Text.Json.Serialization;
using OrderService.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.


// Add logging
builder.Services.AddLogging();

builder.Services.AddSingleton<OrderDbContext>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<OrderService.Services.OrderService>();

//builder.Services.AddSingleton<IRabbitMqConnection>(new RabbitMqConnection(builder.Configuration));
builder.Services.AddSingleton<IRabbitMqConnection, RabbitMqConnection>();
builder.Services.AddScoped<IOrderPlacedProducer, OrderPlacedProducer>();

builder.Services.AddSharedJwtConfig(builder.Configuration);

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
});
builder.Services.AddGrpc();

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

app.UseRouting();

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.UseMiddleware<RoleMiddleware>();

app.MapControllers();

app.MapGrpcService<OrderGrpcService>();

app.Run();
