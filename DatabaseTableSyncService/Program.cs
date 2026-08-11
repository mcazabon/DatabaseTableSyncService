using DatabaseTableSyncService;
using DatabaseTableSyncService.Commands;

var builder = Host.CreateApplicationBuilder(args);

// Register commands
builder.Services.AddSingleton<ICommand, MigrateCommand>();
builder.Services.AddSingleton<ICommand, ValidateCommand>();
builder.Services.AddSingleton<ICommand, ListTablesCommand>();
builder.Services.AddSingleton<ICommand, StatusCommand>();
builder.Services.AddSingleton<ICommand, TestConnectionCommand>();
builder.Services.AddSingleton<ICommand, HelpCommand>();

// Register the worker service
builder.Services.AddHostedService<Worker>();

// TODO: Phase 3 - Register migration services
// builder.Services.Configure<MigrationOptions>(
//     builder.Configuration.GetSection(MigrationOptions.SectionName));
// builder.Services.AddSingleton<ISqlConnectionFactory, SqlConnectionFactory>();
// builder.Services.AddScoped<IMigrationRepository, MigrationRepository>();
// builder.Services.AddScoped<IDataValidationService, DataValidationService>();
// builder.Services.AddScoped<ITableMigrationService, TableMigrationService>();
// builder.Services.AddScoped<IMigrationOrchestrator, MigrationOrchestrator>();

var host = builder.Build();

try
{
    await host.RunAsync();
    return 0; // Success
}
catch (Exception ex)
{
    Console.WriteLine();
    Console.WriteLine("================================================");
    Console.WriteLine("FATAL ERROR");
    Console.WriteLine("================================================");
    Console.WriteLine(ex.Message);
    Console.WriteLine();
    Console.WriteLine("See logs for full details.");
    Console.WriteLine("================================================");
    return 1; // Failure
}
