using Apha.Common.Helpers.Repository;
using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Interfaces;
using Apha.PACT.DataAccess.Data;
using Apha.PACT.DataAccess.Repository;
using Microsoft.EntityFrameworkCore;
using Moq;
using NSubstitute;

namespace Apha.PACT.DataAccess.UnitTests.Repository.CalenderMonthRepositoryTest
{
    public class CalenderMonthRepositoryTests
    {
        private static (
            CalenderMonthRepository Repo,
            Mock<DbSet<CalenderMonth>> CalenderMonthsDbSet,
            Mock<FpsDbContext> Context)
            CreateRepositoryWithMocks(IEnumerable<CalenderMonth> calenderMonths)
        {
            var fpsYearContext = Substitute.For<IFpsRequestContext>();
            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(fpsYearContext);
            var calenderMonthsMockSet = RepositoryTestHelper.CreateMockDbSet(calenderMonths);

            mockContext.Setup(x => x.CalenderMonths).Returns(calenderMonthsMockSet.Object);

            var repo = new CalenderMonthRepository(mockContext.Object);
            return (repo, calenderMonthsMockSet, mockContext);
        }

        private static CalenderMonthRepository CreateRepository(IEnumerable<CalenderMonth> calenderMonths)
            => CreateRepositoryWithMocks(calenderMonths).Repo;

        #region GetAllCalenderMonthsAsync

        [Fact]
        public async Task GetAllCalenderMonthsAsync_WithData_ReturnsOrderedByMonthNumber()
        {
            var calenderMonths = new List<CalenderMonth>
            {
                new() { MonthNumber = 3, MonthName = "March" },
                new() { MonthNumber = 1, MonthName = "January" },
                new() { MonthNumber = 2, MonthName = "February" }
            };
            var repo = CreateRepository(calenderMonths);

            var result = (await repo.GetAllCalenderMonthsAsync()).ToList();

            Assert.Equal(3, result.Count);
            Assert.Equal((short)1, result[0].MonthNumber);
            Assert.Equal("January", result[0].MonthName);
            Assert.Equal((short)2, result[1].MonthNumber);
            Assert.Equal("February", result[1].MonthName);
            Assert.Equal((short)3, result[2].MonthNumber);
            Assert.Equal("March", result[2].MonthName);
        }

        [Fact]
        public async Task GetAllCalenderMonthsAsync_EmptyData_ReturnsEmptyList()
        {
            var repo = CreateRepository([]);

            var result = await repo.GetAllCalenderMonthsAsync();

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllCalenderMonthsAsync_SingleEntry_ReturnsSingleItem()
        {
            var calenderMonths = new List<CalenderMonth>
            {
                new() { MonthNumber = 7, MonthName = "July" }
            };
            var repo = CreateRepository(calenderMonths);

            var result = (await repo.GetAllCalenderMonthsAsync()).ToList();

            Assert.Single(result);
            Assert.Equal((short)7, result[0].MonthNumber);
            Assert.Equal("July", result[0].MonthName);
        }

        [Fact]
        public async Task GetAllCalenderMonthsAsync_WithAllTwelveMonths_ReturnsAllOrderedByMonthNumber()
        {
            var calenderMonths = Enumerable.Range(1, 12)
                .Reverse()
                .Select(n => new CalenderMonth { MonthNumber = (short)n, MonthName = $"Month{n}" })
                .ToList();
            var repo = CreateRepository(calenderMonths);

            var result = (await repo.GetAllCalenderMonthsAsync()).ToList();

            Assert.Equal(12, result.Count);
            for (int i = 0; i < 12; i++)
            {
                Assert.Equal((short)(i + 1), result[i].MonthNumber);
            }
        }

        #endregion
    }
}
