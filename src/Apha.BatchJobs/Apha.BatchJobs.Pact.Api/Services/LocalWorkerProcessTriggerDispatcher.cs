using System.Diagnostics;
using Apha.BatchJobs.Pact.Api.Models;
using Apha.BatchJobs.Pact.Api.Options;
using Microsoft.Extensions.Options;

namespace Apha.BatchJobs.Pact.Api.Services;

public sealed class LocalWorkerProcessTriggerDispatcher : ITriggerDispatcher
{
    private readonly TriggerDispatchOptions _options;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<LocalWorkerProcessTriggerDispatcher> _logger;

    public LocalWorkerProcessTriggerDispatcher(
        IOptions<TriggerDispatchOptions> options,
        IWebHostEnvironment environment,
        ILogger<LocalWorkerProcessTriggerDispatcher> logger)
    {
        _options = options.Value;
        _environment = environment;
        _logger = logger;
    }

    public Task<string> DispatchAsync(BatchTriggerEventDetail detail, CancellationToken cancellationToken = default)
    {
        var workerProjectPath = ResolveWorkerProjectPath();
        if (!File.Exists(workerProjectPath))
        {
            throw new FileNotFoundException($"Worker project not found at '{workerProjectPath}'.");
        }

        var shouldPauseForDebugger = _options.LocalWorker.WaitForDebuggerAttach;

        var workingDirectory = ResolveWorkingDirectory(workerProjectPath);
        var startInfo = new ProcessStartInfo
        {
            FileName = string.IsNullOrWhiteSpace(_options.LocalWorker.DotnetExecutable)
                ? "dotnet"
                : _options.LocalWorker.DotnetExecutable,
            WorkingDirectory = workingDirectory,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        startInfo.ArgumentList.Add("run");
        startInfo.ArgumentList.Add("--no-build");
        startInfo.ArgumentList.Add("--project");
        startInfo.ArgumentList.Add(workerProjectPath);

        startInfo.Environment["ASPNETCORE_ENVIRONMENT"] = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
        startInfo.Environment["BATCH_JOB_NAME"] = detail.JobName;
        startInfo.Environment["BATCH_RUN_MODE"] = detail.RunMode;
        startInfo.Environment["BATCH_JOB_EXECUTION_ID"] = detail.JobExecutionId;
        startInfo.Environment["BATCH_USER_ID"] = detail.RequestedBy;
        startInfo.Environment["BATCH_DEBUG_WAIT_FOR_ATTACH"] = shouldPauseForDebugger ? "true" : "false";
        startInfo.Environment["BATCH_DEBUG_ATTACH_TIMEOUT_SECONDS"] = _options.LocalWorker.DebuggerAttachTimeoutSeconds.ToString();
        startInfo.Environment["BATCH_DEBUG_BREAK_ON_START"] =
            shouldPauseForDebugger && _options.LocalWorker.BreakOnStartAfterAttach ? "true" : "false";

        if (Guid.TryParse(detail.JobExecutionId, out var jobQueueGuid))
        {
            startInfo.Environment["BATCH_JOBQUEUE_ID"] = jobQueueGuid.ToString("D");
        }

        var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException("Failed to start local worker process.");

        _logger.LogInformation(
            "Local worker dispatch started | JobName={JobName} | JobExecutionId={JobExecutionId} | WorkerPid={WorkerPid} | WorkerProjectPath={WorkerProjectPath} | WaitForDebuggerAttach={WaitForDebuggerAttach}",
            detail.JobName,
            detail.JobExecutionId,
            process.Id,
            workerProjectPath,
            shouldPauseForDebugger);

        return Task.FromResult($"localproc-{process.Id}");
    }

    private string ResolveWorkerProjectPath()
    {
        var relativePath = _options.LocalWorker.WorkerProjectPath;
        if (Path.IsPathRooted(relativePath))
        {
            return relativePath;
        }

        var probeRoot = new DirectoryInfo(_environment.ContentRootPath);
        while (probeRoot is not null)
        {
            var candidatePath = Path.GetFullPath(relativePath, probeRoot.FullName);
            if (File.Exists(candidatePath))
            {
                return candidatePath;
            }

            probeRoot = probeRoot.Parent;
        }

        return Path.GetFullPath(relativePath, _environment.ContentRootPath);
    }

    private string ResolveWorkingDirectory(string workerProjectPath)
    {
        if (!string.IsNullOrWhiteSpace(_options.LocalWorker.WorkingDirectory))
        {
            return Path.IsPathRooted(_options.LocalWorker.WorkingDirectory)
                ? _options.LocalWorker.WorkingDirectory
                : Path.GetFullPath(_options.LocalWorker.WorkingDirectory, _environment.ContentRootPath);
        }

        return Path.GetDirectoryName(workerProjectPath) ?? _environment.ContentRootPath;
    }
}
