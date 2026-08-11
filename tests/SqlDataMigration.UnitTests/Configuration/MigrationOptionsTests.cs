using Microsoft.Extensions.Options;
using SqlDataMigration.Infrastructure.Configuration;
using Xunit;

namespace SqlDataMigration.UnitTests.Configuration;

/// <summary>
/// Unit tests for migration configuration.
/// </summary>
public class MigrationOptionsTests
{
    [Fact]
    public void MigrationOptions_ShouldHaveDefaultValues()
    {
        // Arrange & Act
        var options = new MigrationOptions();

        // Assert
        Assert.Equal(3600, options.CommandTimeoutSeconds);
        Assert.Equal(100000, options.BatchSize);
        Assert.Equal(3, options.MaxRetryAttempts);
        Assert.False(options.EnableParallelTables);
        Assert.Equal(2, options.MaximumParallelTables);
        Assert.True(options.ValidateAfterMigration);
        Assert.NotNull(options.Tables);
    }

    [Fact]
    public void TableConfiguration_ShouldStoreTableDetails()
    {
        // Arrange & Act
        var tableConfig = new TableConfiguration
        {
            Schema = "dbo",
            Name = "TestTable",
            Enabled = true,
            BatchColumn = "Id"
        };

        // Assert
        Assert.Equal("dbo", tableConfig.Schema);
        Assert.Equal("TestTable", tableConfig.Name);
        Assert.True(tableConfig.Enabled);
        Assert.Equal("Id", tableConfig.BatchColumn);
    }
}
