using Microsoft.AspNetCore.Mvc;
using Quitly.Api.Application.CheckIns;
using Quitly.Api.Application.Streaks;
using Quitly.Api.Domain.Entities;
using Quitly.Api.Domain.Enums;
using Quitly.Api.Infrastructure.Security;

namespace Quitly.Api.Api;

public static class CheckInEndpoints
{
    public static RouteGroupBuilder MapCheckInEndpoints(this RouteGroupBuilder group)
    {
        var checkIns = group.MapGroup(string.Empty).RequireAuthorization(OwnershipPolicy.OwnerPolicy);

        checkIns.MapPost("/check-ins", UpsertCheckInAsync);
        checkIns.MapGet("/check-ins", ListCheckInsAsync);
        checkIns.MapGet("/streak", GetStreakAsync);

        return group;
    }

    private static async Task<Created<CheckInResponse>> UpsertCheckInAsync(
        [FromBody] CheckInRequest request,
        CheckInService checkInService,
        CancellationToken cancellationToken)
    {
        var checkIn = await checkInService.UpsertAsync(
            new CheckInUpsertCommand(
                request.Day,
                ParseStatus(request.Status),
                ParseMood(request.Mood),
                request.Triggers ?? Array.Empty<string>(),
                request.Note),
            cancellationToken);

        return TypedResults.Created($"/api/v1/check-ins/{checkIn.Id}", CheckInResponse.FromEntity(checkIn));
    }

    private static async Task<Ok<CheckInListResponse>> ListCheckInsAsync(
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to,
        CheckInService checkInService,
        CancellationToken cancellationToken)
    {
        var items = await checkInService.ListAsync(from, to, cancellationToken);
        return TypedResults.Ok(new CheckInListResponse(items.Select(CheckInResponse.FromEntity).ToArray()));
    }

    private static Ok<StreakResponse> GetStreakAsync()
    {
        // Legacy endpoint — replaced by GET /habits/{id}/streak in Feature 008.
        // Returns empty placeholder so existing clients don't break before migration.
        return TypedResults.Ok(new StreakResponse(0, null, null));
    }

    private static CheckInStatus ParseStatus(string value) => value.Trim().ToLowerInvariant() switch
    {
        "abstinent" => CheckInStatus.Abstinent,
        "non_abstinent" => CheckInStatus.NonAbstinent,
        _ => CheckInStatus.Unsure
    };

    private static MoodLevel? ParseMood(string? value) => value?.Trim().ToLowerInvariant() switch
    {
        "very_low" => MoodLevel.VeryLow,
        "low" => MoodLevel.Low,
        "neutral" => MoodLevel.Neutral,
        "good" => MoodLevel.Good,
        "very_good" => MoodLevel.VeryGood,
        _ => null
    };

    public sealed record CheckInRequest(DateOnly Day, string Status, string? Mood, string[]? Triggers, string? Note);

    public sealed record CheckInResponse(Guid Id, DateOnly Day, string Status, string? Mood, string[] Triggers, string? Note, DateTimeOffset CreatedAt)
    {
        public static CheckInResponse FromEntity(CheckIn item)
        {
            return new CheckInResponse(
                item.Id,
                item.Day,
                ToContractCase(item.Status.ToString()),
                item.Mood is null ? null : ToContractCase(item.Mood.ToString()),
                item.CheckInTriggers.Select(trigger => trigger.Trigger?.Code ?? string.Empty).Where(code => code.Length > 0).ToArray(),
                item.Note,
                item.CreatedAt);
        }

        private static string ToContractCase(string value) => value switch
        {
            nameof(CheckInStatus.NonAbstinent) => "non_abstinent",
            nameof(MoodLevel.VeryLow) => "very_low",
            nameof(MoodLevel.VeryGood) => "very_good",
            _ => value.ToLowerInvariant()
        };
    }

    public sealed record CheckInListResponse(CheckInResponse[] Items);

    public sealed record StreakResponse(int CurrentStreakDays, DateOnly? LastAbstinentDay, DateOnly? LastNonAbstinentDay);
}
