namespace Apha.BatchJobs.Infrastructure.Data;

/// <summary>EF entity for fps.scheduled_load_run.</summary>
internal sealed class ScheduledLoadRunTable
{
    public Guid RunId { get; set; }
    public required string JobName { get; set; }
    public int FpsYear { get; set; }
    public DateTime JobStartedAt { get; set; }
    public DateTime? JobCompletedAt { get; set; }
    public string? FinalStatus { get; set; }
    public required string CorrelationId { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>EF entity for fps.scheduled_load_step_run.</summary>
internal sealed class ScheduledLoadStepRunTable
{
    public Guid StepRunId { get; set; }
    public Guid RunId { get; set; }
    public required string StepName { get; set; }
    public int StepSequence { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public required string StepStatus { get; set; }
    public string? ErrorMessage { get; set; }
    public int? RowsAffected { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>EF entity for fps.scheduled_load_validation_result.</summary>
internal sealed class ScheduledLoadValidationResultTable
{
    public Guid ValidationId { get; set; }
    public Guid RunId { get; set; }
    public required string AssertionCode { get; set; }
    public required string AssertionDescription { get; set; }
    public decimal? ExpectedValue { get; set; }
    public decimal? ActualValue { get; set; }
    public bool Passed { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CheckedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
