namespace DatabaseTableSyncService.Commands;

/// <summary>
/// Command to display help information.
/// </summary>
public class HelpCommand : ICommand
{
    private readonly ILogger<HelpCommand> _logger;

    public HelpCommand(ILogger<HelpCommand> logger)
    {
        _logger = logger;
    }

    public string Name => "help";

    public string Description => "Display help information";

    public Task<int> ExecuteAsync(string[] args, CancellationToken cancellationToken)
    {
        Console.WriteLine();
        Console.WriteLine("╔════════════════════════════════════════════════╗");
        Console.WriteLine("║   Database Table Synchronization Service      ║");
        Console.WriteLine("║   Command-Line Interface                       ║");
        Console.WriteLine("╚════════════════════════════════════════════════╝");
        Console.WriteLine();

        Console.WriteLine("USAGE:");
        Console.WriteLine("  dotnet run -- <command> [options]");
        Console.WriteLine();

        Console.WriteLine("COMMANDS:");
        Console.WriteLine();

        var commands = new[]
        {
            ("help", "Display help information"),
            ("list-tables", "List all configured tables"),
            ("test-connection", "Test source and target database connections"),
            ("migrate", "Execute database table migration"),
            ("validate", "Validate data between source and target (row counts)"),
            ("status", "Check migration run status")
        };

        foreach (var (name, description) in commands)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write($"  {name,-18}");
            Console.ResetColor();
            Console.WriteLine($"{description}");
        }

        Console.WriteLine();
        Console.WriteLine("EXAMPLES:");
        Console.WriteLine();
        Console.WriteLine("  # List all configured tables");
        Console.WriteLine("  dotnet run -- list-tables");
        Console.WriteLine();
        Console.WriteLine("  # Test database connections");
        Console.WriteLine("  dotnet run -- test-connection");
        Console.WriteLine();
        Console.WriteLine("  # Run migration for all enabled tables");
        Console.WriteLine("  dotnet run -- migrate");
        Console.WriteLine();
        Console.WriteLine("  # Run migration for a specific table");
        Console.WriteLine("  dotnet run -- migrate --table dbo.Table1");
        Console.WriteLine();
        Console.WriteLine("  # Dry run (validate without migrating)");
        Console.WriteLine("  dotnet run -- migrate --dry-run");
        Console.WriteLine();
        Console.WriteLine("  # Validate data after migration");
        Console.WriteLine("  dotnet run -- validate");
        Console.WriteLine();
        Console.WriteLine("  # Validate specific table");
        Console.WriteLine("  dotnet run -- validate --table dbo.Table1");
        Console.WriteLine();
        Console.WriteLine("  # Check migration status");
        Console.WriteLine("  dotnet run -- status --run-id 104");
        Console.WriteLine();

        Console.WriteLine("OPTIONS:");
        Console.WriteLine();
        Console.WriteLine("  --table <name>    Specify a single table (format: schema.table)");
        Console.WriteLine("  --run-id <id>     Specify migration run ID");
        Console.WriteLine("  --dry-run         Test without making changes");
        Console.WriteLine();

        Console.WriteLine("CONFIGURATION:");
        Console.WriteLine();
        Console.WriteLine("  Connection strings and table settings are in appsettings.json");
        Console.WriteLine("  Use User Secrets for development: dotnet user-secrets set <key> <value>");
        Console.WriteLine();

        return Task.FromResult(0);
    }
}
