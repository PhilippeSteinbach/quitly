namespace Quitly.Api.Configuration;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; init; } = "quitly-local";

    public string Audience { get; init; } = "quitly-web";

    public string SigningKey { get; init; } = "replace-with-a-local-development-key-at-least-32-characters";

    public int AccessTokenMinutes { get; init; } = 30;

    public int RefreshTokenDays { get; init; } = 14;
}
