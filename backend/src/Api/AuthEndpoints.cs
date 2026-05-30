using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Quitly.Api.Domain.Entities;
using Quitly.Api.Persistence;
using Quitly.Api.Infrastructure.Security;
using System.Security.Claims;

namespace Quitly.Api.Api;

public static class AuthEndpoints
{
    public static RouteGroupBuilder MapAuthEndpoints(this RouteGroupBuilder group)
    {
        var auth = group.MapGroup("/auth");

        auth.MapPost("/register", RegisterAsync);
        auth.MapPost("/login", LoginAsync);

        // T010: stubs — actual implementations in T038–T040
        auth.MapPost("/refresh", RefreshAsync).RequireRateLimiting("auth_refresh");
        auth.MapGet("/me", MeAsync).RequireAuthorization();
        auth.MapDelete("/session", RevokeSessionAsync);

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

    // T038: Implemented
    private static async Task<Results<Ok<AuthResponse>, UnauthorizedHttpResult>> RefreshAsync(
        [FromBody] RefreshRequest request,
        QuitlyDbContext dbContext,
        ITokenService tokenService,
        CancellationToken cancellationToken)
    {
        try
        {
            var (accessToken, newRefresh) = await tokenService.RotateRefreshTokenAsync(request.RefreshToken, dbContext, cancellationToken);
            return TypedResults.Ok(new AuthResponse(accessToken, newRefresh.TokenHash));
        }
        catch (InvalidOperationException)
        {
            return TypedResults.Unauthorized();
        }
    }

    // T039: Implemented
    private static Results<Ok<UserProfile>, UnauthorizedHttpResult> MeAsync(ClaimsPrincipal principal)
    {
        var id = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        var email = principal.FindFirstValue(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Email)
                    ?? principal.FindFirstValue(ClaimTypes.Email);

        if (id is null || email is null)
        {
            return TypedResults.Unauthorized();
        }

        return TypedResults.Ok(new UserProfile(Guid.Parse(id), email));
    }

    // T040: Implemented
    private static async Task<IResult> RevokeSessionAsync(
        [FromBody] RefreshRequest request,
        QuitlyDbContext dbContext,
        ITokenService tokenService,
        CancellationToken cancellationToken)
    {
        await tokenService.RevokeRefreshTokenAsync(request.RefreshToken, dbContext, cancellationToken);
        return TypedResults.NoContent();
    }

    public sealed record RegisterRequest(string Email, string Password, string Timezone);

    public sealed record LoginRequest(string Email, string Password);

    public sealed record RefreshRequest(string RefreshToken);

    public sealed record AuthResponse(string AccessToken, string RefreshToken);

    public sealed record UserProfile(Guid Id, string Email);
}
