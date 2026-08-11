namespace SqlDataMigration.Core.Models;

/// <summary>
/// Represents the migration status of an individual table.
/// </summary>
public enum MigrationStatus
{
    /// <summary>
    /// Table migration is waiting to start.
    /// </summary>
    Pending,

    /// <summary>
    /// Table migration is currently in progress.
    /// </summary>
    Running,

    /// <summary>
    /// Table migration completed successfully.
    /// </summary>
    Completed,

    /// <summary>
    /// Table migration failed.
    /// </summary>
    Failed,

    /// <summary>
    /// Table migration was skipped (disabled in configuration).
    /// </summary>
    Skipped
}
