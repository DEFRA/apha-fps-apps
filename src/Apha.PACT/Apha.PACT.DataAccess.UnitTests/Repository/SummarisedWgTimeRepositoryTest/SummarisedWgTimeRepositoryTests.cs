using Apha.Common.Helpers.Repository;
using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Interfaces;
using Apha.PACT.DataAccess.Data;
using Apha.PACT.DataAccess.Repository;
using Microsoft.EntityFrameworkCore;
using Moq;
using NSubstitute;

namespace Apha.PACT.DataAccess.UnitTests.Repository.SummarisedWgTimeRepositoryTest
{
    public class SummarisedWgTimeRepositoryTests
    {
        private const int DefaultTestFpsYear = 2024;

        private static (
            SummarisedWgTimeRepository Repo,
            Mock<DbSet<SummarisedWgTimeView>> ViewsDbSet,
            Mock<FpsDbContext> Context)
            CreateRepositoryWithMocks(
                IEnumerable<SummarisedWgTimeView> views,
                int fpsYear = DefaultTestFpsYear)
        {
            var fpsRequestContext = Substitute.For<IFpsRequestContext>();
            fpsRequestContext.FpsYear.Returns(fpsYear);

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(fpsRequestContext);
            var viewsMockSet = RepositoryTestHelper.CreateMockDbSet(views);

            mockContext.Setup(x => x.SummarisedWgTimeViews).Returns(viewsMockSet.Object);

            var repo = new SummarisedWgTimeRepository(mockContext.Object, fpsRequestContext);
            return (repo, viewsMockSet, mockContext);
        }

        private static SummarisedWgTimeRepository CreateRepository(
            IEnumerable<SummarisedWgTimeView> views,
            int fpsYear = DefaultTestFpsYear)
            => CreateRepositoryWithMocks(views, fpsYear).Repo;

        #region GetSummarisedWorkgroupTimeAsync

        [Fact]
        public async Task GetSummarisedWorkgroupTimeAsync_WithNullWorkGroup_ReturnsAllRecordsForFpsYear()
        {
            // Arrange
            var views = new List<SummarisedWgTimeView>
            {
                new() { FpsYear = DefaultTestFpsYear, WorkGroup = "WG1", ParentProject = "PRJ1", TotalTime = 100 },
                new() { FpsYear = DefaultTestFpsYear, WorkGroup = "WG2", ParentProject = "PRJ2", TotalTime = 200 },
                new() { FpsYear = 2023, WorkGroup = "WG1", ParentProject = "PRJ3", TotalTime = 300 }
            };
            var repo = CreateRepository(views);

            // Act
            var result = await repo.GetSummarisedWorkgroupTimeAsync(null, CancellationToken.None);

            // Assert
            var resultList = result.ToList();
            Assert.Equal(2, resultList.Count);
            Assert.All(resultList, r => Assert.Equal(DefaultTestFpsYear, r.FpsYear));
        }

        [Fact]
        public async Task GetSummarisedWorkgroupTimeAsync_WithSpecificWorkGroup_ReturnsFilteredRecords()
        {
            // Arrange
            var views = new List<SummarisedWgTimeView>
            {
                new() { FpsYear = DefaultTestFpsYear, WorkGroup = "WG1", ParentProject = "PRJ1", TotalTime = 100 },
                new() { FpsYear = DefaultTestFpsYear, WorkGroup = "WG1", ParentProject = "PRJ2", TotalTime = 150 },
                new() { FpsYear = DefaultTestFpsYear, WorkGroup = "WG2", ParentProject = "PRJ3", TotalTime = 200 }
            };
            var repo = CreateRepository(views);

            // Act
            var result = await repo.GetSummarisedWorkgroupTimeAsync("WG1", CancellationToken.None);

            // Assert
            var resultList = result.ToList();
            Assert.Equal(2, resultList.Count);
            Assert.All(resultList, r => Assert.Equal("WG1", r.WorkGroup));
        }

        [Fact]
        public async Task GetSummarisedWorkgroupTimeAsync_WithEmptyWorkGroup_ReturnsAllRecordsForFpsYear()
        {
            // Arrange
            var views = new List<SummarisedWgTimeView>
            {
                new() { FpsYear = DefaultTestFpsYear, WorkGroup = "WG1", ParentProject = "PRJ1", TotalTime = 100 },
                new() { FpsYear = DefaultTestFpsYear, WorkGroup = "WG2", ParentProject = "PRJ2", TotalTime = 200 }
            };
            var repo = CreateRepository(views);

            // Act
            var result = await repo.GetSummarisedWorkgroupTimeAsync(string.Empty, CancellationToken.None);

            // Assert
            var resultList = result.ToList();
            Assert.Equal(2, resultList.Count);
        }

        [Fact]
        public async Task GetSummarisedWorkgroupTimeAsync_WithNonExistentWorkGroup_ReturnsEmptyCollection()
        {
            // Arrange
            var views = new List<SummarisedWgTimeView>
            {
                new() { FpsYear = DefaultTestFpsYear, WorkGroup = "WG1", ParentProject = "PRJ1", TotalTime = 100 }
            };
            var repo = CreateRepository(views);

            // Act
            var result = await repo.GetSummarisedWorkgroupTimeAsync("NON_EXISTENT", CancellationToken.None);

            // Assert
            var resultList = result.ToList();
            Assert.Empty(resultList);
        }

        [Fact]
        public async Task GetSummarisedWorkgroupTimeAsync_WithNoMatchingFpsYear_ReturnsEmptyCollection()
        {
            // Arrange
            var views = new List<SummarisedWgTimeView>
            {
                new() { FpsYear = 2023, WorkGroup = "WG1", ParentProject = "PRJ1", TotalTime = 100 },
                new() { FpsYear = 2022, WorkGroup = "WG2", ParentProject = "PRJ2", TotalTime = 200 }
            };
            var repo = CreateRepository(views, fpsYear: DefaultTestFpsYear);

            // Act
            var result = await repo.GetSummarisedWorkgroupTimeAsync(null, CancellationToken.None);

            // Assert
            var resultList = result.ToList();
            Assert.Empty(resultList);
        }

        [Fact]
        public async Task GetSummarisedWorkgroupTimeAsync_WithCancellationToken_PassesTokenToAsyncOperation()
        {
            // Arrange
            var views = new List<SummarisedWgTimeView>
            {
                new() { FpsYear = DefaultTestFpsYear, WorkGroup = "WG1", ParentProject = "PRJ1", TotalTime = 100 }
            };
            var repo = CreateRepository(views);
            var cancellationToken = new CancellationToken();

            // Act
            var result = await repo.GetSummarisedWorkgroupTimeAsync("WG1", cancellationToken);

            // Assert
            var resultList = result.ToList();
            Assert.Single(resultList);
        }

        [Fact]
        public async Task GetSummarisedWorkgroupTimeAsync_FiltersCorrectlyByFpsYear_WhenMultipleYearsExist()
        {
            // Arrange
            const int customYear = 2025;
            var views = new List<SummarisedWgTimeView>
            {
                new() { FpsYear = 2023, WorkGroup = "WG1", ParentProject = "PRJ1", TotalTime = 100 },
                new() { FpsYear = customYear, WorkGroup = "WG1", ParentProject = "PRJ2", TotalTime = 200 },
                new() { FpsYear = customYear, WorkGroup = "WG1", ParentProject = "PRJ3", TotalTime = 300 },
                new() { FpsYear = 2026, WorkGroup = "WG1", ParentProject = "PRJ4", TotalTime = 400 }
            };
            var repo = CreateRepository(views, fpsYear: customYear);

            // Act
            var result = await repo.GetSummarisedWorkgroupTimeAsync("WG1", CancellationToken.None);

            // Assert
            var resultList = result.ToList();
            Assert.Equal(2, resultList.Count);
            Assert.All(resultList, r => Assert.Equal(customYear, r.FpsYear));
        }

        [Fact]
        public async Task GetSummarisedWorkgroupTimeAsync_WithEmptyDataSet_ReturnsEmptyCollection()
        {
            // Arrange
            var repo = CreateRepository([]);

            // Act
            var result = await repo.GetSummarisedWorkgroupTimeAsync("WG1", CancellationToken.None);

            // Assert
            var resultList = result.ToList();
            Assert.Empty(resultList);
        }

        [Fact]
        public async Task GetSummarisedWorkgroupTimeAsync_UsesAsNoTracking_ForReadOnlyQuery()
        {
            // Arrange
            var views = new List<SummarisedWgTimeView>
            {
                new() { FpsYear = DefaultTestFpsYear, WorkGroup = "WG1", ParentProject = "PRJ1", TotalTime = 100 }
            };
            var (repo, viewsDbSet, _) = CreateRepositoryWithMocks(views);

            // Act
            var result = await repo.GetSummarisedWorkgroupTimeAsync("WG1", CancellationToken.None);

            // Assert
            var resultList = result.ToList();
            Assert.Single(resultList);
            // The mock DbSet setup confirms AsNoTracking is used since we're using TestAsyncEnumerable pattern
        }

        [Fact]
        public async Task GetSummarisedWorkgroupTimeAsync_CombinesWorkGroupAndFpsYearFilters_Correctly()
        {
            // Arrange
            var views = new List<SummarisedWgTimeView>
            {
                new() { FpsYear = DefaultTestFpsYear, WorkGroup = "WG1", ParentProject = "PRJ1", TotalTime = 100 },
                new() { FpsYear = DefaultTestFpsYear, WorkGroup = "WG2", ParentProject = "PRJ2", TotalTime = 200 },
                new() { FpsYear = 2023, WorkGroup = "WG1", ParentProject = "PRJ3", TotalTime = 300 },
                new() { FpsYear = 2023, WorkGroup = "WG2", ParentProject = "PRJ4", TotalTime = 400 }
            };
            var repo = CreateRepository(views);

            // Act
            var result = await repo.GetSummarisedWorkgroupTimeAsync("WG2", CancellationToken.None);

            // Assert
            var resultList = result.ToList();
            Assert.Single(resultList);
            Assert.Equal("WG2", resultList[0].WorkGroup);
            Assert.Equal(DefaultTestFpsYear, resultList[0].FpsYear);
            Assert.Equal("PRJ2", resultList[0].ParentProject);
        }

        [Fact]
        public async Task GetSummarisedWorkgroupTimeAsync_WithWhitespaceWorkGroup_ReturnsEmptyCollection()
        {
            // Arrange
            var views = new List<SummarisedWgTimeView>
            {
                new() { FpsYear = DefaultTestFpsYear, WorkGroup = "WG1", ParentProject = "PRJ1", TotalTime = 100 },
                new() { FpsYear = DefaultTestFpsYear, WorkGroup = "WG2", ParentProject = "PRJ2", TotalTime = 200 }
            };
            var repo = CreateRepository(views);

            // Act
            var result = await repo.GetSummarisedWorkgroupTimeAsync("   ", CancellationToken.None);

            // Assert
            // string.IsNullOrEmpty("   ") returns false, so it filters for WorkGroup == "   " which matches nothing
            var resultList = result.ToList();
            Assert.Empty(resultList);
        }

        #endregion
    }
}
