/*******************************************************************************
 * Stored Procedure: Migration.usp_StartBatch
 * 
 * Marks the start of a batch execution.
 ******************************************************************************/

IF OBJECT_ID('Migration.usp_StartBatch', 'P') IS NOT NULL
	DROP PROCEDURE Migration.usp_StartBatch;
GO

CREATE PROCEDURE Migration.usp_StartBatch
	@TableExecutionId   BIGINT,
	@BatchNumber        INT,
	@StartKey           BIGINT,
	@EndKey             BIGINT,
	@BatchExecutionId   BIGINT OUTPUT
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO Migration.BatchExecution
	(
		TableExecutionId,
		BatchNumber,
		StartKey,
		EndKey,
		StartDateTime,
		Status
	)
	VALUES
	(
		@TableExecutionId,
		@BatchNumber,
		@StartKey,
		@EndKey,
		SYSDATETIME(),
		'Running'
	);

	SET @BatchExecutionId = SCOPE_IDENTITY();

	-- Update table execution with current batch info
	UPDATE Migration.TableExecution
	SET 
		LastProcessedKey = @StartKey,
		BatchNumber = @BatchNumber
	WHERE 
		TableExecutionId = @TableExecutionId;

	RETURN 0;
END
GO

PRINT 'Created procedure Migration.usp_StartBatch';
GO
