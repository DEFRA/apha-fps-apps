using Apha.BatchJobs.Pact.Api.Models;
using Apha.BatchJobs.Pact.Api.Policy;
using Apha.BatchJobs.Pact.Api.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Apha.BatchJobs.Pact.Api.Controllers;

[ApiController]
[Route("api/v1/batch-jobs")]
public sealed class BatchJobTriggerController : ControllerBase
{
    private readonly ITriggerDispatcher _triggerDispatcher;
    private readonly Apha.BatchJobs.Pact.Api.Services.ITriggerAttemptStore _triggerAttemptStore;
    private readonly ILogger<BatchJobTriggerController> _logger;
    private readonly IHostEnvironment _environment;

    public BatchJobTriggerController(
        ITriggerDispatcher triggerDispatcher,
        Apha.BatchJobs.Pact.Api.Services.ITriggerAttemptStore triggerAttemptStore,
        ILogger<BatchJobTriggerController> logger,
        IHostEnvironment environment)
    {
        _triggerDispatcher = triggerDispatcher;
        _triggerAttemptStore = triggerAttemptStore;
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
}