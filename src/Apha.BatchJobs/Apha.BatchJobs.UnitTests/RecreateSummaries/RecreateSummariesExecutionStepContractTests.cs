using Apha.BatchJobs.Domain.Enums;
using Apha.BatchJobs.Infrastructure.Repositories.RecreateSummaries;

namespace Apha.BatchJobs.UnitTests.RecreateSummaries;

public sealed class RecreateSummariesExecutionStepContractTests
{
    [Fact]
    public async Task ExecuteAsync_WhenStepThrows_ShouldReturnFailedStepResult()
    {
        // Arrange
        var stepType = typeof(IRecreateSummariesExecutionStep).Assembly
            .GetType("Apha.BatchJobs.Infrastructure.Repositories.RecreateSummaries.DeleteFpsTotalsStep");

        Assert.NotNull(stepType);

        var step = Activator.CreateInstance(stepType!, nonPublic: true) as IRecreateSummariesExecutionStep;

        Assert.NotNull(step);

        // Act
        var result = await step!.ExecuteAsync(context: null!, cancellationToken: CancellationToken.None);

        // Assert
        Assert.Equal("DeleteFpsTotals", result.StepName);
        Assert.Equal(0, result.RowsAffected);
        Assert.Equal(StepStatus.Failed, result.Status);
        Assert.False(string.IsNullOrWhiteSpace(result.ErrorMessage));
        Assert.True(result.EndTime >= result.StartTime);
    }
}
