using Apha.Common.Helpers.Repository;
using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Interfaces;
using Apha.PACT.DataAccess.Data;
using Apha.PACT.DataAccess.Repository;
using Microsoft.EntityFrameworkCore;
using Moq;
using NSubstitute;

namespace Apha.PACT.DataAccess.UnitTests.Repository.MonthRepositoryTest
{
    public class MonthRepositoryTests
    {
        /// <summary>
        /// MonthRepository has no IFpsYearContext dependency and only reads data.
        /// All query logic (AsNoTracking, OrderBy, ToListAsync) is exercised through the mock DbSet.
        /// </summary>
        private static (
            MonthRepository Repo,
            Mock<DbSet<Month>> MonthsDbSet,
            Mock<FpsDbContext> Context)
            CreateRepositoryWithMocks(IEnumerable<Month> months)
        {
            var fpsYearContext = Substitute.For<IFpsRequestContext>();
            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(fpsYearContext);
            var monthsMockSet = RepositoryTestHelper.CreateMockDbSet(months);

            mockContext.Setup(x => x.Months).Returns(monthsMockSet.Object);

            var repo = new MonthRepository(mockContext.Object);
            return (repo, monthsMockSet, mockContext);
        }

        private static MonthRepository CreateRepository(IEnumerable<Month> months)
            => CreateRepositoryWithMocks(months).Repo;

        #region GetAllMonthsAsync

        [Fact]
        public async Task GetAllMonthsAsync_WithData_ReturnsOrderedList()
        {
            var months = new List<Month>
            {
                new() { MonthNumber = 3, MonthName = "March" },
                new() { MonthNumber = 1, MonthName = "January" },
                new() { MonthNumber = 2, MonthName = "February" }
            };
            var repo = CreateRepository(months);

            var result = (await repo.GetAllMonthsAsync()).ToList();

            Assert.Equal(3, result.Count);
            // OrderBy(m => m.Monthnumber) applied in repository
            Assert.Equal((short)1, result[0].MonthNumber);
            Assert.Equal("January", result[0].MonthName);
            Assert.Equal((short)2, result[1].MonthNumber);
            Assert.Equal("February", result[1].MonthName);
            Assert.Equal((short)3, result[2].MonthNumber);
            Assert.Equal("March", result[2].MonthName);
        }

        [Fact]
        public async Task GetAllMonthsAsync_EmptyData_ReturnsEmptyList()
        {
            var repo = CreateRepository([]);

            var result = await repo.GetAllMonthsAsync();

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllMonthsAsync_SingleEntry_ReturnsSingleItem()
        {
            var months = new List<Month>
            {
                new() { MonthNumber = 1, MonthName = "January" }
            };
            var repo = CreateRepository(months);

            var result = (await repo.GetAllMonthsAsync()).ToList();

            Assert.Single(result);
            Assert.Equal((short)1, result[0].MonthNumber);
            Assert.Equal("January", result[0].MonthName);
        }

        #endregion
    }
}
