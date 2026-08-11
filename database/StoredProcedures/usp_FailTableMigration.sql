/*******************************************************************************
 * Stored Procedure: Migration.usp_FailTableMigration
 * 
 * Records a table migration failure.
 ******************************************************************************/

IF OBJECT_ID('Migration.usp_FailTableMigration', 'P') IS NOT NULL
	DROP PROCEDURE Migration.usp_FailTableMigration;
GO

CREATE PROCEDURE Migration.usp_FailTableMigration
	@TableExecutionId   BIGINT,
	@ErrorMessage       NVARCHAR(MAX)
AS
BEGIN
	SET NOCOUNT ON;

	UPDATE Migration.TableExecution
	SET 
		EndDateTime = SYSDATETIME(),
		Status = 'Failed',
		ErrorMessage = @ErrorMessage,
		RetryCount = RetryCount + 1
	WHERE 
		TableExecutionId = @TableExecutionId;

	RETURN 0;
END
GO

PRINT 'Created procedure Migration.usp_FailTableMigration';
GO
