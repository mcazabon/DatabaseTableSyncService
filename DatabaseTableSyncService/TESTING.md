# Testing the Database Table Synchronization Service

## Quick Start

### Option 1: Run from Visual Studio
1. Open the solution in Visual Studio 2026
2. Press **F5** (Debug) or **Ctrl+F5** (Run without debugging)
3. Watch the console output
4. Press **Ctrl+C** to stop

### Option 2: Run from PowerShell Script
```powershell
cd DatabaseTableSyncService
.\run-service.ps1
```

### Option 3: Run from Command Line
```powershell
cd DatabaseTableSyncService
dotnet run
```

## What You'll See

When the service runs successfully, you'll see:

```
================================================
  Database Table Synchronization Service
  Version 1.0.0
================================================

info: DatabaseTableSyncService.Worker[0]
	  ========================================
info: DatabaseTableSyncService.Worker[0]
	  Database Table Sync Service Starting
info: DatabaseTableSyncService.Worker[0]
	  Started at: 2026-08-07 09:49:56 -04:00
info: DatabaseTableSyncService.Worker[0]
	  ========================================
info: DatabaseTableSyncService.Worker[0]
	  Validating configuration...
info: DatabaseTableSyncService.Worker[0]
	  Source Database:
info: DatabaseTableSyncService.Worker[0]
	  Server: SOURCE_SERVER
info: DatabaseTableSyncService.Worker[0]
	  Database: SOURCE_DATABASE
info: DatabaseTableSyncService.Worker[0]
	  Auth: Windows Integrated
info: DatabaseTableSyncService.Worker[0]
	  Target Database:
info: DatabaseTableSyncService.Worker[0]
	  Server: TARGET_SERVER
info: DatabaseTableSyncService.Worker[0]
	  Database: TARGET_DATABASE
info: DatabaseTableSyncService.Worker[0]
	  Auth: Windows Integrated
info: DatabaseTableSyncService.Worker[0]
	  Migration Settings:
info: DatabaseTableSyncService.Worker[0]
	  Batch Size: 100,000 rows
info: DatabaseTableSyncService.Worker[0]
	  Command Timeout: 3600 seconds
info: DatabaseTableSyncService.Worker[0]
	  Configuration validated successfully
info: DatabaseTableSyncService.Worker[0]
	  Ready to begin migration
info: DatabaseTableSyncService.Worker[0]
	  ========================================
info: DatabaseTableSyncService.Worker[0]
	  Database Table Sync Service Completed
info: DatabaseTableSyncService.Worker[0]
	  Completed at: 2026-08-07 09:50:01 -04:00
info: DatabaseTableSyncService.Worker[0]
	  ========================================
```

## Testing Scenarios

### Scenario 1: Test Configuration Validation

**Test:** Run with default placeholder connection strings

**Expected:** Service starts, validates config, shows warnings about placeholders, then completes

**Command:**
```powershell
dotnet run
```

### Scenario 2: Test with User Secrets

**Setup:**
```powershell
# Set connection strings in User Secrets (won't be committed to git)
dotnet user-secrets set "ConnectionStrings:SourceDatabase" "Server=localhost;Database=TestSource;Integrated Security=true;"
dotnet user-secrets set "ConnectionStrings:TargetDatabase" "Server=localhost;Database=TestTarget;Integrated Security=true;"
```

**Command:**
```powershell
dotnet run
```

**Expected:** Service uses your actual connection strings

### Scenario 3: Test Different Environments

**Development:**
```powershell
dotnet run --environment Development
```

**Production:**
```powershell
dotnet run --environment Production
```

### Scenario 4: Debug in Visual Studio

1. Set breakpoint in `Worker.cs` at line in `ExecuteAsync`
2. Press **F5** to start debugging
3. Service will pause at breakpoint
4. Use **F10** (Step Over) or **F11** (Step Into) to debug
5. Inspect variables in the Locals/Watch windows

### Scenario 5: Test Connection String Errors

**Test invalid connection string:**

Edit `appsettings.json` and set:
```json
"SourceDatabase": ""
```

**Expected output:**
```
FATAL ERROR
================================================
Source database connection string is not configured.
Please update appsettings.json or use User Secrets.
================================================
```

## Debugging Tips

### View Detailed Logs

Update `appsettings.json`:
```json
{
  "Logging": {
	"LogLevel": {
	  "Default": "Debug",
	  "SqlDataMigration": "Trace"
	}
  }
}
```

### Test Specific Configuration

Create a custom config file:
```powershell
# Create appsettings.Test.json
dotnet run --environment Test
```

### Check Exit Codes

```powershell
dotnet run
echo "Exit code: $LASTEXITCODE"
```

- `0` = Success
- `1` = Failure

### Run with Verbose Output

```powershell
dotnet run --verbosity detailed
```

## Integration Testing Checklist

Before running against production databases:

- [ ] Test with placeholder connection strings (validation only)
- [ ] Test with local SQL Server instance
- [ ] Test with small test tables (< 1,000 rows)
- [ ] Verify batch size calculation
- [ ] Test restart capability
- [ ] Verify row count validation
- [ ] Test with one table at a time
- [ ] Monitor SQL Server performance
- [ ] Check transaction log growth
- [ ] Verify network bandwidth usage

## Common Issues

### Issue: "You must install or update .NET to run this application"

**Solution:** The project requires .NET 8 runtime. Your machine has .NET 10. Either:
1. Update `TargetFramework` in `.csproj` to `net10.0`
2. Install .NET 8 runtime from https://dotnet.microsoft.com/download

### Issue: Service runs but nothing happens

**Cause:** Worker service completes immediately because migration logic isn't implemented yet (Phase 3)

**Expected:** This is normal for Phase 1 & 2. Service validates config and exits.

### Issue: Can't connect to database

**Check:**
1. Connection strings are configured
2. SQL Server is running and accessible
3. Firewall allows connection
4. User has permission to connect

**Test connection:**
```powershell
sqlcmd -S YOUR_SERVER -d YOUR_DATABASE -E
```

## Performance Testing

### Monitor During Execution

**SQL Server Activity:**
```sql
-- Monitor active connections
SELECT * FROM sys.dm_exec_sessions WHERE program_name LIKE '%DatabaseTableSyncService%'

-- Monitor I/O
SELECT * FROM sys.dm_io_virtual_file_stats(NULL, NULL)
```

**PowerShell Resource Monitor:**
```powershell
while ($true) {
	Get-Process dotnet | Select-Object CPU, WS, PM | Format-Table
	Start-Sleep -Seconds 5
}
```

## Next Steps

Once Phase 3 is implemented, you'll be able to:
- Test actual data transfer with SqlBulkCopy
- Verify batch processing works correctly
- Test restart from failed batch
- Validate data after migration
- Monitor migration progress in real-time

---

**Current Status:** Phase 1 & 2 Complete - Configuration validation and framework ready
**Next Phase:** Implement data transfer logic
