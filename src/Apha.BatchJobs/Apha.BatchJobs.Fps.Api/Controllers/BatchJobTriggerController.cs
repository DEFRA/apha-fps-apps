using Apha.BatchJobs.Triggering.Models;
using Apha.BatchJobs.Triggering.Policy;
using Apha.BatchJobs.Triggering.Services;
using Microsoft.AspNetCore.Mvc;

namespace Apha.BatchJobs.Fps.Api.Controllers;

[ApiController]
[Route("api/v1/batch-jobs")]
public sealed class BatchJobTriggerController : ControllerBase
{
    private readonly IEventBridgePublisher _eventBridgePublisher;
    private readonly ILogger<BatchJobTriggerController> _logger;

    public BatchJobTriggerController(IEventBridgePublisher eventBridgePublisher, ILogger<BatchJobTriggerController> logger)
    {
        _eventBridgePublisher = eventBridgePublisher;
        _logger = logger;
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
                CanTriggerFromThisApi = route.RouteKind is JobRouteKind.FpsApi or JobRouteKind.Neutral
            });

        return Ok(new { api = "fps.api", jobs = catalog });
    }

    [HttpPost("trigger")]
    public async Task<IActionResult> Trigger([FromBody] BatchTriggerRequest request, CancellationToken cancellationToken)
    {
        if (!BatchJobRoutingPolicy.CanTriggerFromSource(
                request.JobName,
                TriggerApiSource.Fps,
                out var normalizedJobName,
                out var reason))
        {
            return Conflict(new
            {
                accepted = false,
                source = "fps.api",
                jobName = request.JobName,
                reason
            });
        }

        var jobExecutionId = Guid.NewGuid().ToString("N");
        var acceptedAtUtc = DateTime.UtcNow;
        var requestedBy = string.IsNullOrWhiteSpace(request.RequestedBy) ? "fps.api@local" : request.RequestedBy;

        var eventId = await _eventBridgePublisher.PublishAsync(
            new BatchTriggerEventDetail(
                jobExecutionId,
                normalizedJobName,
                "Manual",
                requestedBy,
                acceptedAtUtc),
            cancellationToken);

        _logger.LogInformation(
            "FPS API trigger accepted | JobName={JobName} | JobExecutionId={JobExecutionId} | EventId={EventId}",
            normalizedJobName,
            jobExecutionId,
            eventId);

        return Accepted(new
        {
            accepted = true,
            source = "fps.api",
            jobName = normalizedJobName,
            jobExecutionId,
            eventId,
            status = "TriggerAccepted",
            acceptedAtUtc,
            message = "Event accepted for EventBridge dispatch."
        });
    }
}