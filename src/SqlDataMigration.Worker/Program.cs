using SqlDataMigration.Infrastructure.Configuration;
using SqlDataMigration.Infrastructure.Data;
using SqlDataMigration.Worker;

var builder = Host.CreateApplicationBuilder(args);

// Configure connection strings
builder.Services.Configure<ConnectionStringOptions>(
    builder.Configuration.GetSection(ConnectionStringOptions.SectionName));

// Configure migration options
builder.Services.Configure<MigrationOptions>(
    builder.Configuration.GetSection(MigrationOptions.SectionName));

// Register core services (placeholders for now - will be implemented in Phase 3)
// builder.Services.AddSingleton<ISqlConnectionFactory, SqlConnectionFactory>();
// builder.Services.AddScoped<IMigrationRepository, MigrationRepository>();
// builder.Services.AddScoped<IDataValidationService, DataValidationService>();
// builder.Services.AddScoped<ITableMigrationService, TableMigrationService>();
// builder.Services.AddScoped<IMigrationOrchestrator, MigrationOrchestrator>();

// Register worker service
builder.Services.AddHostedService<MigrationWorker>();

var host = builder.Build();

await host.RunAsync();
