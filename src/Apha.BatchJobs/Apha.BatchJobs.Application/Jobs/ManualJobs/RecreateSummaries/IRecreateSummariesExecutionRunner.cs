namespace Apha.BatchJobs.Application.Jobs.ManualJobs.RecreateSummaries;

public interface IRecreateSummariesExecutionRunner
{
    Task<IReadOnlyList<StepResult>> ExecuteAsync(
        string jobExecutionId,
        int month,
        int year,
        string triggeredBy,
        IRecreateSummariesStepCatalog stepCatalog,
        CancellationToken cancellationToken);
}
