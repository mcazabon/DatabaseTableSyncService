namespace SqlDataMigration.Core.Models;

/// <summary>
/// Represents the type of validation performed.
/// </summary>
public enum ValidationType
{
    /// <summary>
    /// Compare row counts between source and target.
    /// </summary>
    RowCount,

    /// <summary>
    /// Compare minimum and maximum key values.
    /// </summary>
    KeyRange,

    /// <summary>
    /// Compare aggregate values (SUM, COUNT, etc.).
    /// </summary>
    Aggregate,

    /// <summary>
    /// Compare batch-level checksums or hashes.
    /// </summary>
    BatchChecksum,

    /// <summary>
    /// Schema validation between source and target.
    /// </summary>
    Schema
}
