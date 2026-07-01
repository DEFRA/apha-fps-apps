using Apha.Common.Helpers.Repository;
using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Interfaces;
using Apha.PACT.Core.Pagination;
using Apha.PACT.DataAccess.Data;
using Apha.PACT.DataAccess.Repository;
using Microsoft.EntityFrameworkCore;
using Moq;
using Newtonsoft.Json;
using NSubstitute;

namespace Apha.PACT.DataAccess.UnitTests.Repository.MonthHourRepositoryTest
{
    public class MonthHourRepositoryTests
    {
        private static (
            MonthHourRepository Repo,
            Mock<DbSet<MonthHour>> MonthHoursDbSet,
            Mock<FpsDbContext> Context)
            CreateRepositoryWithMocks(IEnumerable<MonthHour> monthHours)
        {
            var fpsRequestContext = Substitute.For<IFpsRequestContext>();
            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(fpsRequestContext);
            var monthHoursMockSet = RepositoryTestHelper.CreateMockDbSet(monthHours);

            mockContext.Setup(x => x.MonthHours).Returns(monthHoursMockSet.Object);

            var repo = new MonthHourRepository(mockContext.Object);
            return (repo, monthHoursMockSet, mockContext);
        }

        private static MonthHourRepository CreateRepository(IEnumerable<MonthHour> monthHours)
            => CreateRepositoryWithMocks(monthHours).Repo;

        [Fact]
        public async Task GetAllAsync_WithMockedQueryable_ThrowsInvalidOperationException()
        {
            var repo = CreateRepository(
            [
                new MonthHour { Year = 2025, Month = 1 }
            ]);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            await Assert.ThrowsAsync<InvalidOperationException>(() => repo.GetAllAsync(query));
        }

        [Fact]
        public async Task GetAllAsync_WithInvalidFilterJson_ThrowsJsonSerializationException()
        {
            var repo = CreateRepository(
            [
                new MonthHour { Year = 2025, Month = 1 }
            ]);
            var query = new PaginationParameters<string>
            {
                Page = 1,
                PageSize = 10,
                Filter = "{\"Year\":"
            };

            await Assert.ThrowsAsync<JsonSerializationException>(() => repo.GetAllAsync(query));
        }

        [Fact]
        public async Task GetByYearAsync_WithData_ReturnsRowsForYearOrderedByMonth()
        {
            var monthHours = new List<MonthHour>
            {
                new() { Year = 2025, Month = 3 },
                new() { Year = 2025, Month = 1 },
                new() { Year = 2024, Month = 2 }
            };
            var repo = CreateRepository(monthHours);

            var result = await repo.GetByYearAsync(2025);

            var list = result.ToList();
            Assert.Equal(2, list.Count);
            Assert.Equal((short)1, list[0].Month);
            Assert.Equal((short)3, list[1].Month);
        }

        [Fact]
        public async Task GetByYearAsync_WithNoMatchingYear_ReturnsEmptyList()
        {
            var repo = CreateRepository([new MonthHour { Year = 2024, Month = 1 }]);

            var result = await repo.GetByYearAsync(2030);

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetDistinctYearsAsync_WithMockedQueryable_ThrowsArgumentException()
        {
            var repo = CreateRepository(
            [
                new MonthHour { Year = 2025, Month = 1 },
                new MonthHour { Year = 2024, Month = 1 }
            ]);

            await Assert.ThrowsAsync<ArgumentException>(() => repo.GetDistinctYearsAsync());
        }
    }
}
