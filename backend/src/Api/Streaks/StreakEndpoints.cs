using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Quitly.Api.Application.Streaks;
using Quitly.Api.Domain.Entities;
using Quitly.Api.Infrastructure.Security;
using Quitly.Api.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Quitly.Api.Api;

public static class StreakEndpoints
{
    public static RouteGroupBuilder MapStreakEndpoints(this RouteGroupBuilder group)
    {
        var habits = group.MapGroup("/habits");

        // US1: GET /habits/{habitId}/streak — requires auth
        habits.MapGet("/{habitId:guid}/streak", GetStreakAsync)
            .RequireAuthorization(OwnershipPolicy.OwnerPolicy);

        // US3: GET /habits/{habitId}/calendar/{year}/{month}
        habits.MapGet("/{habitId:guid}/calendar/{year:int}/{month:int}", GetCalendarMonthAsync)
            .RequireAuthorization(OwnershipPolicy.OwnerPolicy);

        // US2: GET /habits/{habitId}/stats/{year}/{month}
        habits.MapGet("/{habitId:guid}/stats/{year:int}/{month:int}", GetMonthStatsAsync)
            .RequireAuthorization(OwnershipPolicy.OwnerPolicy);

        // US2: GET /habits/{habitId}/stats/{year}
        habits.MapGet("/{habitId:guid}/stats/{year:int}", GetYearStatsAsync)
            .RequireAuthorization(OwnershipPolicy.OwnerPolicy);

        // US2: POST /habits/{habitId}/relapses — record a relapse
        habits.MapPost("/{habitId:guid}/relapses", RecordRelapseAsync)
            .RequireAuthorization(OwnershipPolicy.OwnerPolicy);

        // GET /time — unauthenticated, rate-limited; used for clock sync
        group.MapGet("/time", GetServerTimeAsync)
            .RequireRateLimiting("time_sync");

        return group;
    }

    // ── US1: Streak ─────────────────────────────────────────────────────────

    private static async Task<Results<Ok<StreakResponse>, NotFound>> GetStreakAsync(
        Guid habitId,
        StreakService streakService,
        CancellationToken cancellationToken)
    {
        try
        {
            var dto = await streakService.GetStreakAsync(habitId, cancellationToken);
            return TypedResults.Ok(new StreakResponse(dto.HabitId, dto.CurrentStreakSeconds, dto.ServerUtcMs));
        }
        catch (ArgumentException)
        {
            return TypedResults.NotFound();
        }
    }

    // ── US3: Calendar ────────────────────────────────────────────────────────

    private static async Task<Results<Ok<CalendarMonthResponse>, NotFound>> GetCalendarMonthAsync(
        Guid habitId,
        int year,
        int month,
        CalendarService calendarService,
        CancellationToken cancellationToken)
    {
        if (month < 1 || month > 12)
            return TypedResults.NotFound();

        try
        {
            var dto = await calendarService.GetMonthAsync(habitId, year, month, cancellationToken);
            var days = dto.Days.Select(d => new CalendarDayResponse(
                d.Date.ToString("yyyy-MM-dd"),
                d.Status,
                d.Notes)).ToArray();
            return TypedResults.Ok(new CalendarMonthResponse(year, month, days));
        }
        catch (ArgumentException)
        {
            return TypedResults.NotFound();
        }
    }

    // ── US2: Stats ───────────────────────────────────────────────────────────

    private static async Task<Results<Ok<MonthStatsResponse>, NotFound>> GetMonthStatsAsync(
        Guid habitId,
        int year,
        int month,
        StatsService statsService,
        CancellationToken cancellationToken)
    {
        if (month < 1 || month > 12)
            return TypedResults.NotFound();

        try
        {
            var dto = await statsService.GetMonthStatsAsync(habitId, year, month, cancellationToken);
            return TypedResults.Ok(new MonthStatsResponse(
                dto.Year, dto.Month, dto.AbstinentDays, dto.RelevantDays, dto.RelapseCount, dto.IsCurrentMonth));
        }
        catch (ArgumentException)
        {
            return TypedResults.NotFound();
        }
    }

    private static async Task<Results<Ok<YearStatsResponse>, NotFound>> GetYearStatsAsync(
        Guid habitId,
        int year,
        StatsService statsService,
        CancellationToken cancellationToken)
    {
        try
        {
            var dto = await statsService.GetYearStatsAsync(habitId, year, cancellationToken);
            var months = dto.Months.Select(m => new MonthStatsResponse(
                m.Year, m.Month, m.AbstinentDays, m.RelevantDays, m.RelapseCount, m.IsCurrentMonth)).ToArray();
            return TypedResults.Ok(new YearStatsResponse(dto.Year, dto.TotalAbstinentDays, dto.TotalRelevantDays, months));
        }
        catch (ArgumentException)
        {
            return TypedResults.NotFound();
        }
    }

    // ── US2: Record relapse ──────────────────────────────────────────────────

    private static async Task<Results<Created<RelapseCreatedResponse>, BadRequest<Dictionary<string, string>>, NotFound>> RecordRelapseAsync(
        Guid habitId,
        [FromBody] RecordRelapseRequest request,
        QuitlyDbContext dbContext,
        ICurrentUserAccessor currentUserAccessor,
        FieldEncryptor fieldEncryptor,
        StreakService streakService,
        CancellationToken cancellationToken)
    {
        // Validate: contextNote ≤ 500 chars
        if (request.ContextNote is { Length: > 500 })
            return TypedResults.BadRequest(new Dictionary<string, string> { ["error"] = "context_note_too_long" });

        // Validate: occurredAt ≤ now
        if (request.OccurredAt > DateTimeOffset.UtcNow.AddMinutes(1))
            return TypedResults.BadRequest(new Dictionary<string, string> { ["error"] = "occurred_at_in_future" });

        var userId = currentUserAccessor.GetRequiredUserId();

        var habit = await dbContext.Habits
            .SingleOrDefaultAsync(h => h.Id == habitId && h.UserId == userId, cancellationToken);

        if (habit is null)
            return TypedResults.NotFound();

        var startedAt = habit.StartedAt
            ?? new DateTimeOffset(habit.StartedOn.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);

        if (request.OccurredAt < startedAt)
            return TypedResults.BadRequest(new Dictionary<string, string> { ["error"] = "occurred_at_before_started" });

        // Capture streak before relapse
        var relapseTimes = await dbContext.Relapses
            .AsNoTracking()
            .Where(r => r.HabitId == habitId && r.UserId == userId)
            .Select(r => r.OccurredAt)
            .ToListAsync(cancellationToken);

        var previousStreakSeconds = Domain.Calculation.StreakCalculator
            .CalculateCurrentSeconds(startedAt, relapseTimes, DateTimeOffset.UtcNow);

        var relapse = new Relapse
        {
            UserId = userId,
            HabitId = habitId,
            OccurredAt = request.OccurredAt,
            PreviousStreakSeconds = previousStreakSeconds,
            ContextNoteEncrypted = request.ContextNote is { Length: > 0 } note
                ? fieldEncryptor.Encrypt(note.Trim(), userId)
                : null,
            CreatedAt = DateTimeOffset.UtcNow
        };

        dbContext.Relapses.Add(relapse);
        await dbContext.SaveChangesAsync(cancellationToken);

        return TypedResults.Created(
            $"/api/v1/habits/{habitId}/relapses/{relapse.Id}",
            new RelapseCreatedResponse(relapse.Id, relapse.OccurredAt, previousStreakSeconds));
    }

    // ── Clock sync ───────────────────────────────────────────────────────────

    private static Ok<ServerTimeResponse> GetServerTimeAsync()
        => TypedResults.Ok(new ServerTimeResponse(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()));

    // ── DTOs ─────────────────────────────────────────────────────────────────

    public sealed record StreakResponse(Guid HabitId, long CurrentStreakSeconds, long ServerUtcMs);
    public sealed record ServerTimeResponse(long ServerUtcMs);
    public sealed record CalendarMonthResponse(int Year, int Month, IReadOnlyList<CalendarDayResponse> Days);
    public sealed record CalendarDayResponse(string Date, string Status, IReadOnlyList<string>? Notes);
    public sealed record MonthStatsResponse(int Year, int Month, int AbstinentDays, int RelevantDays, int RelapseCount, bool IsCurrentMonth);
    public sealed record YearStatsResponse(int Year, int TotalAbstinentDays, int TotalRelevantDays, IReadOnlyList<MonthStatsResponse> Months);
    public sealed record RecordRelapseRequest(DateTimeOffset OccurredAt, string? ContextNote);
    public sealed record RelapseCreatedResponse(Guid Id, DateTimeOffset OccurredAt, long PreviousStreakSeconds);
}
