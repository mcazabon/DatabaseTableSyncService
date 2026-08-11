using SqlDataMigration.Core.Models;
using Xunit;

namespace SqlDataMigration.UnitTests.Models;

/// <summary>
/// Unit tests for migration domain models.
/// </summary>
public class MigrationModelsTests
{
    [Fact]
    public void MigrationTableDefinition_ShouldCreateFullyQualifiedName()
    {
        // Arrange
        var table = new MigrationTableDefinition("dbo", "TestTable", "Id", true);

        // Act
        var fullyQualifiedName = table.FullyQualifiedName;

        // Assert
        Assert.Equal("dbo.TestTable", fullyQualifiedName);
    }

    [Fact]
    public void BatchRange_ShouldCalculateEstimatedRowCount()
    {
        // Arrange
        var batchRange = new BatchRange(1, 1, 100000);

        // Act
        var estimatedRowCount = batchRange.EstimatedRowCount;

        // Assert
        Assert.Equal(100000, estimatedRowCount);
    }

    [Fact]
    public void TableValidationResult_ShouldIndicateMatchingRowCounts()
    {
        // Arrange
        var result = new TableValidationResult(
            "dbo",
            "TestTable",
            1000,
            1000,
            ValidationStatus.Passed);

        // Assert
        Assert.True(result.RowCountsMatch);
    }

    [Fact]
    public void TableValidationResult_ShouldIndicateMismatchedRowCounts()
    {
        // Arrange
        var result = new TableValidationResult(
            "dbo",
            "TestTable",
            1000,
            900,
            ValidationStatus.Failed);

        // Assert
        Assert.False(result.RowCountsMatch);
    }
}
