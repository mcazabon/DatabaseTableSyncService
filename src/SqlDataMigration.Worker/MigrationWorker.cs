using Microsoft.Extensions.Options;
using SqlDataMigration.Infrastructure.Configuration;

namespace SqlDataMigration.Worker;

/// <summary>
/// Background worker service for migration execution.
/// </summary>
public sealed class MigrationWorker : BackgroundService
{
    private readonly ILogger<MigrationWorker> _logger;
    private readonly IHostApplicationLifetime _applicationLifetime;
    private readonly MigrationOptions _migrationOptions;

    public MigrationWorker(
        ILogger<MigrationWorker> logger,
        IHostApplicationLifetime applicationLifetime,
        IOptions<MigrationOptions> migrationOptions)
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(applicationLifetime);
        ArgumentNullException.ThrowIfNull(migrationOptions);

        _logger = logger;
        _applicationLifetime = applicationLifetime;
        _migrationOptions = migrationOptions.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            _logger.LogInformation("SQL Data Migration Worker starting at: {Time}", DateTimeOffset.Now);

            // Phase 1/2: Configuration validation only
            ValidateConfiguration();

            _logger.LogInformation("Configuration validated successfully");
            _logger.LogInformation("Enabled tables: {Count}", 
                _migrationOptions.Tables.Count(t => t.Enabled));

            // Phase 3 will implement: await _orchestrator.ExecuteAsync(stoppingToken);

            _logger.LogInformation("SQL Data Migration Worker completed at: {Time}", DateTimeOffset.Now);
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Fatal error during migration execution");
            throw;
        }
        finally
        {
            // Stop the application after migration completes or fails
            _applicationLifetime.StopApplication();
        }
    }

    private void ValidateConfiguration()
    {
        _logger.LogInformation("Validating migration configuration");

        if (_migrationOptions.Tables.Count == 0)
        {
            throw new InvalidOperationException("No tables configured for migration");
        }

        var enabledTables = _migrationOptions.Tables.Where(t => t.Enabled).ToList();

        if (enabledTables.Count == 0)
        {
            throw new InvalidOperationException("No tables are enabled for migration");
        }

        foreach (var table in enabledTables)
        {
            if (string.IsNullOrWhiteSpace(table.Schema))
            {
                throw new InvalidOperationException($"Table '{table.Name}' has no schema specified");
            }

            if (string.IsNullOrWhiteSpace(table.Name))
            {
                throw new InvalidOperationException("A table has no name specified");
            }

            if (string.IsNullOrWhiteSpace(table.BatchColumn))
            {
                throw new InvalidOperationException(
                    $"Table '{table.Schema}.{table.Name}' has no batch column specified");
            }

            _logger.LogInformation(
                "Configured table: {Schema}.{Table} (BatchColumn: {Column})",
                table.Schema,
                table.Name,
                table.BatchColumn);
        }
    }
}
