/*******************************************************************************
 * Stored Procedure: Migration.usp_CompleteBatch
 * 
 * Marks a batch as complete.
 ******************************************************************************/

IF OBJECT_ID('Migration.usp_CompleteBatch', 'P') IS NOT NULL
	DROP PROCEDURE Migration.usp_CompleteBatch;
GO

CREATE PROCEDURE Migration.usp_CompleteBatch
	@BatchExecutionId   BIGINT,
	@RowsProcessed      BIGINT
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @TableExecutionId BIGINT;
	DECLARE @EndKey BIGINT;

	-- Get batch details
	SELECT 
		@TableExecutionId = TableExecutionId,
		@EndKey = EndKey
	FROM 
		Migration.BatchExecution
	WHERE 
		BatchExecutionId = @BatchExecutionId;

	-- Update batch
	UPDATE Migration.BatchExecution
	SET 
		EndDateTime = SYSDATETIME(),
		Status = 'Completed',
		RowsProcessed = @RowsProcessed
	WHERE 
		BatchExecutionId = @BatchExecutionId;

	-- Update table execution with last processed key
	UPDATE Migration.TableExecution
	SET 
		LastProcessedKey = @EndKey,
		RowsTransferred = ISNULL(RowsTransferred, 0) + @RowsProcessed
	WHERE 
		TableExecutionId = @TableExecutionId;

	RETURN 0;
END
GO

PRINT 'Created procedure Migration.usp_CompleteBatch';
GO
