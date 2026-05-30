using Microsoft.EntityFrameworkCore;
using Quitly.Api.Persistence;

namespace Quitly.Api.Application.Metrics;

public sealed class KpiAggregationJob(IServiceScopeFactory serviceScopeFactory, ILogger<KpiAggregationJob> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await AggregateAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "KPI aggregation skipped due to an error. The application keeps running and will retry in 6 hours.");
            }

            try
            {
                await Task.Delay(TimeSpan.FromHours(6), stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }
    }

    private async Task AggregateAsync(CancellationToken cancellationToken)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<QuitlyDbContext>();

        var activeUsers = await dbContext.Users.CountAsync(cancellationToken);
        var weeklyCheckIns = await dbContext.CheckIns
            .CountAsync(item => item.Day >= DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-7)), cancellationToken);
        var recoveryCompletions = await dbContext.RecoveryPlanSteps
            .CountAsync(item => item.CompletedAt != null && item.CreatedAt >= DateTimeOffset.UtcNow.AddDays(-7), cancellationToken);

        logger.LogInformation(
            "KPI aggregation snapshot generated from operational data only. ActiveUsers={ActiveUsers}, WeeklyCheckIns={WeeklyCheckIns}, RecoveryCompletions={RecoveryCompletions}",
            activeUsers,
            weeklyCheckIns,
            recoveryCompletions);
    }
}
