/*******************************************************************************
 * Create Migration.ValidationResult Table
 * 
 * Stores validation results for migrated data.
 * Run this script on the TARGET database.
 ******************************************************************************/

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE schema_id = SCHEMA_ID('Migration') AND name = 'ValidationResult')
BEGIN
	CREATE TABLE Migration.ValidationResult
	(
		ValidationResultId  BIGINT IDENTITY(1,1) NOT NULL,
		MigrationRunId      BIGINT NOT NULL,
		TableExecutionId    BIGINT NULL,
		ValidationType      VARCHAR(30) NOT NULL,
		SourceValue         NVARCHAR(MAX) NULL,
		TargetValue         NVARCHAR(MAX) NULL,
		Result              VARCHAR(30) NOT NULL,
		ValidationDateTime  DATETIME2(7) NOT NULL,
		Details             NVARCHAR(MAX) NULL,

		CONSTRAINT PK_Migration_ValidationResult PRIMARY KEY CLUSTERED (ValidationResultId),
		CONSTRAINT FK_Migration_ValidationResult_Run FOREIGN KEY (MigrationRunId) 
			REFERENCES Migration.Run (MigrationRunId),
		CONSTRAINT FK_Migration_ValidationResult_TableExecution FOREIGN KEY (TableExecutionId) 
			REFERENCES Migration.TableExecution (TableExecutionId),
		CONSTRAINT CK_Migration_ValidationResult_ValidationType CHECK (ValidationType IN ('RowCount', 'KeyRange', 'Aggregate', 'BatchChecksum', 'Schema')),
		CONSTRAINT CK_Migration_ValidationResult_Result CHECK (Result IN ('Passed', 'Failed', 'Error'))
	);

	CREATE NONCLUSTERED INDEX IX_Migration_ValidationResult_MigrationRunId 
		ON Migration.ValidationResult (MigrationRunId) 
		INCLUDE (ValidationType, Result);

	CREATE NONCLUSTERED INDEX IX_Migration_ValidationResult_TableExecutionId 
		ON Migration.ValidationResult (TableExecutionId) 
		INCLUDE (ValidationType, Result);

	PRINT 'Migration.ValidationResult table created successfully';
END
ELSE
BEGIN
	PRINT 'Migration.ValidationResult table already exists';
END
GO
