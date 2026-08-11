using SqlDataMigration.Core.Models;

namespace SqlDataMigration.Core.Interfaces;

/// <summary>
/// Orchestrates the overall migration process.
/// </summary>
public interface IMigrationOrchestrator
{
    /// <summary>
    /// Executes the migration process for all enabled tables.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The overall migration run result.</returns>
    Task<MigrationRunResult> ExecuteAsync(CancellationToken cancellationToken);
}
