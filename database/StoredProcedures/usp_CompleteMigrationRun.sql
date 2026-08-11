/*******************************************************************************
 * Stored Procedure: Migration.usp_CompleteMigrationRun
 * 
 * Marks a migration run as complete with final status.
 ******************************************************************************/

IF OBJECT_ID('Migration.usp_CompleteMigrationRun', 'P') IS NOT NULL
	DROP PROCEDURE Migration.usp_CompleteMigrationRun;
GO

CREATE PROCEDURE Migration.usp_CompleteMigrationRun
	@MigrationRunId BIGINT,
	@Status         VARCHAR(30)
AS
BEGIN
	SET NOCOUNT ON;

	UPDATE Migration.Run
	SET 
		EndDateTime = SYSDATETIME(),
		Status = @Status
	WHERE 
		MigrationRunId = @MigrationRunId;

	RETURN 0;
END
GO

PRINT 'Created procedure Migration.usp_CompleteMigrationRun';
GO
