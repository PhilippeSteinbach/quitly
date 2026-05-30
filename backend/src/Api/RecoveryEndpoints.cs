using Microsoft.AspNetCore.Mvc;
using Quitly.Api.Application.Recovery;
using Quitly.Api.Domain.Entities;
using Quitly.Api.Infrastructure.Security;

namespace Quitly.Api.Api;

public static class RecoveryEndpoints
{
    public static RouteGroupBuilder MapRecoveryEndpoints(this RouteGroupBuilder group)
    {
        var recovery = group.MapGroup(string.Empty).RequireAuthorization(OwnershipPolicy.OwnerPolicy);

        recovery.MapPost("/relapse", CreateRelapseAsync);
        recovery.MapPost("/recovery-steps", UpsertRecoveryStepAsync);

        return group;
    }

    private static async Task<Created<RelapseResponse>> CreateRelapseAsync(
        [FromBody] RelapseRequest request,
        RecoveryService recoveryService,
        CancellationToken cancellationToken)
    {
        var relapse = await recoveryService.CreateRelapseAsync(new RelapseCreateCommand(request.OccurredAt, request.ContextNote), cancellationToken);
        return TypedResults.Created($"/api/v1/relapse/{relapse.Id}", RelapseResponse.FromEntity(relapse));
    }

    private static async Task<Created<RecoveryStepResponse>> UpsertRecoveryStepAsync(
        [FromBody] RecoveryStepRequest request,
        RecoveryService recoveryService,
        CancellationToken cancellationToken)
    {
        var step = await recoveryService.UpsertRecoveryStepAsync(new RecoveryStepCommand(request.RelapseId, request.StepText, request.Completed), cancellationToken);
        return TypedResults.Created($"/api/v1/recovery-steps/{step.Id}", RecoveryStepResponse.FromEntity(step));
    }

    public sealed record RelapseRequest(DateTimeOffset OccurredAt, string? ContextNote);

    public sealed record RecoveryStepRequest(Guid RelapseId, string StepText, bool Completed = false);

    public sealed record RelapseResponse(Guid Id, DateTimeOffset OccurredAt, string? ContextNote, DateTimeOffset CreatedAt)
    {
        // ContextNote is decrypted by StreakEndpoints (Feature 008 calendar). Legacy callers get null.
        public static RelapseResponse FromEntity(Relapse entity) => new(entity.Id, entity.OccurredAt, null, entity.CreatedAt);
    }

    public sealed record RecoveryStepResponse(Guid Id, Guid RelapseId, string StepText, int DueWithinHours, DateTimeOffset? CompletedAt, DateTimeOffset CreatedAt)
    {
        public static RecoveryStepResponse FromEntity(RecoveryPlanStep entity) => new(entity.Id, entity.RelapseId, entity.StepText, entity.DueWithinHours, entity.CompletedAt, entity.CreatedAt);
    }
}
