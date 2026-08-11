namespace SqlDataMigration.Core.Models;

/// <summary>
/// Represents the result of migrating a single table.
/// </summary>
/// <param name="Schema">The schema name of the table.</param>
/// <param name="TableName">The table name.</param>
/// <param name="SourceRows">The number of rows in the source table.</param>
/// <param name="TargetRows">The number of rows in the target table after migration.</param>
/// <param name="RowsTransferred">The number of rows transferred during this migration.</param>
/// <param name="Status">The migration status.</param>
/// <param name="Duration">How long the migration took.</param>
/// <param name="ErrorMessage">Error message if migration failed.</param>
/// <param name="ValidationStatus">The validation status for this table.</param>
public sealed record TableMigrationResult(
    string Schema,
    string TableName,
    long SourceRows,
    long TargetRows,
    long RowsTransferred,
    MigrationStatus Status,
    TimeSpan Duration,
    string? ErrorMessage,
    ValidationStatus ValidationStatus = ValidationStatus.NotValidated);
