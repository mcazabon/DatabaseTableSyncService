/*******************************************************************************
 * Stored Procedure: Migration.usp_StartTableMigration
 * 
 * Marks the start of a table migration.
 ******************************************************************************/

IF OBJECT_ID('Migration.usp_StartTableMigration', 'P') IS NOT NULL
	DROP PROCEDURE Migration.usp_StartTableMigration;
GO

CREATE PROCEDURE Migration.usp_StartTableMigration
	@MigrationRunId     BIGINT,
	@SchemaName         NVARCHAR(128),
	@TableName          NVARCHAR(128),
	@TableExecutionId   BIGINT OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO Migration.TableExecution
	(
		MigrationRunId,
		SchemaName,
		TableName,
		Status,
		StartDateTime
	)
	VALUES
	(
		@MigrationRunId,
		@SchemaName,
		@TableName,
		'Running',
		SYSDATETIME()
	);

	SET @TableExecutionId = SCOPE_IDENTITY();

	RETURN 0;
END
GO

PRINT 'Created procedure Migration.usp_StartTableMigration';
GO
