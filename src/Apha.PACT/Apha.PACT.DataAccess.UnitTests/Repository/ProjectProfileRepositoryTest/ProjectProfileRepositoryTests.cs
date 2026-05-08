using Apha.Common.Helpers.Repository;
using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Interfaces;
using Apha.PACT.DataAccess.Data;
using Apha.PACT.DataAccess.Repository;
using NSubstitute;

namespace Apha.PACT.DataAccess.UnitTests.Repository.ProjectProfileRepositoryTest
{
    public class ProjectProfileRepositoryTests
    {
        private const int DefaultFpsYear = 2024;

        private static ProjectProfileRepository CreateRepository(
            IEnumerable<ProjectMonthFinal> projectMonthFinals,
            IEnumerable<ProjectMonth> projectMonths,
            IEnumerable<PeriodMonth> periodMonths)
        {
            var fpsRequestContext = Substitute.For<IFpsRequestContext>();
            fpsRequestContext.FpsYear.Returns(DefaultFpsYear);

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(fpsRequestContext);

            var finalsMockSet = RepositoryTestHelper.CreateMockDbSet(projectMonthFinals);
            var monthsMockSet = RepositoryTestHelper.CreateMockDbSet(projectMonths);
            var periodMonthsMockSet = RepositoryTestHelper.CreateMockDbSet(periodMonths);

            mockContext.Setup(x => x.ProjectMonthFinals).Returns(finalsMockSet.Object);
            mockContext.Setup(x => x.ProjectMonths).Returns(monthsMockSet.Object);
            mockContext.Setup(x => x.PeriodMonths).Returns(periodMonthsMockSet.Object);

            return new ProjectProfileRepository(mockContext.Object);
        }

        #region GetProfileGraphDataAsync

        [Fact]
        public async Task GetProfileGraphDataAsync_MatchingProjectAndMonthNo_ReturnsJoinedData()
        {
            var finals = new List<ProjectMonthFinal>
            {
                new() { Project = "PRJ1", MonthNo = 1, TotalCost = 500m },
                new() { Project = "PRJ1", MonthNo = 2, TotalCost = 800m }
            };
            var months = new List<ProjectMonth>
            {
                new() { Project = "PRJ1", MonthNo = 1, CostProfile = 100m, FpsYear = DefaultFpsYear },
                new() { Project = "PRJ1", MonthNo = 2, CostProfile = 200m, FpsYear = DefaultFpsYear }
            };

            var repo = CreateRepository(finals, months, []);

            var result = await repo.GetProfileGraphDataAsync("PRJ1");

            Assert.Equal(2, result.Count);
            Assert.All(result, r => Assert.Equal("PRJ1", finals.First(f => f.MonthNo == r.MonthNo).Project));
        }

        [Fact]
        public async Task GetProfileGraphDataAsync_MapsMonthNoProfileAndCostCorrectly()
        {
            var finals = new List<ProjectMonthFinal>
            {
                new() { Project = "PRJ1", MonthNo = 3, TotalCost = 999m }
            };
            var months = new List<ProjectMonth>
            {
                new() { Project = "PRJ1", MonthNo = 3, CostProfile = 250m, FpsYear = DefaultFpsYear }
            };

            var repo = CreateRepository(finals, months, []);

            var result = await repo.GetProfileGraphDataAsync("PRJ1");

            Assert.Single(result);
            Assert.Equal(3, result[0].MonthNo);
            Assert.Equal(250m, result[0].Profile);
            Assert.Equal(999m, result[0].Cost);
        }

        [Fact]
        public async Task GetProfileGraphDataAsync_ReturnsResultsOrderedByMonthNo()
        {
            var finals = new List<ProjectMonthFinal>
            {
                new() { Project = "PRJ1", MonthNo = 3, TotalCost = 300m },
                new() { Project = "PRJ1", MonthNo = 1, TotalCost = 100m },
                new() { Project = "PRJ1", MonthNo = 2, TotalCost = 200m }
            };
            var months = new List<ProjectMonth>
            {
                new() { Project = "PRJ1", MonthNo = 3, CostProfile = 30m, FpsYear = DefaultFpsYear },
                new() { Project = "PRJ1", MonthNo = 1, CostProfile = 10m, FpsYear = DefaultFpsYear },
                new() { Project = "PRJ1", MonthNo = 2, CostProfile = 20m, FpsYear = DefaultFpsYear }
            };

            var repo = CreateRepository(finals, months, []);

            var result = await repo.GetProfileGraphDataAsync("PRJ1");

            Assert.Equal(3, result.Count);
            Assert.Equal(1, result[0].MonthNo);
            Assert.Equal(2, result[1].MonthNo);
            Assert.Equal(3, result[2].MonthNo);
        }

        [Fact]
        public async Task GetProfileGraphDataAsync_NoMatchingProject_ReturnsEmptyList()
        {
            var finals = new List<ProjectMonthFinal>
            {
                new() { Project = "PRJ1", MonthNo = 1, TotalCost = 500m }
            };
            var months = new List<ProjectMonth>
            {
                new() { Project = "PRJ1", MonthNo = 1, CostProfile = 100m, FpsYear = DefaultFpsYear }
            };

            var repo = CreateRepository(finals, months, []);

            var result = await repo.GetProfileGraphDataAsync("PRJ_NONE");

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetProfileGraphDataAsync_NoMatchingMonthNo_ReturnsEmptyList()
        {
            var finals = new List<ProjectMonthFinal>
            {
                new() { Project = "PRJ1", MonthNo = 1, TotalCost = 500m }
            };
            var months = new List<ProjectMonth>
            {
                new() { Project = "PRJ1", MonthNo = 2, CostProfile = 100m, FpsYear = DefaultFpsYear }
            };

            var repo = CreateRepository(finals, months, []);

            var result = await repo.GetProfileGraphDataAsync("PRJ1");

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetProfileGraphDataAsync_EmptyTables_ReturnsEmptyList()
        {
            var repo = CreateRepository([], [], []);

            var result = await repo.GetProfileGraphDataAsync("PRJ1");

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetProfileGraphDataAsync_OnlyMatchesSpecifiedProject_ExcludesOtherProjects()
        {
            var finals = new List<ProjectMonthFinal>
            {
                new() { Project = "PRJ1", MonthNo = 1, TotalCost = 100m },
                new() { Project = "PRJ2", MonthNo = 1, TotalCost = 999m }
            };
            var months = new List<ProjectMonth>
            {
                new() { Project = "PRJ1", MonthNo = 1, CostProfile = 50m, FpsYear = DefaultFpsYear },
                new() { Project = "PRJ2", MonthNo = 1, CostProfile = 999m, FpsYear = DefaultFpsYear }
            };

            var repo = CreateRepository(finals, months, []);

            var result = await repo.GetProfileGraphDataAsync("PRJ1");

            Assert.Single(result);
            Assert.Equal(100m, result[0].Cost);
            Assert.Equal(50m, result[0].Profile);
        }

        [Fact]
        public async Task GetProfileGraphDataAsync_NullCostProfile_MapsCostProfileAsNull()
        {
            var finals = new List<ProjectMonthFinal>
            {
                new() { Project = "PRJ1", MonthNo = 1, TotalCost = null }
            };
            var months = new List<ProjectMonth>
            {
                new() { Project = "PRJ1", MonthNo = 1, CostProfile = null, FpsYear = DefaultFpsYear }
            };

            var repo = CreateRepository(finals, months, []);

            var result = await repo.GetProfileGraphDataAsync("PRJ1");

            Assert.Single(result);
            Assert.Null(result[0].Profile);
            Assert.Null(result[0].Cost);
        }

        #endregion

        #region GetCumulativeGraphDataAsync

        [Fact]
        public async Task GetCumulativeGraphDataAsync_MatchingData_ReturnsCumulativeResult()
        {
            var finals = new List<ProjectMonthFinal>
            {
                new() { Project = "PRJ1", MonthNo = 1, CumCost = 100m },
                new() { Project = "PRJ1", MonthNo = 2, CumCost = 300m }
            };
            var months = new List<ProjectMonth>
            {
                new() { Project = "PRJ1", MonthNo = 1, CostProfile = 50m, FpsYear = DefaultFpsYear },
                new() { Project = "PRJ1", MonthNo = 2, CostProfile = 150m, FpsYear = DefaultFpsYear }
            };
            var periodMonths = new List<PeriodMonth>
            {
                new() { EndMonth = 1, MonthNo = 1 },
                new() { EndMonth = 2, MonthNo = 2 }
            };

            var repo = CreateRepository(finals, months, periodMonths);

            var result = await repo.GetCumulativeGraphDataAsync("PRJ1");

            Assert.NotEmpty(result);
        }

        [Fact]
        public async Task GetCumulativeGraphDataAsync_ReturnsResultsOrderedByMonthNo()
        {
            var finals = new List<ProjectMonthFinal>
            {
                new() { Project = "PRJ1", MonthNo = 3, CumCost = 300m },
                new() { Project = "PRJ1", MonthNo = 1, CumCost = 100m },
                new() { Project = "PRJ1", MonthNo = 2, CumCost = 200m }
            };
            var months = new List<ProjectMonth>
            {
                new() { Project = "PRJ1", MonthNo = 1, CostProfile = 10m, FpsYear = DefaultFpsYear },
                new() { Project = "PRJ1", MonthNo = 2, CostProfile = 20m, FpsYear = DefaultFpsYear },
                new() { Project = "PRJ1", MonthNo = 3, CostProfile = 30m, FpsYear = DefaultFpsYear }
            };
            var periodMonths = new List<PeriodMonth>
            {
                new() { EndMonth = 1, MonthNo = 1 },
                new() { EndMonth = 2, MonthNo = 2 },
                new() { EndMonth = 3, MonthNo = 3 }
            };

            var repo = CreateRepository(finals, months, periodMonths);

            var result = await repo.GetCumulativeGraphDataAsync("PRJ1");

            Assert.NotEmpty(result);
            var monthNos = result.Select(r => r.MonthNo).ToList();
            Assert.Equal(monthNos.OrderBy(x => x).ToList(), monthNos);
        }

        [Fact]
        public async Task GetCumulativeGraphDataAsync_NoMatchingProject_ReturnsEmptyList()
        {
            var finals = new List<ProjectMonthFinal>
            {
                new() { Project = "PRJ1", MonthNo = 1, CumCost = 100m }
            };
            var months = new List<ProjectMonth>
            {
                new() { Project = "PRJ1", MonthNo = 1, CostProfile = 50m, FpsYear = DefaultFpsYear }
            };
            var periodMonths = new List<PeriodMonth>
            {
                new() { EndMonth = 1, MonthNo = 1 }
            };

            var repo = CreateRepository(finals, months, periodMonths);

            var result = await repo.GetCumulativeGraphDataAsync("PRJ_NONE");

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetCumulativeGraphDataAsync_EmptyTables_ReturnsEmptyList()
        {
            var repo = CreateRepository([], [], []);

            var result = await repo.GetCumulativeGraphDataAsync("PRJ1");

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetCumulativeGraphDataAsync_NoPeriodMonths_ReturnsEmptyList()
        {
            var finals = new List<ProjectMonthFinal>
            {
                new() { Project = "PRJ1", MonthNo = 1, CumCost = 100m }
            };
            var months = new List<ProjectMonth>
            {
                new() { Project = "PRJ1", MonthNo = 1, CostProfile = 50m, FpsYear = DefaultFpsYear }
            };

            var repo = CreateRepository(finals, months, []);

            var result = await repo.GetCumulativeGraphDataAsync("PRJ1");

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetCumulativeGraphDataAsync_GroupsCorrectlyByMonthNoAndCumCost()
        {
            // Two ProjectMonth rows with same MonthNo join to same ProjectMonthFinal;
            // grouping should collapse them and sum their CostProfiles.
            var finals = new List<ProjectMonthFinal>
            {
                new() { Project = "PRJ1", MonthNo = 1, CumCost = 500m }
            };
            var months = new List<ProjectMonth>
            {
                new() { Project = "PRJ1", MonthNo = 1, CostProfile = 100m, FpsYear = DefaultFpsYear },
                new() { Project = "PRJ1", MonthNo = 2, CostProfile = 200m, FpsYear = DefaultFpsYear }
            };
            var periodMonths = new List<PeriodMonth>
            {
                new() { EndMonth = 1, MonthNo = 1 },
                new() { EndMonth = 1, MonthNo = 2 }
            };

            var repo = CreateRepository(finals, months, periodMonths);

            var result = await repo.GetCumulativeGraphDataAsync("PRJ1");

            Assert.Single(result);
            Assert.Equal(1, result[0].MonthNo);
            Assert.Equal(500m, result[0].Cost);
        }

        [Fact]
        public async Task GetCumulativeGraphDataAsync_NullCumCost_MapsNullCost()
        {
            var finals = new List<ProjectMonthFinal>
            {
                new() { Project = "PRJ1", MonthNo = 1, CumCost = null }
            };
            var months = new List<ProjectMonth>
            {
                new() { Project = "PRJ1", MonthNo = 1, CostProfile = 50m, FpsYear = DefaultFpsYear }
            };
            var periodMonths = new List<PeriodMonth>
            {
                new() { EndMonth = 1, MonthNo = 1 }
            };

            var repo = CreateRepository(finals, months, periodMonths);

            var result = await repo.GetCumulativeGraphDataAsync("PRJ1");

            Assert.Single(result);
            Assert.Null(result[0].Cost);
        }

        #endregion
    }
}
