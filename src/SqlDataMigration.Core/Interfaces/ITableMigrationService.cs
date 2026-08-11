using SqlDataMigration.Core.Models;

namespace SqlDataMigration.Core.Interfaces;

/// <summary>
/// Service responsible for migrating a single table.
/// </summary>
public interface ITableMigrationService
{
    /// <summary>
    /// Migrates a single table from source to target.
    /// </summary>
    /// <param name="table">The table definition to migrate.</param>
    /// <param name="migrationRunId">The migration run identifier.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The result of the table migration.</returns>
    Task<TableMigrationResult> MigrateAsync(
        MigrationTableDefinition table,
        long migrationRunId,
        CancellationToken cancellationToken);
}
