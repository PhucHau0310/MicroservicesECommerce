using PaymentService.GrpcServices;
using SharedLibrary.Auth.Configuration;
using SharedLibrary.Auth.Middleware;
using System.Text.Json.Serialization;
using PaymentService.Data;
using PaymentService.Configurations;
using PaymentService.Repositories.Interfaces;
using PaymentService.Services.Interfaces;
using PaymentService.Repositories;
using PaymentService.Messaging.Interfaces;
using PaymentService.Messaging;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var connectionString = builder.Configuration.GetConnectionString("PostgresConnection");
builder.Services.AddDbContext<PaymentDbContext>(options =>
    options.UseNpgsql(connectionString));

// Add logging
builder.Services.AddLogging();

var vnPayConfig = new VnPayConfig();
builder.Configuration.GetSection("Vnpay").Bind(vnPayConfig);
builder.Services.AddSingleton(vnPayConfig);

builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
builder.Services.AddScoped<IPaymentService, PaymentService.Services.PaymentService>();
builder.Services.AddScoped<IVnPayService, PaymentService.Services.VnPayService>();

builder.Services.AddSingleton<IRabbitMqConnection, RabbitMqConnection>();
builder.Services.AddScoped<IPaymentProducer, PaymentProducer>();

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

app.MapGrpcService<PaymentGrpcService>();

app.Run();
