/*******************************************************************************
 * Stored Procedure: Migration.usp_CreateMigrationRun
 * 
 * Creates a new migration run record.
 ******************************************************************************/

IF OBJECT_ID('Migration.usp_CreateMigrationRun', 'P') IS NOT NULL
	DROP PROCEDURE Migration.usp_CreateMigrationRun;
GO

CREATE PROCEDURE Migration.usp_CreateMigrationRun
	@RunGuid            UNIQUEIDENTIFIER,
	@SourceServer       NVARCHAR(255),
	@SourceDatabase     NVARCHAR(255),
	@TargetServer       NVARCHAR(255),
	@TargetDatabase     NVARCHAR(255),
	@StartedBy          NVARCHAR(255),
	@ApplicationVersion VARCHAR(50),
	@MigrationRunId     BIGINT OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO Migration.Run
	(
		RunGuid,
		StartDateTime,
		Status,
		SourceServer,
		SourceDatabase,
		TargetServer,
		TargetDatabase,
		StartedBy,
		ApplicationVersion
	)
	VALUES
	(
		@RunGuid,
		SYSDATETIME(),
		'Running',
		@SourceServer,
		@SourceDatabase,
		@TargetServer,
		@TargetDatabase,
		@StartedBy,
		@ApplicationVersion
	);

	SET @MigrationRunId = SCOPE_IDENTITY();

	RETURN 0;
END
GO

PRINT 'Created procedure Migration.usp_CreateMigrationRun';
GO
