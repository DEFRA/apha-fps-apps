using Apha.Common.Helpers.Repository;
using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Interfaces;
using Apha.PACT.DataAccess.Data;
using Apha.PACT.DataAccess.Repository;
using NSubstitute;

namespace Apha.PACT.DataAccess.UnitTests.Repository.MonthlyTimeRepositoryTest
{
    public class MonthlyTimeRepositoryTests
    {
        private const int DefaultFpsYear = 2024;

        private static MonthlyTimeRepository CreateRepository(IEnumerable<MonthlyTime> monthlyTimes)
        {
            var fpsRequestContext = Substitute.For<IFpsRequestContext>();
            fpsRequestContext.FpsYear.Returns(DefaultFpsYear);

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(fpsRequestContext);
            var mockSet = RepositoryTestHelper.CreateMockDbSet(monthlyTimes);
            RepositoryTestHelper.SetupDbSetOperations(mockSet);

            mockContext.Setup(x => x.MonthlyTimes).Returns(mockSet.Object);

            return new MonthlyTimeRepository(mockContext.Object);
        }

        #region HasMonthlyTimeEntriesAsync

        [Fact]
        public async Task HasMonthlyTimeEntriesAsync_MatchingAllThreeFields_ReturnsTrue()
        {
            var monthlyTimes = new List<MonthlyTime>
            {
                new() { WorkGroup = "WG1", TimeCode = "TC1", ParentProject = "PP1", PactStaffId = "S1", Month = 1, FpsYear = DefaultFpsYear }
            };

            var repo = CreateRepository(monthlyTimes);

            var result = await repo.HasMonthlyTimeEntriesAsync("WG1", "TC1", "PP1");

            Assert.True(result);
        }

        [Fact]
        public async Task HasMonthlyTimeEntriesAsync_NoMatchingRows_ReturnsFalse()
        {
            var monthlyTimes = new List<MonthlyTime>
            {
                new() { WorkGroup = "WG1", TimeCode = "TC1", ParentProject = "PP1", PactStaffId = "S1", Month = 1, FpsYear = DefaultFpsYear }
            };

            var repo = CreateRepository(monthlyTimes);

            var result = await repo.HasMonthlyTimeEntriesAsync("WG2", "TC2", "PP2");

            Assert.False(result);
        }

        [Fact]
        public async Task HasMonthlyTimeEntriesAsync_WorkGroupDiffers_ReturnsFalse()
        {
            var monthlyTimes = new List<MonthlyTime>
            {
                new() { WorkGroup = "WG1", TimeCode = "TC1", ParentProject = "PP1", PactStaffId = "S1", Month = 1, FpsYear = DefaultFpsYear }
            };

            var repo = CreateRepository(monthlyTimes);

            var result = await repo.HasMonthlyTimeEntriesAsync("WG_DIFFERENT", "TC1", "PP1");

            Assert.False(result);
        }

        [Fact]
        public async Task HasMonthlyTimeEntriesAsync_TimeCodeDiffers_ReturnsFalse()
        {
            var monthlyTimes = new List<MonthlyTime>
            {
                new() { WorkGroup = "WG1", TimeCode = "TC1", ParentProject = "PP1", PactStaffId = "S1", Month = 1, FpsYear = DefaultFpsYear }
            };

            var repo = CreateRepository(monthlyTimes);

            var result = await repo.HasMonthlyTimeEntriesAsync("WG1", "TC_DIFFERENT", "PP1");

            Assert.False(result);
        }

        [Fact]
        public async Task HasMonthlyTimeEntriesAsync_ParentProjectDiffers_ReturnsFalse()
        {
            var monthlyTimes = new List<MonthlyTime>
            {
                new() { WorkGroup = "WG1", TimeCode = "TC1", ParentProject = "PP1", PactStaffId = "S1", Month = 1, FpsYear = DefaultFpsYear }
            };

            var repo = CreateRepository(monthlyTimes);

            var result = await repo.HasMonthlyTimeEntriesAsync("WG1", "TC1", "PP_DIFFERENT");

            Assert.False(result);
        }

        [Fact]
        public async Task HasMonthlyTimeEntriesAsync_EmptyRepository_ReturnsFalse()
        {
            var repo = CreateRepository(Enumerable.Empty<MonthlyTime>());

            var result = await repo.HasMonthlyTimeEntriesAsync("WG1", "TC1", "PP1");

            Assert.False(result);
        }

        [Fact]
        public async Task HasMonthlyTimeEntriesAsync_MultipleRows_OnlyOneMatches_ReturnsTrue()
        {
            var monthlyTimes = new List<MonthlyTime>
            {
                new() { WorkGroup = "WG1", TimeCode = "TC1", ParentProject = "PP1", PactStaffId = "S1", Month = 1, FpsYear = DefaultFpsYear },
                new() { WorkGroup = "WG2", TimeCode = "TC2", ParentProject = "PP2", PactStaffId = "S2", Month = 2, FpsYear = DefaultFpsYear }
            };

            var repo = CreateRepository(monthlyTimes);

            var result = await repo.HasMonthlyTimeEntriesAsync("WG1", "TC1", "PP1");

            Assert.True(result);
        }

        [Fact]
        public async Task HasMonthlyTimeEntriesAsync_MultipleMatchingRows_ReturnsTrue()
        {
            var monthlyTimes = new List<MonthlyTime>
            {
                new() { WorkGroup = "WG1", TimeCode = "TC1", ParentProject = "PP1", PactStaffId = "S1", Month = 1, FpsYear = DefaultFpsYear },
                new() { WorkGroup = "WG1", TimeCode = "TC1", ParentProject = "PP1", PactStaffId = "S2", Month = 2, FpsYear = DefaultFpsYear }
            };

            var repo = CreateRepository(monthlyTimes);

            var result = await repo.HasMonthlyTimeEntriesAsync("WG1", "TC1", "PP1");

            Assert.True(result);
        }

        #endregion
    }
}
