using Apha.BatchJobs.Pact.Api.Models;
using Apha.BatchJobs.Pact.Api.Policy;
using Apha.BatchJobs.Pact.Api.Services;
using Apha.BatchJobs.Domain.Enums;
using Apha.BatchJobs.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text.Json;
using System.Security.Claims;

namespace Apha.BatchJobs.Pact.Api.Controllers;

[ApiController]
[Route("api/v1/batch-jobs")]
public sealed class BatchJobTriggerController : ControllerBase
{
    private readonly ITriggerDispatcher _triggerDispatcher;
    private readonly Apha.BatchJobs.Pact.Api.Services.ITriggerAttemptStore _triggerAttemptStore;
    private readonly IJobExecutionRepository _jobExecutionRepository;
    private readonly IBatchLockRepository _batchLockRepository;
    private readonly ILogger<BatchJobTriggerController> _logger;
    private readonly IHostEnvironment _environment;

    public BatchJobTriggerController(
        ITriggerDispatcher triggerDispatcher,
        Apha.BatchJobs.Pact.Api.Services.ITriggerAttemptStore triggerAttemptStore,
        IJobExecutionRepository jobExecutionRepository,
        IBatchLockRepository batchLockRepository,
        ILogger<BatchJobTriggerController> logger,
        IHostEnvironment environment)
    {
        _triggerDispatcher = triggerDispatcher;
        _triggerAttemptStore = triggerAttemptStore;
        _jobExecutionRepository = jobExecutionRepository;
        _batchLockRepository = batchLockRepository;
        _logger = logger;
        _environment = environment;
    }

    [HttpGet("catalog")]
    public IActionResult GetCatalog()
    {
        var catalog = BatchJobRoutingPolicy
            .GetCatalog()
            .Select(route => new
            {
                route.JobName,
                route.Description,
                route.RouteKind,
                CanTriggerFromThisApi = route.RouteKind is JobRouteKind.PactApi or JobRouteKind.Neutral
            });

        return Ok(new { api = "pact.api", jobs = catalog });
    }

    [HttpPost("trigger")]
    public async Task<IActionResult> Trigger([FromBody] BatchTriggerRequest request, CancellationToken cancellationToken)
    {
        if (!BatchJobRoutingPolicy.CanTriggerFromSource(
                request.JobName,
                TriggerApiSource.Pact,
                out var normalizedJobName,
                out var reason))
        {
            return Conflict(new
            {
                accepted = false,
                source = "pact.api",
                jobName = request.JobName,
                reason
            });
        }

        var jobExecutionId = Guid.NewGuid().ToString("N");
        var acceptedAtUtc = DateTime.UtcNow;
        var requestedBy = ResolveRequestedBy(request.RequestedBy);

        var eventId = await _triggerDispatcher.DispatchAsync(
            new BatchTriggerEventDetail(
                jobExecutionId,
                normalizedJobName,
                "Manual",
                requestedBy,
                acceptedAtUtc),
            cancellationToken);

        _logger.LogInformation(
            "PACT API trigger accepted | JobName={JobName} | JobExecutionId={JobExecutionId} | EventId={EventId}",
            normalizedJobName,
            jobExecutionId,
            eventId);

        int? workerPid = null;
        var workerProcessLaunched = false;
        if (eventId.StartsWith("localproc-", StringComparison.OrdinalIgnoreCase)
            && int.TryParse(eventId["localproc-".Length..], out var parsedWorkerPid))
        {
            workerPid = parsedWorkerPid;
            workerProcessLaunched = true;
        }

        var status = workerProcessLaunched ? "WorkerProcessStarted" : "TriggerAccepted";
        var message = workerProcessLaunched
            ? "Trigger accepted and local worker process launched. Attach debugger to workerPid."
            : "Trigger accepted for dispatch.";

        await _triggerAttemptStore.SaveAsync(
            new Apha.BatchJobs.Pact.Api.Services.TriggerAttemptRecord
            {
                JobExecutionId = jobExecutionId,
                JobName = normalizedJobName,
                AcceptedAtUtc = acceptedAtUtc,
                EventId = eventId,
                WorkerProcessLaunched = workerProcessLaunched,
                Status = status,
                WorkerExitCode = null,
                StoredAtUtc = DateTime.UtcNow
            },
            cancellationToken);

        return Accepted(new
        {
            accepted = true,
            source = "pact.api",
            jobName = normalizedJobName,
            jobExecutionId,
            eventId,
            workerPid,
            workerProcessLaunched,
            status,
            acceptedAtUtc,
            message
        });
    }

    [HttpPost("{jobName}/cancel")]
    public async Task<IActionResult> Cancel(string jobName, [FromBody] BatchCancelRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(jobName))
        {
            return BadRequest(new { accepted = false, reason = "jobName is required." });
        }

        if (request is null || string.IsNullOrWhiteSpace(request.JobExecutionId))
        {
            return BadRequest(new { accepted = false, reason = "jobExecutionId is required." });
        }

        if (!Guid.TryParse(request.JobExecutionId, out var jobExecutionId))
        {
            return BadRequest(new { accepted = false, reason = "jobExecutionId must be a valid GUID." });
        }

        var requestedBy = ResolveRequestedBy(request.RequestedBy);
        var execution = await _jobExecutionRepository.GetExecutionByJobExecutionIdAsync(jobExecutionId, cancellationToken);

        if (execution is not null
            && !string.Equals(execution.JobName, jobName, StringComparison.OrdinalIgnoreCase))
        {
            return Conflict(new
            {
                accepted = false,
                reason = "jobExecutionId does not belong to the requested jobName.",
                jobName,
                request.JobExecutionId
            });
        }

        var terminalStatuses = new[] { JobStatus.Completed, JobStatus.Failed, JobStatus.Cancelled, JobStatus.Skipped };
        if (execution is not null && terminalStatuses.Contains(execution.Status))
        {
            var terminalResponse = new BatchCancelResponse
            {
                JobName = execution.JobName,
                JobExecutionId = request.JobExecutionId,
                CancellationStatus = BatchCancellationStatus.NoOpTerminal,
                Accepted = false,
                AlreadyRequested = false,
                NoOpTerminal = true,
                ExecutionState = execution.Status.ToString(),
                Message = "Execution is already terminal. Cancellation request is a no-op."
            };

            return Ok(terminalResponse);
        }

        var created = await _jobExecutionRepository.TryRequestCancellationAsync(jobExecutionId, requestedBy, cancellationToken);
        if (!created)
        {
            var alreadyRequestedResponse = new BatchCancelResponse
            {
                JobName = execution?.JobName ?? jobName,
                JobExecutionId = request.JobExecutionId,
                CancellationStatus = BatchCancellationStatus.AlreadyRequested,
                Accepted = false,
                AlreadyRequested = true,
                NoOpTerminal = false,
                ExecutionState = execution?.Status.ToString() ?? "NotYetPersisted",
                Message = "Cancellation was already requested for this execution."
            };

            return Ok(alreadyRequestedResponse);
        }

        _logger.LogInformation(
            "Cancellation accepted | JobName={JobName} | JobExecutionId={JobExecutionId} | RequestedBy={RequestedBy}",
            execution?.JobName ?? jobName,
            jobExecutionId,
            requestedBy);

        (bool Terminated, int? WorkerPid) localWorkerTermination = execution is not null
            ? TryTerminateLocalWorkerProcess(jobExecutionId)
            : (Terminated: false, WorkerPid: null);

        if (execution is not null && localWorkerTermination.Terminated && localWorkerTermination.WorkerPid.HasValue)
        {
            _logger.LogInformation(
                "Local worker process terminated after cancellation request | JobName={JobName} | JobExecutionId={JobExecutionId} | WorkerPid={WorkerPid}",
                execution!.JobName,
                jobExecutionId,
                localWorkerTermination.WorkerPid.Value);

            try
            {
                execution.Status = JobStatus.Cancelled;
                execution.CompletedAt = DateTime.UtcNow;
                execution.ErrorMessage = "Terminated by cancellation request in local worker mode.";

                await _jobExecutionRepository.UpdateExecutionRecordAsync(execution, cancellationToken);
                await _batchLockRepository.ReleaseLockAsync(execution.JobName, execution.JobQueueId, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Failed to persist terminal cancellation state after local worker termination | JobName={JobName} | JobExecutionId={JobExecutionId}",
                    execution.JobName,
                    jobExecutionId);
            }
        }

        var acceptedResponse = new BatchCancelResponse
        {
            JobName = execution?.JobName ?? jobName,
            JobExecutionId = request.JobExecutionId,
            CancellationStatus = BatchCancellationStatus.Accepted,
            Accepted = true,
            AlreadyRequested = false,
            NoOpTerminal = false,
            ExecutionState = execution?.Status.ToString() ?? "NotYetPersisted",
            Message = localWorkerTermination.Terminated
                ? "Cancellation request accepted. Local worker process was terminated."
                : execution is null
                    ? "Cancellation request accepted before execution row exists. Request will be applied as soon as the worker starts."
                    : "Cancellation request accepted. Worker will stop at the next cancellation checkpoint."
        };

        return Accepted(acceptedResponse);
    }

    private string ResolveRequestedBy(string? requestedByFromRequest)
    {
        var identity = User;
        if (identity?.Identity?.IsAuthenticated == true)
        {
            var fromClaims = identity.FindFirstValue(ClaimTypes.Email)
                ?? identity.FindFirstValue("preferred_username")
                ?? identity.FindFirstValue(ClaimTypes.Upn)
                ?? identity.Identity?.Name;

            if (!string.IsNullOrWhiteSpace(fromClaims))
            {
                return fromClaims;
            }
        }

        if ((_environment.IsDevelopment() || _environment.IsEnvironment("Local"))
            && !string.IsNullOrWhiteSpace(requestedByFromRequest))
        {
            return requestedByFromRequest.Trim();
        }

        return "pact.api@system";
    }

    private (bool Terminated, int? WorkerPid) TryTerminateLocalWorkerProcess(Guid jobExecutionId)
    {
        if (!(_environment.IsDevelopment() || _environment.IsEnvironment("Local")))
        {
            return (false, null);
        }

        var metadataDirectory = LocalWorkerProcessRegistry.GetMetadataDirectory(_environment.ContentRootPath);
        if (!Directory.Exists(metadataDirectory))
        {
            return (false, null);
        }

        var targetExecutionId = NormalizeExecutionId(jobExecutionId.ToString());

        foreach (var metadataPath in Directory.GetFiles(metadataDirectory, "pid-*.json"))
        {
            try
            {
                var json = System.IO.File.ReadAllText(metadataPath);
                var record = JsonSerializer.Deserialize<LocalWorkerProcessRecord>(json);
                if (record is null)
                {
                    continue;
                }

                if (!string.Equals(NormalizeExecutionId(record.JobExecutionId), targetExecutionId, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                try
                {
                    var process = Process.GetProcessById(record.Pid);
                    if (!process.HasExited)
                    {
                        process.Kill(entireProcessTree: true);
                    }
                }
                catch (ArgumentException)
                {
                    // Process already exited.
                }

                try
                {
                    if (System.IO.File.Exists(metadataPath))
                    {
                        System.IO.File.Delete(metadataPath);
                    }
                }
                catch
                {
                    // Best effort cleanup only.
                }

                return (true, record.Pid);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Failed while attempting local worker termination from metadata | MetadataPath={MetadataPath}",
                    metadataPath);
            }
        }

        return (false, null);
    }

    private static string NormalizeExecutionId(string value)
    {
        return value.Replace("-", string.Empty, StringComparison.Ordinal).Trim();
    }
}
