/*******************************************************************************
 * Create Migration.TableExecution Table
 * 
 * Tracks individual table migration execution within a migration run.
 * Run this script on the TARGET database.
 ******************************************************************************/

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE schema_id = SCHEMA_ID('Migration') AND name = 'TableExecution')
BEGIN
	CREATE TABLE Migration.TableExecution
	(
		TableExecutionId    BIGINT IDENTITY(1,1) NOT NULL,
		MigrationRunId      BIGINT NOT NULL,
		SchemaName          NVARCHAR(128) NOT NULL,
		TableName           NVARCHAR(128) NOT NULL,
		Status              VARCHAR(30) NOT NULL,
		StartDateTime       DATETIME2(7) NOT NULL,
		EndDateTime         DATETIME2(7) NULL,
		SourceRowCount      BIGINT NULL,
		TargetRowCount      BIGINT NULL,
		RowsTransferred     BIGINT NULL,
		LastProcessedKey    BIGINT NULL,
		BatchNumber         INT NULL,
		ErrorMessage        NVARCHAR(MAX) NULL,
		RetryCount          INT NOT NULL DEFAULT 0,
		ValidationStatus    VARCHAR(30) NOT NULL DEFAULT 'NotValidated',

		CONSTRAINT PK_Migration_TableExecution PRIMARY KEY CLUSTERED (TableExecutionId),
		CONSTRAINT FK_Migration_TableExecution_Run FOREIGN KEY (MigrationRunId) 
			REFERENCES Migration.Run (MigrationRunId),
		CONSTRAINT CK_Migration_TableExecution_Status CHECK (Status IN ('Pending', 'Running', 'Completed', 'Failed', 'Skipped')),
		CONSTRAINT CK_Migration_TableExecution_ValidationStatus CHECK (ValidationStatus IN ('NotValidated', 'InProgress', 'Passed', 'Failed', 'Error'))
	);

	CREATE NONCLUSTERED INDEX IX_Migration_TableExecution_MigrationRunId 
		ON Migration.TableExecution (MigrationRunId) 
		INCLUDE (SchemaName, TableName, Status);

	CREATE NONCLUSTERED INDEX IX_Migration_TableExecution_Status 
		ON Migration.TableExecution (Status) 
		INCLUDE (MigrationRunId, SchemaName, TableName);

	PRINT 'Migration.TableExecution table created successfully';
END
ELSE
BEGIN
	PRINT 'Migration.TableExecution table already exists';
END
GO
