namespace SqlDataMigration.Core.Models;

/// <summary>
/// Represents the result of validating a table.
/// </summary>
/// <param name="Schema">The schema name.</param>
/// <param name="TableName">The table name.</param>
/// <param name="SourceRowCount">Row count in source table.</param>
/// <param name="TargetRowCount">Row count in target table.</param>
/// <param name="Status">Validation status.</param>
/// <param name="ErrorMessage">Error message if validation failed.</param>
public sealed record TableValidationResult(
    string Schema,
    string TableName,
    long SourceRowCount,
    long TargetRowCount,
    ValidationStatus Status,
    string? ErrorMessage = null)
{
    /// <summary>
    /// Gets whether the row counts match.
    /// </summary>
    public bool RowCountsMatch => SourceRowCount == TargetRowCount;
}
