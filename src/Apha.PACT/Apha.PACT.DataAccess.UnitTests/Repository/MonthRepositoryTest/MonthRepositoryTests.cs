using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Interfaces;
using Apha.PACT.DataAccess.Data;
using Apha.PACT.DataAccess.Repository;
using Microsoft.EntityFrameworkCore;
using NSubstitute;

namespace Apha.PACT.DataAccess.UnitTests.Repository.MonthRepositoryTest
{
    public class MonthRepositoryTests
    {
        private readonly FpsDbContext _context;
        private readonly IMonthRepository _repository;
        private readonly IFpsRequestContext _fpsRequestContext;

        public MonthRepositoryTests()
        {
            _fpsRequestContext = Substitute.For<IFpsRequestContext>();
            _fpsRequestContext.FpsYear.Returns(2024);

            var options = new DbContextOptionsBuilder<FpsDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new FpsDbContext(options, _fpsRequestContext);
            _repository = new MonthRepository(_context);
        }

        [Fact]
        public async Task GetAllMonthsAsync_ReturnsMonthsOrderedByNumber()
        {
            // Arrange
            var months = new List<Month>
            {
                new() { MonthNumber = 3, MonthName = "March" },
                new() { MonthNumber = 1, MonthName = "January" },
                new() { MonthNumber = 2, MonthName = "February" }
            };

            await _context.Months.AddRangeAsync(months);
            await _context.SaveChangesAsync();

            // Act
            var result = (await _repository.GetAllMonthsAsync()).ToList();

            // Assert
            Assert.Equal(3, result.Count);
            Assert.Equal((short)1, result[0].MonthNumber);
            Assert.Equal((short)2, result[1].MonthNumber);
            Assert.Equal((short)3, result[2].MonthNumber);
        }

        [Fact]
        public async Task GetAllMonthsAsync_ReturnsEmptyList_WhenNoMonthsExist()
        {
            // Act
            var result = await _repository.GetAllMonthsAsync();

            // Assert
            Assert.Empty(result);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}
