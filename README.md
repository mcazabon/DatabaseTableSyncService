# SQL Data Migration Utility

A production-ready C#/.NET application for migrating large SQL Server tables between instances with comprehensive validation, restart capability, and batch processing support.

## Overview

This utility is designed to migrate approximately six very large SQL Server tables (potentially terabytes of data) from a source SQL Server instance to a target instance. The application emphasizes:

- **Streaming data transfer** using `SqlDataReader` and `SqlBulkCopy`
- **SQL Server-based operations** for set-based processing
- **Batch processing** for large tables with restart capability
- **Comprehensive validation** at multiple levels
- **Migration auditing** with full history tracking
- **Configuration-driven** table definitions

## Architecture

The solution follows clean architecture principles:

### Projects

- **SqlDataMigration.Core**: Domain models, enums, and interfaces (no infrastructure dependencies)
- **SqlDataMigration.Infrastructure**: SQL Server implementations, repositories, and data access
- **SqlDataMigration.Worker**: Console application with dependency injection and orchestration
- **SqlDataMigration.UnitTests**: Unit tests for business logic
- **SqlDataMigration.IntegrationTests**: Integration tests for SQL Server operations

### Technology Stack

- **.NET 8.0**
- **Microsoft.Data.SqlClient** for database access
- **Microsoft.Extensions.Hosting** for dependency injection
- **Microsoft.Extensions.Configuration** for settings management
- **Microsoft.Extensions.Logging** for structured logging

## Database Setup

The migration control framework uses a dedicated `Migration` schema on the **target** database.

### Installation

1. Connect to your target SQL Server database
2. Run the master deployment script:

```powershell
sqlcmd -S YOUR_TARGET_SERVER -d YOUR_TARGET_DATABASE -i database\DeployAll.sql
```

Or execute scripts individually in this order:

```powershell
sqlcmd -S YOUR_TARGET_SERVER -d YOUR_TARGET_DATABASE -i database\Scripts\01_CreateSchema.sql
sqlcmd -S YOUR_TARGET_SERVER -d YOUR_TARGET_DATABASE -i database\Tables\MigrationRun.sql
sqlcmd -S YOUR_TARGET_SERVER -d YOUR_TARGET_DATABASE -i database\Tables\TableExecution.sql
sqlcmd -S YOUR_TARGET_SERVER -d YOUR_TARGET_DATABASE -i database\Tables\BatchExecution.sql
sqlcmd -S YOUR_TARGET_SERVER -d YOUR_TARGET_DATABASE -i database\Tables\ValidationResult.sql
sqlcmd -S YOUR_TARGET_SERVER -d YOUR_TARGET_DATABASE -i database\StoredProcedures\*.sql
```

### Migration Control Tables

The database framework includes:

- **Migration.Run**: Tracks overall migration execution
- **Migration.TableExecution**: Tracks individual table migrations
- **Migration.BatchExecution**: Tracks batch-level processing for restart capability
- **Migration.ValidationResult**: Stores validation results

### Stored Procedures

All control-table operations use stored procedures:

- `Migration.usp_CreateMigrationRun`
- `Migration.usp_CompleteMigrationRun`
- `Migration.usp_StartTableMigration`
- `Migration.usp_CompleteTableMigration`
- `Migration.usp_FailTableMigration`
- `Migration.usp_StartBatch`
- `Migration.usp_CompleteBatch`
- `Migration.usp_FailBatch`
- `Migration.usp_GetMigrationStatus`

## Configuration

Configure the application using `appsettings.json`:

### Connection Strings

```json
{
  "ConnectionStrings": {
	"SourceDatabase": "Server=SOURCE_SERVER;Database=SOURCE_DB;Integrated Security=true;Encrypt=true;",
	"TargetDatabase": "Server=TARGET_SERVER;Database=TARGET_DB;Integrated Security=true;Encrypt=true;"
  }
}
```

**Important**: Do not store passwords in configuration files. Use:
- Windows Integrated Security for development
- User Secrets for local development: `dotnet user-secrets set "ConnectionStrings:SourceDatabase" "your-connection-string"`
- Enterprise secret management (CyberArk, Azure Key Vault) for production

### Migration Settings

```json
{
  "Migration": {
	"CommandTimeoutSeconds": 3600,
	"BatchSize": 100000,
	"MaxRetryAttempts": 3,
	"EnableParallelTables": false,
	"MaximumParallelTables": 2,
	"ValidateAfterMigration": true,

	"Tables": [
	  {
		"Schema": "dbo",
		"Name": "YourTableName",
		"Enabled": true,
		"BatchColumn": "Id"
	  }
	]
  }
}
```

### Configuration Options

| Setting | Description | Default |
|---------|-------------|---------|
| `CommandTimeoutSeconds` | SQL command timeout | 3600 |
| `BatchSize` | Rows per batch | 100000 |
| `MaxRetryAttempts` | Retry attempts for transient failures | 3 |
| `EnableParallelTables` | Enable parallel table migration | false |
| `MaximumParallelTables` | Max tables to migrate in parallel | 2 |
| `ValidateAfterMigration` | Validate data after migration | true |

### Table Configuration

Each table requires:

- `Schema`: The schema name (e.g., "dbo")
- `Name`: The table name
- `Enabled`: Whether to migrate this table
- `BatchColumn`: The column to use for batching (typically a sequential numeric primary key)

## Building the Solution

```powershell
# Restore dependencies
dotnet restore SqlDataMigration.sln

# Build solution
dotnet build SqlDataMigration.sln --configuration Release

# Run tests
dotnet test SqlDataMigration.sln
```

## Running the Application

### Development

```powershell
cd src\SqlDataMigration.Worker
dotnet run
```

### Production

```powershell
dotnet publish src\SqlDataMigration.Worker -c Release -o publish
cd publish
SqlDataMigration.Worker.exe
```

## Development Status

### ✅ Phase 1 - Framework (COMPLETED)

- [x] Visual Studio solution structure
- [x] Project references and dependencies
- [x] Domain models and enums
- [x] Core interfaces
- [x] Configuration classes
- [x] Dependency injection setup
- [x] Logging infrastructure

### ✅ Phase 2 - Migration Database Framework (COMPLETED)

- [x] Migration schema
- [x] Control tables (Run, TableExecution, BatchExecution, ValidationResult)
- [x] Stored procedures
- [x] Repository implementation
- [x] SQL connection factory

### 🚧 Phase 3 - Single-Table Proof of Concept (NEXT)

- [ ] Implement `ITableMigrationService`
- [ ] Implement `IDataTransferStrategy` with `SqlBulkCopy`
- [ ] Batch range calculation
- [ ] Streaming data reader implementation
- [ ] Progress tracking
- [ ] Restart capability from last successful batch

### 📋 Phase 4 - Validation (PLANNED)

- [ ] Row count validation
- [ ] Min/max key validation
- [ ] Aggregate validation (SUM, COUNT, etc.)
- [ ] Optional batch checksum validation
- [ ] Schema validation

### 📋 Phase 5 - Multi-Table Migration (PLANNED)

- [ ] Sequential table migration
- [ ] Parallel table migration (optional)
- [ ] Configuration for all six tables
- [ ] Performance optimization

### 📋 Phase 6 - Operational Hardening (PLANNED)

- [ ] Retry logic for transient failures
- [ ] Resume from incomplete run
- [ ] Structured error reporting
- [ ] Performance metrics
- [ ] Dry-run mode
- [ ] Command-line interface
- [ ] Operational documentation

## Key Design Principles

### 1. Streaming Data Transfer

The application **never** loads entire tables into memory:

```csharp
// ✅ Correct - Streaming
await using var reader = await command.ExecuteReaderAsync();
await using var bulkCopy = new SqlBulkCopy(targetConnection);
bulkCopy.EnableStreaming = true;
await bulkCopy.WriteToServerAsync(reader);

// ❌ Incorrect - In-memory
var dataTable = new DataTable();
dataTable.Load(reader); // DON'T DO THIS
```

### 2. SQL Server Performs Heavy Lifting

SQL Server handles:
- Row counts via `COUNT_BIG(*)`
- Aggregate validation
- Key range identification
- Set-based comparisons

C# handles:
- Orchestration
- Batch coordination
- Progress tracking
- Error handling

### 3. Batch Processing with Restart

Large tables are processed in batches using key ranges:

```sql
SELECT *
FROM dbo.LargeTable
WHERE Id > @LastProcessedId
  AND Id <= @CurrentMaximumId
ORDER BY Id;
```

If migration fails at batch 42, restart continues from batch 43.

### 4. Configuration-Driven

Tables can be enabled/disabled without recompilation:

```json
{
  "Schema": "dbo",
  "Name": "Table3",
  "Enabled": false,  // Skip this table
  "BatchColumn": "Id"
}
```

## Validation Levels

The utility performs multi-level validation:

1. **Row Count**: Compare `COUNT_BIG(*)` between source and target
2. **Key Range**: Compare `MIN(Id)` and `MAX(Id)`
3. **Aggregate**: Compare `SUM()`, `COUNT()`, etc. for applicable columns
4. **Batch Checksum**: Optional batch-level data checksums (future)

## Logging

Structured logging captures:
- Migration run ID
- Table name
- Batch number
- Rows processed
- Duration
- Rows per second
- Errors and retries
- Validation results

Example log output:

```
[2026-08-07 10:15:23] Information: Creating new migration run
[2026-08-07 10:15:23] Information: Created migration run with ID: 104
[2026-08-07 10:15:24] Information: Starting table migration: dbo.Table1 (MigrationRunId: 104)
[2026-08-07 10:15:24] Information: Started table execution with ID: 450
```

## Security Considerations

- Use Windows Integrated Security where possible
- Never commit connection strings with passwords
- Use User Secrets for development
- Use enterprise secret management (CyberArk, Azure Key Vault) for production
- Configure encrypted SQL connections (`Encrypt=true`)
- Set `TrustServerCertificate=false` in production
- Use least-privilege service accounts

## Performance Considerations

### Before Migration

Consider these optimizations on the **target** database:

- Pre-size data files and transaction logs
- Temporarily disable non-essential nonclustered indexes
- Disable triggers (if safe)
- Use `SIMPLE` recovery model during migration (if acceptable)

### After Migration

- Rebuild indexes: `ALTER INDEX ALL ON dbo.TableName REBUILD`
- Update statistics: `UPDATE STATISTICS dbo.TableName WITH FULLSCAN`
- Re-enable triggers and constraints
- Restore recovery model

### During Migration

Monitor:
- SQL Server CPU and memory
- Disk I/O throughput
- Network bandwidth
- Transaction log growth

## Troubleshooting

### Migration Fails to Start

- Verify SQL Server connectivity
- Check connection strings
- Ensure Migration schema exists on target database
- Verify table exists in both source and target
- Check SQL Server permissions

### Batch Fails

- Review error message in `Migration.BatchExecution.ErrorMessage`
- Check SQL Server error logs
- Verify network stability
- Increase `CommandTimeoutSeconds` for large batches

### Validation Fails

- Check for ongoing writes to source or target
- Verify schema compatibility
- Review `Migration.ValidationResult` table for details

### Resume Migration

The migration automatically resumes from the last successful batch. To manually check status:

```sql
EXEC Migration.usp_GetMigrationStatus @MigrationRunId = 104;
```

## Testing

### Unit Tests

```powershell
dotnet test tests\SqlDataMigration.UnitTests
```

Unit tests cover:
- Configuration validation
- Domain model behavior
- Migration state transitions
- Validation logic

### Integration Tests

```powershell
dotnet test tests\SqlDataMigration.IntegrationTests
```

Integration tests require:
- SQL Server instance (LocalDB or full SQL Server)
- Test databases created
- Connection strings configured in `appsettings.Test.json`

## Contributing

1. Follow clean architecture principles
2. Use async/await for all I/O operations
3. Enable nullable reference types
4. Use file-scoped namespaces
5. Add XML documentation to public interfaces
6. Write unit tests for business logic
7. Write integration tests for SQL operations

## License

Internal enterprise use only.

## Support

For issues or questions, contact the database migration team.
