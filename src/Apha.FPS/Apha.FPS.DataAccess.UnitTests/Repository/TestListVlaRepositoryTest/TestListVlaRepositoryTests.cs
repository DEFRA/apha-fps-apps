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

            return new TestListVlaRepository(mockContext.Object, mockRequestContext.Object);
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
            var result = await repo.GetPagedAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.PaginationData.TotalRecords);
        }

        [Fact]
        public async Task GetPagedAsync_WithNonMatchingFpsYear_ReturnsEmpty()
        {
            // Arrange
            var entities = new List<TestOrProduct> { CreateEntity("ITEM001") };
            var repo = CreateRepository(entities, 9999);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act — repo context has year 9999, entities have year 2025
            var result = await repo.GetPagedAsync(query);

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
            var result = await repo.GetPagedAsync(query);

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
            var result = await repo.GetPagedAsync(query);

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
            var result = await repo.GetAllByYearAsync();

            // Assert
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetAllByYearAsync_WithNonMatchingYear_ReturnsEmpty()
        {
            // Arrange
            var entities = new List<TestOrProduct> { CreateEntity("ITEM001") };
            var repo = CreateRepository(entities, 9999);

            // Act
            var result = await repo.GetAllByYearAsync();

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
            var result = await repo.GetByKeyAsync(DefaultItemCode);

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
            var result = await repo.GetByKeyAsync("NOTEXIST");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByKeyAsync_WithWrongFpsYear_ReturnsNull()
        {
            // Arrange
            var entities = new List<TestOrProduct> { CreateEntity(DefaultItemCode) };
            var repo = CreateRepository(entities, 9999);

            // Act — correct ItemCode but repo context has year 9999, entities have year 2025
            var result = await repo.GetByKeyAsync(DefaultItemCode);

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
            var result = await repo.ExistsAsync(DefaultItemCode);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ExistsAsync_WithNonExistingRecord_ReturnsFalse()
        {
            // Arrange
            var repo = CreateRepository(Enumerable.Empty<TestOrProduct>());

            // Act
            var result = await repo.ExistsAsync("NOTEXIST");

            // Assert
            Assert.False(result);
        }

        #endregion

        #region AddAsync

        [Fact]
        public async Task AddAsync_ValidEntity_ReturnsAddedEntity()
        {
            // Arrange
            var repo = CreateRepository(Enumerable.Empty<TestOrProduct>());
            var entity = CreateEntity(DefaultItemCode);

            // Act
            var result = await repo.AddAsync(entity);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(DefaultItemCode, result.ItemCode);
            Assert.Equal(DefaultFpsYear, result.FpsYear);
        }

        [Fact]
        public async Task AddAsync_ValidEntity_CallsDbSetAdd()
        {
            // Arrange
            var mockRequestContext = CreateMockRequestContext();
            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(mockRequestContext.Object);
            var mockSet = RepositoryTestHelper.CreateMockDbSet(Enumerable.Empty<TestOrProduct>());
            mockContext.Setup(x => x.TestOrProducts).Returns(mockSet.Object);
            var repo = new TestListVlaRepository(mockContext.Object, mockRequestContext.Object);
            var entity = CreateEntity(DefaultItemCode);

            // Act
            await repo.AddAsync(entity);

            // Assert
            mockSet.Verify(s => s.Add(entity), Moq.Times.Once);
        }

        #endregion

        #region UpdateAsync

        [Fact]
        public async Task UpdateAsync_ExistingEntity_ReturnsUpdatedEntity()
        {
            // Arrange
            var existing = CreateEntity(DefaultItemCode);
            var repo = CreateRepository(new List<TestOrProduct> { existing });

            var updated = new TestOrProduct
            {
                ItemCode         = DefaultItemCode,
                FpsYear          = DefaultFpsYear,
                ItemDescription  = "Updated Description",
                TestManager      = "TM01",
                JobStatus        = "Active",
                UnitPriceVla     = 200m,
                PriceAhvg        = 50m,
                Owner            = "PA",
                ChargeMethod     = "F",
                ShortDescription = "Short",
                DefraUnitPrice   = 150m
            };

            // Act
            var result = await repo.UpdateAsync(updated);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Updated Description", result.ItemDescription);
            Assert.Equal("TM01", result.TestManager);
            Assert.Equal("Active", result.JobStatus);
            Assert.Equal(200m, result.UnitPriceVla);
            Assert.Equal(50m, result.PriceAhvg);
            Assert.Equal("PA", result.Owner);
            Assert.Equal("F", result.ChargeMethod);
            Assert.Equal("Short", result.ShortDescription);
            Assert.Equal(150m, result.DefraUnitPrice);
        }

        [Fact]
        public async Task UpdateAsync_NonExistingEntity_ThrowsKeyNotFoundException()
        {
            // Arrange
            var repo = CreateRepository(Enumerable.Empty<TestOrProduct>());
            var entity = CreateEntity("NOTEXIST");

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => repo.UpdateAsync(entity));
        }

        #endregion

        #region DeleteAsync

        [Fact]
        public async Task DeleteAsync_ExistingRecord_ReturnsTrue()
        {
            // Arrange
            var entities = new List<TestOrProduct> { CreateEntity(DefaultItemCode) };
            var repo = CreateRepository(entities);

            // Act
            var result = await repo.DeleteAsync(DefaultItemCode);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task DeleteAsync_NonExistingRecord_ReturnsFalse()
        {
            // Arrange
            var repo = CreateRepository(Enumerable.Empty<TestOrProduct>());

            // Act
            var result = await repo.DeleteAsync("NOTEXIST");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteAsync_ExistingRecord_CallsDbSetRemove()
        {
            // Arrange
            var entity = CreateEntity(DefaultItemCode);
            var mockRequestContext = CreateMockRequestContext();
            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(mockRequestContext.Object);
            var mockSet = RepositoryTestHelper.CreateMockDbSet(new List<TestOrProduct> { entity });
            mockContext.Setup(x => x.TestOrProducts).Returns(mockSet.Object);
            var repo = new TestListVlaRepository(mockContext.Object, mockRequestContext.Object);

            // Act
            await repo.DeleteAsync(DefaultItemCode);

            // Assert
            mockSet.Verify(s => s.Remove(It.IsAny<TestOrProduct>()), Moq.Times.Once);
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

        #region GetPagedAsync - Filters

        [Fact]
        public async Task GetPagedAsync_WithJsonItemCodeFilter_FiltersResults()
        {
            // Arrange
            var entities = new List<TestOrProduct>
            {
                new() { ItemCode = "ALPHA001", FpsYear = DefaultFpsYear, ItemDescription = "Desc A", DefraUnitPrice = 0m },
                new() { ItemCode = "BETA002",  FpsYear = DefaultFpsYear, ItemDescription = "Desc B", DefraUnitPrice = 0m }
            };
            var repo = CreateRepository(entities);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                Filter = "{\"ItemCode\":\"ALPHA\"}"
            };

            // Act
            var result = await repo.GetPagedAsync(query);

            // Assert
            Assert.Single(result.Data);
            Assert.Equal("ALPHA001", result.Data.First().ItemCode);
        }

        [Fact]
        public async Task GetPagedAsync_WithJsonItemDescriptionFilter_FiltersResults()
        {
            // Arrange
            var entities = new List<TestOrProduct>
            {
                new() { ItemCode = "I001", FpsYear = DefaultFpsYear, ItemDescription = "BloodTest", DefraUnitPrice = 0m },
                new() { ItemCode = "I002", FpsYear = DefaultFpsYear, ItemDescription = "UrineTest", DefraUnitPrice = 0m }
            };
            var repo = CreateRepository(entities);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                Filter = "{\"ItemDescription\":\"Blood\"}"
            };

            // Act
            var result = await repo.GetPagedAsync(query);

            // Assert
            Assert.Single(result.Data);
            Assert.Equal("I001", result.Data.First().ItemCode);
        }

        [Fact]
        public async Task GetPagedAsync_WithJsonOwnerFilter_FiltersResults()
        {
            // Arrange
            var entities = new List<TestOrProduct>
            {
                new() { ItemCode = "I001", FpsYear = DefaultFpsYear, Owner = "PT", DefraUnitPrice = 0m },
                new() { ItemCode = "I002", FpsYear = DefaultFpsYear, Owner = "PA", DefraUnitPrice = 0m }
            };
            var repo = CreateRepository(entities);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                Filter = "{\"Owner\":\"PT\"}"
            };

            // Act
            var result = await repo.GetPagedAsync(query);

            // Assert
            Assert.Single(result.Data);
            Assert.Equal("I001", result.Data.First().ItemCode);
        }

        [Fact]
        public async Task GetPagedAsync_WithNullFilter_ReturnsAll()
        {
            // Arrange
            var entities = new List<TestOrProduct>
            {
                new() { ItemCode = "I001", FpsYear = DefaultFpsYear, DefraUnitPrice = 0m },
                new() { ItemCode = "I002", FpsYear = DefaultFpsYear, DefraUnitPrice = 0m }
            };
            var repo = CreateRepository(entities);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = null };

            // Act
            var result = await repo.GetPagedAsync(query);

            // Assert
            Assert.Equal(2, result.PaginationData.TotalRecords);
        }

        [Fact]
        public async Task GetPagedAsync_WithEmptyFilter_ReturnsAll()
        {
            // Arrange
            var entities = new List<TestOrProduct>
            {
                new() { ItemCode = "I001", FpsYear = DefaultFpsYear, DefraUnitPrice = 0m },
                new() { ItemCode = "I002", FpsYear = DefaultFpsYear, DefraUnitPrice = 0m }
            };
            var repo = CreateRepository(entities);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = "" };

            // Act
            var result = await repo.GetPagedAsync(query);

            // Assert
            Assert.Equal(2, result.PaginationData.TotalRecords);
        }

        #endregion

        #region GetPagedAsync - Sorting

        [Fact]
        public async Task GetPagedAsync_SortByItemCodeAscending_ReturnsOrderedResults()
        {
            // Arrange
            var entities = new List<TestOrProduct>
            {
                new() { ItemCode = "ZITEM", FpsYear = DefaultFpsYear, DefraUnitPrice = 0m },
                new() { ItemCode = "AITEM", FpsYear = DefaultFpsYear, DefraUnitPrice = 0m }
            };
            var repo = CreateRepository(entities);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = "itemcode", Descending = false };

            // Act
            var result = await repo.GetPagedAsync(query);

            // Assert
            Assert.Equal("AITEM", result.Data.First().ItemCode);
        }

        [Fact]
        public async Task GetPagedAsync_SortByItemCodeDescending_ReturnsOrderedResults()
        {
            // Arrange
            var entities = new List<TestOrProduct>
            {
                new() { ItemCode = "AITEM", FpsYear = DefaultFpsYear, DefraUnitPrice = 0m },
                new() { ItemCode = "ZITEM", FpsYear = DefaultFpsYear, DefraUnitPrice = 0m }
            };
            var repo = CreateRepository(entities);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = "itemcode", Descending = true };

            // Act
            var result = await repo.GetPagedAsync(query);

            // Assert
            Assert.Equal("ZITEM", result.Data.First().ItemCode);
        }

        [Fact]
        public async Task GetPagedAsync_SortByOwnerAscending_ReturnsOrderedResults()
        {
            // Arrange
            var entities = new List<TestOrProduct>
            {
                new() { ItemCode = "I002", FpsYear = DefaultFpsYear, Owner = "ZZ", DefraUnitPrice = 0m },
                new() { ItemCode = "I001", FpsYear = DefaultFpsYear, Owner = "AA", DefraUnitPrice = 0m }
            };
            var repo = CreateRepository(entities);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = "owner", Descending = false };

            // Act
            var result = await repo.GetPagedAsync(query);

            // Assert
            Assert.Equal("I001", result.Data.First().ItemCode);
        }

        [Fact]
        public async Task GetPagedAsync_SortByDefraUnitPriceAscending_ReturnsOrderedResults()
        {
            // Arrange
            var entities = new List<TestOrProduct>
            {
                new() { ItemCode = "I002", FpsYear = DefaultFpsYear, DefraUnitPrice = 500m },
                new() { ItemCode = "I001", FpsYear = DefaultFpsYear, DefraUnitPrice = 100m }
            };
            var repo = CreateRepository(entities);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = "defraunitprice", Descending = false };

            // Act
            var result = await repo.GetPagedAsync(query);

            // Assert
            Assert.Equal("I001", result.Data.First().ItemCode);
        }

        [Fact]
        public async Task GetPagedAsync_SortByUnitPriceVlaAscending_ReturnsOrderedResults()
        {
            // Arrange
            var entities = new List<TestOrProduct>
            {
                new() { ItemCode = "I002", FpsYear = DefaultFpsYear, UnitPriceVla = 999m, DefraUnitPrice = 0m },
                new() { ItemCode = "I001", FpsYear = DefaultFpsYear, UnitPriceVla = 10m, DefraUnitPrice = 0m }
            };
            var repo = CreateRepository(entities);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = "unitpricevla", Descending = false };

            // Act
            var result = await repo.GetPagedAsync(query);

            // Assert
            Assert.Equal("I001", result.Data.First().ItemCode);
        }

        [Fact]
        public async Task GetPagedAsync_SortByItemDescriptionAscending_ReturnsOrderedResults()
        {
            // Arrange
            var entities = new List<TestOrProduct>
            {
                new() { ItemCode = "I002", FpsYear = DefaultFpsYear, ItemDescription = "Zeta Test", DefraUnitPrice = 0m },
                new() { ItemCode = "I001", FpsYear = DefaultFpsYear, ItemDescription = "Alpha Test", DefraUnitPrice = 0m }
            };
            var repo = CreateRepository(entities);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = "itemdescription", Descending = false };

            // Act
            var result = await repo.GetPagedAsync(query);

            // Assert
            Assert.Equal("I001", result.Data.First().ItemCode);
        }

        [Fact]
        public async Task GetPagedAsync_SortByJobStatusAscending_ReturnsOrderedResults()
        {
            // Arrange
            var entities = new List<TestOrProduct>
            {
                new() { ItemCode = "I002", FpsYear = DefaultFpsYear, JobStatus = "Z-Status", DefraUnitPrice = 0m },
                new() { ItemCode = "I001", FpsYear = DefaultFpsYear, JobStatus = "A-Status", DefraUnitPrice = 0m }
            };
            var repo = CreateRepository(entities);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = "jobstatus", Descending = false };

            // Act
            var result = await repo.GetPagedAsync(query);

            // Assert
            Assert.Equal("I001", result.Data.First().ItemCode);
        }

        [Fact]
        public async Task GetPagedAsync_SortByTestManagerAscending_ReturnsOrderedResults()
        {
            // Arrange
            var entities = new List<TestOrProduct>
            {
                new() { ItemCode = "I002", FpsYear = DefaultFpsYear, TestManager = "ZManager", DefraUnitPrice = 0m },
                new() { ItemCode = "I001", FpsYear = DefaultFpsYear, TestManager = "AManager", DefraUnitPrice = 0m }
            };
            var repo = CreateRepository(entities);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = "testmanager", Descending = false };

            // Act
            var result = await repo.GetPagedAsync(query);

            // Assert
            Assert.Equal("I001", result.Data.First().ItemCode);
        }

        [Fact]
        public async Task GetPagedAsync_SortByShortDescriptionAscending_ReturnsOrderedResults()
        {
            // Arrange
            var entities = new List<TestOrProduct>
            {
                new() { ItemCode = "I002", FpsYear = DefaultFpsYear, ShortDescription = "ZShort", DefraUnitPrice = 0m },
                new() { ItemCode = "I001", FpsYear = DefaultFpsYear, ShortDescription = "AShort", DefraUnitPrice = 0m }
            };
            var repo = CreateRepository(entities);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = "shortdescription", Descending = false };

            // Act
            var result = await repo.GetPagedAsync(query);

            // Assert
            Assert.Equal("I001", result.Data.First().ItemCode);
        }

        [Fact]
        public async Task UnknownSortBy_DefaultsToItemCodeAscending()
        {
            // Arrange
            var entities = new List<TestOrProduct>
            {
                new() { ItemCode = "ZITEM", FpsYear = DefaultFpsYear, DefraUnitPrice = 0m },
                new() { ItemCode = "AITEM", FpsYear = DefaultFpsYear, DefraUnitPrice = 0m }
            };
            var repo = CreateRepository(entities);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = "unknowncolumn" };

            // Act
            var result = await repo.GetPagedAsync(query);

            // Assert
            Assert.Equal("AITEM", result.Data.First().ItemCode);
        }

        #endregion
    }
}
