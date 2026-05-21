using System.Diagnostics;
using Amazon.ECS;
using Amazon.ECS.Model;
using Microsoft.Extensions.Options;

namespace Apha.BatchJobs.Api.Services;

/// <summary>
/// Production-shaped trigger implementation.
/// Builds an ECS RunTask request and currently executes a local PowerShell fallback.
/// </summary>
public sealed class JobTriggerService : IJobTriggerService
{
    private readonly IAmazonECS _ecsClient;
    private readonly EventBridgeDispatchOptions _options;
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _hostEnvironment;
    private readonly ILogger<JobTriggerService> _logger;

    public JobTriggerService(
        IAmazonECS ecsClient,
        IOptions<EventBridgeDispatchOptions> options,
        IConfiguration configuration,
        IWebHostEnvironment hostEnvironment,
        ILogger<JobTriggerService> logger)
    {
        _ecsClient = ecsClient;
        _options = options.Value;
        _configuration = configuration;
        _hostEnvironment = hostEnvironment;
        _logger = logger;
    }

    public global::System.Threading.Tasks.Task<TriggerResult> TriggerAsync(string jobName, CancellationToken cancellationToken = default)
    {
        var operationId = Guid.NewGuid().ToString("N");
        var acceptedAtUtc = DateTime.UtcNow;
        var request = BuildRunTaskRequest(jobName, operationId);

        _logger.LogInformation(
            "Prepared ECS RunTask request | OperationId={OperationId} | Cluster={Cluster} | TaskDefinition={TaskDefinition} | JobName={JobName}",
            operationId,
            request.Cluster,
            request.TaskDefinition,
            jobName);

        _ = global::System.Threading.Tasks.Task.Run(async () =>
        {
            try
            {
                // Production call path (kept intentionally commented while AWS execution is disabled).
                // var ecsResponse = await _ecsClient.RunTaskAsync(request, cancellationToken);
                // _logger.LogInformation(
                //     "Cloud trigger started | OperationId={OperationId} | Tasks={TaskCount}",
                //     operationId,
                //     ecsResponse.Tasks.Count);

                await RunPowerShellFallbackAsync(jobName, operationId, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Accepted trigger failed | OperationId={OperationId} | JobName={JobName}",
                    operationId,
                    jobName);
            }
        }, CancellationToken.None);

        return global::System.Threading.Tasks.Task.FromResult(new TriggerResult(operationId, acceptedAtUtc));
    }

    private RunTaskRequest BuildRunTaskRequest(string jobName, string operationId)
    {
        var launchType = ParseLaunchType(_options.LaunchType);
        var assignPublicIp = _options.AssignPublicIp ? AssignPublicIp.ENABLED : AssignPublicIp.DISABLED;

        return new RunTaskRequest
        {
            Cluster = _options.Cluster,
            TaskDefinition = _options.TaskDefinition,
            LaunchType = launchType,
            PlatformVersion = _options.PlatformVersion,
            Count = 1,
            NetworkConfiguration = new NetworkConfiguration
            {
                AwsvpcConfiguration = new AwsVpcConfiguration
                {
                    AssignPublicIp = assignPublicIp,
                    Subnets = _options.Subnets,
                    SecurityGroups = _options.SecurityGroups
                }
            },
            Overrides = new TaskOverride
            {
                ContainerOverrides =
                [
                    new ContainerOverride
                    {
                        Name = _options.ContainerName,
                        Environment =
                        [
                            new Amazon.ECS.Model.KeyValuePair { Name = "BATCH_JOB_NAME", Value = jobName },
                            new Amazon.ECS.Model.KeyValuePair { Name = "BATCH_RUN_MODE", Value = "Manual" },
                            new Amazon.ECS.Model.KeyValuePair { Name = "BATCH_OPERATION_ID", Value = operationId }
                        ]
                    }
                ]
            }
        };
    }

    private async global::System.Threading.Tasks.Task RunPowerShellFallbackAsync(string jobName, string operationId, CancellationToken cancellationToken)
    {
        var scriptPath = Path.GetFullPath(Path.Combine(_hostEnvironment.ContentRootPath, "..", _options.PowerShellScriptPath));
        var executable = _options.PowerShellExecutable;
        var arguments = $"-NoProfile -ExecutionPolicy Bypass -File \"{scriptPath}\" -JobName \"{jobName}\"";

        _logger.LogInformation(
            "Running PowerShell fallback trigger | OperationId={OperationId} | Script={ScriptPath} | JobName={JobName}",
            operationId,
            scriptPath,
            jobName);

        var startInfo = new ProcessStartInfo
        {
            FileName = executable,
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = Path.GetFullPath(Path.Combine(_hostEnvironment.ContentRootPath, ".."))
        };

        var connectionString = _configuration.GetConnectionString("BatchJobsConnectionString");
        if (!string.IsNullOrWhiteSpace(connectionString))
            startInfo.Environment["ConnectionStrings__BatchJobsConnectionString"] = connectionString;

        startInfo.Environment["ASPNETCORE_ENVIRONMENT"] = _hostEnvironment.EnvironmentName;
        startInfo.Environment["DOTNET_ENVIRONMENT"] = _hostEnvironment.EnvironmentName;
        startInfo.Environment["BATCH_JOB_NAME"] = jobName;
        startInfo.Environment["BATCH_RUN_MODE"] = "Manual";
        startInfo.Environment["BATCH_OPERATION_ID"] = operationId;

        using var process = new Process { StartInfo = startInfo };
        process.Start();

        var stdOutTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var stdErrTask = process.StandardError.ReadToEndAsync(cancellationToken);

        await process.WaitForExitAsync(cancellationToken);

        var stdOut = await stdOutTask;
        var stdErr = await stdErrTask;

        if (process.ExitCode == 0)
        {
            _logger.LogInformation(
                "PowerShell fallback completed | OperationId={OperationId} | JobName={JobName} | ExitCode={ExitCode} | StdOut={StdOut}",
                operationId,
                jobName,
                process.ExitCode,
                stdOut);
            return;
        }

        throw new InvalidOperationException(
            $"PowerShell fallback failed for job '{jobName}' with exit code {process.ExitCode}. StdErr: {stdErr}");
    }

    private static LaunchType ParseLaunchType(string launchType) =>
        string.IsNullOrWhiteSpace(launchType)
            ? LaunchType.FARGATE
            : LaunchType.FindValue(launchType);
}
