using SqlDataMigration.Core.Models;

namespace SqlDataMigration.Core.Interfaces;

/// <summary>
/// Repository for migration control data.
/// </summary>
public interface IMigrationRepository
{
    /// <summary>
    /// Creates a new migration run record.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The migration run identifier.</returns>
    Task<long> CreateMigrationRunAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Completes a migration run.
    /// </summary>
    /// <param name="migrationRunId">The migration run identifier.</param>
    /// <param name="status">The final status.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task CompleteMigrationRunAsync(
        long migrationRunId,
        MigrationRunStatus status,
        CancellationToken cancellationToken);

    /// <summary>
    /// Marks the start of a table migration.
    /// </summary>
    /// <param name="migrationRunId">The migration run identifier.</param>
    /// <param name="table">The table being migrated.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The table execution identifier.</returns>
    Task<long> StartTableAsync(
        long migrationRunId,
        MigrationTableDefinition table,
        CancellationToken cancellationToken);

    /// <summary>
    /// Marks the completion of a table migration.
    /// </summary>
    /// <param name="tableExecutionId">The table execution identifier.</param>
    /// <param name="result">The migration result.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task CompleteTableAsync(
        long tableExecutionId,
        TableMigrationResult result,
        CancellationToken cancellationToken);

    /// <summary>
    /// Records a table migration failure.
    /// </summary>
    /// <param name="tableExecutionId">The table execution identifier.</param>
    /// <param name="exception">The exception that occurred.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task FailTableAsync(
        long tableExecutionId,
        Exception exception,
        CancellationToken cancellationToken);

    /// <summary>
    /// Marks the start of a batch.
    /// </summary>
    /// <param name="tableExecutionId">The table execution identifier.</param>
    /// <param name="batchRange">The batch range being processed.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The batch execution identifier.</returns>
    Task<long> StartBatchAsync(
        long tableExecutionId,
        BatchRange batchRange,
        CancellationToken cancellationToken);

    /// <summary>
    /// Marks the completion of a batch.
    /// </summary>
    /// <param name="batchExecutionId">The batch execution identifier.</param>
    /// <param name="rowsProcessed">Number of rows processed in this batch.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task CompleteBatchAsync(
        long batchExecutionId,
        long rowsProcessed,
        CancellationToken cancellationToken);

    /// <summary>
    /// Records a batch failure.
    /// </summary>
    /// <param name="batchExecutionId">The batch execution identifier.</param>
    /// <param name="exception">The exception that occurred.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task FailBatchAsync(
        long batchExecutionId,
        Exception exception,
        CancellationToken cancellationToken);

    /// <summary>
    /// Gets the row count for a table in the source database.
    /// </summary>
    /// <param name="table">The table definition.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The row count.</returns>
    Task<long> GetSourceRowCountAsync(
        MigrationTableDefinition table,
        CancellationToken cancellationToken);

    /// <summary>
    /// Gets the row count for a table in the target database.
    /// </summary>
    /// <param name="table">The table definition.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The row count.</returns>
    Task<long> GetTargetRowCountAsync(
        MigrationTableDefinition table,
        CancellationToken cancellationToken);
}
