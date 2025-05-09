using ReviewService.Data;
using ReviewService.Repositories.Interfaces;
using ReviewService.Repositories;
using System.Text.Json.Serialization;
using SharedLibrary.Auth.Configuration;
using SharedLibrary.Auth.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.


// Add logging
builder.Services.AddLogging();


builder.Services.AddSingleton<ReviewDbContext>();
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
builder.Services.AddScoped<ReviewService.Services.ReviewService>();

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

app.UseAuthentication();

app.UseAuthorization();

app.UseMiddleware<RoleMiddleware>();

app.MapControllers();

app.Run();
