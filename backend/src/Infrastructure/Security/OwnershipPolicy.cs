using System.Security.Claims;

namespace Quitly.Api.Infrastructure.Security;

public static class OwnershipPolicy
{
    public const string OwnerPolicy = "OwnerPolicy";

    public static AuthorizationOptions AddOwnershipPolicy(this AuthorizationOptions options)
    {
        options.AddPolicy(OwnerPolicy, policy => policy.RequireAuthenticatedUser().RequireClaim(ClaimTypes.NameIdentifier));
        return options;
    }
}

public interface ICurrentUserAccessor
{
    Guid GetRequiredUserId();
}

public sealed class CurrentUserAccessor(IHttpContextAccessor httpContextAccessor) : ICurrentUserAccessor
{
    public Guid GetRequiredUserId()
    {
        var claimValue = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

        return Guid.TryParse(claimValue, out var userId)
            ? userId
            : throw new UnauthorizedAccessException("Authenticated user is required.");
    }
}
