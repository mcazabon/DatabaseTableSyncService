namespace SqlDataMigration.Core.Models;

/// <summary>
/// Defines a table to be migrated.
/// </summary>
/// <param name="Schema">The schema name of the table.</param>
/// <param name="Name">The table name.</param>
/// <param name="BatchColumn">The column to use for batching (typically a sequential numeric key).</param>
/// <param name="Enabled">Whether this table is enabled for migration.</param>
public sealed record MigrationTableDefinition(
    string Schema,
    string Name,
    string BatchColumn,
    bool Enabled)
{
    /// <summary>
    /// Gets the fully qualified table name.
    /// </summary>
    public string FullyQualifiedName => $"{Schema}.{Name}";
}
