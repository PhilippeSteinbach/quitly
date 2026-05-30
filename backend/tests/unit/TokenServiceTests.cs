using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Quitly.Api.Configuration;
using Quitly.Api.Domain.Entities;
using Quitly.Api.Infrastructure.Security;
using Quitly.Api.Persistence;

namespace Quitly.Api.Tests.Unit;

public sealed class TokenServiceTests : IDisposable
{
    private readonly QuitlyDbContext _dbContext;
    private readonly TokenService _sut;

    public TokenServiceTests()
    {
        var options = new DbContextOptionsBuilder<QuitlyDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _dbContext = new QuitlyDbContext(options);

        var jwtOptions = Options.Create(new JwtOptions
        {
            SigningKey = "super-secret-test-key-that-is-long-enough-for-hmac",
            Issuer = "test",
            Audience = "test",
            AccessTokenMinutes = 15
        });

        _sut = new TokenService(jwtOptions);
    }

    public void Dispose() => _dbContext.Dispose();

    private User BuildUser() =>
        new User { Id = Guid.NewGuid(), Email = "test@example.com", PasswordHash = "hash" };

    // ── IssueRefreshToken ────────────────────────────────────────────────────

    [Fact]
    public async Task IssueRefreshToken_StoreshashNotRaw()
    {
        var user = BuildUser();

        var token = await _sut.IssueRefreshTokenAsync(user, _dbContext);

        // The raw token is returned to the caller via the swapped TokenHash property
        var raw = token.TokenHash;
        raw.Should().NotBeNullOrEmpty();

        // But the stored record must have the hash, not the raw value
        var stored = await _dbContext.RefreshTokens.SingleAsync();
        stored.TokenHash.Should().NotBe(raw);
        stored.TokenHash.Should().Be(TokenService.HashToken(raw));
    }

    [Fact]
    public async Task IssueRefreshToken_EnforcesMaxFiveActiveTokens()
    {
        var user = BuildUser();

        // Issue 5 tokens
        for (var i = 0; i < 5; i++)
        {
            await _sut.IssueRefreshTokenAsync(user, _dbContext);
        }

        var activeCount = await _dbContext.RefreshTokens.CountAsync(t => t.RevokedAt == null);
        activeCount.Should().Be(5);

        // Issue a 6th — should revoke the oldest
        await _sut.IssueRefreshTokenAsync(user, _dbContext);

        var activeAfter = await _dbContext.RefreshTokens.CountAsync(t => t.RevokedAt == null);
        activeAfter.Should().Be(5);

        var total = await _dbContext.RefreshTokens.CountAsync();
        total.Should().Be(6);
    }

    // ── RotateRefreshToken ───────────────────────────────────────────────────

    [Fact]
    public async Task RotateRefreshToken_RevokesOldAndIssuesNew()
    {
        var user = BuildUser();
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        var issued = await _sut.IssueRefreshTokenAsync(user, _dbContext);
        var rawToken = issued.TokenHash; // raw for transport

        var (accessToken, newRefresh) = await _sut.RotateRefreshTokenAsync(rawToken, _dbContext);

        // Old token is revoked
        var oldStored = await _dbContext.RefreshTokens
            .SingleAsync(t => t.TokenHash == TokenService.HashToken(rawToken));
        oldStored.RevokedAt.Should().NotBeNull();

        // New token is active
        var newRaw = newRefresh.TokenHash;
        var newStored = await _dbContext.RefreshTokens
            .SingleAsync(t => t.TokenHash == TokenService.HashToken(newRaw));
        newStored.RevokedAt.Should().BeNull();

        accessToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task RotateRefreshToken_ThrowsOnAlreadyRevokedToken()
    {
        var user = BuildUser();
        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        var issued = await _sut.IssueRefreshTokenAsync(user, _dbContext);
        var raw = issued.TokenHash;

        // First rotation succeeds
        await _sut.RotateRefreshTokenAsync(raw, _dbContext);

        // Second rotation with the same raw token should throw (replay rejection)
        var act = async () => await _sut.RotateRefreshTokenAsync(raw, _dbContext);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("refresh_token_invalid");
    }

    [Fact]
    public async Task RotateRefreshToken_ThrowsOnExpiredToken()
    {
        var user = BuildUser();

        var expired = new RefreshToken
        {
            UserId = user.Id,
            User = user,
            TokenHash = TokenService.HashToken("expired-token"),
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(-1)
        };

        _dbContext.RefreshTokens.Add(expired);
        await _dbContext.SaveChangesAsync();

        var act = async () => await _sut.RotateRefreshTokenAsync("expired-token", _dbContext);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("refresh_token_invalid");
    }

    // ── RevokeRefreshToken ───────────────────────────────────────────────────

    [Fact]
    public async Task RevokeRefreshToken_IsIdempotent()
    {
        var user = BuildUser();
        var issued = await _sut.IssueRefreshTokenAsync(user, _dbContext);
        var raw = issued.TokenHash;

        await _sut.RevokeRefreshTokenAsync(raw, _dbContext);
        // Second call should not throw
        var act = async () => await _sut.RevokeRefreshTokenAsync(raw, _dbContext);
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task RevokeRefreshToken_SetsRevokedAt()
    {
        var user = BuildUser();
        var issued = await _sut.IssueRefreshTokenAsync(user, _dbContext);
        var raw = issued.TokenHash;

        await _sut.RevokeRefreshTokenAsync(raw, _dbContext);

        var stored = await _dbContext.RefreshTokens
            .SingleAsync(t => t.TokenHash == TokenService.HashToken(raw));
        stored.RevokedAt.Should().NotBeNull();
    }
}
