/*******************************************************************************
 * Stored Procedure: Migration.usp_CompleteTableMigration
 * 
 * Marks a table migration as complete.
 ******************************************************************************/

IF OBJECT_ID('Migration.usp_CompleteTableMigration', 'P') IS NOT NULL
	DROP PROCEDURE Migration.usp_CompleteTableMigration;
GO

CREATE PROCEDURE Migration.usp_CompleteTableMigration
	@TableExecutionId   BIGINT,
	@Status             VARCHAR(30),
	@SourceRowCount     BIGINT,
	@TargetRowCount     BIGINT,
	@RowsTransferred    BIGINT,
	@ValidationStatus   VARCHAR(30),
	@ErrorMessage       NVARCHAR(MAX) = NULL
AS
BEGIN
	SET NOCOUNT ON;

	UPDATE Migration.TableExecution
	SET 
		EndDateTime = SYSDATETIME(),
		Status = @Status,
		SourceRowCount = @SourceRowCount,
		TargetRowCount = @TargetRowCount,
		RowsTransferred = @RowsTransferred,
		ValidationStatus = @ValidationStatus,
		ErrorMessage = @ErrorMessage
	WHERE 
		TableExecutionId = @TableExecutionId;

	RETURN 0;
END
GO

PRINT 'Created procedure Migration.usp_CompleteTableMigration';
GO
