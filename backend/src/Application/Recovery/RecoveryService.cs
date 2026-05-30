using Microsoft.EntityFrameworkCore;
using Quitly.Api.Domain.Entities;
using Quitly.Api.Infrastructure.Security;
using Quitly.Api.Persistence;

namespace Quitly.Api.Application.Recovery;

public sealed class RecoveryService(QuitlyDbContext dbContext, ICurrentUserAccessor currentUserAccessor, FieldEncryptor fieldEncryptor)
{
    public async Task<Relapse> CreateRelapseAsync(RelapseCreateCommand command, CancellationToken cancellationToken)
    {
        var userId = currentUserAccessor.GetRequiredUserId();
        var habit = await dbContext.Habits.SingleOrDefaultAsync(item => item.UserId == userId && item.Active, cancellationToken)
            ?? throw new ArgumentException("An active habit is required before recording a relapse.");

        var relapse = new Relapse
        {
            UserId = userId,
            HabitId = habit.Id,
            OccurredAt = command.OccurredAt,
            ContextNoteEncrypted = command.ContextNote is { Length: > 0 } note
                ? fieldEncryptor.Encrypt(note.Trim(), userId)
                : null,
            CreatedAt = DateTimeOffset.UtcNow
        };

        dbContext.Relapses.Add(relapse);
        await dbContext.SaveChangesAsync(cancellationToken);
        return relapse;
    }

    public async Task<RecoveryPlanStep> UpsertRecoveryStepAsync(RecoveryStepCommand command, CancellationToken cancellationToken)
    {
        var userId = currentUserAccessor.GetRequiredUserId();
        var relapse = await dbContext.Relapses.SingleOrDefaultAsync(item => item.Id == command.RelapseId && item.UserId == userId, cancellationToken)
            ?? throw new ArgumentException("Relapse event not found for current user.");

        var step = await dbContext.RecoveryPlanSteps
            .Where(item => item.RelapseId == relapse.Id && item.UserId == userId)
            .OrderByDescending(item => item.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (step is null)
        {
            step = new RecoveryPlanStep
            {
                RelapseId = relapse.Id,
                UserId = userId,
                CreatedAt = DateTimeOffset.UtcNow
            };

            dbContext.RecoveryPlanSteps.Add(step);
        }

        step.StepText = command.StepText.Trim();
        step.DueWithinHours = 24;
        step.CompletedAt = command.Completed ? DateTimeOffset.UtcNow : null;

        await dbContext.SaveChangesAsync(cancellationToken);
        return step;
    }
}

public sealed record RelapseCreateCommand(DateTimeOffset OccurredAt, string? ContextNote);

public sealed record RecoveryStepCommand(Guid RelapseId, string StepText, bool Completed);
