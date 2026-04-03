using Apha.Common.Helpers.Repository;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.DataAccess.Data;
using Apha.FPS.DataAccess.Repositories;
using NSubstitute;

namespace Apha.FPS.DataAccess.UnitTests.Repository
{
    public class BaseRepositoryTests
    {
        private const int DefaultTestFpsYear = 2024;

        /// <summary>
        /// Creates a BaseRepository instance using a mocked FpsDbContext.
        /// IFpsYearContext is substituted via NSubstitute.
        /// BaseRepository is concrete so it is instantiated directly — no subclass needed.
        /// </summary>
        private static BaseRepository CreateRepository()
        {
            var fpsYearContext = Substitute.For<IFpsYearContext>();
            fpsYearContext.FpsYear.Returns(DefaultTestFpsYear);

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(fpsYearContext);
            return new BaseRepository(mockContext.Object);
        }

        #region Constructor

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenContextIsNull()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new BaseRepository(null!));
        }

        #endregion

        #region ApplyPaging

        [Fact]
        public void ApplyPaging_ReturnsCorrectPage_WhenPageOneRequested()
        {
            // Arrange
            var source = Enumerable.Range(1, 10).Select(i => $"Item{i}").ToList();
            var repo = CreateRepository();

            // Act
            var result = repo.ApplyPaging(source, page: 1, pageSize: 3);

            // Assert
            Assert.Equal(3, result.Data.Count());
            Assert.Equal("Item1", result.Data.First());
            Assert.Equal("Item3", result.Data.Last());
        }

        [Fact]
        public void ApplyPaging_ReturnsCorrectPage_WhenMiddlePageRequested()
        {
            // Arrange
            var source = Enumerable.Range(1, 10).Select(i => $"Item{i}").ToList();
            var repo = CreateRepository();

            // Act
            var result = repo.ApplyPaging(source, page: 2, pageSize: 3);

            // Assert
            Assert.Equal(3, result.Data.Count());
            Assert.Equal("Item4", result.Data.First());
            Assert.Equal("Item6", result.Data.Last());
        }

        [Fact]
        public void ApplyPaging_ReturnsRemainingItems_WhenLastPageIsPartial()
        {
            // Arrange — 10 items with pageSize 3: last page (4) has only 1 item
            var source = Enumerable.Range(1, 10).Select(i => $"Item{i}").ToList();
            var repo = CreateRepository();

            // Act
            var result = repo.ApplyPaging(source, page: 4, pageSize: 3);

            // Assert
            Assert.Single(result.Data);
            Assert.Equal("Item10", result.Data.First());
        }

        [Fact]
        public void ApplyPaging_ReturnsCorrectPaginationData_WithTotalPagesAndRecords()
        {
            // Arrange — 10 items, pageSize 3 → TotalPages = ceil(10/3) = 4
            var source = Enumerable.Range(1, 10).Select(i => $"Item{i}").ToList();
            var repo = CreateRepository();

            // Act
            var result = repo.ApplyPaging(source, page: 1, pageSize: 3);

            // Assert
            Assert.Equal(1, result.PaginationData.PageNumber);
            Assert.Equal(3, result.PaginationData.PageSize);
            Assert.Equal(4, result.PaginationData.TotalPages);
            Assert.Equal(10, result.PaginationData.TotalRecords);
        }

        [Fact]
        public void ApplyPaging_ReturnsEmptyData_WhenSourceIsEmpty()
        {
            // Arrange
            var source = Enumerable.Empty<string>().ToList();
            var repo = CreateRepository();

            // Act
            var result = repo.ApplyPaging(source, page: 1, pageSize: 10);

            // Assert
            Assert.Empty(result.Data);
            Assert.Equal(0, result.PaginationData.TotalRecords);
            Assert.Equal(0, result.PaginationData.TotalPages);
        }

        [Fact]
        public void ApplyPaging_ReturnsSinglePage_WhenItemsExactlyFillPageSize()
        {
            // Arrange — 5 items, pageSize 5 → exactly 1 full page
            var source = Enumerable.Range(1, 5).Select(i => $"Item{i}").ToList();
            var repo = CreateRepository();

            // Act
            var result = repo.ApplyPaging(source, page: 1, pageSize: 5);

            // Assert
            Assert.Equal(5, result.Data.Count());
            Assert.Equal(1, result.PaginationData.TotalPages);
            Assert.Equal(5, result.PaginationData.TotalRecords);
        }

        [Fact]
        public void ApplyPaging_ReturnsEmptyData_WhenPageExceedsTotalPages()
        {
            // Arrange — requesting page 99 of a 3-item list should return no data
            var source = Enumerable.Range(1, 3).Select(i => $"Item{i}").ToList();
            var repo = CreateRepository();

            // Act
            var result = repo.ApplyPaging(source, page: 99, pageSize: 3);

            // Assert
            Assert.Empty(result.Data);
            Assert.Equal(3, result.PaginationData.TotalRecords);
            Assert.Equal(1, result.PaginationData.TotalPages);
            Assert.Equal(99, result.PaginationData.PageNumber);
        }

        #endregion
    }
}