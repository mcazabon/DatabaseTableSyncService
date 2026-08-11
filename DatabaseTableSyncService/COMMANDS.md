# Command-Line Interface Guide

Your Database Table Synchronization Service now has a full command-line interface! Here are all the commands you can test.

## 📋 Available Commands

### 1. **help** - Display Help
Shows all available commands and examples.

```powershell
dotnet run -- help
```

### 2. **list-tables** - List Configured Tables
Shows all tables in your configuration with their enabled/disabled status.

```powershell
dotnet run -- list-tables
```

**Example Output:**
```
========================================
CONFIGURED TABLES
========================================

Total: 6 table(s)
Enabled: 6 | Disabled: 0

✓ ENABLED  | dbo.Table1 | BatchColumn: Id
✓ ENABLED  | dbo.Table2 | BatchColumn: Id
✓ ENABLED  | dbo.Table3 | BatchColumn: Id
✓ ENABLED  | dbo.Table4 | BatchColumn: Id
✓ ENABLED  | dbo.Table5 | BatchColumn: Id
✓ ENABLED  | dbo.Table6 | BatchColumn: Id

========================================
```

### 3. **test-connection** - Test Database Connections
Tests connectivity to source and target databases.

```powershell
dotnet run -- test-connection
```

**Example Output:**
```
========================================
TEST DATABASE CONNECTIONS
========================================

Testing SOURCE database connection...
  Server: SQLSERVER01
  Database: ProductionDB
  Auth: Windows Integrated
  Status: ✓ CONNECTED
  Version: Microsoft SQL Server 2022 (RTM) - 16.0.1000.6

Testing TARGET database connection...
  Server: SQLSERVER02
  Database: ProductionDB
  Auth: Windows Integrated
  Status: ✓ CONNECTED
  Version: Microsoft SQL Server 2022 (RTM) - 16.0.1000.6

========================================
✓ All connections successful
========================================
```

### 4. **migrate** - Execute Migration
Runs the database table migration.

```powershell
# Migrate all enabled tables
dotnet run -- migrate

# Migrate a specific table
dotnet run -- migrate --table dbo.Table1

# Dry run (test without migrating)
dotnet run -- migrate --dry-run

# Dry run for specific table
dotnet run -- migrate --table dbo.Table3 --dry-run
```

**Example Output:**
```
========================================
MIGRATE COMMAND
========================================
Migrating all enabled tables
Found 6 enabled table(s)
  • dbo.Table1 (BatchColumn: Id)
  • dbo.Table2 (BatchColumn: Id)
  • dbo.Table3 (BatchColumn: Id)
  • dbo.Table4 (BatchColumn: Id)
  • dbo.Table5 (BatchColumn: Id)
  • dbo.Table6 (BatchColumn: Id)
========================================
MIGRATION COMPLETED
========================================
```

### 5. **validate** - Validate Data
Validates row counts between source and target (runs without migrating).

```powershell
# Validate all tables
dotnet run -- validate

# Validate specific table
dotnet run -- validate --table dbo.Table1
```

**Example Output:**
```
========================================
VALIDATE COMMAND
========================================
Validating 6 table(s)...
Table: dbo.Table1
  ✓ Row count validation: PASSED (placeholder)
  ✓ Key range validation: PASSED (placeholder)
Table: dbo.Table2
  ✓ Row count validation: PASSED (placeholder)
  ✓ Key range validation: PASSED (placeholder)
========================================
VALIDATION COMPLETED
========================================
```

### 6. **status** - Check Migration Status
Checks the status of a migration run (requires run ID from database).

```powershell
dotnet run -- status --run-id 104
```

**Example Output:**
```
========================================
MIGRATION STATUS
========================================
Checking status for Migration Run ID: 104

Run ID: 104
Status: Running (placeholder)
Started: 2026-08-07 09:00:00
Tables Completed: 2 / 6
Current Table: dbo.Table3
Progress: 33.5%

========================================
```

## 🧪 Testing Examples

### Quick Test Sequence

```powershell
# 1. Show help
dotnet run -- help

# 2. List configured tables
dotnet run -- list-tables

# 3. Test database connections (update connection strings first!)
dotnet run -- test-connection

# 4. Dry run to test configuration
dotnet run -- migrate --dry-run

# 5. Validate (when you have data)
dotnet run -- validate

# 6. Check status (when you have a run ID)
dotnet run -- status --run-id 104
```

### Test Individual Tables

```powershell
# Test Table1 only
dotnet run -- migrate --table dbo.Table1 --dry-run

# Validate Table2
dotnet run -- validate --table dbo.Table2

# Migrate Table3 (when ready)
dotnet run -- migrate --table dbo.Table3
```

### Progressive Testing

```powershell
# Step 1: Verify configuration
dotnet run -- list-tables

# Step 2: Test connections (requires real connection strings)
dotnet run -- test-connection

# Step 3: Dry run to validate everything
dotnet run -- migrate --dry-run

# Step 4: Migrate one small table
dotnet run -- migrate --table dbo.Table1

# Step 5: Validate the migrated table
dotnet run -- validate --table dbo.Table1

# Step 6: Check status
dotnet run -- status --run-id 104

# Step 7: Run full migration
dotnet run -- migrate
```

## 🔧 PowerShell Test Script

Create `test-commands.ps1`:

```powershell
#!/usr/bin/env pwsh

Write-Host "`n=== Testing All Commands ===`n" -ForegroundColor Cyan

Write-Host "1. Help Command" -ForegroundColor Yellow
dotnet run -- help
Read-Host "`nPress Enter to continue"

Write-Host "`n2. List Tables" -ForegroundColor Yellow
dotnet run -- list-tables
Read-Host "`nPress Enter to continue"

Write-Host "`n3. Test Connections" -ForegroundColor Yellow
dotnet run -- test-connection
Read-Host "`nPress Enter to continue"

Write-Host "`n4. Dry Run Migration" -ForegroundColor Yellow
dotnet run -- migrate --dry-run
Read-Host "`nPress Enter to continue"

Write-Host "`n5. Validate" -ForegroundColor Yellow
dotnet run -- validate
Read-Host "`nPress Enter to continue"

Write-Host "`n=== All Commands Tested ===`n" -ForegroundColor Green
```

Run with:
```powershell
.\test-commands.ps1
```

## 📊 Command Reference Table

| Command | Purpose | Requires DB | Phase |
|---------|---------|-------------|-------|
| `help` | Show available commands | No | 1 |
| `list-tables` | List configured tables | No | 1 |
| `test-connection` | Test DB connectivity | Yes | 2 |
| `migrate --dry-run` | Validate config | No | 2 |
| `migrate` | Execute migration | Yes | 3 |
| `migrate --table` | Migrate one table | Yes | 3 |
| `validate` | Validate data | Yes | 4 |
| `status --run-id` | Check run status | Yes | 3 |

## 💡 Tips

### Debug Mode
Add `--verbosity detailed` for more output:
```powershell
dotnet run --verbosity detailed -- migrate --dry-run
```

### Environment Variables
Set environment before running:
```powershell
$env:DOTNET_ENVIRONMENT = "Production"
dotnet run -- migrate
```

### Exit Codes
Check if command succeeded:
```powershell
dotnet run -- list-tables
if ($LASTEXITCODE -eq 0) {
	Write-Host "Success!" -ForegroundColor Green
} else {
	Write-Host "Failed!" -ForegroundColor Red
}
```

### Pipe Output
Save command output:
```powershell
dotnet run -- list-tables > tables.log
dotnet run -- status --run-id 104 > status-104.log
```

## 🚀 Next Steps

1. **Update Connection Strings**: Edit `appsettings.json` with real database servers
2. **Test Connections**: Run `test-connection` to verify connectivity
3. **Configure Tables**: Update table names in `appsettings.json`
4. **Dry Run**: Test with `migrate --dry-run`
5. **Phase 3**: Once migration logic is implemented, test with small tables first

## ⚠️ Current Limitations (Phase 2)

- ✅ Commands work and parse arguments
- ✅ Configuration validation works
- ✅ Connection testing works
- ⏳ Actual migration not yet implemented (Phase 3)
- ⏳ Validation queries not yet implemented (Phase 4)
- ⏳ Status checking not yet implemented (Phase 3)

The commands are ready and will call the actual implementation once Phase 3 is complete!
