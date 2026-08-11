namespace SqlDataMigration.Core.Models;

/// <summary>
/// Represents the overall result of a migration run.
/// </summary>
/// <param name="MigrationRunId">The migration run identifier.</param>
/// <param name="Status">The overall status.</param>
/// <param name="TableResults">Results for each table that was processed.</param>
/// <param name="TotalDuration">Total time for the entire migration run.</param>
/// <param name="TotalRowsMigrated">Total rows migrated across all tables.</param>
public sealed record MigrationRunResult(
    long MigrationRunId,
    MigrationRunStatus Status,
    IReadOnlyList<TableMigrationResult> TableResults,
    TimeSpan TotalDuration,
    long TotalRowsMigrated);
