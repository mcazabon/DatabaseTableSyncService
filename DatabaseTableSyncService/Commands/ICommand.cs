namespace DatabaseTableSyncService.Commands;

/// <summary>
/// Interface for executable commands.
/// </summary>
public interface ICommand
{
    /// <summary>
    /// Gets the command name.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the command description.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Executes the command.
    /// </summary>
    /// <param name="args">Command arguments.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Exit code (0 = success, non-zero = failure).</returns>
    Task<int> ExecuteAsync(string[] args, CancellationToken cancellationToken);
}
