using Microsoft.Data.SqlClient;

namespace DatabaseTableSyncService.Commands;

/// <summary>
/// Command to test database connections.
/// </summary>
public class TestConnectionCommand : ICommand
{
    private readonly ILogger<TestConnectionCommand> _logger;
    private readonly IConfiguration _configuration;

    public TestConnectionCommand(
        ILogger<TestConnectionCommand> logger,
        IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public string Name => "test-connection";

    public string Description => "Test source and target database connections";

    public async Task<int> ExecuteAsync(string[] args, CancellationToken cancellationToken)
    {
        _logger.LogInformation("========================================");
        _logger.LogInformation("TEST DATABASE CONNECTIONS");
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

        var success = true;

        // Test source connection
        _logger.LogInformation("");
        _logger.LogInformation("Testing SOURCE database connection...");
        success &= await TestConnectionAsync("Source", sourceConnectionString, cancellationToken);

        // Test target connection
        _logger.LogInformation("");
        _logger.LogInformation("Testing TARGET database connection...");
        success &= await TestConnectionAsync("Target", targetConnectionString, cancellationToken);

        _logger.LogInformation("");
        _logger.LogInformation("========================================");
        if (success)
        {
            _logger.LogInformation("✓ All connections successful");
            _logger.LogInformation("========================================");
            return 0;
        }
        else
        {
            _logger.LogError("✗ One or more connections failed");
            _logger.LogInformation("========================================");
            return 1;
        }
    }

    private async Task<bool> TestConnectionAsync(
        string name,
        string connectionString,
        CancellationToken cancellationToken)
    {
        try
        {
            var builder = new SqlConnectionStringBuilder(connectionString);

            _logger.LogInformation("  Server: {Server}", builder.DataSource);
            _logger.LogInformation("  Database: {Database}", builder.InitialCatalog);
            _logger.LogInformation("  Auth: {Auth}",
                builder.IntegratedSecurity ? "Windows Integrated" : "SQL Server");

            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync(cancellationToken);

            // Get SQL Server version
            await using var command = new SqlCommand("SELECT @@VERSION", connection);
            var version = await command.ExecuteScalarAsync(cancellationToken);

            _logger.LogInformation("  Status: ✓ CONNECTED");
            _logger.LogInformation("  Version: {Version}",
                version?.ToString()?.Split('\n')[0] ?? "Unknown");

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError("  Status: ✗ FAILED");
            _logger.LogError("  Error: {Message}", ex.Message);
            return false;
        }
    }
}
