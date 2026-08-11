# How to Run the Service

## 🚨 The `--` Separator Explained

### The Rule:
- ✅ **Use `--` with `dotnet run`**
- ❌ **DO NOT use `--` when running `.exe` directly**

---

## Why?

The `--` tells `dotnet` to stop parsing its own arguments and pass everything after to your application.

### Example 1: dotnet run
```powershell
dotnet run -- list-tables
		   ↑
		   This tells dotnet: "stop here, pass list-tables to the app"
```

### Example 2: Running .exe directly
```powershell
DatabaseTableSyncService.exe list-tables
							 ↑
							 No dotnet command, so no separator needed
```

### What Happens If You Use `--` with .exe?
```powershell
DatabaseTableSyncService.exe -- list-tables
							 ↑
							 This becomes the first argument!
							 Your app sees: args[0] = "--", args[1] = "list-tables"
```

---

## ✅ Correct Usage

### During Development (using dotnet run)
```powershell
cd DatabaseTableSyncService
dotnet run -- help
dotnet run -- list-tables
dotnet run -- test-connection
dotnet run -- migrate --dry-run
dotnet run -- validate
```

### Production (using compiled .exe)
```powershell
cd bin\Debug\net8.0
DatabaseTableSyncService.exe help
DatabaseTableSyncService.exe list-tables
DatabaseTableSyncService.exe test-connection
DatabaseTableSyncService.exe migrate --dry-run
DatabaseTableSyncService.exe validate
```

### With Full Path
```powershell
# Good
C:\MyApp\DatabaseTableSyncService.exe list-tables

# Bad
C:\MyApp\DatabaseTableSyncService.exe -- list-tables
```

---

## ❌ Common Mistakes

| ❌ Wrong | ✅ Correct |
|----------|-----------|
| `DatabaseTableSyncService.exe -- help` | `DatabaseTableSyncService.exe help` |
| `DatabaseTableSyncService.exe -- list-tables` | `DatabaseTableSyncService.exe list-tables` |
| `dotnet run list-tables` | `dotnet run -- list-tables` |

---

## 📋 Quick Test

Try this to verify it works:

```powershell
# Navigate to project folder
cd C:\Users\mcazabon.DOZER\source\repos\DatabaseTableSyncService\DatabaseTableSyncService

# Using dotnet run (requires --)
dotnet run -- list-tables

# If you want to use the .exe, you'll need .NET 8 x64 runtime installed
# (Your machine currently has .NET 10 x64 and .NET 8 x86)
```

---

## 🎯 Summary

- **`dotnet run`** → Use `--` separator
- **`.exe`** → No `--` separator
- **Your error** → You used `DatabaseTableSyncService.exe -- list-tables` (wrong!)
- **Fix** → Use `DatabaseTableSyncService.exe list-tables` (no `--`)

**Or just stick with `dotnet run --` during development - it's simpler!**
