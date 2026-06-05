using Apha.Common.Helpers.Repository;
using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Interfaces;
using Apha.PACT.DataAccess.Data;
using Apha.PACT.DataAccess.Repository;
using NSubstitute;

namespace Apha.PACT.DataAccess.UnitTests.Repository.CalenderMonthRepositoryTest
{
    public class CalenderMonthRepositoryTests
    {
        private static CalenderMonthRepository CreateRepository(IEnumerable<CalenderMonth> months)
        {
            var fpsRequestContext = Substitute.For<IFpsRequestContext>();
            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(fpsRequestContext);
            var mockSet = RepositoryTestHelper.CreateMockDbSet(months);

            mockContext.Setup(x => x.CalenderMonths).Returns(mockSet.Object);

            return new CalenderMonthRepository(mockContext.Object);
        }

        #region GetCalenderMonthsAsync

        [Fact]
        public async Task GetCalenderMonthsAsync_WithData_ReturnsAllMonthsOrderedByAccntsPeriod()
        {
            var months = new List<CalenderMonth>
            {
                new() { MonthNumber = 3, MonthName = "June",  AccntsPeriod = 3, Fquarter = 1 },
                new() { MonthNumber = 1, MonthName = "April", AccntsPeriod = 1, Fquarter = 1 },
                new() { MonthNumber = 2, MonthName = "May",   AccntsPeriod = 2, Fquarter = 1 }
            };
            var repo = CreateRepository(months);

            var result = (await repo.GetCalenderMonthsAsync()).ToList();

            Assert.Equal(3, result.Count);
            Assert.Equal((short)1, result[0].AccntsPeriod);
            Assert.Equal((short)2, result[1].AccntsPeriod);
            Assert.Equal((short)3, result[2].AccntsPeriod);
        }

        [Fact]
        public async Task GetCalenderMonthsAsync_EmptyTable_ReturnsEmpty()
        {
            var repo = CreateRepository([]);

            var result = await repo.GetCalenderMonthsAsync();

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetCalenderMonthsAsync_SingleEntry_ReturnsSingleItem()
        {
            var months = new List<CalenderMonth>
            {
                new() { MonthNumber = 1, MonthName = "April", AccntsPeriod = 1, Fquarter = 1 }
            };
            var repo = CreateRepository(months);

            var result = (await repo.GetCalenderMonthsAsync()).ToList();

            Assert.Single(result);
            Assert.Equal("April",    result[0].MonthName);
            Assert.Equal((short)1,   result[0].MonthNumber);
            Assert.Equal((short)1,   result[0].AccntsPeriod);
            Assert.Equal((short)1,   result[0].Fquarter);
        }

        [Fact]
        public async Task GetCalenderMonthsAsync_AllFieldsPreserved_InReturnedItems()
        {
            var months = new List<CalenderMonth>
            {
                new() { MonthNumber = 5, MonthName = "August", AccntsPeriod = 5, Fquarter = 2 }
            };
            var repo = CreateRepository(months);

            var result = (await repo.GetCalenderMonthsAsync()).ToList();

            Assert.Single(result);
            Assert.Equal((short)5,    result[0].MonthNumber);
            Assert.Equal("August",    result[0].MonthName);
            Assert.Equal((short)5,    result[0].AccntsPeriod);
            Assert.Equal((short)2,    result[0].Fquarter);
        }

        [Fact]
        public async Task GetCalenderMonthsAsync_NullableFields_ReturnedAsNull()
        {
            var months = new List<CalenderMonth>
            {
                new() { MonthNumber = null, MonthName = null, AccntsPeriod = null, Fquarter = null }
            };
            var repo = CreateRepository(months);

            var result = (await repo.GetCalenderMonthsAsync()).ToList();

            Assert.Single(result);
            Assert.Null(result[0].MonthNumber);
            Assert.Null(result[0].MonthName);
            Assert.Null(result[0].AccntsPeriod);
            Assert.Null(result[0].Fquarter);
        }

        #endregion
    }
}
