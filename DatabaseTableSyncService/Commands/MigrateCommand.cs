using Microsoft.Data.SqlClient;

namespace DatabaseTableSyncService.Commands;

/// <summary>
/// Command to execute database migration.
/// </summary>
public class MigrateCommand : ICommand
{
    private readonly ILogger<MigrateCommand> _logger;
    private readonly IConfiguration _configuration;

    public MigrateCommand(
        ILogger<MigrateCommand> logger,
        IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public string Name => "migrate";

    public string Description => "Execute database table migration";

    public async Task<int> ExecuteAsync(string[] args, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Raw migrate args: {Args}", string.Join(" ", args));

        _logger.LogInformation("========================================");
        _logger.LogInformation("MIGRATE COMMAND");
        _logger.LogInformation("========================================");

        // Parse arguments
        string? specificTable = null;
        bool dryRun = false;

        for (int i = 0; i < args.Length; i++)
        {
            if (IsOption(args[i], "table") && i + 1 < args.Length)
            {
                specificTable = args[i + 1].Trim();
                i++;
            }
            else if (IsOption(args[i], "dry-run"))
            {
                dryRun = true;
            }
        }

        _logger.LogDebug("Parsed migrate options: specificTable={SpecificTable}, dryRun={DryRun}",
            specificTable ?? "<none>",
            dryRun);

        if (dryRun)
        {
            _logger.LogWarning("DRY RUN MODE - No data will be migrated");
        }

        if (!string.IsNullOrEmpty(specificTable))
        {
            _logger.LogInformation("Migrating single table: {Table}", specificTable);
        }
        else
        {
            _logger.LogInformation("Migrating all enabled tables");
        }

        // Get table configuration
        var tables = _configuration.GetSection("Migration:Tables")
            .Get<List<TableConfig>>() ?? new List<TableConfig>();

        List<TableConfig> tablesToMigrate;

        if (!string.IsNullOrWhiteSpace(specificTable))
        {
            tablesToMigrate = tables
                .Where(t => string.Equals(
                    $"{t.Schema}.{t.Name}",
                    specificTable,
                    StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (tablesToMigrate.Count == 0)
            {
                _logger.LogError(
                    "Specified table '{Table}' was not found in Migration:Tables configuration",
                    specificTable);
                return 1;
            }
        }
        else
        {
            tablesToMigrate = tables.Where(t => t.Enabled).ToList();
        }

        if (tablesToMigrate.Count == 0)
        {
            _logger.LogError("No tables are enabled for migration");
            return 1;
        }

        _logger.LogInformation("Found {Count} table(s) to migrate", tablesToMigrate.Count);

        foreach (var table in tablesToMigrate)
        {
            _logger.LogInformation("  • {Schema}.{Name} (BatchColumn: {Column})",
                table.Schema, table.Name, table.BatchColumn);
        }

        if (dryRun)
        {
            _logger.LogInformation("Dry run enabled: data transfer is skipped, but connectivity and table validation still run");
        }

        var sourceConnectionString = _configuration.GetConnectionString("SourceDatabase");
        var targetConnectionString = _configuration.GetConnectionString("TargetDatabase");

        if (string.IsNullOrWhiteSpace(sourceConnectionString))
        {
            _logger.LogError("Source database connection string not configured");
            return 1;
        }

        if (string.IsNullOrWhiteSpace(targetConnectionString))
        {
            _logger.LogError("Target database connection string not configured");
            return 1;
        }

        var commandTimeoutSeconds = _configuration.GetValue<int>("Migration:CommandTimeoutSeconds", 3600);
        var batchSize = _configuration.GetValue<int>("Migration:BatchSize", 100000);

        if (commandTimeoutSeconds <= 0)
        {
            _logger.LogError("Migration:CommandTimeoutSeconds must be greater than 0");
            return 1;
        }

        if (batchSize <= 0)
        {
            _logger.LogError("Migration:BatchSize must be greater than 0");
            return 1;
        }

        await using var sourceConnection = new SqlConnection(sourceConnectionString);
        await using var targetConnection = new SqlConnection(targetConnectionString);

        try
        {
            await sourceConnection.OpenAsync(cancellationToken);
            await targetConnection.OpenAsync(cancellationToken);
        }
        catch (SqlException ex)
        {
            _logger.LogError(ex, "Failed to connect to source/target SQL Server");
            return 1;
        }

        var totalRowsCopied = 0L;
        var startedAt = DateTimeOffset.UtcNow;

        foreach (var table in tablesToMigrate)
        {
            var tableName = $"{table.Schema}.{table.Name}";
            _logger.LogInformation("----------------------------------------");
            _logger.LogInformation("Processing table {Table}", tableName);

            var validationResult = await ValidateTableAsync(
                sourceConnection,
                targetConnection,
                table,
                commandTimeoutSeconds,
                cancellationToken);

            if (!validationResult.Success)
            {
                _logger.LogError("Table validation failed for {Table}: {Reason}", tableName, validationResult.ErrorMessage);
                return 1;
            }

            if (dryRun)
            {
                _logger.LogInformation("Dry run: validation passed for {Table}", tableName);
                continue;
            }

            var rowsCopied = await MigrateTableAsync(
                sourceConnection,
                targetConnection,
                table,
                commandTimeoutSeconds,
                batchSize,
                cancellationToken);

            if (rowsCopied < 0)
            {
                return 1;
            }

            totalRowsCopied += rowsCopied;
        }

        var duration = DateTimeOffset.UtcNow - startedAt;

        _logger.LogInformation("========================================");
        if (dryRun)
        {
            _logger.LogInformation("DRY RUN COMPLETE - No changes made");
        }
        else
        {
            _logger.LogInformation("MIGRATION COMPLETED");
            _logger.LogInformation("Total rows copied: {Rows:N0}", totalRowsCopied);
            _logger.LogInformation("Total duration: {Duration}", duration);
        }
        _logger.LogInformation("========================================");
        return 0;
    }

    private async Task<TableValidationResult> ValidateTableAsync(
        SqlConnection sourceConnection,
        SqlConnection targetConnection,
        TableConfig table,
        int commandTimeoutSeconds,
        CancellationToken cancellationToken)
    {
        var sourceColumns = await GetColumnsAsync(
            sourceConnection,
            table.Schema,
            table.Name,
            commandTimeoutSeconds,
            cancellationToken);

        if (sourceColumns.Count == 0)
        {
            return TableValidationResult.Failed("Source table does not exist or has no columns");
        }

        var targetColumns = await GetColumnsAsync(
            targetConnection,
            table.Schema,
            table.Name,
            commandTimeoutSeconds,
            cancellationToken);

        if (targetColumns.Count == 0)
        {
            return TableValidationResult.Failed("Target table does not exist or has no columns");
        }

        if (!sourceColumns.Any(c => c.Equals(table.BatchColumn, StringComparison.OrdinalIgnoreCase)))
        {
            return TableValidationResult.Failed($"BatchColumn '{table.BatchColumn}' does not exist in source table");
        }

        if (!targetColumns.Any(c => c.Equals(table.BatchColumn, StringComparison.OrdinalIgnoreCase)))
        {
            return TableValidationResult.Failed($"BatchColumn '{table.BatchColumn}' does not exist in target table");
        }

        var missingInTarget = sourceColumns
            .Where(sourceColumn => !targetColumns.Contains(sourceColumn, StringComparer.OrdinalIgnoreCase))
            .ToList();

        if (missingInTarget.Count > 0)
        {
            return TableValidationResult.Failed(
                $"Target table is missing source columns: {string.Join(", ", missingInTarget)}");
        }

        return TableValidationResult.Passed();
    }

    private async Task<long> MigrateTableAsync(
        SqlConnection sourceConnection,
        SqlConnection targetConnection,
        TableConfig table,
        int commandTimeoutSeconds,
        int batchSize,
        CancellationToken cancellationToken)
    {
        var tableName = $"{QuoteIdentifier(table.Schema)}.{QuoteIdentifier(table.Name)}";
        var batchColumn = QuoteIdentifier(table.BatchColumn);

        var sourceStats = await GetSourceStatsAsync(
            sourceConnection,
            tableName,
            batchColumn,
            commandTimeoutSeconds,
            cancellationToken);

        if (!sourceStats.Success)
        {
            _logger.LogError("Cannot migrate {Table}: {Reason}", $"{table.Schema}.{table.Name}", sourceStats.ErrorMessage);
            return -1;
        }

        _logger.LogInformation(
            "Source rows: {Rows:N0}, key range: {MinKey:N0}..{MaxKey:N0}",
            sourceStats.TotalRows,
            sourceStats.MinKey,
            sourceStats.MaxKey);

        if (sourceStats.TotalRows == 0)
        {
            _logger.LogInformation("Source table is empty. Skipping copy.");
            return 0;
        }

        var targetCountBefore = await GetTableCountAsync(
            targetConnection,
            tableName,
            commandTimeoutSeconds,
            cancellationToken);

        var copiedRows = 0L;
        var batchNumber = 0L;
        var currentStart = sourceStats.MinKey;

        while (currentStart <= sourceStats.MaxKey)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var currentEnd = currentStart > long.MaxValue - batchSize
                ? long.MaxValue
                : currentStart + batchSize - 1;

            if (currentEnd > sourceStats.MaxKey)
            {
                currentEnd = sourceStats.MaxKey;
            }

            batchNumber++;

            var rowsInBatch = await GetBatchRowCountAsync(
                sourceConnection,
                tableName,
                batchColumn,
                currentStart,
                currentEnd,
                commandTimeoutSeconds,
                cancellationToken);

            if (rowsInBatch > 0)
            {
                _logger.LogInformation(
                    "Batch {BatchNumber}: key {StartKey:N0}..{EndKey:N0}, rows {Rows:N0}",
                    batchNumber,
                    currentStart,
                    currentEnd,
                    rowsInBatch);

                await CopyBatchAsync(
                    sourceConnection,
                    targetConnection,
                    tableName,
                    batchColumn,
                    currentStart,
                    currentEnd,
                    commandTimeoutSeconds,
                    batchSize,
                    cancellationToken);

                copiedRows += rowsInBatch;
                _logger.LogInformation(
                    "Batch {BatchNumber} completed. Cumulative rows copied: {Rows:N0}",
                    batchNumber,
                    copiedRows);
            }
            else
            {
                _logger.LogDebug(
                    "Batch {BatchNumber}: key {StartKey:N0}..{EndKey:N0}, no rows in source",
                    batchNumber,
                    currentStart,
                    currentEnd);
            }

            if (currentEnd == long.MaxValue)
            {
                break;
            }

            currentStart = currentEnd + 1;
        }

        var targetCountAfter = await GetTableCountAsync(
            targetConnection,
            tableName,
            commandTimeoutSeconds,
            cancellationToken);

        _logger.LogInformation(
            "Table {Table} migration done. Rows copied: {Copied:N0}, target count before/after: {Before:N0}/{After:N0}",
            $"{table.Schema}.{table.Name}",
            copiedRows,
            targetCountBefore,
            targetCountAfter);

        return copiedRows;
    }

    private async Task CopyBatchAsync(
        SqlConnection sourceConnection,
        SqlConnection targetConnection,
        string tableName,
        string batchColumn,
        long startKey,
        long endKey,
        int commandTimeoutSeconds,
        int batchSize,
        CancellationToken cancellationToken)
    {
        var selectSql = $"""
            SELECT *
            FROM {tableName}
            WHERE {batchColumn} >= @StartKey AND {batchColumn} <= @EndKey
            ORDER BY {batchColumn};
            """;

        await using var sourceCommand = new SqlCommand(selectSql, sourceConnection)
        {
            CommandTimeout = commandTimeoutSeconds
        };
        sourceCommand.Parameters.Add(new SqlParameter("@StartKey", startKey));
        sourceCommand.Parameters.Add(new SqlParameter("@EndKey", endKey));

        await using var reader = await sourceCommand.ExecuteReaderAsync(
            System.Data.CommandBehavior.SequentialAccess,
            cancellationToken);

        using var bulkCopy = new SqlBulkCopy(
            targetConnection,
            SqlBulkCopyOptions.KeepIdentity | SqlBulkCopyOptions.TableLock,
            null)
        {
            DestinationTableName = tableName,
            BatchSize = batchSize,
            BulkCopyTimeout = commandTimeoutSeconds,
            EnableStreaming = true
        };

        for (var i = 0; i < reader.FieldCount; i++)
        {
            var columnName = reader.GetName(i);
            bulkCopy.ColumnMappings.Add(columnName, columnName);
        }

        await bulkCopy.WriteToServerAsync(reader, cancellationToken);
    }

    private async Task<SourceStatsResult> GetSourceStatsAsync(
        SqlConnection connection,
        string tableName,
        string batchColumn,
        int commandTimeoutSeconds,
        CancellationToken cancellationToken)
    {
        var statsSql = $"""
            SELECT
                COUNT_BIG(1) AS TotalRows,
                MIN(TRY_CONVERT(BIGINT, {batchColumn})) AS MinKey,
                MAX(TRY_CONVERT(BIGINT, {batchColumn})) AS MaxKey,
                SUM(CASE WHEN {batchColumn} IS NULL THEN 1 ELSE 0 END) AS NullKeyRows,
                SUM(CASE WHEN {batchColumn} IS NOT NULL AND TRY_CONVERT(BIGINT, {batchColumn}) IS NULL THEN 1 ELSE 0 END) AS NonNumericKeyRows
            FROM {tableName};
            """;

        await using var command = new SqlCommand(statsSql, connection)
        {
            CommandTimeout = commandTimeoutSeconds
        };

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return SourceStatsResult.Failed("Unable to read source table stats");
        }

        var totalRows = reader.GetInt64(reader.GetOrdinal("TotalRows"));
        var minKeyOrdinal = reader.GetOrdinal("MinKey");
        var maxKeyOrdinal = reader.GetOrdinal("MaxKey");
        var nullKeyRows = reader.GetInt64(reader.GetOrdinal("NullKeyRows"));
        var nonNumericKeyRows = reader.GetInt64(reader.GetOrdinal("NonNumericKeyRows"));

        if (totalRows == 0)
        {
            return SourceStatsResult.Passed(0, 0, 0);
        }

        if (reader.IsDBNull(minKeyOrdinal) || reader.IsDBNull(maxKeyOrdinal))
        {
            return SourceStatsResult.Failed("Batch column values are not suitable for numeric key-range batching");
        }

        if (nullKeyRows > 0)
        {
            return SourceStatsResult.Failed(
                $"Batch column contains {nullKeyRows:N0} NULL rows. Configure a non-null numeric batch column.");
        }

        if (nonNumericKeyRows > 0)
        {
            return SourceStatsResult.Failed(
                $"Batch column contains {nonNumericKeyRows:N0} non-numeric rows. Configure a numeric batch column.");
        }

        var minKey = reader.GetInt64(minKeyOrdinal);
        var maxKey = reader.GetInt64(maxKeyOrdinal);

        return SourceStatsResult.Passed(totalRows, minKey, maxKey);
    }

    private async Task<long> GetTableCountAsync(
        SqlConnection connection,
        string tableName,
        int commandTimeoutSeconds,
        CancellationToken cancellationToken)
    {
        var sql = $"SELECT COUNT_BIG(1) FROM {tableName};";
        await using var command = new SqlCommand(sql, connection)
        {
            CommandTimeout = commandTimeoutSeconds
        };

        var result = await command.ExecuteScalarAsync(cancellationToken);
        return Convert.ToInt64(result);
    }

    private async Task<long> GetBatchRowCountAsync(
        SqlConnection connection,
        string tableName,
        string batchColumn,
        long startKey,
        long endKey,
        int commandTimeoutSeconds,
        CancellationToken cancellationToken)
    {
        var sql = $"""
            SELECT COUNT_BIG(1)
            FROM {tableName}
            WHERE {batchColumn} >= @StartKey AND {batchColumn} <= @EndKey;
            """;

        await using var command = new SqlCommand(sql, connection)
        {
            CommandTimeout = commandTimeoutSeconds
        };
        command.Parameters.Add(new SqlParameter("@StartKey", startKey));
        command.Parameters.Add(new SqlParameter("@EndKey", endKey));

        var result = await command.ExecuteScalarAsync(cancellationToken);
        return Convert.ToInt64(result);
    }

    private async Task<List<string>> GetColumnsAsync(
        SqlConnection connection,
        string schema,
        string table,
        int commandTimeoutSeconds,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT COLUMN_NAME
            FROM INFORMATION_SCHEMA.COLUMNS
            WHERE TABLE_SCHEMA = @Schema AND TABLE_NAME = @Table
            ORDER BY ORDINAL_POSITION;
            """;

        await using var command = new SqlCommand(sql, connection)
        {
            CommandTimeout = commandTimeoutSeconds
        };
        command.Parameters.Add(new SqlParameter("@Schema", schema));
        command.Parameters.Add(new SqlParameter("@Table", table));

        var columns = new List<string>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            columns.Add(reader.GetString(0));
        }

        return columns;
    }

    private static string QuoteIdentifier(string value) => $"[{value.Replace("]", "]]", StringComparison.Ordinal)}]";

    private class TableConfig
    {
        public string Schema { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public bool Enabled { get; set; }
        public string BatchColumn { get; set; } = string.Empty;
    }

    private sealed class TableValidationResult
    {
        public bool Success { get; init; }

        public string? ErrorMessage { get; init; }

        public static TableValidationResult Passed() => new() { Success = true };

        public static TableValidationResult Failed(string message) => new() { Success = false, ErrorMessage = message };
    }

    private sealed class SourceStatsResult
    {
        public bool Success { get; init; }

        public string? ErrorMessage { get; init; }

        public long TotalRows { get; init; }

        public long MinKey { get; init; }

        public long MaxKey { get; init; }

        public static SourceStatsResult Passed(long totalRows, long minKey, long maxKey) =>
            new() { Success = true, TotalRows = totalRows, MinKey = minKey, MaxKey = maxKey };

        public static SourceStatsResult Failed(string message) => new() { Success = false, ErrorMessage = message };
    }

    private static bool IsOption(string value, string optionName) =>
        value.Equals($"--{optionName}", StringComparison.OrdinalIgnoreCase)
        || value.Equals($"-{optionName}", StringComparison.OrdinalIgnoreCase);
}
