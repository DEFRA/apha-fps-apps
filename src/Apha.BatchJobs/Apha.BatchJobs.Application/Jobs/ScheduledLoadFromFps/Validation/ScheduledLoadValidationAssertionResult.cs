namespace Apha.BatchJobs.Application.Jobs.ScheduledLoadFromFps.Validation;

/// <summary>
/// Represents a single cross-validation assertion outcome.
/// </summary>
public sealed record ScheduledLoadValidationAssertionResult(
    string AssertionCode,
    string AssertionDescription,
    decimal ExpectedValue,
    decimal ActualValue,
    bool Passed,
    string? ErrorMessage = null);
