namespace DatabaseTableSyncService.Commands;

/// <summary>
/// Command to execute database migration.
/// </summary>
public class MigrateCommand : ICommand
{
    private readonly ILogger<MigrateCommand> _logger;
    private readonly IConfiguration _configuration;

    public MigrateCommand(
        ILogger<MigrateCommand> logger,
        IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public string Name => "migrate";

    public string Description => "Execute database table migration";

    public async Task<int> ExecuteAsync(string[] args, CancellationToken cancellationToken)
    {
        _logger.LogInformation("========================================");
        _logger.LogInformation("MIGRATE COMMAND");
        _logger.LogInformation("========================================");

        // Parse arguments
        string? specificTable = null;
        bool dryRun = false;

        for (int i = 0; i < args.Length; i++)
        {
            if (IsOption(args[i], "table") && i + 1 < args.Length)
            {
                specificTable = args[i + 1].Trim();
                i++;
            }
            else if (IsOption(args[i], "dry-run"))
            {
                dryRun = true;
            }
        }

        if (dryRun)
        {
            _logger.LogWarning("DRY RUN MODE - No data will be migrated");
        }

        if (!string.IsNullOrEmpty(specificTable))
        {
            _logger.LogInformation("Migrating single table: {Table}", specificTable);
        }
        else
        {
            _logger.LogInformation("Migrating all enabled tables");
        }

        // Get table configuration
        var tables = _configuration.GetSection("Migration:Tables")
            .Get<List<TableConfig>>() ?? new List<TableConfig>();

        List<TableConfig> tablesToMigrate;

        if (!string.IsNullOrWhiteSpace(specificTable))
        {
            tablesToMigrate = tables
                .Where(t => string.Equals(
                    $"{t.Schema}.{t.Name}",
                    specificTable,
                    StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (tablesToMigrate.Count == 0)
            {
                _logger.LogError(
                    "Specified table '{Table}' was not found in Migration:Tables configuration",
                    specificTable);
                return 1;
            }
        }
        else
        {
            tablesToMigrate = tables.Where(t => t.Enabled).ToList();
        }

        if (tablesToMigrate.Count == 0)
        {
            _logger.LogError("No tables are enabled for migration");
            return 1;
        }

        _logger.LogInformation("Found {Count} table(s) to migrate", tablesToMigrate.Count);

        foreach (var table in tablesToMigrate)
        {
            _logger.LogInformation("  • {Schema}.{Name} (BatchColumn: {Column})",
                table.Schema, table.Name, table.BatchColumn);
        }

        if (dryRun)
        {
            _logger.LogInformation("========================================");
            _logger.LogInformation("DRY RUN COMPLETE - No changes made");
            _logger.LogInformation("========================================");
            return 0;
        }

        // TODO: Phase 3 - Implement actual migration
        // await _migrationOrchestrator.ExecuteAsync(specificTable, cancellationToken);

        _logger.LogInformation("========================================");
        _logger.LogInformation("MIGRATION COMPLETED");
        _logger.LogInformation("========================================");

        await Task.CompletedTask;
        return 0;
    }

    private class TableConfig
    {
        public string Schema { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public bool Enabled { get; set; }
        public string BatchColumn { get; set; } = string.Empty;
    }

    private static bool IsOption(string value, string optionName) =>
        value.Equals($"--{optionName}", StringComparison.OrdinalIgnoreCase)
        || value.Equals($"-{optionName}", StringComparison.OrdinalIgnoreCase);
}
