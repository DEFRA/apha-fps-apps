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
    private readonly IHostEnvironment _environment;
    private readonly ILogger<EventBridgeJobDispatcher> _logger;

    public EventBridgeJobDispatcher(
        IAmazonEventBridge eventBridge,
        IConfiguration configuration,
        IHostEnvironment environment,
        ILogger<EventBridgeJobDispatcher> logger)
    {
        _eventBridge = eventBridge;
        _configuration = configuration;
        _environment = environment;
        _logger = logger;
    }

    public async Task<string> RunBatchJobAsync(
        string jobName,
        CancellationToken cancellationToken = default)
    {
        var legacyDispatchEnabled = _configuration.GetValue<bool?>("BatchJobsApi:EnableLegacyDispatch") ?? false;
        var isLocalOrDevelopment = _environment.IsDevelopment() || _environment.IsEnvironment("Local");
        if (!legacyDispatchEnabled && !isLocalOrDevelopment)
        {
            throw new InvalidOperationException(
                "Legacy Apha.BatchJobs.Api dispatch is disabled outside Local/Development. " +
                "Use Apha.BatchJobs.Pact.Api or Apha.BatchJobs.Fps.Api EventBridge trigger endpoints.");
        }

        var eventBusName = _configuration["EventBridge:EventBusName"];
        if (string.IsNullOrWhiteSpace(eventBusName))
        {
            throw new InvalidOperationException("EventBridge:EventBusName configuration is required.");
        }

        var source = _configuration["EventBridge:Source"];
        if (!string.Equals(source, "fps.api", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(source, "pact.api", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "EventBridge:Source must be one of: fps.api, pact.api.");
        }

        var detailType = _configuration["EventBridge:DetailType"] ?? "BatchJob.TriggerRequested";
        var requestedBy = _configuration["EventBridge:RequestedBy"] ?? "api-local";
        var jobExecutionId = Guid.NewGuid().ToString("N");
        var requestedAtUtc = DateTime.UtcNow;

        var detailPayload = new
        {
            jobExecutionId,
            jobName,
            runMode = "Manual",
            requestedBy,
            userId = requestedBy,
            requestedAtUtc
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
            "Published EventBridge trigger | EventId={EventId} | JobName={JobName} | JobExecutionId={JobExecutionId} | RequestedBy={RequestedBy} | EventBus={EventBus}",
            resultEntry.EventId,
            jobName,
            jobExecutionId,
            requestedBy,
            eventBusName);

        return jobExecutionId;
    }
}
