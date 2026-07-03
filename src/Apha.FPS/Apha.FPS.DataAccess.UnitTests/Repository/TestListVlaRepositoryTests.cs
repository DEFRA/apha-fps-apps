using Apha.Common.Helpers.Repository;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using Apha.FPS.DataAccess.Data;
using Apha.FPS.DataAccess.Repositories;
using Moq;
using Xunit;

namespace Apha.FPS.DataAccess.UnitTests.Repository.TestListVlaRepositoryTest
{
    public class TestListVlaRepositoryTests
    {
        private const int DefaultFpsYear = 2025;
        private const string DefaultUserEmail = "test@example.com";
        private const string DefaultItemCode = "TEST001";

        private static Mock<IFpsRequestContext> CreateMockRequestContext(int year = DefaultFpsYear)
        {
            var mock = new Mock<IFpsRequestContext>();
            mock.Setup(x => x.FpsYear).Returns(year);
            mock.Setup(x => x.UserEmailId).Returns(DefaultUserEmail);
            return mock;
        }

        private static TestListVlaRepository CreateRepository(
            IEnumerable<TestOrProduct>? testOrProducts = null,
            int fpsYear = DefaultFpsYear)
        {
            var mockRequestContext = CreateMockRequestContext(fpsYear);
            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(mockRequestContext.Object);

            var testOrProductSet = RepositoryTestHelper.CreateMockDbSet(
                testOrProducts ?? Enumerable.Empty<TestOrProduct>());
            mockContext.Setup(x => x.TestOrProducts).Returns(testOrProductSet.Object);

            return new TestListVlaRepository(mockContext.Object);
        }

        #region GetPagedAsync

        [Fact]
        public async Task GetPagedAsync_WithMatchingFpsYear_ReturnsPagedData()
        {
            // Arrange
            var entities = new List<TestOrProduct>
            {
                CreateEntity("ITEM001"),
                CreateEntity("ITEM002")
            };
            var repo = CreateRepository(entities);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await repo.GetPagedAsync(query, DefaultFpsYear);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.PaginationData.TotalRecords);
        }

        [Fact]
        public async Task GetPagedAsync_WithNonMatchingFpsYear_ReturnsEmpty()
        {
            // Arrange
            var entities = new List<TestOrProduct> { CreateEntity("ITEM001") };
            var repo = CreateRepository(entities, DefaultFpsYear);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act — pass a different year
            var result = await repo.GetPagedAsync(query, 9999);

            // Assert
            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetPagedAsync_WithStringFilter_FiltersResultsByItemCode()
        {
            // Arrange
            var entities = new List<TestOrProduct>
            {
                new() { ItemCode = "ALPHA001", FpsYear = DefaultFpsYear, ItemDescription = "Alpha Test", DefraUnitPrice = 0m },
                new() { ItemCode = "BETA002",  FpsYear = DefaultFpsYear, ItemDescription = "Beta Test",  DefraUnitPrice = 0m }
            };
            var repo = CreateRepository(entities);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                Filter = "ALPHA"
            };

            // Act
            var result = await repo.GetPagedAsync(query, DefaultFpsYear);

            // Assert
            Assert.Single(result.Data);
            Assert.Equal("ALPHA001", result.Data.First().ItemCode);
        }

        [Fact]
        public async Task GetPagedAsync_WithPaging_ReturnsCorrectPage()
        {
            // Arrange
            var entities = Enumerable.Range(1, 15)
                .Select(i => new TestOrProduct
                {
                    ItemCode = $"ITEM{i:D3}",
                    FpsYear = DefaultFpsYear,
                    DefraUnitPrice = 0m
                }).ToList();

            var repo = CreateRepository(entities);
            var query = new PaginationParameters<string> { Page = 2, PageSize = 5 };

            // Act
            var result = await repo.GetPagedAsync(query, DefaultFpsYear);

            // Assert
            Assert.Equal(5, result.Data.Count());
            Assert.Equal(15, result.PaginationData.TotalRecords);
        }

        #endregion

        #region GetAllByYearAsync

        [Fact]
        public async Task GetAllByYearAsync_WithMatchingRecords_ReturnsAll()
        {
            // Arrange
            var entities = new List<TestOrProduct>
            {
                CreateEntity("ITEM001"),
                CreateEntity("ITEM002")
            };
            var repo = CreateRepository(entities);

            // Act
            var result = await repo.GetAllByYearAsync(DefaultFpsYear);

            // Assert
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetAllByYearAsync_WithNonMatchingYear_ReturnsEmpty()
        {
            // Arrange
            var entities = new List<TestOrProduct> { CreateEntity("ITEM001") };
            var repo = CreateRepository(entities);

            // Act
            var result = await repo.GetAllByYearAsync(9999);

            // Assert
            Assert.Empty(result);
        }

        #endregion

        #region GetByKeyAsync

        [Fact]
        public async Task GetByKeyAsync_WithExistingCompositeKey_ReturnsRecord()
        {
            // Arrange
            var entities = new List<TestOrProduct> { CreateEntity(DefaultItemCode) };
            var repo = CreateRepository(entities);

            // Act
            var result = await repo.GetByKeyAsync(DefaultItemCode, DefaultFpsYear);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(DefaultItemCode, result!.ItemCode);
        }

        [Fact]
        public async Task GetByKeyAsync_WithNonExistingKey_ReturnsNull()
        {
            // Arrange
            var repo = CreateRepository(Enumerable.Empty<TestOrProduct>());

            // Act
            var result = await repo.GetByKeyAsync("NOTEXIST", DefaultFpsYear);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByKeyAsync_WithWrongFpsYear_ReturnsNull()
        {
            // Arrange
            var entities = new List<TestOrProduct> { CreateEntity(DefaultItemCode) };
            var repo = CreateRepository(entities);

            // Act — correct ItemCode but wrong FpsYear
            var result = await repo.GetByKeyAsync(DefaultItemCode, 9999);

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region ExistsAsync

        [Fact]
        public async Task ExistsAsync_WithExistingRecord_ReturnsTrue()
        {
            // Arrange
            var entities = new List<TestOrProduct> { CreateEntity(DefaultItemCode) };
            var repo = CreateRepository(entities);

            // Act
            var result = await repo.ExistsAsync(DefaultItemCode, DefaultFpsYear);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ExistsAsync_WithNonExistingRecord_ReturnsFalse()
        {
            // Arrange
            var repo = CreateRepository(Enumerable.Empty<TestOrProduct>());

            // Act
            var result = await repo.ExistsAsync("NOTEXIST", DefaultFpsYear);

            // Assert
            Assert.False(result);
        }

        #endregion

        #region Helper Methods

        private static TestOrProduct CreateEntity(string itemCode = DefaultItemCode) =>
            new()
            {
                ItemCode = itemCode,
                FpsYear = DefaultFpsYear,
                ItemDescription = "Test Description",
                Owner = "PT",
                DefraUnitPrice = 100m
            };

        #endregion
    }
}
