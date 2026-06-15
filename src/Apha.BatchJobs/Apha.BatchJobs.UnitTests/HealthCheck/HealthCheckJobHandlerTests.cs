using System;
using System.Threading;
using System.Threading.Tasks;
using Apha.BatchJobs.Application.Jobs.HealthCheck;
using Apha.BatchJobs.Domain.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Apha.BatchJobs.UnitTests.HealthCheck
{
    public class HealthCheckJobHandlerTests
    {
        [Fact]
        public void Constructor_SetsPropertiesAndThrowsOnNulls()
        {
            var dbContextFactory = new Mock<IDbContextFactory<Infrastructure.Data.BatchJobsDbContext>>().Object;
            var logger = new Mock<ILogger<HealthCheckJobHandler>>().Object;
            var options = Options.Create(new BatchJobSettings());

            var handler = new HealthCheckJobHandler(dbContextFactory, logger, options);
            Assert.Equal("HealthCheck", handler.Name);
            Assert.Equal("NoWriteValidation", handler.IdempotencyStrategy);
            Assert.Null(handler.ScheduleExpression);
            Assert.Equal("On-demand health check (no schedule)", handler.ScheduleDescription);
            Assert.Equal(300, handler.MaxExecutionSeconds);

            Assert.Throws<ArgumentNullException>(() => new HealthCheckJobHandler(null, logger, options));
            Assert.Throws<ArgumentNullException>(() => new HealthCheckJobHandler(dbContextFactory, null, options));
        }

        [Fact]
        public void HealthCheckJobRequest_Defaults_AreCorrect()
        {
            var req = new HealthCheckJobRequest();
            Assert.Equal(100, req.RecordCount);
            Assert.Equal(10, req.DelayPerRecordMs);
            Assert.False(req.ShouldFail);
        }

        // Add more tests for execution logic if/when ExecuteAsync is available
    }
}
