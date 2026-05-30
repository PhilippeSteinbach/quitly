using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Quitly.Api.Application.Habits;
using Quitly.Api.Domain.Entities;
using Quitly.Api.Domain.Enums;
using Quitly.Api.Infrastructure.Security;

namespace Quitly.Api.Api;

public static class HabitEndpoints
{
    public static RouteGroupBuilder MapHabitEndpoints(this RouteGroupBuilder group)
    {
        var habits = group.MapGroup("/habit")
            .RequireAuthorization(OwnershipPolicy.OwnerPolicy);

        habits.MapGet(string.Empty, GetActiveHabitAsync);
        habits.MapPut(string.Empty, UpsertHabitAsync);

        return group;
    }

    private static async Task<Results<Ok<HabitResponse>, NotFound>> GetActiveHabitAsync(
        HabitService habitService,
        CancellationToken cancellationToken)
    {
        var habit = await habitService.GetActiveHabitAsync(cancellationToken);
        return habit is null
            ? TypedResults.NotFound()
            : TypedResults.Ok(HabitResponse.FromEntity(habit));
    }

    private static async Task<Ok<HabitResponse>> UpsertHabitAsync(
        [FromBody] HabitUpsertRequest request,
        HabitService habitService,
        CancellationToken cancellationToken)
    {
        var habit = await habitService.UpsertActiveHabitAsync(
            new HabitUpsertCommand(ParseCategory(request.Category), ParseMode(request.Mode), request.Title, request.StartedOn ?? DateOnly.FromDateTime(DateTime.UtcNow)),
            cancellationToken);

        return TypedResults.Ok(HabitResponse.FromEntity(habit));
    }

    private static HabitCategory ParseCategory(string value) => value.Trim().ToLowerInvariant() switch
    {
        "smoking" => HabitCategory.Smoking,
        "social_media" => HabitCategory.SocialMedia,
        "sugar" => HabitCategory.Sugar,
        "impulse_buying" => HabitCategory.ImpulseBuying,
        _ => HabitCategory.Custom
    };

    private static HabitMode ParseMode(string value) => value.Trim().ToLowerInvariant() switch
    {
        "reduce" => HabitMode.Reduce,
        _ => HabitMode.Quit
    };

    public sealed record HabitUpsertRequest(string Category, string Mode, string Title, DateOnly? StartedOn);

    public sealed record HabitResponse(Guid Id, string Category, string Mode, string Title, bool Active, DateOnly StartedOn)
    {
        public static HabitResponse FromEntity(Habit habit)
        {
            return new HabitResponse(
                habit.Id,
                ToContractCase(habit.Category.ToString()),
                habit.Mode.ToString().ToLowerInvariant(),
                habit.Title,
                habit.Active,
                habit.StartedOn);
        }

        private static string ToContractCase(string value) => value switch
        {
            nameof(HabitCategory.SocialMedia) => "social_media",
            nameof(HabitCategory.ImpulseBuying) => "impulse_buying",
            _ => value.ToLowerInvariant()
        };
    }
}
