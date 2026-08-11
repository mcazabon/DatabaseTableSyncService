/*******************************************************************************
 * Create Migration Schema
 * 
 * This script creates the Migration schema for storing migration control data.
 * Run this script on the TARGET database.
 ******************************************************************************/

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Migration')
BEGIN
	EXEC('CREATE SCHEMA Migration');
	PRINT 'Migration schema created successfully';
END
ELSE
BEGIN
	PRINT 'Migration schema already exists';
END
GO
