using Microsoft.Data.SqlClient;

namespace DatabaseTableSyncService.Commands;

/// <summary>
/// Command to validate migrated data without performing migration.
/// </summary>
public class ValidateCommand : ICommand
{
    private readonly ILogger<ValidateCommand> _logger;
    private readonly IConfiguration _configuration;

    public ValidateCommand(
        ILogger<ValidateCommand> logger,
        IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public string Name => "validate";

    public string Description => "Validate data between source and target (row counts)";

    public async Task<int> ExecuteAsync(string[] args, CancellationToken cancellationToken)
    {
        _logger.LogInformation("========================================");
        _logger.LogInformation("VALIDATE COMMAND");
        _logger.LogInformation("========================================");

        var sourceConnectionString = _configuration.GetConnectionString("SourceDatabase");
        var targetConnectionString = _configuration.GetConnectionString("TargetDatabase");

        if (string.IsNullOrWhiteSpace(sourceConnectionString))
        {
            _logger.LogError("Source database connection string not configured");
            return 1;
        }

        if (string.IsNullOrWhiteSpace(targetConnectionString))
        {
            _logger.LogError("Target database connection string not configured");
            return 1;
        }

        var sourceConnectionValid = await ValidateConnectionSettingsAsync(
            "Source",
            sourceConnectionString,
            cancellationToken);

        var targetConnectionValid = await ValidateConnectionSettingsAsync(
            "Target",
            targetConnectionString,
            cancellationToken);

        if (!sourceConnectionValid || !targetConnectionValid)
        {
            _logger.LogError("Validation failed due to invalid or unreachable database connection settings");
            return 1;
        }

        // Parse arguments
        string? specificTable = null;

        for (int i = 0; i < args.Length; i++)
        {
            if (IsOption(args[i], "table") && i + 1 < args.Length)
            {
                specificTable = args[i + 1];
                i++;
            }
        }

        var tables = _configuration.GetSection("Migration:Tables")
            .Get<List<TableConfig>>() ?? new List<TableConfig>();

        var tablesToValidate = string.IsNullOrEmpty(specificTable)
            ? tables.Where(t => t.Enabled).ToList()
            : tables.Where(t => t.Enabled && $"{t.Schema}.{t.Name}" == specificTable).ToList();

        if (tablesToValidate.Count == 0)
        {
            _logger.LogError("No tables found to validate");
            return 1;
        }

        _logger.LogInformation("Validating {Count} table(s)...", tablesToValidate.Count);

        // TODO: Phase 4 - Implement actual validation
        // foreach (var table in tablesToValidate)
        // {
        //     var result = await _validationService.ValidateAsync(table, cancellationToken);
        //     LogValidationResult(result);
        // }

        foreach (var table in tablesToValidate)
        {
            _logger.LogInformation("Table: {Schema}.{Name}", table.Schema, table.Name);
            _logger.LogInformation("  ✓ Row count validation: PASSED (placeholder)");
            _logger.LogInformation("  ✓ Key range validation: PASSED (placeholder)");
        }

        _logger.LogInformation("========================================");
        _logger.LogInformation("VALIDATION COMPLETED");
        _logger.LogInformation("========================================");

        await Task.CompletedTask;
        return 0;
    }

    private async Task<bool> ValidateConnectionSettingsAsync(
        string connectionName,
        string connectionString,
        CancellationToken cancellationToken)
    {
        try
        {
            var builder = new SqlConnectionStringBuilder(connectionString);

            if (string.IsNullOrWhiteSpace(builder.DataSource))
            {
                _logger.LogError("{ConnectionName} connection string is missing server/data source", connectionName);
                return false;
            }

            if (string.IsNullOrWhiteSpace(builder.InitialCatalog))
            {
                _logger.LogError("{ConnectionName} connection string is missing database/initial catalog", connectionName);
                return false;
            }

            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync(cancellationToken);

            return true;
        }
        catch (ArgumentException ex)
        {
            _logger.LogError("{ConnectionName} connection string format is invalid: {Message}", connectionName, ex.Message);
            return false;
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError("{ConnectionName} connection settings are invalid: {Message}", connectionName, ex.Message);
            return false;
        }
        catch (SqlException ex)
        {
            _logger.LogError("{ConnectionName} database connection failed: {Message}", connectionName, ex.Message);
            return false;
        }
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
