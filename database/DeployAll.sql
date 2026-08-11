/*******************************************************************************
 * Master Deployment Script
 * 
 * Executes all database scripts in the correct order.
 * Run this script on the TARGET database.
 ******************************************************************************/

PRINT '========================================';
PRINT 'SQL Data Migration - Database Setup';
PRINT '========================================';
PRINT '';

-- Create schema
PRINT 'Step 1: Creating schema...';
:r .\Scripts\01_CreateSchema.sql
PRINT '';

-- Create tables
PRINT 'Step 2: Creating tables...';
:r .\Tables\MigrationRun.sql
:r .\Tables\TableExecution.sql
:r .\Tables\BatchExecution.sql
:r .\Tables\ValidationResult.sql
PRINT '';

-- Create stored procedures
PRINT 'Step 3: Creating stored procedures...';
:r .\StoredProcedures\usp_CreateMigrationRun.sql
:r .\StoredProcedures\usp_CompleteMigrationRun.sql
:r .\StoredProcedures\usp_StartTableMigration.sql
:r .\StoredProcedures\usp_CompleteTableMigration.sql
:r .\StoredProcedures\usp_FailTableMigration.sql
:r .\StoredProcedures\usp_StartBatch.sql
:r .\StoredProcedures\usp_CompleteBatch.sql
:r .\StoredProcedures\usp_FailBatch.sql
:r .\StoredProcedures\usp_GetMigrationStatus.sql
PRINT '';

PRINT '========================================';
PRINT 'Database setup completed successfully';
PRINT '========================================';
GO
