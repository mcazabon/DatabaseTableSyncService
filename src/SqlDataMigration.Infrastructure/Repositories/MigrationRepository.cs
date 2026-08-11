using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SqlDataMigration.Core.Interfaces;
using SqlDataMigration.Core.Models;
using SqlDataMigration.Infrastructure.Configuration;
using SqlDataMigration.Infrastructure.Interfaces;
using System.Data;
using System.Reflection;

namespace SqlDataMigration.Infrastructure.Repositories;

/// <summary>
/// Repository for migration control data stored in SQL Server.
/// </summary>
public sealed class MigrationRepository : IMigrationRepository
{
    private readonly ISqlConnectionFactory _connectionFactory;
    private readonly ILogger<MigrationRepository> _logger;
    private readonly int _commandTimeout;

    public MigrationRepository(
        ISqlConnectionFactory connectionFactory,
        IOptions<MigrationOptions> options,
        ILogger<MigrationRepository> logger)
    {
        ArgumentNullException.ThrowIfNull(connectionFactory);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(logger);

        _connectionFactory = connectionFactory;
        _logger = logger;
        _commandTimeout = options.Value.CommandTimeoutSeconds;
    }

    /// <inheritdoc />
    public async Task<long> CreateMigrationRunAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating new migration run");

        await using var connection = await _connectionFactory.CreateTargetConnectionAsync(cancellationToken);

        await using var command = new SqlCommand("Migration.usp_CreateMigrationRun", connection)
        {
            CommandType = CommandType.StoredProcedure,
            CommandTimeout = _commandTimeout
        };

        command.Parameters.Add("@RunGuid", SqlDbType.UniqueIdentifier).Value = Guid.NewGuid();
        command.Parameters.Add("@StartedBy", SqlDbType.NVarChar, 255).Value = 
            Environment.UserName ?? "Unknown";
        command.Parameters.Add("@ApplicationVersion", SqlDbType.VarChar, 50).Value = 
            Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0";

        var sourceBuilder = new SqlConnectionStringBuilder(connection.ConnectionString);
        command.Parameters.Add("@SourceServer", SqlDbType.NVarChar, 255).Value = 
            sourceBuilder.DataSource;
        command.Parameters.Add("@SourceDatabase", SqlDbType.NVarChar, 255).Value = 
            sourceBuilder.InitialCatalog;

        var targetBuilder = new SqlConnectionStringBuilder(connection.ConnectionString);
        command.Parameters.Add("@TargetServer", SqlDbType.NVarChar, 255).Value = 
            targetBuilder.DataSource;
        command.Parameters.Add("@TargetDatabase", SqlDbType.NVarChar, 255).Value = 
            targetBuilder.InitialCatalog;

        var migrationRunIdParam = command.Parameters.Add("@MigrationRunId", SqlDbType.BigInt);
        migrationRunIdParam.Direction = ParameterDirection.Output;

        await command.ExecuteNonQueryAsync(cancellationToken);

        var migrationRunId = (long)migrationRunIdParam.Value;

        _logger.LogInformation("Created migration run with ID: {MigrationRunId}", migrationRunId);

        return migrationRunId;
    }

    /// <inheritdoc />
    public async Task CompleteMigrationRunAsync(
        long migrationRunId,
        MigrationRunStatus status,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Completing migration run {MigrationRunId} with status {Status}",
            migrationRunId,
            status);

        await using var connection = await _connectionFactory.CreateTargetConnectionAsync(cancellationToken);

        await using var command = new SqlCommand("Migration.usp_CompleteMigrationRun", connection)
        {
            CommandType = CommandType.StoredProcedure,
            CommandTimeout = _commandTimeout
        };

        command.Parameters.Add("@MigrationRunId", SqlDbType.BigInt).Value = migrationRunId;
        command.Parameters.Add("@Status", SqlDbType.VarChar, 30).Value = status.ToString();

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<long> StartTableAsync(
        long migrationRunId,
        MigrationTableDefinition table,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Starting table migration: {Schema}.{Table} (MigrationRunId: {MigrationRunId})",
            table.Schema,
            table.Name,
            migrationRunId);

        await using var connection = await _connectionFactory.CreateTargetConnectionAsync(cancellationToken);

        await using var command = new SqlCommand("Migration.usp_StartTableMigration", connection)
        {
            CommandType = CommandType.StoredProcedure,
            CommandTimeout = _commandTimeout
        };

        command.Parameters.Add("@MigrationRunId", SqlDbType.BigInt).Value = migrationRunId;
        command.Parameters.Add("@SchemaName", SqlDbType.NVarChar, 128).Value = table.Schema;
        command.Parameters.Add("@TableName", SqlDbType.NVarChar, 128).Value = table.Name;

        var tableExecutionIdParam = command.Parameters.Add("@TableExecutionId", SqlDbType.BigInt);
        tableExecutionIdParam.Direction = ParameterDirection.Output;

        await command.ExecuteNonQueryAsync(cancellationToken);

        var tableExecutionId = (long)tableExecutionIdParam.Value;

        _logger.LogInformation(
            "Started table execution with ID: {TableExecutionId}",
            tableExecutionId);

        return tableExecutionId;
    }

    /// <inheritdoc />
    public async Task CompleteTableAsync(
        long tableExecutionId,
        TableMigrationResult result,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Completing table execution {TableExecutionId}: {Schema}.{Table} - {Status}",
            tableExecutionId,
            result.Schema,
            result.TableName,
            result.Status);

        await using var connection = await _connectionFactory.CreateTargetConnectionAsync(cancellationToken);

        await using var command = new SqlCommand("Migration.usp_CompleteTableMigration", connection)
        {
            CommandType = CommandType.StoredProcedure,
            CommandTimeout = _commandTimeout
        };

        command.Parameters.Add("@TableExecutionId", SqlDbType.BigInt).Value = tableExecutionId;
        command.Parameters.Add("@Status", SqlDbType.VarChar, 30).Value = result.Status.ToString();
        command.Parameters.Add("@SourceRowCount", SqlDbType.BigInt).Value = result.SourceRows;
        command.Parameters.Add("@TargetRowCount", SqlDbType.BigInt).Value = result.TargetRows;
        command.Parameters.Add("@RowsTransferred", SqlDbType.BigInt).Value = result.RowsTransferred;
        command.Parameters.Add("@ValidationStatus", SqlDbType.VarChar, 30).Value = 
            result.ValidationStatus.ToString();

        if (result.ErrorMessage != null)
        {
            command.Parameters.Add("@ErrorMessage", SqlDbType.NVarChar, -1).Value = result.ErrorMessage;
        }

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task FailTableAsync(
        long tableExecutionId,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(
            exception,
            "Failing table execution {TableExecutionId}",
            tableExecutionId);

        await using var connection = await _connectionFactory.CreateTargetConnectionAsync(cancellationToken);

        await using var command = new SqlCommand("Migration.usp_FailTableMigration", connection)
        {
            CommandType = CommandType.StoredProcedure,
            CommandTimeout = _commandTimeout
        };

        command.Parameters.Add("@TableExecutionId", SqlDbType.BigInt).Value = tableExecutionId;
        command.Parameters.Add("@ErrorMessage", SqlDbType.NVarChar, -1).Value = 
            exception.ToString();

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<long> StartBatchAsync(
        long tableExecutionId,
        BatchRange batchRange,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug(
            "Starting batch {BatchNumber} for table execution {TableExecutionId}",
            batchRange.BatchNumber,
            tableExecutionId);

        await using var connection = await _connectionFactory.CreateTargetConnectionAsync(cancellationToken);

        await using var command = new SqlCommand("Migration.usp_StartBatch", connection)
        {
            CommandType = CommandType.StoredProcedure,
            CommandTimeout = _commandTimeout
        };

        command.Parameters.Add("@TableExecutionId", SqlDbType.BigInt).Value = tableExecutionId;
        command.Parameters.Add("@BatchNumber", SqlDbType.Int).Value = batchRange.BatchNumber;
        command.Parameters.Add("@StartKey", SqlDbType.BigInt).Value = batchRange.StartKey;
        command.Parameters.Add("@EndKey", SqlDbType.BigInt).Value = batchRange.EndKey;

        var batchExecutionIdParam = command.Parameters.Add("@BatchExecutionId", SqlDbType.BigInt);
        batchExecutionIdParam.Direction = ParameterDirection.Output;

        await command.ExecuteNonQueryAsync(cancellationToken);

        return (long)batchExecutionIdParam.Value;
    }

    /// <inheritdoc />
    public async Task CompleteBatchAsync(
        long batchExecutionId,
        long rowsProcessed,
        CancellationToken cancellationToken)
    {
        await using var connection = await _connectionFactory.CreateTargetConnectionAsync(cancellationToken);

        await using var command = new SqlCommand("Migration.usp_CompleteBatch", connection)
        {
            CommandType = CommandType.StoredProcedure,
            CommandTimeout = _commandTimeout
        };

        command.Parameters.Add("@BatchExecutionId", SqlDbType.BigInt).Value = batchExecutionId;
        command.Parameters.Add("@RowsProcessed", SqlDbType.BigInt).Value = rowsProcessed;

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task FailBatchAsync(
        long batchExecutionId,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(
            exception,
            "Failing batch execution {BatchExecutionId}",
            batchExecutionId);

        await using var connection = await _connectionFactory.CreateTargetConnectionAsync(cancellationToken);

        await using var command = new SqlCommand("Migration.usp_FailBatch", connection)
        {
            CommandType = CommandType.StoredProcedure,
            CommandTimeout = _commandTimeout
        };

        command.Parameters.Add("@BatchExecutionId", SqlDbType.BigInt).Value = batchExecutionId;
        command.Parameters.Add("@ErrorMessage", SqlDbType.NVarChar, -1).Value = 
            exception.ToString();

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    /// <inheritdoc />
    public async Task<long> GetSourceRowCountAsync(
        MigrationTableDefinition table,
        CancellationToken cancellationToken)
    {
        await using var connection = await _connectionFactory.CreateSourceConnectionAsync(cancellationToken);

        await using var command = new SqlCommand(
            $"SELECT COUNT_BIG(*) FROM [{table.Schema}].[{table.Name}]",
            connection)
        {
            CommandTimeout = _commandTimeout
        };

        var result = await command.ExecuteScalarAsync(cancellationToken);

        return result != null && result != DBNull.Value ? Convert.ToInt64(result) : 0;
    }

    /// <inheritdoc />
    public async Task<long> GetTargetRowCountAsync(
        MigrationTableDefinition table,
        CancellationToken cancellationToken)
    {
        await using var connection = await _connectionFactory.CreateTargetConnectionAsync(cancellationToken);

        await using var command = new SqlCommand(
            $"SELECT COUNT_BIG(*) FROM [{table.Schema}].[{table.Name}]",
            connection)
        {
            CommandTimeout = _commandTimeout
        };

        var result = await command.ExecuteScalarAsync(cancellationToken);

        return result != null && result != DBNull.Value ? Convert.ToInt64(result) : 0;
    }
}
