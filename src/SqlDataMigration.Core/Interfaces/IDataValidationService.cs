using SqlDataMigration.Core.Models;

namespace SqlDataMigration.Core.Interfaces;

/// <summary>
/// Service for validating migrated data.
/// </summary>
public interface IDataValidationService
{
    /// <summary>
    /// Validates that data was successfully migrated.
    /// </summary>
    /// <param name="table">The table to validate.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>The validation result.</returns>
    Task<TableValidationResult> ValidateAsync(
        MigrationTableDefinition table,
        CancellationToken cancellationToken);
}
