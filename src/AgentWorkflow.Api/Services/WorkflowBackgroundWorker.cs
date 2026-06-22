using AgentWorkflow.Core.Application;

namespace AgentWorkflow.Api.Services;

public sealed class WorkflowBackgroundWorker(
    ITaskScheduler scheduler,
    ILogger<WorkflowBackgroundWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Workflow background worker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var processed = await scheduler.ProcessNextAsync(stoppingToken);
                if (processed is null)
                {
                    await scheduler.WaitForWorkAsync(stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Workflow background worker failed while processing queue work.");
                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
            }
        }

        logger.LogInformation("Workflow background worker stopped gracefully.");
    }
}
