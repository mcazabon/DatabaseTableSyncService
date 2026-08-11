namespace SqlDataMigration.Core.Models;

/// <summary>
/// Represents a range of keys for batch processing.
/// </summary>
/// <param name="BatchNumber">The batch sequence number.</param>
/// <param name="StartKey">The starting key value for this batch.</param>
/// <param name="EndKey">The ending key value for this batch.</param>
public sealed record BatchRange(
    int BatchNumber,
    long StartKey,
    long EndKey)
{
    /// <summary>
    /// Gets the estimated number of rows in this batch.
    /// </summary>
    public long EstimatedRowCount => EndKey - StartKey + 1;
}
