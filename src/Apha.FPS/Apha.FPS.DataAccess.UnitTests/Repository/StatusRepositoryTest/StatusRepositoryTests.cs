using Apha.Common.Helpers.Repository;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.DataAccess.Data;
using Apha.FPS.DataAccess.Repositories;
using NSubstitute;

namespace Apha.FPS.DataAccess.UnitTests.Repository.StatusRepositoryTest
{
    public class StatusRepositoryTests
    {
        private const int DefaultTestFpsYear = 2024;

        /// <summary>
        /// Creates a StatusRepository with in-memory Statuses data.
        /// IFpsYearContext is substituted via NSubstitute.
        /// Status has no FpsCalYear query filter, so year value is irrelevant.
        /// </summary>
        private static StatusRepository CreateRepository(IEnumerable<Status> statuses)
        {
            var fpsYearContext = Substitute.For<IFpsYearContext>();
            fpsYearContext.FPSYear.Returns(DefaultTestFpsYear);

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(fpsYearContext);

            var statusesMockSet = RepositoryTestHelper.CreateMockDbSet(statuses);
            mockContext.Setup(x => x.Statuses).Returns(statusesMockSet.Object);

            return new StatusRepository(mockContext.Object);
        }

        #region GetAllStatusesAsync

        [Fact]
        public async Task GetAllStatusesAsync_ReturnsAllStatuses_WhenDataExists()
        {
            // Arrange
            var statuses = new List<Status>
            {
                new() { StatusValue = "Active" },
                new() { StatusValue = "Closed" },
                new() { StatusValue = "Pending" }
            };
            var repo = CreateRepository(statuses);

            // Act
            var result = await repo.GetAllStatusesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count());
        }

        [Fact]
        public async Task GetAllStatusesAsync_ReturnsEmptyCollection_WhenNoStatusesExist()
        {
            // Arrange
            var repo = CreateRepository(new List<Status>());

            // Act
            var result = await repo.GetAllStatusesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllStatusesAsync_ReturnsCorrectData_WhenSingleStatusExists()
        {
            // Arrange
            var statuses = new List<Status>
            {
                new() { StatusValue = "Active" }
            };
            var repo = CreateRepository(statuses);

            // Act
            var result = await repo.GetAllStatusesAsync();

            // Assert
            var single = Assert.Single(result);
            Assert.Equal("Active", single.StatusValue);
        }

        [Fact]
        public async Task GetAllStatusesAsync_ReturnsStatusesOrderedByStatusValue_WhenDataIsUnordered()
        {
            // Arrange — seed data intentionally out of order to verify OrderBy(s => s.StatusValue)
            var statuses = new List<Status>
            {
                new() { StatusValue = "Pending" },
                new() { StatusValue = "Active" },
                new() { StatusValue = "Closed" }
            };
            var repo = CreateRepository(statuses);

            // Act
            var result = await repo.GetAllStatusesAsync();

            // Assert
            var list = result.ToList();
            Assert.Equal("Active",  list[0].StatusValue);
            Assert.Equal("Closed",  list[1].StatusValue);
            Assert.Equal("Pending", list[2].StatusValue);
        }

        #endregion
    }
}   