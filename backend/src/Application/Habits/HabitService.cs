using Microsoft.EntityFrameworkCore;
using Quitly.Api.Domain.Entities;
using Quitly.Api.Domain.Enums;
using Quitly.Api.Infrastructure.Security;
using Quitly.Api.Persistence;

namespace Quitly.Api.Application.Habits;

public sealed class HabitService(QuitlyDbContext dbContext, ICurrentUserAccessor currentUserAccessor)
{
    public async Task<Habit?> GetActiveHabitAsync(CancellationToken cancellationToken)
    {
        var userId = currentUserAccessor.GetRequiredUserId();

        return await dbContext.Habits
            .AsNoTracking()
            .SingleOrDefaultAsync(item => item.UserId == userId && item.Active, cancellationToken);
    }

    public async Task<Habit> UpsertActiveHabitAsync(HabitUpsertCommand command, CancellationToken cancellationToken)
    {
        var userId = currentUserAccessor.GetRequiredUserId();

        var existingHabits = await dbContext.Habits
            .Where(item => item.UserId == userId && item.Active)
            .ToListAsync(cancellationToken);

        foreach (var existing in existingHabits)
        {
            existing.Active = false;
            existing.UpdatedAt = DateTimeOffset.UtcNow;
        }

        var habit = new Habit
        {
            UserId = userId,
            Category = command.Category,
            Mode = command.Mode,
            Title = command.Title.Trim(),
            Active = true,
            StartedOn = command.StartedOn,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        dbContext.Habits.Add(habit);
        await dbContext.SaveChangesAsync(cancellationToken);
        return habit;
    }
}

public sealed record HabitUpsertCommand(HabitCategory Category, HabitMode Mode, string Title, DateOnly StartedOn);
