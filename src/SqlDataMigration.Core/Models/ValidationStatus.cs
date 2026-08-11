namespace SqlDataMigration.Core.Models;

/// <summary>
/// Represents the status of data validation.
/// </summary>
public enum ValidationStatus
{
    /// <summary>
    /// Validation has not been performed.
    /// </summary>
    NotValidated,

    /// <summary>
    /// Validation is currently in progress.
    /// </summary>
    InProgress,

    /// <summary>
    /// Validation completed and data matches.
    /// </summary>
    Passed,

    /// <summary>
    /// Validation completed but data does not match.
    /// </summary>
    Failed,

    /// <summary>
    /// Validation encountered an error.
    /// </summary>
    Error
}
