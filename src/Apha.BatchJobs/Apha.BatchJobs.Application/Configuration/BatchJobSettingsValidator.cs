using Apha.BatchJobs.Application.Orchestration;
using Microsoft.Extensions.Options;

namespace Apha.BatchJobs.Application.Configuration;

/// <summary>
/// Rejects lock settings under which a healthy run could lose its lease. Between the last
/// renewal of a failed attempt and the first renewal of the next one, up to
/// heartbeat + longest retry delay + heartbeat passes with no renewal, so that must stay
/// below the lease. Skipped when leases are disabled (LockTimeoutSeconds = 0).
/// </summary>
public sealed class BatchJobSettingsValidator : IValidateOptions<BatchJobSettings>
{
    public ValidateOptionsResult Validate(string? name, BatchJobSettings options)
    {
        if (options.LockTimeoutSeconds <= 0)
            return ValidateOptionsResult.Success;

        // Same fallbacks JobOrchestrator applies to out-of-range values.
        var heartbeatSeconds = options.HeartbeatIntervalSeconds > 0 ? options.HeartbeatIntervalSeconds : 30;
        var retryDelaySeconds = options.RetryDelaySeconds >= 0 ? options.RetryDelaySeconds : 1;
        var maxRetryDelaySeconds = retryDelaySeconds + JobOrchestrator.RetryJitterUpperBoundExclusive(retryDelaySeconds) - 1;

        var longestGapSeconds = options.RetryAttempts > 0
            ? heartbeatSeconds + maxRetryDelaySeconds + heartbeatSeconds
            : heartbeatSeconds;

        if (longestGapSeconds >= options.LockTimeoutSeconds)
        {
            return ValidateOptionsResult.Fail(
                $"BatchJobs:LockTimeoutSeconds ({options.LockTimeoutSeconds}) must be greater than the longest gap between lock renewals " +
                $"({longestGapSeconds}s: HeartbeatIntervalSeconds={heartbeatSeconds}, RetryAttempts={options.RetryAttempts}, " +
                $"RetryDelaySeconds={retryDelaySeconds} plus up to {maxRetryDelaySeconds - retryDelaySeconds}s jitter).");
        }

        return ValidateOptionsResult.Success;
    }
}
