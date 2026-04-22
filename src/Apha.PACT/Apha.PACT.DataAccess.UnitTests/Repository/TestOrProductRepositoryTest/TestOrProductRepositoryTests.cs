using Apha.Common.Helpers.Repository;
using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Interfaces;
using Apha.PACT.Core.Pagination;
using Apha.PACT.DataAccess.Data;
using Apha.PACT.DataAccess.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Moq;
using NSubstitute;

namespace Apha.PACT.DataAccess.UnitTests.Repository.TestOrProductRepositoryTest
{
    /// <summary>
    /// Unit tests for TestOrProductRepository (Data Access Layer).
    /// Tests data access operations without using real database or InMemory database.
    /// Uses mocked DbContext and DbSet for testing.
    /// </summary>
    public class TestOrProductRepositoryTests
    {
        private const int DefaultTestFpsYear = 2024;

        /// <summary>
        /// Creates a TestOrProductRepository alongside mocked DbSet and context for call verification.
        /// </summary>
        private static (
            TestOrProductRepository Repo,
            Mock<DbSet<TestOrProduct>> TestOrProductsDbSet,
            Mock<FpsDbContext> Context)
            CreateRepositoryWithMocks(
                IEnumerable<TestOrProduct> testOrProducts,
                int fpsYear = DefaultTestFpsYear)
        {
            var fpsYearContext = Substitute.For<IFpsYearContext>();
            fpsYearContext.FPSYear.Returns(fpsYear);

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(fpsYearContext);
            var testOrProductsMockSet = RepositoryTestHelper.CreateMockDbSet(testOrProducts);

            RepositoryTestHelper.SetupDbSetOperations(testOrProductsMockSet);
            testOrProductsMockSet
                .Setup(x => x.AddAsync(It.IsAny<TestOrProduct>(), It.IsAny<CancellationToken>()))
                .Returns((TestOrProduct _, CancellationToken __) => new ValueTask<EntityEntry<TestOrProduct>>());
            RepositoryTestHelper.SetupSaveChanges(mockContext);

            // Setup Entry for Update operations - directly setup to return a mock that allows State property setting
            mockContext.Setup(x => x.Entry(It.IsAny<TestOrProduct>()))
                .Returns((TestOrProduct entity) =>
                {
                    var mockEntry = new Mock<EntityEntry<TestOrProduct>>(MockBehavior.Loose, new object[] { null! });
                    mockEntry.SetupProperty(e => e.State);
                    return mockEntry.Object;
                });

            mockContext.Setup(x => x.TestOrProducts).Returns(testOrProductsMockSet.Object);

            var repo = new TestOrProductRepository(mockContext.Object, fpsYearContext);
            return (repo, testOrProductsMockSet, mockContext);
        }

        private static TestOrProductRepository CreateRepository(
            IEnumerable<TestOrProduct> testOrProducts,
            int fpsYear = DefaultTestFpsYear)
            => CreateRepositoryWithMocks(testOrProducts, fpsYear).Repo;

        #region GetPagedTestOrProductsAsync

        [Fact]
        public async Task GetPagedTestOrProductsAsync_NoFilter_ReturnsAllRecordsPaged()
        {
            // Arrange
            var testOrProducts = new List<TestOrProduct>
            {
                new() { ItemCode = "TEST001", DefraUnitPrice = 100m, FpsYear = DefaultTestFpsYear },
                new() { ItemCode = "TEST002", DefraUnitPrice = 200m, FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(testOrProducts);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await repo.GetPagedTestOrProductsAsync(query);

            // Assert
            Assert.Equal(2, result.PaginationData.TotalRecords);
            Assert.Equal(2, result.Data.Count());
        }

        [Fact]
        public async Task GetPagedTestOrProductsAsync_WithItemCodeFilter_ReturnsFilteredResult()
        {
            // Arrange
            var testOrProducts = new List<TestOrProduct>
            {
                new() { ItemCode = "TEST001", DefraUnitPrice = 100m, FpsYear = DefaultTestFpsYear },
                new() { ItemCode = "PROD001", DefraUnitPrice = 200m, FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(testOrProducts);
            var query = new PaginationParameters<string>
            {
                Page = 1,
                PageSize = 10,
                Filter = "{\"ItemCode\":\"TEST\"}"
            };

            // Act
            var result = await repo.GetPagedTestOrProductsAsync(query);

            // Assert
            Assert.Single(result.Data);
            Assert.Equal("TEST001", result.Data.First().ItemCode);
        }

        [Fact]
        public async Task GetPagedTestOrProductsAsync_WithSorting_ReturnsSortedResult()
        {
            // Arrange
            var testOrProducts = new List<TestOrProduct>
            {
                new() { ItemCode = "TEST002", DefraUnitPrice = 200m, FpsYear = DefaultTestFpsYear },
                new() { ItemCode = "TEST001", DefraUnitPrice = 100m, FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(testOrProducts);
            var query = new PaginationParameters<string>
            {
                Page = 1,
                PageSize = 10,
                SortBy = "itemcode",
                Descending = false
            };

            // Act
            var result = await repo.GetPagedTestOrProductsAsync(query);

            // Assert
            Assert.Equal("TEST001", result.Data.First().ItemCode);
            Assert.Equal("TEST002", result.Data.Last().ItemCode);
        }

        [Fact]
        public async Task GetPagedTestOrProductsAsync_DescendingSort_ReturnsSortedDescending()
        {
            // Arrange
            var testOrProducts = new List<TestOrProduct>
            {
                new() { ItemCode = "TEST001", DefraUnitPrice = 100m, FpsYear = DefaultTestFpsYear },
                new() { ItemCode = "TEST002", DefraUnitPrice = 200m, FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(testOrProducts);
            var query = new PaginationParameters<string>
            {
                Page = 1,
                PageSize = 10,
                SortBy = "defraunitprice",
                Descending = true
            };

            // Act
            var result = await repo.GetPagedTestOrProductsAsync(query);

            // Assert
            Assert.Equal("TEST002", result.Data.First().ItemCode);
        }

        [Fact]
        public async Task GetPagedTestOrProductsAsync_Pagination_ReturnsCorrectPage()
        {
            // Arrange
            var testOrProducts = Enumerable.Range(1, 25).Select(i => new TestOrProduct
            {
                ItemCode = $"TEST{i:D3}",
                DefraUnitPrice = i * 10m,
                FpsYear = DefaultTestFpsYear
            }).ToList();
            var repo = CreateRepository(testOrProducts);
            var query = new PaginationParameters<string> { Page = 2, PageSize = 10 };

            // Act
            var result = await repo.GetPagedTestOrProductsAsync(query);

            // Assert
            Assert.Equal(10, result.Data.Count());
            Assert.Equal(25, result.PaginationData.TotalRecords);
            Assert.Equal(2, result.PaginationData.PageNumber);
        }

        [Fact]
        public async Task GetPagedTestOrProductsAsync_EmptyFilter_ReturnsAllRecords()
        {
            // Arrange
            var testOrProducts = new List<TestOrProduct>
            {
                new() { ItemCode = "TEST001", FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(testOrProducts);
            var query = new PaginationParameters<string> { Filter = "" };

            // Act
            var result = await repo.GetPagedTestOrProductsAsync(query);

            // Assert
            Assert.Single(result.Data);
        }

        #endregion

        #region GetTestOrProductByIdAsync

        [Fact]
        public async Task GetTestOrProductByIdAsync_ExistingItemCode_ReturnsTestOrProduct()
        {
            // Arrange
            var testOrProducts = new List<TestOrProduct>
            {
                new() { ItemCode = "TEST001", DefraUnitPrice = 100m, FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(testOrProducts);

            // Act
            var result = await repo.GetTestOrProductByIdAsync("TEST001");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("TEST001", result.ItemCode);
            Assert.Equal(100m, result.DefraUnitPrice);
        }

        [Fact]
        public async Task GetTestOrProductByIdAsync_NonExistentItemCode_ReturnsNull()
        {
            // Arrange
            var repo = CreateRepository([]);

            // Act
            var result = await repo.GetTestOrProductByIdAsync("MISSING");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetTestOrProductByIdAsync_CaseSensitiveMatch_FindsExactMatch()
        {
            // Arrange
            var testOrProducts = new List<TestOrProduct>
            {
                new() { ItemCode = "TEST001", FpsYear = DefaultTestFpsYear },
                new() { ItemCode = "test001", FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(testOrProducts);

            // Act
            var result = await repo.GetTestOrProductByIdAsync("TEST001");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("TEST001", result.ItemCode);
        }

        #endregion

        #region CreateTestOrProductAsync

        [Fact]
        public async Task CreateTestOrProductAsync_ValidEntity_SetsFpsYearAndReturnsEntity()
        {
            // Arrange
            var (repo, mockSet, mockContext) = CreateRepositoryWithMocks([], 2025);
            var entity = new TestOrProduct { ItemCode = "TEST001", DefraUnitPrice = 100m };

            // Act
            var result = await repo.CreateTestOrProductAsync(entity);

            // Assert
            Assert.Equal(2025, result.FpsYear);
            Assert.Equal("TEST001", result.ItemCode);
            mockSet.Verify(x => x.AddAsync(It.IsAny<TestOrProduct>(), It.IsAny<CancellationToken>()), Times.Once);
            mockContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CreateTestOrProductAsync_MultipleEntities_EachGetsFpsYear()
        {
            // Arrange
            var (repo, _, _) = CreateRepositoryWithMocks([], 2024);
            var entity1 = new TestOrProduct { ItemCode = "TEST001", DefraUnitPrice = 100m };
            var entity2 = new TestOrProduct { ItemCode = "TEST002", DefraUnitPrice = 200m };

            // Act
            var result1 = await repo.CreateTestOrProductAsync(entity1);
            var result2 = await repo.CreateTestOrProductAsync(entity2);

            // Assert
            Assert.Equal(2024, result1.FpsYear);
            Assert.Equal(2024, result2.FpsYear);
        }

        #endregion

        #region UpdateTestOrProductAsync

        [Fact]
        public async Task UpdateTestOrProductAsync_ValidEntity_SetsFpsYearAndUpdatesState()
        {
            // Arrange
            var existingEntity = new TestOrProduct { ItemCode = "TEST001", DefraUnitPrice = 100m, FpsYear = 2023 };
            var (repo, _, mockContext) = CreateRepositoryWithMocks([existingEntity], 2024);
            var entityToUpdate = new TestOrProduct { ItemCode = "TEST001", DefraUnitPrice = 150m };

            // Act
            var result = await repo.UpdateTestOrProductAsync(entityToUpdate);

            // Assert
            Assert.Equal(2024, result.FpsYear);
            Assert.Equal(150m, result.DefraUnitPrice);
            mockContext.Verify(x => x.Entry(entityToUpdate), Times.Once);
            mockContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpdateTestOrProductAsync_UpdatesExistingRecord_OverridesPreviousFpsYear()
        {
            // Arrange
            var existingEntity = new TestOrProduct { ItemCode = "TEST001", DefraUnitPrice = 100m, FpsYear = 2023 };
            var (repo, _, _) = CreateRepositoryWithMocks([existingEntity], 2025);
            var entityToUpdate = new TestOrProduct { ItemCode = "TEST001", DefraUnitPrice = 150m, FpsYear = 2023 };

            // Act
            var result = await repo.UpdateTestOrProductAsync(entityToUpdate);

            // Assert
            Assert.Equal(2025, result.FpsYear); // Should be updated to current FPS year
        }

        #endregion

        #region DeleteTestOrProductAsync

        [Fact]
        public async Task DeleteTestOrProductAsync_ExistingItemInCurrentYear_ReturnsTrue()
        {
            // Arrange
            var testOrProducts = new List<TestOrProduct>
            {
                new() { ItemCode = "TEST001", DefraUnitPrice = 100m, FpsYear = DefaultTestFpsYear }
            };
            var (repo, mockSet, mockContext) = CreateRepositoryWithMocks(testOrProducts, DefaultTestFpsYear);

            // Act
            var result = await repo.DeleteTestOrProductAsync("TEST001");

            // Assert
            Assert.True(result);
            mockSet.Verify(x => x.Remove(It.IsAny<TestOrProduct>()), Times.Once);
            mockContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeleteTestOrProductAsync_NonExistentItem_ReturnsFalse()
        {
            // Arrange
            var (repo, mockSet, mockContext) = CreateRepositoryWithMocks([]);

            // Act
            var result = await repo.DeleteTestOrProductAsync("MISSING");

            // Assert
            Assert.False(result);
            mockSet.Verify(x => x.Remove(It.IsAny<TestOrProduct>()), Times.Never);
            mockContext.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task DeleteTestOrProductAsync_ItemInDifferentYear_ReturnsFalse()
        {
            // Arrange
            var testOrProducts = new List<TestOrProduct>
            {
                new() { ItemCode = "TEST001", DefraUnitPrice = 100m, FpsYear = 2023 }
            };
            var (repo, mockSet, _) = CreateRepositoryWithMocks(testOrProducts, 2024);

            // Act
            var result = await repo.DeleteTestOrProductAsync("TEST001");

            // Assert
            Assert.False(result);
            mockSet.Verify(x => x.Remove(It.IsAny<TestOrProduct>()), Times.Never);
        }

        [Fact]
        public async Task DeleteTestOrProductAsync_MultipleYearsSameItemCode_DeletesOnlyCurrentYear()
        {
            // Arrange
            var testOrProducts = new List<TestOrProduct>
            {
                new() { ItemCode = "TEST001", DefraUnitPrice = 100m, FpsYear = 2023 },
                new() { ItemCode = "TEST001", DefraUnitPrice = 150m, FpsYear = DefaultTestFpsYear }
            };
            var (repo, mockSet, _) = CreateRepositoryWithMocks(testOrProducts, DefaultTestFpsYear);

            // Act
            var result = await repo.DeleteTestOrProductAsync("TEST001");

            // Assert
            Assert.True(result);
            mockSet.Verify(x => x.Remove(It.Is<TestOrProduct>(e => e.FpsYear == DefaultTestFpsYear)), Times.Once);
        }

        #endregion

        #region GetOwnersAsync

        [Fact]
        public async Task GetOwnersAsync_MultipleOwners_ReturnsDistinctOrderedList()
        {
            // Arrange
            var testOrProducts = new List<TestOrProduct>
            {
                new() { ItemCode = "TEST001", Owner = "OW3", FpsYear = DefaultTestFpsYear },
                new() { ItemCode = "TEST002", Owner = "OW1", FpsYear = DefaultTestFpsYear },
                new() { ItemCode = "TEST003", Owner = "OW2", FpsYear = DefaultTestFpsYear },
                new() { ItemCode = "TEST004", Owner = "OW1", FpsYear = DefaultTestFpsYear } // Duplicate
            };
            var repo = CreateRepository(testOrProducts);

            // Act
            var result = (await repo.GetOwnersAsync()).ToList();

            // Assert
            Assert.Equal(3, result.Count);
            Assert.Equal("OW1", result[0]); // Should be ordered
            Assert.Equal("OW2", result[1]);
            Assert.Equal("OW3", result[2]);
        }

        [Fact]
        public async Task GetOwnersAsync_NullOwners_ExcludesNullValues()
        {
            // Arrange
            var testOrProducts = new List<TestOrProduct>
            {
                new() { ItemCode = "TEST001", Owner = "OW1", FpsYear = DefaultTestFpsYear },
                new() { ItemCode = "TEST002", Owner = null, FpsYear = DefaultTestFpsYear },
                new() { ItemCode = "TEST003", Owner = "OW2", FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(testOrProducts);

            // Act
            var result = (await repo.GetOwnersAsync()).ToList();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.DoesNotContain(null, result);
        }

        [Fact]
        public async Task GetOwnersAsync_NoOwners_ReturnsEmptyList()
        {
            // Arrange
            var repo = CreateRepository([]);

            // Act
            var result = await repo.GetOwnersAsync();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetOwnersAsync_AllNullOwners_ReturnsEmptyList()
        {
            // Arrange
            var testOrProducts = new List<TestOrProduct>
            {
                new() { ItemCode = "TEST001", Owner = null, FpsYear = DefaultTestFpsYear },
                new() { ItemCode = "TEST002", Owner = null, FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(testOrProducts);

            // Act
            var result = await repo.GetOwnersAsync();

            // Assert
            Assert.Empty(result);
        }

        #endregion

        #region Filter Tests

        [Fact]
        public async Task GetPagedTestOrProductsAsync_FilterByItemDescription_ReturnsMatchingRecords()
        {
            // Arrange
            var testOrProducts = new List<TestOrProduct>
            {
                new() { ItemCode = "TEST001", ItemDescription = "Blood Test", FpsYear = DefaultTestFpsYear },
                new() { ItemCode = "TEST002", ItemDescription = "Urine Test", FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(testOrProducts);
            var query = new PaginationParameters<string>
            {
                Filter = "{\"ItemDescription\":\"Blood\"}"
            };

            // Act
            var result = await repo.GetPagedTestOrProductsAsync(query);

            // Assert
            Assert.Single(result.Data);
            Assert.Equal("TEST001", result.Data.First().ItemCode);
        }

        [Fact]
        public async Task GetPagedTestOrProductsAsync_FilterByOwner_ReturnsMatchingRecords()
        {
            // Arrange
            var testOrProducts = new List<TestOrProduct>
            {
                new() { ItemCode = "TEST001", Owner = "OW1", FpsYear = DefaultTestFpsYear },
                new() { ItemCode = "TEST002", Owner = "OW2", FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(testOrProducts);
            var query = new PaginationParameters<string>
            {
                Filter = "{\"Owner\":\"OW1\"}"
            };

            // Act
            var result = await repo.GetPagedTestOrProductsAsync(query);

            // Assert
            Assert.Single(result.Data);
            Assert.Equal("TEST001", result.Data.First().ItemCode);
        }

        [Fact]
        public async Task GetPagedTestOrProductsAsync_FilterByTestManager_ReturnsMatchingRecords()
        {
            // Arrange
            var testOrProducts = new List<TestOrProduct>
            {
                new() { ItemCode = "TEST001", TestManager = "TM001", FpsYear = DefaultTestFpsYear },
                new() { ItemCode = "TEST002", TestManager = "TM002", FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(testOrProducts);
            var query = new PaginationParameters<string>
            {
                Filter = "{\"TestManager\":\"TM001\"}"
            };

            // Act
            var result = await repo.GetPagedTestOrProductsAsync(query);

            // Assert
            Assert.Single(result.Data);
            Assert.Equal("TEST001", result.Data.First().ItemCode);
        }

        [Fact]
        public async Task GetPagedTestOrProductsAsync_InvalidJsonFilter_ReturnsAllRecords()
        {
            // Arrange
            var testOrProducts = new List<TestOrProduct>
            {
                new() { ItemCode = "TEST001", FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(testOrProducts);
            var query = new PaginationParameters<string>
            {
                Filter = "null" // null JSON deserializes to null, which should return all records
            };

            // Act
            var result = await repo.GetPagedTestOrProductsAsync(query);

            // Assert
            Assert.Single(result.Data);
        }

        #endregion

        #region Sorting Tests

        [Theory]
        [InlineData("itemdescription")]
        [InlineData("shortdescription")]
        [InlineData("owner")]
        [InlineData("testmanager")]
        [InlineData("unitpricevla")]
        public async Task GetPagedTestOrProductsAsync_SortByDifferentColumns_AppliesSorting(string sortColumn)
        {
            // Arrange
            var testOrProducts = new List<TestOrProduct>
            {
                new() { ItemCode = "TEST001", ItemDescription = "B", ShortDescription = "B", Owner = "B", TestManager = "B", UnitPriceVla = 200m, FpsYear = DefaultTestFpsYear },
                new() { ItemCode = "TEST002", ItemDescription = "A", ShortDescription = "A", Owner = "A", TestManager = "A", UnitPriceVla = 100m, FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(testOrProducts);
            var query = new PaginationParameters<string>
            {
                SortBy = sortColumn,
                Descending = false
            };

            // Act
            var result = await repo.GetPagedTestOrProductsAsync(query);

            // Assert
            Assert.Equal(2, result.Data.Count());
            // First item should have "A" values or lower numeric value
        }

        [Fact]
        public async Task GetPagedTestOrProductsAsync_InvalidSortColumn_DefaultsToItemCode()
        {
            // Arrange
            var testOrProducts = new List<TestOrProduct>
            {
                new() { ItemCode = "TEST002", FpsYear = DefaultTestFpsYear },
                new() { ItemCode = "TEST001", FpsYear = DefaultTestFpsYear }
            };
            var repo = CreateRepository(testOrProducts);
            var query = new PaginationParameters<string>
            {
                SortBy = "INVALID_COLUMN"
            };

            // Act
            var result = await repo.GetPagedTestOrProductsAsync(query);

            // Assert
            Assert.Equal("TEST001", result.Data.First().ItemCode);
        }

        #endregion
    }
}
