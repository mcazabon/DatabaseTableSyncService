namespace DatabaseTableSyncService.Commands;

/// <summary>
/// Command to list configured tables.
/// </summary>
public class ListTablesCommand : ICommand
{
    private readonly ILogger<ListTablesCommand> _logger;
    private readonly IConfiguration _configuration;

    public ListTablesCommand(
        ILogger<ListTablesCommand> logger,
        IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public string Name => "list-tables";

    public string Description => "List all configured tables";

    public Task<int> ExecuteAsync(string[] args, CancellationToken cancellationToken)
    {
        _logger.LogInformation("========================================");
        _logger.LogInformation("CONFIGURED TABLES");
        _logger.LogInformation("========================================");

        var tables = _configuration.GetSection("Migration:Tables")
            .Get<List<TableConfig>>() ?? new List<TableConfig>();

        if (tables.Count == 0)
        {
            _logger.LogWarning("No tables configured");
            return Task.FromResult(1);
        }

        _logger.LogInformation("");
        _logger.LogInformation("Total: {Count} table(s)", tables.Count);
        _logger.LogInformation("Enabled: {Enabled} | Disabled: {Disabled}",
            tables.Count(t => t.Enabled),
            tables.Count(t => !t.Enabled));
        _logger.LogInformation("");

        foreach (var table in tables)
        {
            var status = table.Enabled ? "✓ ENABLED " : "✗ DISABLED";
            var color = table.Enabled ? ConsoleColor.Green : ConsoleColor.Gray;

            Console.ForegroundColor = color;
            _logger.LogInformation("{Status} | {Schema}.{Name} | BatchColumn: {Column}",
                status, table.Schema, table.Name, table.BatchColumn);
            Console.ResetColor();
        }

        _logger.LogInformation("");
        _logger.LogInformation("========================================");

        return Task.FromResult(0);
    }

    private class TableConfig
    {
        public string Schema { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public bool Enabled { get; set; }
        public string BatchColumn { get; set; } = string.Empty;
    }
}
