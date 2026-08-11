/*******************************************************************************
 * Create Migration.BatchExecution Table
 * 
 * Tracks individual batch execution within a table migration.
 * Enables restart capability from the last successful batch.
 * Run this script on the TARGET database.
 ******************************************************************************/

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE schema_id = SCHEMA_ID('Migration') AND name = 'BatchExecution')
BEGIN
	CREATE TABLE Migration.BatchExecution
	(
		BatchExecutionId    BIGINT IDENTITY(1,1) NOT NULL,
		TableExecutionId    BIGINT NOT NULL,
		BatchNumber         INT NOT NULL,
		StartKey            BIGINT NOT NULL,
		EndKey              BIGINT NOT NULL,
		RowsProcessed       BIGINT NULL,
		StartDateTime       DATETIME2(7) NOT NULL,
		EndDateTime         DATETIME2(7) NULL,
		Status              VARCHAR(30) NOT NULL,
		ErrorMessage        NVARCHAR(MAX) NULL,

		CONSTRAINT PK_Migration_BatchExecution PRIMARY KEY CLUSTERED (BatchExecutionId),
		CONSTRAINT FK_Migration_BatchExecution_TableExecution FOREIGN KEY (TableExecutionId) 
			REFERENCES Migration.TableExecution (TableExecutionId),
		CONSTRAINT CK_Migration_BatchExecution_Status CHECK (Status IN ('Pending', 'Running', 'Completed', 'Failed'))
	);

	CREATE NONCLUSTERED INDEX IX_Migration_BatchExecution_TableExecutionId 
		ON Migration.BatchExecution (TableExecutionId, BatchNumber);

	CREATE NONCLUSTERED INDEX IX_Migration_BatchExecution_Status 
		ON Migration.BatchExecution (Status) 
		INCLUDE (TableExecutionId, BatchNumber);

	PRINT 'Migration.BatchExecution table created successfully';
END
ELSE
BEGIN
	PRINT 'Migration.BatchExecution table already exists';
END
GO
