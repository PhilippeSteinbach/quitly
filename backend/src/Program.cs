using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Quitly.Api.Api;
using Quitly.Api.Application.CheckIns;
using Quitly.Api.Application.Habits;
using Quitly.Api.Application.Metrics;
using Quitly.Api.Application.Insights;
using Quitly.Api.Application.Prompts;
using Quitly.Api.Application.Recovery;
using Quitly.Api.Application.Streaks;
using Quitly.Api.Configuration;
using Quitly.Api.Infrastructure.Http;
using Quitly.Api.Infrastructure.Security;
using Quitly.Api.Persistence;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();
builder.Services.AddRouting(options => options.LowercaseUrls = true);
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<ICurrentUserAccessor, CurrentUserAccessor>();
builder.Services.AddScoped<HabitService>();
builder.Services.AddScoped<CheckInService>();
builder.Services.AddScoped<PromptService>();
builder.Services.AddScoped<WeeklyInsightService>();
builder.Services.AddScoped<RecoveryService>();
builder.Services.AddScoped<StreakService>();
builder.Services.AddHostedService<KpiAggregationJob>();

var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? "Host=localhost;Port=5432;Database=quitly;Username=postgres;Password=postgres";

builder.Services.AddDbContext<QuitlyDbContext>(options => options.UseNpgsql(connectionString));
builder.Services.AddQuitlyRateLimiting();

var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = signingKey,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(OwnershipPolicy.OwnerPolicy, policy =>
        policy.RequireAuthenticatedUser().RequireClaim(System.Security.Claims.ClaimTypes.NameIdentifier));
});

var app = builder.Build();

app.UseQuitlyErrorHandling();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseQuitlySecurityHeaders();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health", () => TypedResults.Ok(new { status = "ok" }))
    .WithName("HealthCheck");

var api = app.MapGroup("/api/v1");
api.MapGet("/ping", () => TypedResults.Ok(new { message = "Quitly API ready" }));
api.MapAuthEndpoints();
api.MapHabitEndpoints();
api.MapCheckInEndpoints();
api.MapRecoveryEndpoints();
api.MapInsightPromptEndpoints();

app.Run();

public partial class Program;

