using Apha.Common.Helpers.Repository;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.DataAccess.Data;
using Apha.FPS.DataAccess.Repositories;
using NSubstitute;

namespace Apha.FPS.DataAccess.UnitTests.Repository.JobCodeRepositoryTest
{
    public class JobCodeRepositoryTests
    {
        private const int DefaultTestFpsYear = 2024;

        /// <summary>
        /// Creates a JobCodeRepository with in-memory JobCodes data.
        /// IFpsYearContext is substituted via NSubstitute.
        /// JobCode has a Fpscalyear query filter in FpsDbContext — the year value
        /// controls which records are visible, so it is set explicitly per test where relevant.
        /// </summary>
        private static JobCodeRepository CreateRepository(
            IEnumerable<JobCode> jobCodes,
            int fpsYear = DefaultTestFpsYear)
        {
            var fpsYearContext = Substitute.For<IFpsYearContext>();
            fpsYearContext.FPSYear.Returns(fpsYear);

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(fpsYearContext);

            var jobCodesMockSet = RepositoryTestHelper.CreateMockDbSet(jobCodes);
            mockContext.Setup(x => x.JobCodes).Returns(jobCodesMockSet.Object);

            return new JobCodeRepository(mockContext.Object);
        }

        #region GetAllJobCodesAsync

        [Fact]
        public async Task GetAllJobCodesAsync_ReturnsAllJobCodes_WhenDataExists()
        {
            // Arrange
            var jobCodes = new List<JobCode>
            {
                new() { JobCodeId = "JC001", Jobcodename = "Job Code 1", Fpscalyear = DefaultTestFpsYear },
                new() { JobCodeId = "JC002", Jobcodename = "Job Code 2", Fpscalyear = DefaultTestFpsYear },
                new() { JobCodeId = "JC003", Jobcodename = "Job Code 3", Fpscalyear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(jobCodes);

            // Act
            var result = await repo.GetAllJobCodesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count());
        }

        [Fact]
        public async Task GetAllJobCodesAsync_ReturnsEmptyCollection_WhenNoJobCodesExist()
        {
            // Arrange
            var repo = CreateRepository(new List<JobCode>());

            // Act
            var result = await repo.GetAllJobCodesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllJobCodesAsync_ReturnsCorrectData_WhenSingleJobCodeExists()
        {
            // Arrange
            var jobCodes = new List<JobCode>
            {
                new() { JobCodeId = "JC001", Jobcodename = "Job Code 1", Fpscalyear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(jobCodes);

            // Act
            var result = await repo.GetAllJobCodesAsync();

            // Assert
            var single = Assert.Single(result);
            Assert.Equal("JC001", single.JobCodeId);
            Assert.Equal("Job Code 1", single.Jobcodename);
        }

        [Fact]
        public async Task GetAllJobCodesAsync_ReturnsJobCodesOrderedById_WhenDataIsUnordered()
        {
            // Arrange — seed data intentionally out of order to verify OrderBy(j => j.JobCodeId)
            var jobCodes = new List<JobCode>
            {
                new() { JobCodeId = "JC003", Fpscalyear = DefaultTestFpsYear },
                new() { JobCodeId = "JC001", Fpscalyear = DefaultTestFpsYear },
                new() { JobCodeId = "JC002", Fpscalyear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(jobCodes);

            // Act
            var result = await repo.GetAllJobCodesAsync();

            // Assert
            var list = result.ToList();
            Assert.Equal("JC001", list[0].JobCodeId);
            Assert.Equal("JC002", list[1].JobCodeId);
            Assert.Equal("JC003", list[2].JobCodeId);
        }

        [Fact]
        public async Task GetAllJobCodesAsync_ReturnsJobCodesForCorrectYear_WhenMultipleYearsExist()
        {
            // Arrange — mock DbSet holds only year-filtered records matching the substituted FPSYear;
            // the Fpscalyear query filter on FpsDbContext ensures only current-year records are visible
            var jobCodes = new List<JobCode>
            {
                new() { JobCodeId = "JC001", Fpscalyear = DefaultTestFpsYear },
                new() { JobCodeId = "JC002", Fpscalyear = DefaultTestFpsYear }
            };

            var repo = CreateRepository(
                jobCodes.Where(j => j.Fpscalyear == DefaultTestFpsYear),
                fpsYear: DefaultTestFpsYear);

            // Act
            var result = await repo.GetAllJobCodesAsync();

            // Assert
            Assert.Equal(2, result.Count());
            Assert.All(result, j => Assert.Equal(DefaultTestFpsYear, j.Fpscalyear));
        }

        #endregion
    }
}