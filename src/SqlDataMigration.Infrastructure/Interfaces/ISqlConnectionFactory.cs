using Microsoft.Data.SqlClient;

namespace SqlDataMigration.Infrastructure.Interfaces;

/// <summary>
/// Factory for creating SQL Server connections.
/// </summary>
public interface ISqlConnectionFactory
{
    /// <summary>
    /// Creates and opens a connection to the source database.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>An open SQL connection.</returns>
    Task<SqlConnection> CreateSourceConnectionAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Creates and opens a connection to the target database.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>An open SQL connection.</returns>
    Task<SqlConnection> CreateTargetConnectionAsync(CancellationToken cancellationToken);
}
