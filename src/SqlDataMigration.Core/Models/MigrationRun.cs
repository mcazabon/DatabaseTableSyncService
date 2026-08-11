namespace SqlDataMigration.Core.Models;

/// <summary>
/// Represents a migration run execution.
/// </summary>
/// <param name="MigrationRunId">The unique identifier for this migration run.</param>
/// <param name="RunGuid">A GUID for this migration run.</param>
/// <param name="Status">The current status of the migration run.</param>
/// <param name="StartDateTime">When the migration run started.</param>
/// <param name="EndDateTime">When the migration run ended (if completed).</param>
/// <param name="SourceServer">The source SQL Server instance.</param>
/// <param name="SourceDatabase">The source database name.</param>
/// <param name="TargetServer">The target SQL Server instance.</param>
/// <param name="TargetDatabase">The target database name.</param>
/// <param name="StartedBy">The user or service account that started the migration.</param>
/// <param name="ApplicationVersion">The version of the migration application.</param>
public sealed record MigrationRun(
    long MigrationRunId,
    Guid RunGuid,
    MigrationRunStatus Status,
    DateTimeOffset StartDateTime,
    DateTimeOffset? EndDateTime,
    string SourceServer,
    string SourceDatabase,
    string TargetServer,
    string TargetDatabase,
    string StartedBy,
    string ApplicationVersion);
