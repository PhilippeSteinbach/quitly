using Microsoft.EntityFrameworkCore;
using Quitly.Api.Domain.Entities;
using Quitly.Api.Domain.Enums;
using Quitly.Api.Infrastructure.Security;
using Quitly.Api.Persistence;

namespace Quitly.Api.Application.Prompts;

public sealed class PromptService(QuitlyDbContext dbContext, ICurrentUserAccessor currentUserAccessor)
{
    public async Task<PromptPayload> GetTodayAsync(CancellationToken cancellationToken)
    {
        var userId = currentUserAccessor.GetRequiredUserId();
        var reminder = await dbContext.Reminders.SingleOrDefaultAsync(item => item.UserId == userId, cancellationToken)
            ?? new Reminder { UserId = userId };

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var hasCheckInToday = await dbContext.CheckIns.AnyAsync(item => item.UserId == userId && item.Day == today, cancellationToken);

        if (!reminder.PassivePromptEnabled || hasCheckInToday)
        {
            return new PromptPayload(false, string.Empty);
        }

        var message = reminder.PromptTone == PromptTone.Gentle
            ? "A short check-in is enough for today. One minute counts."
            : "Your daily check-in is still open.";

        return new PromptPayload(true, message);
    }

    public async Task<Reminder> UpdatePreferencesAsync(UpdatePromptPreferenceCommand command, CancellationToken cancellationToken)
    {
        var userId = currentUserAccessor.GetRequiredUserId();
        var reminder = await dbContext.Reminders.SingleOrDefaultAsync(item => item.UserId == userId, cancellationToken);

        if (reminder is null)
        {
            reminder = new Reminder { UserId = userId };
            dbContext.Reminders.Add(reminder);
        }

        reminder.PassivePromptEnabled = command.PassivePromptEnabled;
        reminder.PromptTone = command.PromptTone;
        reminder.UpdatedAt = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        return reminder;
    }
}

public sealed record PromptPayload(bool ShowPrompt, string Message);

public sealed record UpdatePromptPreferenceCommand(bool PassivePromptEnabled, PromptTone PromptTone);
