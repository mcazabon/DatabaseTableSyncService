# Quick Command Reference

## ✅ All Commands Working!

Your service now has a full CLI. Here's what you can test:

## 🚨 IMPORTANT: Two Ways to Run

### Option 1: Using `dotnet run` (Development)
```powershell
# Use -- to separate dotnet arguments from app arguments
dotnet run -- list-tables
```

### Option 2: Using Compiled `.exe` (Production)
```powershell
# NO -- separator when running executable directly!
DatabaseTableSyncService.exe list-tables
```

### 📋 Command List

```powershell
# ===== Using dotnet run =====
dotnet run -- help
dotnet run -- list-tables
dotnet run -- test-connection
dotnet run -- migrate --dry-run
dotnet run -- migrate
dotnet run -- migrate --table dbo.Table1
dotnet run -- validate
dotnet run -- validate --table dbo.Table1
dotnet run -- status --run-id 104

# ===== Using executable directly =====
DatabaseTableSyncService.exe help
DatabaseTableSyncService.exe list-tables
DatabaseTableSyncService.exe test-connection
DatabaseTableSyncService.exe migrate --dry-run
DatabaseTableSyncService.exe migrate
DatabaseTableSyncService.exe migrate --table dbo.Table1
DatabaseTableSyncService.exe validate
DatabaseTableSyncService.exe validate --table dbo.Table1
DatabaseTableSyncService.exe status --run-id 104
```

### 🎯 Quick Test Sequence

```powershell
# Step 1: See all commands
dotnet run -- help

# Step 2: Check configured tables
dotnet run -- list-tables

# Step 3: Dry run to validate config
dotnet run -- migrate --dry-run

# Step 4: Validate (placeholder for now)
dotnet run -- validate
```

### 💡 Real-World Usage (After Phase 3)

```powershell
# Before migration: test connections
dotnet run -- test-connection

# Test with one table first
dotnet run -- migrate --table dbo.Table1

# Validate that table
dotnet run -- validate --table dbo.Table1

# If successful, migrate all
dotnet run -- migrate

# Check status during migration
dotnet run -- status --run-id 104

# Validate all after completion
dotnet run -- validate
```

### ⚙️ Advanced Options

```powershell
# Dry run specific table
dotnet run -- migrate --table dbo.CustomerHistory --dry-run

# Different environment
$env:DOTNET_ENVIRONMENT = "Production"
dotnet run -- migrate

# Save output to file
dotnet run -- list-tables > tables.log
```

### 📊 Current Status

✅ **Working:**
- All commands parse correctly
- Configuration validation works
- Table listing works
- Dry-run mode works
- Help system works

⏳ **Waiting for Phase 3:**
- Actual data migration
- Database connectivity
- Status checking from database
- Validation queries

### 🚀 Next Steps

1. Update `appsettings.json` with real connection strings
2. Run `test-connection` to verify database access
3. Phase 3: Implement actual migration logic
4. Test with small tables first
5. Run full migration

---

**All commands are ready and waiting for Phase 3 implementation!** 🎉
