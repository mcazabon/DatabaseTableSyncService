namespace SqlDataMigration.Core.Models;

/// <summary>
/// Represents the overall status of a migration run.
/// </summary>
public enum MigrationRunStatus
{
    /// <summary>
    /// Migration run has been created but not started.
    /// </summary>
    Pending,

    /// <summary>
    /// Migration run is currently in progress.
    /// </summary>
    Running,

    /// <summary>
    /// Migration run completed successfully for all enabled tables.
    /// </summary>
    Completed,

    /// <summary>
    /// Migration run completed with one or more table failures.
    /// </summary>
    CompletedWithErrors,

    /// <summary>
    /// Migration run failed and could not complete.
    /// </summary>
    Failed,

    /// <summary>
    /// Migration run was cancelled by user or system.
    /// </summary>
    Cancelled
}
