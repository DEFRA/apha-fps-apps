using System.Text.Json;
using Amazon.EventBridge;
using Amazon.EventBridge.Model;
using Apha.BatchJobs.Triggering.Models;
using Apha.BatchJobs.Triggering.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Apha.BatchJobs.Triggering.Services;

public sealed class EventBridgePublisher : IEventBridgePublisher
{
    private readonly IAmazonEventBridge _eventBridge;
    private readonly EventBridgePublisherOptions _options;
    private readonly ILogger<EventBridgePublisher> _logger;

    public EventBridgePublisher(
        IAmazonEventBridge eventBridge,
        IOptions<EventBridgePublisherOptions> options,
        ILogger<EventBridgePublisher> logger)
    {
        _eventBridge = eventBridge;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<string> PublishAsync(BatchTriggerEventDetail detail, CancellationToken cancellationToken = default)
    {
        var payload = JsonSerializer.Serialize(new
        {
            detail.JobExecutionId,
            detail.JobName,
            detail.RunMode,
            detail.RequestedBy,
            detail.RequestedAtUtc
        });

        if (_options.DryRun)
        {
            var simulatedEventId = $"dryrun-{detail.JobExecutionId}";
            _logger.LogInformation(
                "EventBridge dry-run publish | Source={Source} | DetailType={DetailType} | EventId={EventId} | Payload={Payload}",
                _options.Source,
                _options.DetailType,
                simulatedEventId,
                payload);
            return simulatedEventId;
        }

        var request = new PutEventsRequest
        {
            Entries =
            [
                new PutEventsRequestEntry
                {
                    EventBusName = _options.EventBusName,
                    Source = _options.Source,
                    DetailType = _options.DetailType,
                    Detail = payload
                }
            ]
        };

        var response = await _eventBridge.PutEventsAsync(request, cancellationToken);
        if (response.FailedEntryCount > 0 || response.Entries.Count == 0 || string.IsNullOrWhiteSpace(response.Entries[0].EventId))
        {
            var failure = response.Entries
                .Where(e => !string.IsNullOrWhiteSpace(e.ErrorCode))
                .Select(e => $"{e.ErrorCode}: {e.ErrorMessage}");
            throw new InvalidOperationException($"EventBridge publish failed. {string.Join("; ", failure)}");
        }

        return response.Entries[0].EventId;
    }
}