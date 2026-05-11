using Apha.Common.Helpers.Repository;
using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Interfaces;
using Apha.PACT.DataAccess.Data;
using Apha.PACT.DataAccess.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Moq;
using NSubstitute;

namespace Apha.PACT.DataAccess.UnitTests.Repository.ProjectMonthRepositoryTest
{
    public class ProjectMonthRepositoryTests
    {
        private const int DefaultFpsYear = 2024;

        /// <summary>
        /// Creates a ProjectMonthRepository alongside mocked DbSets and context for call verification.
        /// AddAsync is set up explicitly since it differs from the base SetupDbSetOperations.
        /// UpdateAsync uses Entry().State — tested via Callback+Throws pattern (mirrors ProjectSubContractRepositoryTests).
        /// </summary>
        private static (
            ProjectMonthRepository Repo,
            Mock<DbSet<ProjectMonth>> ProjectMonthsDbSet,
            Mock<FpsDbContext> Context)
            CreateRepositoryWithMocks(
                IEnumerable<ProjectMonth> projectMonths,
                IEnumerable<Month>? months = null,
                int fpsYear = DefaultFpsYear)
        {
            var fpsRequestContext = Substitute.For<IFpsRequestContext>();
            fpsRequestContext.FpsYear.Returns(fpsYear);

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(fpsRequestContext);

            var projectMonthsMockSet = RepositoryTestHelper.CreateMockDbSet(projectMonths);
            RepositoryTestHelper.SetupDbSetOperations(projectMonthsMockSet);
            projectMonthsMockSet
                .Setup(x => x.AddAsync(It.IsAny<ProjectMonth>(), It.IsAny<CancellationToken>()))
                .Returns((ProjectMonth _, CancellationToken __) => new ValueTask<EntityEntry<ProjectMonth>>());
            RepositoryTestHelper.SetupSaveChanges(mockContext);

            mockContext.Setup(x => x.ProjectMonths).Returns(projectMonthsMockSet.Object);

            var monthsMockSet = RepositoryTestHelper.CreateMockDbSet(months ?? []);
            mockContext.Setup(x => x.Months).Returns(monthsMockSet.Object);

            var repo = new ProjectMonthRepository(mockContext.Object, fpsRequestContext);
            return (repo, projectMonthsMockSet, mockContext);
        }

        private static ProjectMonthRepository CreateRepository(
            IEnumerable<ProjectMonth> projectMonths,
            IEnumerable<Month>? months = null,
            int fpsYear = DefaultFpsYear)
            => CreateRepositoryWithMocks(projectMonths, months, fpsYear).Repo;

        #region GetMonthsAsync

        [Fact]
        public async Task GetMonthsAsync_WithData_ReturnsAllMonthsWithMappedFields()
        {
            var months = new List<Month>
            {
                new() { MonthNumber = 1, MonthName = "January",  AccntsPeriod = 1 },
                new() { MonthNumber = 2, MonthName = "February", AccntsPeriod = 2 },
                new() { MonthNumber = 3, MonthName = "March",    AccntsPeriod = 3 }
            };
            var repo = CreateRepository([], months);

            var result = await repo.GetMonthsAsync();

            Assert.Equal(3, result.Count);
            Assert.Equal((short)1, result[0].MonthNumber);
            Assert.Equal("January", result[0].MonthName);
            Assert.Equal((short)1, result[0].AccntsPeriod);
        }

        [Fact]
        public async Task GetMonthsAsync_EmptyTable_ReturnsEmptyList()
        {
            var repo = CreateRepository([], []);

            var result = await repo.GetMonthsAsync();

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetMonthsAsync_MapsAccntsPeriodCorrectly()
        {
            var months = new List<Month>
            {
                new() { MonthNumber = 4, MonthName = "April", AccntsPeriod = 10 }
            };
            var repo = CreateRepository([], months);

            var result = await repo.GetMonthsAsync();

            Assert.Single(result);
            Assert.Equal((short)10, result[0].AccntsPeriod);
        }

        #endregion

        #region GetProjectMonthByProjectAsync

        [Fact]
        public async Task GetProjectMonthByProjectAsync_MatchingProject_ReturnsFilteredList()
        {
            var projectMonths = new List<ProjectMonth>
            {
                new() { Project = "PRJ1", MonthNo = 1, FpsYear = DefaultFpsYear },
                new() { Project = "PRJ1", MonthNo = 2, FpsYear = DefaultFpsYear },
                new() { Project = "PRJ2", MonthNo = 1, FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(projectMonths);

            var result = await repo.GetProjectMonthByProjectAsync("PRJ1");

            Assert.Equal(2, result.Count);
            Assert.All(result, r => Assert.Equal("PRJ1", r.Project));
        }

        [Fact]
        public async Task GetProjectMonthByProjectAsync_ReturnsResultsOrderedByMonthNo()
        {
            var projectMonths = new List<ProjectMonth>
            {
                new() { Project = "PRJ1", MonthNo = 3, FpsYear = DefaultFpsYear },
                new() { Project = "PRJ1", MonthNo = 1, FpsYear = DefaultFpsYear },
                new() { Project = "PRJ1", MonthNo = 2, FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(projectMonths);

            var result = await repo.GetProjectMonthByProjectAsync("PRJ1");

            Assert.Equal(3, result.Count);
            Assert.Equal(1, result[0].MonthNo);
            Assert.Equal(2, result[1].MonthNo);
            Assert.Equal(3, result[2].MonthNo);
        }

        [Fact]
        public async Task GetProjectMonthByProjectAsync_NoMatchingProject_ReturnsEmptyList()
        {
            var projectMonths = new List<ProjectMonth>
            {
                new() { Project = "PRJ1", MonthNo = 1, FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(projectMonths);

            var result = await repo.GetProjectMonthByProjectAsync("PRJ_NONE");

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetProjectMonthByProjectAsync_EmptyTable_ReturnsEmptyList()
        {
            var repo = CreateRepository([]);

            var result = await repo.GetProjectMonthByProjectAsync("PRJ1");

            Assert.Empty(result);
        }

        #endregion

        #region GetProjectMonthAsync

        [Fact]
        public async Task GetProjectMonthAsync_MatchingProjectAndMonthNo_ReturnsEntity()
        {
            var projectMonths = new List<ProjectMonth>
            {
                new() { Project = "PRJ1", MonthNo = 3, CostProfile = 250m, FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(projectMonths);

            var result = await repo.GetProjectMonthAsync("PRJ1", 3);

            Assert.NotNull(result);
            Assert.Equal("PRJ1", result.Project);
            Assert.Equal(3, result.MonthNo);
            Assert.Equal(250m, result.CostProfile);
        }

        [Fact]
        public async Task GetProjectMonthAsync_ProjectNotFound_ReturnsNull()
        {
            var projectMonths = new List<ProjectMonth>
            {
                new() { Project = "PRJ1", MonthNo = 1, FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(projectMonths);

            var result = await repo.GetProjectMonthAsync("PRJ_NONE", 1);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetProjectMonthAsync_MonthNoNotFound_ReturnsNull()
        {
            var projectMonths = new List<ProjectMonth>
            {
                new() { Project = "PRJ1", MonthNo = 1, FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(projectMonths);

            var result = await repo.GetProjectMonthAsync("PRJ1", 99);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetProjectMonthAsync_BothProjectAndMonthNoNotFound_ReturnsNull()
        {
            var repo = CreateRepository([]);

            var result = await repo.GetProjectMonthAsync("PRJ_NONE", 99);

            Assert.Null(result);
        }

        #endregion

        #region CreateProjectMonthAsync

        [Fact]
        public async Task CreateProjectMonthAsync_ValidEntity_SetsFpsYearAndSaves()
        {
            var (repo, projectMonthsMockSet, mockContext) = CreateRepositoryWithMocks([]);
            var entity = new ProjectMonth { Project = "PRJ1", MonthNo = 1, CostProfile = 100m };

            var result = await repo.CreateProjectMonthAsync(entity);

            Assert.NotNull(result);
            Assert.Equal(DefaultFpsYear, result.FpsYear);
            projectMonthsMockSet.Verify(x => x.AddAsync(It.IsAny<ProjectMonth>(), It.IsAny<CancellationToken>()), Times.Once);
            RepositoryTestHelper.VerifySaveChanges(mockContext);
        }

        [Fact]
        public async Task CreateProjectMonthAsync_SetsFpsYear_FromRequestContext()
        {
            const int customYear = 2025;
            var (repo, _, _) = CreateRepositoryWithMocks([], fpsYear: customYear);
            var entity = new ProjectMonth { Project = "PRJ1", MonthNo = 1 };

            var result = await repo.CreateProjectMonthAsync(entity);

            Assert.Equal(customYear, result.FpsYear);
        }

        [Fact]
        public async Task CreateProjectMonthAsync_ReturnsTheSameEntityInstance()
        {
            var (repo, _, _) = CreateRepositoryWithMocks([]);
            var entity = new ProjectMonth { Project = "PRJ1", MonthNo = 5, CostProfile = 500m };

            var result = await repo.CreateProjectMonthAsync(entity);

            Assert.Same(entity, result);
        }

        #endregion

        #region UpdateProjectMonthAsync

        [Fact]
        public async Task UpdateProjectMonthAsync_ValidEntity_UpdatesCostProfileAndSaves()
        {
            var projectMonths = new List<ProjectMonth>
            {
                new() { Project = "PRJ1", MonthNo = 1, CostProfile = 100m, FpsYear = DefaultFpsYear }
            };
            var (repo, _, mockContext) = CreateRepositoryWithMocks(projectMonths);
            var entity = new ProjectMonth { Project = "PRJ1", MonthNo = 1, CostProfile = 999m };

            var result = await repo.UpdateProjectMonthAsync(entity);

            Assert.Equal(999m, result.CostProfile);
            RepositoryTestHelper.VerifySaveChanges(mockContext);
        }

        [Fact]
        public async Task UpdateProjectMonthAsync_NotFound_ThrowsKeyNotFoundException()
        {
            const int customYear = 2025;
            var projectMonths = new List<ProjectMonth>
            {
                new() { Project = "PRJ1", MonthNo = 1, CostProfile = 100m, FpsYear = DefaultFpsYear }
            };
            var (repo, _, _) = CreateRepositoryWithMocks(projectMonths, fpsYear: customYear);
            var entity = new ProjectMonth { Project = "PRJ1", MonthNo = 1, CostProfile = 999m };

            await Assert.ThrowsAsync<KeyNotFoundException>(() => repo.UpdateProjectMonthAsync(entity));
        }

        #endregion

        #region DeleteProjectMonthAsync

        [Fact]
        public async Task DeleteProjectMonthAsync_ExistingRecord_RemovesAndReturnsTrue()
        {
            var projectMonths = new List<ProjectMonth>
            {
                new() { Project = "PRJ1", MonthNo = 1, FpsYear = DefaultFpsYear }
            };
            var (repo, projectMonthsMockSet, mockContext) = CreateRepositoryWithMocks(projectMonths);

            var result = await repo.DeleteProjectMonthAsync("PRJ1", 1);

            Assert.True(result);
            RepositoryTestHelper.VerifyRemove(projectMonthsMockSet);
            RepositoryTestHelper.VerifySaveChanges(mockContext);
        }

        [Fact]
        public async Task DeleteProjectMonthAsync_ProjectNotFound_ReturnsFalse()
        {
            var repo = CreateRepository([]);

            var result = await repo.DeleteProjectMonthAsync("PRJ_NONE", 1);

            Assert.False(result);
        }

        [Fact]
        public async Task DeleteProjectMonthAsync_MonthNoNotFound_ReturnsFalse()
        {
            var projectMonths = new List<ProjectMonth>
            {
                new() { Project = "PRJ1", MonthNo = 1, FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(projectMonths);

            var result = await repo.DeleteProjectMonthAsync("PRJ1", 99);

            Assert.False(result);
        }

        [Fact]
        public async Task DeleteProjectMonthAsync_WrongFpsYear_ReturnsFalse()
        {
            var projectMonths = new List<ProjectMonth>
            {
                new() { Project = "PRJ1", MonthNo = 1, FpsYear = 2020 }
            };
            var repo = CreateRepository(projectMonths, fpsYear: DefaultFpsYear);

            var result = await repo.DeleteProjectMonthAsync("PRJ1", 1);

            Assert.False(result);
        }

        [Fact]
        public async Task DeleteProjectMonthAsync_BothProjectAndMonthNoNotFound_ReturnsFalse()
        {
            var projectMonths = new List<ProjectMonth>
            {
                new() { Project = "PRJ1", MonthNo = 1, FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(projectMonths);

            var result = await repo.DeleteProjectMonthAsync("PRJ_NONE", 99);

            Assert.False(result);
        }

        #endregion
    }
}
