/*******************************************************************************
 * Stored Procedure: Migration.usp_FailBatch
 * 
 * Records a batch failure.
 ******************************************************************************/

IF OBJECT_ID('Migration.usp_FailBatch', 'P') IS NOT NULL
	DROP PROCEDURE Migration.usp_FailBatch;
GO

CREATE PROCEDURE Migration.usp_FailBatch
	@BatchExecutionId   BIGINT,
	@ErrorMessage       NVARCHAR(MAX)
AS
BEGIN
	SET NOCOUNT ON;

	UPDATE Migration.BatchExecution
	SET 
		EndDateTime = SYSDATETIME(),
		Status = 'Failed',
		ErrorMessage = @ErrorMessage
	WHERE 
		BatchExecutionId = @BatchExecutionId;

	RETURN 0;
END
GO

PRINT 'Created procedure Migration.usp_FailBatch';
GO
