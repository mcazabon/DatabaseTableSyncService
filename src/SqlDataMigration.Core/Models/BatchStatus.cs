namespace SqlDataMigration.Core.Models;

/// <summary>
/// Represents the status of an individual batch within a table migration.
/// </summary>
public enum BatchStatus
{
    /// <summary>
    /// Batch is waiting to be processed.
    /// </summary>
    Pending,

    /// <summary>
    /// Batch is currently being processed.
    /// </summary>
    Running,

    /// <summary>
    /// Batch completed successfully.
    /// </summary>
    Completed,

    /// <summary>
    /// Batch processing failed.
    /// </summary>
    Failed
}
