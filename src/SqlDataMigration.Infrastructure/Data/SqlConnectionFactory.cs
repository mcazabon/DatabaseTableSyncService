using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SqlDataMigration.Infrastructure.Interfaces;

namespace SqlDataMigration.Infrastructure.Data;

/// <summary>
/// Factory for creating SQL Server connections.
/// </summary>
public sealed class SqlConnectionFactory : ISqlConnectionFactory
{
    private readonly string _sourceConnectionString;
    private readonly string _targetConnectionString;
    private readonly ILogger<SqlConnectionFactory> _logger;

    public SqlConnectionFactory(
        IOptions<ConnectionStringOptions> options,
        ILogger<SqlConnectionFactory> logger)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(logger);

        var connectionStrings = options.Value;

        if (string.IsNullOrWhiteSpace(connectionStrings.SourceDatabase))
        {
            throw new InvalidOperationException(
                "Source database connection string is not configured.");
        }

        if (string.IsNullOrWhiteSpace(connectionStrings.TargetDatabase))
        {
            throw new InvalidOperationException(
                "Target database connection string is not configured.");
        }

        _sourceConnectionString = connectionStrings.SourceDatabase;
        _targetConnectionString = connectionStrings.TargetDatabase;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<SqlConnection> CreateSourceConnectionAsync(
        CancellationToken cancellationToken)
    {
        _logger.LogDebug("Creating source database connection");

        var connection = new SqlConnection(_sourceConnectionString);

        try
        {
            await connection.OpenAsync(cancellationToken);
            _logger.LogDebug("Source connection opened successfully");
            return connection;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to open source database connection");
            await connection.DisposeAsync();
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<SqlConnection> CreateTargetConnectionAsync(
        CancellationToken cancellationToken)
    {
        _logger.LogDebug("Creating target database connection");

        var connection = new SqlConnection(_targetConnectionString);

        try
        {
            await connection.OpenAsync(cancellationToken);
            _logger.LogDebug("Target connection opened successfully");
            return connection;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to open target database connection");
            await connection.DisposeAsync();
            throw;
        }
    }
}

/// <summary>
/// Configuration for connection strings.
/// </summary>
public sealed class ConnectionStringOptions
{
    public const string SectionName = "ConnectionStrings";

    public string SourceDatabase { get; set; } = string.Empty;
    public string TargetDatabase { get; set; } = string.Empty;
}
