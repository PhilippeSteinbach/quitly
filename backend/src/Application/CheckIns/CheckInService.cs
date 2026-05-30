using Microsoft.EntityFrameworkCore;
using Quitly.Api.Domain.Entities;
using Quitly.Api.Domain.Enums;
using Quitly.Api.Infrastructure.Security;
using Quitly.Api.Persistence;

namespace Quitly.Api.Application.CheckIns;

public sealed class CheckInService(QuitlyDbContext dbContext, ICurrentUserAccessor currentUserAccessor)
{
    public async Task<IReadOnlyList<CheckIn>> ListAsync(DateOnly? from, DateOnly? to, CancellationToken cancellationToken)
    {
        var userId = currentUserAccessor.GetRequiredUserId();
        var query = dbContext.CheckIns
            .AsNoTracking()
            .Include(item => item.CheckInTriggers)
            .ThenInclude(item => item.Trigger)
            .Where(item => item.UserId == userId);

        if (from is not null)
        {
            query = query.Where(item => item.Day >= from);
        }

        if (to is not null)
        {
            query = query.Where(item => item.Day <= to);
        }

        return await query.OrderByDescending(item => item.Day).ToListAsync(cancellationToken);
    }

    public async Task<CheckIn> UpsertAsync(CheckInUpsertCommand command, CancellationToken cancellationToken)
    {
        var userId = currentUserAccessor.GetRequiredUserId();
        var habit = await dbContext.Habits.SingleOrDefaultAsync(item => item.UserId == userId && item.Active, cancellationToken)
            ?? throw new ArgumentException("An active habit is required before saving a check-in.");

        var existing = await dbContext.CheckIns
            .Include(item => item.CheckInTriggers)
            .ThenInclude(item => item.Trigger)
            .SingleOrDefaultAsync(item => item.UserId == userId && item.Day == command.Day, cancellationToken);

        var checkIn = existing ?? new CheckIn
        {
            UserId = userId,
            HabitId = habit.Id,
            Day = command.Day,
            CreatedAt = DateTimeOffset.UtcNow
        };

        checkIn.Status = command.Status;
        checkIn.Mood = command.Mood;
        checkIn.Note = command.Note?.Trim();
        checkIn.Source = existing is null ? CheckInSource.Manual : CheckInSource.Correction;
        checkIn.UpdatedAt = DateTimeOffset.UtcNow;

        if (existing is null)
        {
            dbContext.CheckIns.Add(checkIn);
        }

        checkIn.CheckInTriggers.Clear();
        foreach (var triggerCode in command.TriggerCodes.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var normalizedCode = triggerCode.Trim().ToLowerInvariant();
            var trigger = await dbContext.Triggers.SingleOrDefaultAsync(item => item.Code == normalizedCode, cancellationToken)
                ?? new Trigger { Code = normalizedCode, Label = ToLabel(normalizedCode), Active = true };

            if (trigger.Id == Guid.Empty)
            {
                trigger.Id = Guid.NewGuid();
            }

            if (dbContext.Entry(trigger).State == EntityState.Detached)
            {
                dbContext.Triggers.Add(trigger);
            }

            checkIn.CheckInTriggers.Add(new CheckInTrigger
            {
                CheckIn = checkIn,
                Trigger = trigger,
                TriggerId = trigger.Id,
                CheckInId = checkIn.Id
            });
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return checkIn;
    }

    private static string ToLabel(string code)
    {
        return string.Join(' ', code.Split('_', '-', StringSplitOptions.RemoveEmptyEntries)
            .Select(segment => char.ToUpperInvariant(segment[0]) + segment[1..]));
    }
}

public sealed record CheckInUpsertCommand(DateOnly Day, CheckInStatus Status, MoodLevel? Mood, IReadOnlyList<string> TriggerCodes, string? Note);
