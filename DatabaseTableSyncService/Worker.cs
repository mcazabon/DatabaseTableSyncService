using DatabaseTableSyncService.Commands;

namespace DatabaseTableSyncService;

/// <summary>
/// Background worker service for database table synchronization.
/// This service runs once and then stops (not a continuous loop).
/// </summary>
public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IHostApplicationLifetime _applicationLifetime;
    private readonly IConfiguration _configuration;
    private readonly IEnumerable<ICommand> _commands;

    public Worker(
        ILogger<Worker> logger,
        IHostApplicationLifetime applicationLifetime,
        IConfiguration configuration,
        IEnumerable<ICommand> commands)
    {
        _logger = logger;
        _applicationLifetime = applicationLifetime;
        _configuration = configuration;
        _commands = commands;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        int exitCode;

        try
        {
            _logger.LogInformation("Executable: {Path}", Environment.ProcessPath ?? "<unknown>");
            _logger.LogInformation("Working directory: {Path}", Environment.CurrentDirectory);

            // Banner
            Console.WriteLine();
            Console.WriteLine("================================================");
            Console.WriteLine("  Database Table Synchronization Service");
            Console.WriteLine("  Version 1.0.0");
            Console.WriteLine("================================================");
            Console.WriteLine();

            // Get command-line arguments
            var args = Environment.GetCommandLineArgs().Skip(1).ToArray();
            _logger.LogDebug("Raw command line args: {Args}", string.Join(" ", args));
            var normalizedCommand = args.Length > 0
                ? args[0].TrimStart('-', '/')
                : string.Empty;
            _logger.LogDebug("Normalized command: {Command}", normalizedCommand);

            // If no command specified, show help
            if (args.Length == 0
                || normalizedCommand.Equals("help", StringComparison.OrdinalIgnoreCase)
                || normalizedCommand.Equals("h", StringComparison.OrdinalIgnoreCase))
            {
                var helpCommand = _commands.FirstOrDefault(c => c.Name == "help");
                exitCode = helpCommand != null
                    ? await helpCommand.ExecuteAsync(args, stoppingToken)
                    : 1;
            }
            else
            {
                // Find and execute the command
                var commandName = normalizedCommand;
                var command = _commands.FirstOrDefault(c => c.Name.Equals(commandName, StringComparison.OrdinalIgnoreCase));

                if (command == null)
                {
                    _logger.LogError("Unknown command: {Command}", commandName);
                    _logger.LogInformation("Run 'dotnet run -- help' to see available commands");
                    exitCode = 1;
                }
                else
                {
                    // Execute the command with remaining arguments
                    var commandArgs = args.Skip(1).ToArray();
                    exitCode = await command.ExecuteAsync(commandArgs, stoppingToken);
                }
            }

            Environment.ExitCode = exitCode;
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Fatal error during command execution");
            Environment.ExitCode = 1;
            throw;
        }
        finally
        {
            // Stop the application after completion
            _applicationLifetime.StopApplication();
        }
    }

    private void ValidateConfiguration()
    {
        _logger.LogInformation("Validating configuration...");

        // Check connection strings
        var sourceDb = _configuration.GetConnectionString("SourceDatabase");
        var targetDb = _configuration.GetConnectionString("TargetDatabase");

        if (string.IsNullOrWhiteSpace(sourceDb))
        {
            throw new InvalidOperationException(
                "Source database connection string is not configured. " +
                "Please update appsettings.json or use User Secrets.");
        }

        if (string.IsNullOrWhiteSpace(targetDb))
        {
            throw new InvalidOperationException(
                "Target database connection string is not configured. " +
                "Please update appsettings.json or use User Secrets.");
        }

        // Log connection info (without credentials)
        LogConnectionInfo("Source", sourceDb);
        LogConnectionInfo("Target", targetDb);

        // Check migration settings
        var batchSize = _configuration.GetValue<int>("Migration:BatchSize");
        var timeout = _configuration.GetValue<int>("Migration:CommandTimeoutSeconds");

        _logger.LogInformation("Migration Settings:");
        _logger.LogInformation("  Batch Size: {BatchSize:N0} rows", batchSize);
        _logger.LogInformation("  Command Timeout: {Timeout} seconds", timeout);
    }

    private void LogConnectionInfo(string name, string connectionString)
    {
        try
        {
            var builder = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(connectionString);
            _logger.LogInformation("{Name} Database:", name);
            _logger.LogInformation("  Server: {Server}", builder.DataSource);
            _logger.LogInformation("  Database: {Database}", builder.InitialCatalog);
            _logger.LogInformation("  Auth: {Auth}",
                builder.IntegratedSecurity ? "Windows Integrated" : "SQL Authentication");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not parse {Name} connection string", name);
        }
    }
}
