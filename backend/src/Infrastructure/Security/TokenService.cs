using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Quitly.Api.Configuration;
using Quitly.Api.Domain.Entities;
using Quitly.Api.Persistence;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Quitly.Api.Infrastructure.Security;

public interface ITokenService
{
    (string AccessToken, string RefreshToken) CreateTokens(User user);
    Task<RefreshToken> IssueRefreshTokenAsync(User user, QuitlyDbContext dbContext, CancellationToken cancellationToken = default);
    Task<(string AccessToken, RefreshToken NewRefreshToken)> RotateRefreshTokenAsync(string rawToken, QuitlyDbContext dbContext, CancellationToken cancellationToken = default);
    Task RevokeRefreshTokenAsync(string rawToken, QuitlyDbContext dbContext, CancellationToken cancellationToken = default);
}

public sealed class TokenService(IOptions<JwtOptions> jwtOptions) : ITokenService
{
    private const int MaxActiveTokensPerUser = 5;
    private const int RefreshTokenDays = 30;

    public (string AccessToken, string RefreshToken) CreateTokens(User user)
    {
        var accessToken = CreateAccessToken(user);
        var rawRefresh = GenerateRawToken();
        return (accessToken, rawRefresh);
    }

    public async Task<RefreshToken> IssueRefreshTokenAsync(User user, QuitlyDbContext dbContext, CancellationToken cancellationToken = default)
    {
        var rawToken = GenerateRawToken();
        var tokenHash = HashToken(rawToken);

        // Enforce max 5 active tokens per user — revoke oldest on overflow
        var activeTokens = await dbContext.RefreshTokens
            .Where(t => t.UserId == user.Id && t.RevokedAt == null && t.ExpiresAt > DateTimeOffset.UtcNow)
            .OrderBy(t => t.CreatedAt)
            .ToListAsync(cancellationToken);

        if (activeTokens.Count >= MaxActiveTokensPerUser)
        {
            foreach (var old in activeTokens.Take(activeTokens.Count - MaxActiveTokensPerUser + 1))
            {
                old.RevokedAt = DateTimeOffset.UtcNow;
            }
        }

        var refreshToken = new RefreshToken
        {
            UserId = user.Id,
            TokenHash = tokenHash,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(RefreshTokenDays)
        };

        dbContext.RefreshTokens.Add(refreshToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        // Detach before swapping so the change tracker still holds the hash
        dbContext.Entry(refreshToken).State = EntityState.Detached;
        refreshToken.TokenHash = rawToken; // swap hash for raw — one-time transport only
        return refreshToken;
    }

    public async Task<(string AccessToken, RefreshToken NewRefreshToken)> RotateRefreshTokenAsync(
        string rawToken, QuitlyDbContext dbContext, CancellationToken cancellationToken = default)
    {
        var tokenHash = HashToken(rawToken);
        var existing = await dbContext.RefreshTokens
            .Include(t => t.User)
            .SingleOrDefaultAsync(t => t.TokenHash == tokenHash, cancellationToken);

        if (existing is null || !existing.IsActive)
        {
            throw new InvalidOperationException("refresh_token_invalid");
        }

        // Revoke old token
        existing.RevokedAt = DateTimeOffset.UtcNow;

        // Issue new token
        var newRaw = GenerateRawToken();
        var newHash = HashToken(newRaw);

        var newToken = new RefreshToken
        {
            UserId = existing.UserId,
            TokenHash = newHash,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(RefreshTokenDays)
        };

        existing.ReplacedById = newToken.Id;
        dbContext.RefreshTokens.Add(newToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        var accessToken = CreateAccessToken(existing.User);
        dbContext.Entry(newToken).State = EntityState.Detached;
        newToken.TokenHash = newRaw; // swap for transport
        return (accessToken, newToken);
    }

    public async Task RevokeRefreshTokenAsync(string rawToken, QuitlyDbContext dbContext, CancellationToken cancellationToken = default)
    {
        var tokenHash = HashToken(rawToken);
        var existing = await dbContext.RefreshTokens
            .SingleOrDefaultAsync(t => t.TokenHash == tokenHash, cancellationToken);

        if (existing is null || existing.RevokedAt is not null)
        {
            return; // idempotent
        }

        existing.RevokedAt = DateTimeOffset.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private string CreateAccessToken(User user)
    {
        var options = jwtOptions.Value;
        var now = DateTime.UtcNow;
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email)
        };

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.SigningKey));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: options.Issuer,
            audience: options.Audience,
            claims: claims,
            notBefore: now,
            expires: now.AddMinutes(options.AccessTokenMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string GenerateRawToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(48);
        return Convert.ToBase64String(bytes);
    }

    public static string HashToken(string rawToken)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }
}
