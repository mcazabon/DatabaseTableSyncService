/*******************************************************************************
 * Create Migration.Run Table
 * 
 * Tracks overall migration run execution.
 * Run this script on the TARGET database.
 ******************************************************************************/

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE schema_id = SCHEMA_ID('Migration') AND name = 'Run')
BEGIN
	CREATE TABLE Migration.Run
	(
		MigrationRunId      BIGINT IDENTITY(1,1) NOT NULL,
		RunGuid             UNIQUEIDENTIFIER NOT NULL,
		StartDateTime       DATETIME2(7) NOT NULL,
		EndDateTime         DATETIME2(7) NULL,
		Status              VARCHAR(30) NOT NULL,
		SourceServer        NVARCHAR(255) NOT NULL,
		SourceDatabase      NVARCHAR(255) NOT NULL,
		TargetServer        NVARCHAR(255) NOT NULL,
		TargetDatabase      NVARCHAR(255) NOT NULL,
		StartedBy           NVARCHAR(255) NOT NULL,
		ApplicationVersion  VARCHAR(50) NOT NULL,

		CONSTRAINT PK_Migration_Run PRIMARY KEY CLUSTERED (MigrationRunId),
		CONSTRAINT UQ_Migration_Run_RunGuid UNIQUE (RunGuid),
		CONSTRAINT CK_Migration_Run_Status CHECK (Status IN ('Pending', 'Running', 'Completed', 'CompletedWithErrors', 'Failed', 'Cancelled'))
	);

	CREATE NONCLUSTERED INDEX IX_Migration_Run_Status 
		ON Migration.Run (Status) 
		INCLUDE (StartDateTime, EndDateTime);

	CREATE NONCLUSTERED INDEX IX_Migration_Run_StartDateTime 
		ON Migration.Run (StartDateTime DESC);

	PRINT 'Migration.Run table created successfully';
END
ELSE
BEGIN
	PRINT 'Migration.Run table already exists';
END
GO
