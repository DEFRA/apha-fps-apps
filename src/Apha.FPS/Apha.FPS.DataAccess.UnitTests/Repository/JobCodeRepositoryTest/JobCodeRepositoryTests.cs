using Apha.Common.Helpers.Repository;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.DataAccess.Data;
using Apha.FPS.DataAccess.Repositories;
using Moq;
using NSubstitute;

namespace Apha.FPS.DataAccess.UnitTests.Repository.JobCodeRepositoryTest
{
    public class JobCodeRepositoryTests
    {
        private const int DefaultTestFpsYear = 2024;
        private const string DefaultUserEmail = "test@example.com";

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
            var fpsYearContext = Substitute.For<IFpsRequestContext>();
            fpsYearContext.FpsYear.Returns(fpsYear);

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(fpsYearContext);

            var jobCodesMockSet = RepositoryTestHelper.CreateMockDbSet(jobCodes);
            mockContext.Setup(x => x.JobCodes).Returns(jobCodesMockSet.Object);

            return new JobCodeRepository(mockContext.Object, fpsYearContext);
        }

        /// <summary>
        /// Creates a JobCodeRepository with in-memory ProjectViews data for ZT tests.
        /// </summary>
        private static JobCodeRepository CreateRepositoryWithProjectViews(
            IEnumerable<ProjectView> projectViews,
            string userEmail = DefaultUserEmail,
            int fpsYear = DefaultTestFpsYear)
        {
            var fpsYearContext = Substitute.For<IFpsRequestContext>();
            fpsYearContext.FpsYear.Returns(fpsYear);
            fpsYearContext.UserEmailId.Returns(userEmail);

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(fpsYearContext);

            var projectViewsMockSet = RepositoryTestHelper.CreateMockDbSet(projectViews);
            mockContext.Setup(x => x.ProjectViews).Returns(projectViewsMockSet.Object);

            // JobCodes not needed for ZT tests but supply empty to avoid null
            var jobCodesMockSet = RepositoryTestHelper.CreateMockDbSet(new List<JobCode>());
            mockContext.Setup(x => x.JobCodes).Returns(jobCodesMockSet.Object);

            return new JobCodeRepository(mockContext.Object, fpsYearContext);
        }

        #region GetAllJobCodesAsync

        [Fact]
        public async Task GetAllJobCodesAsync_ReturnsAllJobCodes_WhenDataExists()
        {
            // Arrange
            var jobCodes = new List<JobCode>
            {
                new() { JobCodeId = "JC001", JobCodeName = "Job Code 1", FpsYear = DefaultTestFpsYear },
                new() { JobCodeId = "JC002", JobCodeName = "Job Code 2", FpsYear = DefaultTestFpsYear },
                new() { JobCodeId = "JC003", JobCodeName = "Job Code 3", FpsYear = DefaultTestFpsYear }
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
                new() { JobCodeId = "JC001", JobCodeName = "Job Code 1", FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(jobCodes);

            // Act
            var result = await repo.GetAllJobCodesAsync();

            // Assert
            var single = Assert.Single(result);
            Assert.Equal("JC001", single.JobCodeId);
            Assert.Equal("Job Code 1", single.JobCodeName);
        }

        [Fact]
        public async Task GetAllJobCodesAsync_ReturnsJobCodesOrderedById_WhenDataIsUnordered()
        {
            // Arrange — seed data intentionally out of order to verify OrderBy(j => j.JobCodeId)
            var jobCodes = new List<JobCode>
            {
                new() { JobCodeId = "JC003", FpsYear = DefaultTestFpsYear },
                new() { JobCodeId = "JC001", FpsYear = DefaultTestFpsYear },
                new() { JobCodeId = "JC002", FpsYear = DefaultTestFpsYear }
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
                new() { JobCodeId = "JC001", FpsYear = DefaultTestFpsYear },
                new() { JobCodeId = "JC002", FpsYear = DefaultTestFpsYear }
            };

            var repo = CreateRepository(
                jobCodes.Where(j => j.FpsYear == DefaultTestFpsYear),
                fpsYear: DefaultTestFpsYear);

            // Act
            var result = await repo.GetAllJobCodesAsync();

            // Assert
            Assert.Equal(2, result.Count());
            Assert.All(result, j => Assert.Equal(DefaultTestFpsYear, j.FpsYear));
        }

        #endregion

        #region GetZtJobCodesAsync

        [Fact]
        public async Task GetZtJobCodesAsync_WithMatchingData_ReturnsZtJobCodes()
        {
            // Arrange
            var projectViews = new List<ProjectView>
            {
                new() { ParentProject = "ZT001", ProjectTitle = "ZT Project 1", Program = "zt_prog", UserEmail = DefaultUserEmail },
                new() { ParentProject = "ZT002", ProjectTitle = "ZT Project 2", Program = "zt_prog", UserEmail = DefaultUserEmail }
            };
            var repo = CreateRepositoryWithProjectViews(projectViews);

            // Act
            var result = await repo.GetZtJobCodesAsync();

            // Assert
            Assert.NotNull(result);
            var list = result.ToList();
            Assert.Equal(2, list.Count);
            Assert.Contains(list, x => x.JobCode == "ZT001" && x.Description == "ZT Project 1");
            Assert.Contains(list, x => x.JobCode == "ZT002" && x.Description == "ZT Project 2");
        }

        [Fact]
        public async Task GetZtJobCodesAsync_WithNoMatchingProgram_ReturnsEmpty()
        {
            // Arrange
            var projectViews = new List<ProjectView>
            {
                new() { ParentProject = "P001", ProjectTitle = "Non ZT", Program = "other_prog", UserEmail = DefaultUserEmail }
            };
            var repo = CreateRepositoryWithProjectViews(projectViews);

            // Act
            var result = await repo.GetZtJobCodesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetZtJobCodesAsync_WithNoMatchingEmail_ReturnsEmpty()
        {
            // Arrange
            var projectViews = new List<ProjectView>
            {
                new() { ParentProject = "ZT001", ProjectTitle = "ZT Project 1", Program = "zt_prog", UserEmail = "other@example.com" }
            };
            var repo = CreateRepositoryWithProjectViews(projectViews, userEmail: DefaultUserEmail);

            // Act
            var result = await repo.GetZtJobCodesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetZtJobCodesAsync_WithEmptyProjectViews_ReturnsEmpty()
        {
            // Arrange
            var repo = CreateRepositoryWithProjectViews(new List<ProjectView>());

            // Act
            var result = await repo.GetZtJobCodesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetZtJobCodesAsync_WithMixedData_ReturnsOnlyZtMatches()
        {
            // Arrange
            var projectViews = new List<ProjectView>
            {
                new() { ParentProject = "ZT001", ProjectTitle = "ZT Match", Program = "zt_prog", UserEmail = DefaultUserEmail },
                new() { ParentProject = "P002", ProjectTitle = "No Match Prog", Program = "other_prog", UserEmail = DefaultUserEmail },
                new() { ParentProject = "ZT003", ProjectTitle = "No Match Email", Program = "zt_prog", UserEmail = "other@example.com" }
            };
            var repo = CreateRepositoryWithProjectViews(projectViews);

            // Act
            var result = await repo.GetZtJobCodesAsync();

            // Assert
            var list = result.ToList();
            Assert.Single(list);
            Assert.Equal("ZT001", list[0].JobCode);
            Assert.Equal("ZT Match", list[0].Description);
        }

        [Fact]
        public async Task GetZtJobCodesAsync_CaseInsensitiveProgram_ReturnsMatch()
        {
            // Arrange — Program is stored with mixed case, query uses ToLower() comparison
            var projectViews = new List<ProjectView>
            {
                new() { ParentProject = "ZT010", ProjectTitle = "Mixed Case", Program = "ZT_PROG", UserEmail = DefaultUserEmail }
            };
            var repo = CreateRepositoryWithProjectViews(projectViews);

            // Act
            var result = await repo.GetZtJobCodesAsync();

            // Assert
            var list = result.ToList();
            Assert.Single(list);
            Assert.Equal("ZT010", list[0].JobCode);
        }

        [Fact]
        public async Task GetZtJobCodesAsync_WithNullProgram_DoesNotMatch()
        {
            // Arrange
            var projectViews = new List<ProjectView>
            {
                new() { ParentProject = "ZT001", ProjectTitle = "Null Prog", Program = null, UserEmail = DefaultUserEmail }
            };
            var repo = CreateRepositoryWithProjectViews(projectViews);

            // Act
            var result = await repo.GetZtJobCodesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        #endregion
    }
}