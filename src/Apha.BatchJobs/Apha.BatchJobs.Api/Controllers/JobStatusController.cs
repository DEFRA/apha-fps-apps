using Apha.BatchJobs.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Apha.BatchJobs.Api.Controllers;

/// <summary>
/// Provides job status and pre-check endpoints for the UI layer.
/// </summary>
[ApiController]
[Route("api/batch-jobs")]
public sealed class JobStatusController : ControllerBase
{
    private readonly IJobStatusService _statusService;
    private readonly ILogger<JobStatusController> _logger;

    public JobStatusController(
        IJobStatusService statusService,
        ILogger<JobStatusController> logger)
    {
        _statusService = statusService;
        _logger = logger;
    }

    /// <summary>
    /// GET api/batch-jobs
    /// Returns the current status of all registered batch jobs.
    /// Use this to show a status dashboard or populate a job list in the UI.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<JobStatusResult>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        _logger.LogInformation("GetAll job statuses requested");
        var results = await _statusService.GetAllStatusesAsync(cancellationToken);
        return Ok(results);
    }

    /// <summary>
    /// GET api/batch-jobs/{jobName}/status
    /// Returns the current status of a specific batch job.
    /// The UI should call this BEFORE showing the trigger button as enabled.
    /// If IsRunning is true, the button should be disabled with a message like "Job already running".
    /// </summary>
    [HttpGet("{jobName}/status")]
    [ProducesResponseType(typeof(JobStatusResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStatus(
        string jobName,
        [FromQuery] string? jobExecutionId,
        [FromQuery] DateTime? acceptedAtUtc,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(jobName))
            return BadRequest(new { error = "Job name is required." });

        Guid? correlatedExecutionId = null;
        if (!string.IsNullOrWhiteSpace(jobExecutionId))
        {
            if (!Guid.TryParse(jobExecutionId, out var parsedExecutionId))
            {
                return BadRequest(new { error = "jobExecutionId must be a valid GUID." });
            }

            correlatedExecutionId = parsedExecutionId;
        }

        _logger.LogInformation(
            "Status check requested for job: {JobName} | JobExecutionId={JobExecutionId} | AcceptedAtUtc={AcceptedAtUtc}",
            jobName,
            correlatedExecutionId?.ToString("D") ?? "n/a",
            acceptedAtUtc?.ToString("O") ?? "n/a");

        try
        {
            var result = await _statusService.GetStatusAsync(jobName, correlatedExecutionId, acceptedAtUtc, cancellationToken);

            if (correlatedExecutionId.HasValue && result.LastExecution is null)
            {
                if (result.StartupWatchdog is not null)
                {
                    return Ok(result);
                }

                return NotFound(new
                {
                    error = $"Execution '{correlatedExecutionId.Value:D}' was not found for job '{jobName}'.",
                    jobName,
                    jobExecutionId = correlatedExecutionId.Value
                });
            }

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Unknown job requested: {JobName} | {Message}", jobName, ex.Message);
            return NotFound(new { error = $"Job '{jobName}' is not registered.", jobName });
        }
    }

    /// <summary>
    /// GET api/batch-jobs/executions/{jobExecutionId}
    /// Returns current status for a single execution correlated by jobExecutionId.
    /// </summary>
    [HttpGet("executions/{jobExecutionId:guid}")]
    [ProducesResponseType(typeof(JobStatusResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStatusByExecution(Guid jobExecutionId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Execution status requested | JobExecutionId={JobExecutionId}", jobExecutionId);

        var result = await _statusService.GetStatusByExecutionIdAsync(jobExecutionId, cancellationToken);
        if (result is null)
        {
            return NotFound(new
            {
                error = $"Execution '{jobExecutionId:D}' was not found.",
                jobExecutionId
            });
        }

        return Ok(result);
    }

    /// <summary>
    /// GET api/batch-jobs/{jobName}/can-run
    /// Lightweight endpoint for the UI to decide whether to enable the trigger button.
    /// Returns 200 OK with { canRun: true } or { canRun: false, reason: "..." }.
    /// This is the recommended endpoint to call on page load or before button click.
    /// </summary>
    [HttpGet("{jobName}/can-run")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CanRun(string jobName, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(jobName))
            return BadRequest(new { error = "Job name is required." });

        try
        {
            var status = await _statusService.GetStatusAsync(jobName, null, null, cancellationToken);

            if (status.IsRunning)
            {
                return Ok(new
                {
                    canRun = false,
                    reason = "Job is already running",
                    jobQueueId = status.ActiveLock?.JobQueueId,
                    acquiredAt = status.ActiveLock?.AcquiredAt,
                    expiresAt = status.ActiveLock?.ExpiresAt
                });
            }

            return Ok(new { canRun = true });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Unknown job in can-run check: {JobName} | {Message}", jobName, ex.Message);
            return NotFound(new { error = $"Job '{jobName}' is not registered.", jobName });
        }
    }

    /// <summary>
    /// POST api/batch-jobs/{jobName}/trigger
    /// Validates and starts an ad-hoc run asynchronously.
    /// Returns 202 Accepted immediately so UI can poll status endpoints.
    /// </summary>
    [HttpPost("{jobName}/trigger")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Trigger(string jobName, CancellationToken cancellationToken)
    {
        _logger.LogWarning(
            "Apha.BatchJobs.Api trigger endpoint is disabled. Use canonical PACT/FPS trigger APIs instead | JobName={JobName}",
            jobName);

        return StatusCode(StatusCodes.Status410Gone, new
        {
            accepted = false,
            reason = "This trigger endpoint is retired. Use Apha.BatchJobs.Pact.Api or Apha.BatchJobs.Fps.Api canonical trigger endpoints.",
            jobName
        });
    }
}
