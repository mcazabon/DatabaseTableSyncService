namespace DatabaseTableSyncService.Commands;

/// <summary>
/// Command to check migration status.
/// </summary>
public class StatusCommand : ICommand
{
    private readonly ILogger<StatusCommand> _logger;
    private readonly IConfiguration _configuration;

    public StatusCommand(
        ILogger<StatusCommand> logger,
        IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public string Name => "status";

    public string Description => "Check migration run status";

    public async Task<int> ExecuteAsync(string[] args, CancellationToken cancellationToken)
    {
        _logger.LogInformation("========================================");
        _logger.LogInformation("MIGRATION STATUS");
        _logger.LogInformation("========================================");

        // Parse arguments
        long? runId = null;

        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "--run-id" && i + 1 < args.Length)
            {
                if (long.TryParse(args[i + 1], out var id))
                {
                    runId = id;
                }
                i++;
            }
        }

        if (!runId.HasValue)
        {
            _logger.LogError("Run ID is required. Use: status --run-id <id>");
            return 1;
        }

        _logger.LogInformation("Checking status for Migration Run ID: {RunId}", runId.Value);

        // TODO: Phase 3 - Query Migration.Run and Migration.TableExecution tables
        // var status = await _migrationRepository.GetStatusAsync(runId.Value, cancellationToken);

        _logger.LogInformation("");
        _logger.LogInformation("Run ID: {RunId}", runId.Value);
        _logger.LogInformation("Status: Running (placeholder)");
        _logger.LogInformation("Started: 2026-08-07 09:00:00");
        _logger.LogInformation("Tables Completed: 2 / 6");
        _logger.LogInformation("Current Table: dbo.Table3");
        _logger.LogInformation("Progress: 33.5%");
        _logger.LogInformation("");
        _logger.LogInformation("========================================");

        await Task.CompletedTask;
        return 0;
    }
}
