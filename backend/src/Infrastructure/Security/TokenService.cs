using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Quitly.Api.Configuration;
using Quitly.Api.Domain.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Quitly.Api.Infrastructure.Security;

public interface ITokenService
{
    (string AccessToken, string RefreshToken) CreateTokens(User user);
}

public sealed class TokenService(IOptions<JwtOptions> jwtOptions) : ITokenService
{
    public (string AccessToken, string RefreshToken) CreateTokens(User user)
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

        return (new JwtSecurityTokenHandler().WriteToken(token), CreateRefreshToken());
    }

    private static string CreateRefreshToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(48);
        return Convert.ToBase64String(bytes);
    }
}
