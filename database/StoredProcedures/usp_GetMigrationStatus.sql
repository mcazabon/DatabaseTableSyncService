/*******************************************************************************
 * Stored Procedure: Migration.usp_GetMigrationStatus
 * 
 * Retrieves the current status of a migration run.
 ******************************************************************************/

IF OBJECT_ID('Migration.usp_GetMigrationStatus', 'P') IS NOT NULL
	DROP PROCEDURE Migration.usp_GetMigrationStatus;
GO

CREATE PROCEDURE Migration.usp_GetMigrationStatus
	@MigrationRunId BIGINT
AS
BEGIN
	SET NOCOUNT ON;

	-- Migration run details
	SELECT 
		r.MigrationRunId,
		r.RunGuid,
		r.StartDateTime,
		r.EndDateTime,
		r.Status,
		r.SourceServer,
		r.SourceDatabase,
		r.TargetServer,
		r.TargetDatabase,
		r.StartedBy,
		r.ApplicationVersion
	FROM 
		Migration.Run r
	WHERE 
		r.MigrationRunId = @MigrationRunId;

	-- Table execution summary
	SELECT 
		te.TableExecutionId,
		te.SchemaName,
		te.TableName,
		te.Status,
		te.StartDateTime,
		te.EndDateTime,
		te.SourceRowCount,
		te.TargetRowCount,
		te.RowsTransferred,
		te.LastProcessedKey,
		te.BatchNumber,
		te.ValidationStatus,
		te.ErrorMessage
	FROM 
		Migration.TableExecution te
	WHERE 
		te.MigrationRunId = @MigrationRunId
	ORDER BY 
		te.TableExecutionId;

	RETURN 0;
END
GO

PRINT 'Created procedure Migration.usp_GetMigrationStatus';
GO
