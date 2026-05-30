using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Quitly.Api.Domain.Entities;
using Quitly.Api.Persistence;
using Quitly.Api.Infrastructure.Security;

namespace Quitly.Api.Api;

public static class AuthEndpoints
{
    public static RouteGroupBuilder MapAuthEndpoints(this RouteGroupBuilder group)
    {
        var auth = group.MapGroup("/auth");

        auth.MapPost("/register", RegisterAsync);
        auth.MapPost("/login", LoginAsync);

        return group;
    }

    private static async Task<Results<Created<AuthResponse>, Conflict<Dictionary<string, string>>>> RegisterAsync(
        [FromBody] RegisterRequest request,
        QuitlyDbContext dbContext,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var exists = await dbContext.Users.AnyAsync(user => user.Email == email, cancellationToken);

        if (exists)
        {
            return TypedResults.Conflict(new Dictionary<string, string> { ["error"] = "email_taken" });
        }

        var user = new User
        {
            Email = email,
            PasswordHash = passwordHasher.Hash(request.Password),
            Timezone = request.Timezone,
            Locale = "en"
        };

        dbContext.Users.Add(user);
        dbContext.Reminders.Add(new Reminder { UserId = user.Id });
        dbContext.Streaks.Add(new Streak { UserId = user.Id });

        await dbContext.SaveChangesAsync(cancellationToken);

        var tokens = tokenService.CreateTokens(user);
        return TypedResults.Created($"/api/v1/users/{user.Id}", new AuthResponse(tokens.AccessToken, tokens.RefreshToken));
    }

    private static async Task<Results<Ok<AuthResponse>, UnauthorizedHttpResult>> LoginAsync(
        [FromBody] LoginRequest request,
        QuitlyDbContext dbContext,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await dbContext.Users.SingleOrDefaultAsync(candidate => candidate.Email == email, cancellationToken);

        if (user is null || !passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            return TypedResults.Unauthorized();
        }

        var tokens = tokenService.CreateTokens(user);
        return TypedResults.Ok(new AuthResponse(tokens.AccessToken, tokens.RefreshToken));
    }

    public sealed record RegisterRequest(string Email, string Password, string Timezone);

    public sealed record LoginRequest(string Email, string Password);

    public sealed record AuthResponse(string AccessToken, string RefreshToken);
}
