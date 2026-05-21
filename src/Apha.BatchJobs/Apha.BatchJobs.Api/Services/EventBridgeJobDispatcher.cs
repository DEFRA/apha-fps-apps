using System.Text.Json;
using Amazon.EventBridge;
using Amazon.EventBridge.Model;

namespace Apha.BatchJobs.Api.Services;

/// <summary>
/// Dispatches batch job trigger requests to EventBridge via PutEvents.
/// </summary>
public sealed class EventBridgeJobDispatcher : IJobDispatchService
{
    private readonly IAmazonEventBridge _eventBridge;
    private readonly IConfiguration _configuration;
    private readonly ILogger<EventBridgeJobDispatcher> _logger;

    public EventBridgeJobDispatcher(
        IAmazonEventBridge eventBridge,
        IConfiguration configuration,
        ILogger<EventBridgeJobDispatcher> logger)
    {
        _eventBridge = eventBridge;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<string> RunBatchJobAsync(
        string jobName,
        CancellationToken cancellationToken = default)
    {
        var eventBusName = _configuration["EventBridge:EventBusName"];
        if (string.IsNullOrWhiteSpace(eventBusName))
        {
            throw new InvalidOperationException("EventBridge:EventBusName configuration is required.");
        }

        var source = _configuration["EventBridge:Source"] ?? "apha.batchjobs.api";
        var detailType = _configuration["EventBridge:DetailType"] ?? "BatchJobTriggerRequested";
        var requestedBy = _configuration["EventBridge:RequestedBy"] ?? "api-local";
        var jobQueueId = Guid.NewGuid();

        var detailPayload = new
        {
            jobName,
            runMode = "AdHoc",
            jobQueueId = jobQueueId.ToString(),
            userId = requestedBy,
            requestedAtUtc = DateTime.UtcNow
        };

        var request = new PutEventsRequest
        {
            Entries =
            [
                new PutEventsRequestEntry
                {
                    EventBusName = eventBusName,
                    Source = source,
                    DetailType = detailType,
                    Time = DateTime.UtcNow,
                    Detail = JsonSerializer.Serialize(detailPayload)
                }
            ]
        };

        var response = await _eventBridge.PutEventsAsync(request, cancellationToken);
        var resultEntry = response.Entries.FirstOrDefault();
        if (resultEntry is null)
        {
            throw new InvalidOperationException("EventBridge PutEvents returned no result entries.");
        }

        if (!string.IsNullOrWhiteSpace(resultEntry.ErrorCode))
        {
            var message = $"EventBridge PutEvents failed: {resultEntry.ErrorCode} - {resultEntry.ErrorMessage}";
            _logger.LogError(message);
            throw new InvalidOperationException(message);
        }

        _logger.LogInformation(
            "Published EventBridge trigger | EventId={EventId} | JobName={JobName} | JobQueueId={JobQueueId} | EventBus={EventBus}",
            resultEntry.EventId,
            jobName,
            jobQueueId,
            eventBusName);

        return resultEntry.EventId ?? jobQueueId.ToString();
    }
}
