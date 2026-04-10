namespace AphaBatchJobs.Core.Interfaces;

using AphaBatchJobs.Core.Models;

/// <summary>
/// Interface contract for adhoc batch jobs that can be executed on-demand via CLI trigger.
/// Adhoc jobs are identified by a unique JobName and executed when explicitly requested
/// through the --adhoc command line argument with the job name parameter.
/// 
/// Implementations of this interface are automatically discovered and registered in the
/// dependency injection container during application startup. When an adhoc job is triggered,
/// the JobRunnerService locates the implementation by matching the JobName property.
/// 
/// All adhoc jobs run in AWS ECS Fargate containers and must handle their own error scenarios,
/// returning appropriate JobExecutionResult with exit codes for container orchestration.
/// </summary>
public interface IAdhocJob
{
    /// <summary>
    /// Gets the unique name identifier for this adhoc job.
    /// This name is used to match CLI arguments when triggering the job via --adhoc parameter.
    /// The name must be unique across all registered adhoc jobs in the system.
    /// 
    /// Example: If JobName returns "SampleJob", the job is triggered via:
    /// dotnet run --project AphaBatchJobs.Host -- --adhoc SampleJob
    /// </summary>
    /// <value>A non-null, non-empty string uniquely identifying this adhoc job</value>
    string JobName { get; }

    /// <summary>
    /// Executes the adhoc job logic asynchronously with full context and cancellation support.
    /// This method contains the core business logic for the adhoc batch operation against PostgreSQL.
    /// 
    /// The method receives a JobExecutionContext containing correlation id for distributed tracing,
    /// job metadata, and execution timestamp. All database operations should use the correlation id
    /// for logging to enable end-to-end request tracking in CloudWatch logs.
    /// 
    /// Implementations must:
    /// - Handle all exceptions internally and return appropriate JobExecutionResult
    /// - Respect the CancellationToken for graceful shutdown in ECS Fargate
    /// - Log all operations with the correlation id from context
    /// - Return exit codes that map to container exit status (0=success, non-zero=failure)
    /// - Complete within reasonable time limits to avoid ECS task timeout
    /// 
    /// The returned JobExecutionResult determines the process exit code which ECS Fargate
    /// uses to determine task success or failure for monitoring and alerting.
    /// </summary>
    /// <param name="context">
    /// Execution context containing job name, correlation id, trigger type, and start timestamp.
    /// Use context.CorrelationId in all log statements for request tracing.
    /// </param>
    /// <param name="cancellationToken">
    /// Cancellation token for graceful shutdown. Monitor this token during long-running operations
    /// and abort processing if cancellation is requested (e.g., ECS task stop signal).
    /// </param>
    /// <returns>
    /// A Task that resolves to JobExecutionResult containing:
    /// - Status: String representation of execution outcome
    /// - Message: Descriptive message about what occurred during execution
    /// - ExitCode: Integer exit code (0 for success, non-zero for various failure scenarios)
    /// </returns>
    Task<JobExecutionResult> ExecuteAsync(JobExecutionContext context, CancellationToken cancellationToken);
}


// Changes made:
// 1. Removed redundant 'using System.Threading;' and 'using System.Threading.Tasks;' - these are unnecessary in .NET 10 
//    as Task and CancellationToken are part of the global usings enabled by default in modern .NET projects
// 2. Maintained namespace declaration using file-scoped namespace (already correct for .NET 10)
// 3. Preserved all XML documentation as it provides valuable context for AWS ECS Fargate and PostgreSQL usage patterns
// 4. No functional changes - interface contract remains identical for backward compatibility
// 5. Code follows .NET 10 conventions with minimal, clean using statements