namespace SqlDataMigration.Infrastructure.Configuration;

/// <summary>
/// Configuration options for migration behavior.
/// </summary>
public sealed class MigrationOptions
{
    public const string SectionName = "Migration";

    /// <summary>
    /// SQL command timeout in seconds.
    /// </summary>
    public int CommandTimeoutSeconds { get; set; } = 3600;

    /// <summary>
    /// Number of rows to process per batch.
    /// </summary>
    public int BatchSize { get; set; } = 100000;

    /// <summary>
    /// Maximum number of retry attempts for transient failures.
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 3;

    /// <summary>
    /// Whether to enable parallel table migration.
    /// </summary>
    public bool EnableParallelTables { get; set; } = false;

    /// <summary>
    /// Maximum number of tables to migrate in parallel.
    /// </summary>
    public int MaximumParallelTables { get; set; } = 2;

    /// <summary>
    /// Whether to validate data after migration.
    /// </summary>
    public bool ValidateAfterMigration { get; set; } = true;

    /// <summary>
    /// Table definitions to migrate.
    /// </summary>
    public List<TableConfiguration> Tables { get; set; } = new();
}

/// <summary>
/// Configuration for a single table.
/// </summary>
public sealed class TableConfiguration
{
    /// <summary>
    /// The schema name.
    /// </summary>
    public string Schema { get; set; } = string.Empty;

    /// <summary>
    /// The table name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Whether this table is enabled for migration.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// The column to use for batching (typically primary key).
    /// </summary>
    public string BatchColumn { get; set; } = "Id";
}
