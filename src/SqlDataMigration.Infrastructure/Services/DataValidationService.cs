using Microsoft.Extensions.Logging;
using SqlDataMigration.Core.Interfaces;
using SqlDataMigration.Core.Models;

namespace SqlDataMigration.Infrastructure.Services;

/// <summary>
/// Service for validating migrated data.
/// </summary>
public sealed class DataValidationService : IDataValidationService
{
    private readonly IMigrationRepository _repository;
    private readonly ILogger<DataValidationService> _logger;

    public DataValidationService(
        IMigrationRepository repository,
        ILogger<DataValidationService> logger)
    {
        ArgumentNullException.ThrowIfNull(repository);
        ArgumentNullException.ThrowIfNull(logger);

        _repository = repository;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<TableValidationResult> ValidateAsync(
        MigrationTableDefinition table,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Starting validation for table {Schema}.{Table}",
            table.Schema,
            table.Name);

        try
        {
            // Level 1: Row count validation
            var sourceCount = await _repository.GetSourceRowCountAsync(table, cancellationToken);
            var targetCount = await _repository.GetTargetRowCountAsync(table, cancellationToken);

            _logger.LogInformation(
                "Validation results for {Schema}.{Table} - Source: {SourceCount:N0}, Target: {TargetCount:N0}",
                table.Schema,
                table.Name,
                sourceCount,
                targetCount);

            var status = sourceCount == targetCount 
                ? ValidationStatus.Passed 
                : ValidationStatus.Failed;

            string? errorMessage = null;
            if (status == ValidationStatus.Failed)
            {
                errorMessage = $"Row count mismatch: Source={sourceCount:N0}, Target={targetCount:N0}";
            }

            return new TableValidationResult(
                table.Schema,
                table.Name,
                sourceCount,
                targetCount,
                status,
                errorMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Validation error for table {Schema}.{Table}",
                table.Schema,
                table.Name);

            return new TableValidationResult(
                table.Schema,
                table.Name,
                0,
                0,
                ValidationStatus.Error,
                ex.Message);
        }
    }
}
