using Microsoft.AspNetCore.Mvc;
using Quitly.Api.Application.Insights;
using Quitly.Api.Application.Prompts;
using Quitly.Api.Domain.Entities;
using Quitly.Api.Domain.Enums;
using Quitly.Api.Infrastructure.Security;

namespace Quitly.Api.Api;

public static class InsightPromptEndpoints
{
    public static RouteGroupBuilder MapInsightPromptEndpoints(this RouteGroupBuilder group)
    {
        var endpoints = group.MapGroup(string.Empty).RequireAuthorization(OwnershipPolicy.OwnerPolicy);

        endpoints.MapGet("/prompts/today", GetPromptAsync);
        endpoints.MapPut("/prompts/preferences", UpdatePromptPreferencesAsync);
        endpoints.MapGet("/insights/weekly", GetWeeklyInsightAsync);

        return group;
    }

    private static async Task<Ok<PromptPayloadResponse>> GetPromptAsync(
        PromptService promptService,
        CancellationToken cancellationToken)
    {
        var prompt = await promptService.GetTodayAsync(cancellationToken);
        return TypedResults.Ok(new PromptPayloadResponse(prompt.ShowPrompt, prompt.Message));
    }

    private static async Task<Ok<PromptPreferenceResponse>> UpdatePromptPreferencesAsync(
        [FromBody] PromptPreferenceRequest request,
        PromptService promptService,
        CancellationToken cancellationToken)
    {
        var reminder = await promptService.UpdatePreferencesAsync(
            new UpdatePromptPreferenceCommand(request.PassivePromptEnabled, ParsePromptTone(request.PromptTone)),
            cancellationToken);

        return TypedResults.Ok(new PromptPreferenceResponse(reminder.PassivePromptEnabled, reminder.PromptTone.ToString().ToLowerInvariant()));
    }

    private static async Task<Ok<WeeklyInsightResponse>> GetWeeklyInsightAsync(
        [FromQuery] DateOnly? weekStart,
        WeeklyInsightService weeklyInsightService,
        CancellationToken cancellationToken)
    {
        var insight = await weeklyInsightService.GetOrGenerateAsync(weekStart, cancellationToken);
        return TypedResults.Ok(WeeklyInsightResponse.FromEntity(insight));
    }

    private static PromptTone ParsePromptTone(string? value) => value?.Trim().ToLowerInvariant() switch
    {
        "neutral" => PromptTone.Neutral,
        _ => PromptTone.Gentle
    };

    public sealed record PromptPayloadResponse(bool ShowPrompt, string Message);

    public sealed record PromptPreferenceRequest(bool PassivePromptEnabled, string? PromptTone);

    public sealed record PromptPreferenceResponse(bool PassivePromptEnabled, string PromptTone);

    public sealed record WeeklyInsightResponse(DateOnly WeekStart, int CheckInCount, int AbstinentDays, string[] TopTriggers, Dictionary<string, int> MoodTrend, string SummaryText, string Confidence)
    {
        public static WeeklyInsightResponse FromEntity(WeeklyInsight entity) => new(
            entity.WeekStart,
            entity.CheckInCount,
            entity.AbstinentDays,
            entity.TopTriggers.ToArray(),
            entity.MoodTrend,
            entity.SummaryText,
            entity.Confidence.ToString().ToLowerInvariant());
    }
}
