namespace SqlDataMigration.Core.Models;

/// <summary>
/// Represents the result of a data transfer operation.
/// </summary>
/// <param name="RowsTransferred">Number of rows successfully transferred.</param>
/// <param name="Success">Whether the transfer was successful.</param>
/// <param name="ErrorMessage">Error message if transfer failed.</param>
/// <param name="Duration">How long the transfer took.</param>
public sealed record TransferResult(
    long RowsTransferred,
    bool Success,
    string? ErrorMessage,
    TimeSpan Duration);
