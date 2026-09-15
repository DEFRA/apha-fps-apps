using System.Net.Sockets;
using Apha.BatchJobs.Domain.Constants;
using Apha.BatchJobs.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Apha.BatchJobs.Application.FailureHandling;

/// <summary>
/// Maps a non-cancellation exception to an exit code, failure category, and CloudWatch
/// <c>ErrorType</c> marker. Shared by <c>JobOrchestrator</c> and the Worker's run summary so
/// both use the same classification without re-logging. <c>OperationCanceledException</c> is
/// handled elsewhere, not here.
/// </summary>
public sealed class BatchFailureClassifier
{
    private readonly IConfiguration _configuration;

    public BatchFailureClassifier(IConfiguration configuration)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    /// <summary>Classifies a non-cancellation exception. Do not pass <see cref="OperationCanceledException"/>.</summary>
    public BatchFailureClassification Classify(Exception exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        return exception switch
        {
            JobValidationException => Build(BatchExitCodes.ConfigurationFailure, BatchFailureCategory.Configuration, MarkerKey.Validation),
            MabArchiveYearConfigurationException => Build(BatchExitCodes.ConfigurationFailure, BatchFailureCategory.Configuration, MarkerKey.General),
            NotificationSettingsConfigurationException => Build(BatchExitCodes.ConfigurationFailure, BatchFailureCategory.Configuration, MarkerKey.General),
            JobLockException => Build(BatchExitCodes.LockFailure, BatchFailureCategory.Concurrency, MarkerKey.Concurrency),
            BusinessEmailException => Build(BatchExitCodes.EmailFailure, BatchFailureCategory.Email, MarkerKey.General),
            _ => ClassifyByExceptionChain(exception)
        };
    }

    private BatchFailureClassification ClassifyByExceptionChain(Exception exception)
    {
        for (var current = exception; current is not null; current = current.InnerException)
        {
            switch (current)
            {
                case PostgresException or DbUpdateException:
                    return Build(BatchExitCodes.DatabaseFailure, BatchFailureCategory.Sql, MarkerKey.Sql);
                case NpgsqlException or SocketException:
                    return Build(BatchExitCodes.DatabaseFailure, BatchFailureCategory.DependencyOutage, MarkerKey.Sql);
                case TimeoutException:
                    return Build(BatchExitCodes.DatabaseFailure, BatchFailureCategory.Timeout, MarkerKey.General);
                case UnauthorizedAccessException:
                    return Build(BatchExitCodes.UnhandledFailure, BatchFailureCategory.Authorization, MarkerKey.General);
            }
        }

        return Build(BatchExitCodes.UnhandledFailure, BatchFailureCategory.Business, MarkerKey.General);
    }

    private BatchFailureClassification Build(int exitCode, BatchFailureCategory category, MarkerKey markerKey)
    {
        var marker = markerKey switch
        {
            MarkerKey.Sql => _configuration["ExceptionTypes:Sql"] ?? "FPSBatchJobs.SQL_EXCEPTION",
            MarkerKey.Concurrency => _configuration["ExceptionTypes:Concurrency"] ?? "FPSBatchJobs.CONCURRENCY_EXCEPTION",
            MarkerKey.Validation => _configuration["ExceptionTypes:Validation"] ?? "FPSBatchJobs.VALIDATION_EXCEPTION",
            _ => _configuration["ExceptionTypes:General"] ?? "FPSBatchJobs.GENERAL_EXCEPTION"
        };

        return new BatchFailureClassification(exitCode, category, marker);
    }

    private enum MarkerKey
    {
        General,
        Sql,
        Concurrency,
        Validation
    }
}
