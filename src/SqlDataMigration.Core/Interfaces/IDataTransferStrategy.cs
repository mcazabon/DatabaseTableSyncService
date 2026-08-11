using SqlDataMigration.Core.Models;

namespace SqlDataMigration.Core.Interfaces;

/// <summary>
/// Strategy for transferring data between source and target databases.
/// </summary>
public interface IDataTransferStrategy
{
    /// <summary>
    /// Transfers data for a table.
    /// </summary>
    /// <param name="table">The table definition.</param>
    /// <param name="migrationRunId">The migration run identifier.</param>
    /// <param name="tableExecutionId">The table execution identifier.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The transfer result.</returns>
    Task<TransferResult> TransferAsync(
        MigrationTableDefinition table,
        long migrationRunId,
        long tableExecutionId,
        CancellationToken cancellationToken);
}
