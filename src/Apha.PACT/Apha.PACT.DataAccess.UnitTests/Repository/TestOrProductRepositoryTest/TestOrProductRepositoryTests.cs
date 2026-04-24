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
    public class TestOrProductRepositoryTests
    {
        private record RepositoryContext(
            TestorProductRepository Repo,
            Mock<DbSet<TestorProduct>> TestorProductsDbSet,
            Mock<FpsDbContext> MockContext,
            IFpsRequestContext FpsRequestContext);

        private static RepositoryContext CreateRepositoryContext(
                int fpsYear = 2024,
                IEnumerable<TestorProduct> testorProducts = null!,
                bool setupForModification = false)
        {
            var mockContext = new Mock<FpsDbContext>();
            var fpsRequestContext = Substitute.For<IFpsRequestContext>();
            fpsRequestContext.FpsYear.Returns(fpsYear);

            var testorProductsList = (testorProducts ?? Enumerable.Empty<TestorProduct>()).ToList();
            var testorProductsMockSet = CreateMockDbSet(testorProductsList);

            if (setupForModification)
            {
                mockContext.Setup(m => m.SaveChangesAsync(default))
                    .ReturnsAsync(1);
                mockContext.Setup(m => m.TestorProducts.AddAsync(It.IsAny<TestorProduct>(), default))
                    .Returns((TestorProduct _, CancellationToken __) => new ValueTask<EntityEntry<TestorProduct>>());
                mockContext.Setup(m => m.Entry(It.IsAny<TestorProduct>()))
                    .Returns((TestorProduct entity) =>
                    {
                        var entry = new Mock<EntityEntry<TestorProduct>>();
                        entry.Object.State = EntityState.Modified;
                        return entry.Object;
                    });
            }

            mockContext.Setup(x => x.TestorProducts).Returns(testorProductsMockSet.Object);

            var repo = new TestorProductRepository(mockContext.Object, fpsRequestContext);
            return new RepositoryContext(repo, testorProductsMockSet, mockContext, fpsRequestContext);
        }

        private static TestorProductRepository CreateRepository(
            IEnumerable<TestorProduct> testorProducts,
            Mock<FpsDbContext> mockContext = null!,
            IFpsRequestContext fpsRequestContext = null!)
        {
            mockContext ??= new Mock<FpsDbContext>();
            fpsRequestContext ??= Substitute.For<IFpsRequestContext>();
            fpsRequestContext.FpsYear.Returns(2024);

            var testorProductsList = testorProducts.ToList();
            var mockSet = CreateMockDbSet(testorProductsList);
            mockContext.Setup(x => x.TestorProducts).Returns(mockSet.Object);

            return new TestorProductRepository(mockContext.Object, fpsRequestContext);
        }

        private static Mock<DbSet<T>> CreateMockDbSet<T>(List<T> data) where T : class
        {
            var queryable = data.AsQueryable();
            var mockSet = new Mock<DbSet<T>>();
            mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
            mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
            mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(() => queryable.GetEnumerator());
            mockSet.As<IAsyncEnumerable<T>>().Setup(m => m.GetAsyncEnumerator(default))
                .Returns(new TestAsyncEnumerator<T>(queryable.GetEnumerator()));
            return mockSet;
        }

        #region GetPagedTestOrProductsAsync

        [Fact]
        public async Task GetPagedTestOrProductsAsync_ReturnsPagedData()
        {
            // Arrange
            var testorProducts = Enumerable.Range(1, 25).Select(i => new TestorProduct
            {
                ItemCode = $"TEST{i:D3}",
                ItemDescription = $"Test {i}",
                DefraUnitPrice = i * 10m,
                FpsYear = 2024
            });
            var repo = CreateRepository(testorProducts);
            var parameters = new PaginationParameters<string>
            {
                Page = 2,
                PageSize = 10
            };

            // Act
            var result = await repo.GetPagedTestOrProductsAsync(parameters);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(10, result.Data.Count());
            Assert.Equal(25, result.PaginationData.TotalRecords);
            Assert.Equal(3, result.PaginationData.TotalPages);
        }

        #endregion

        #region GetTestOrProductByIdAsync

        [Fact]
        public async Task GetTestOrProductByIdAsync_ExistingItemCode_ReturnsEntity()
        {
            // Arrange
            var testorProducts = new List<TestorProduct>
            {
                new() { ItemCode = "TEST001", ItemDescription = "Test One", DefraUnitPrice = 100m, FpsYear = 2024 }
            };
            var repo = CreateRepository(testorProducts);

            // Act
            var result = await repo.GetTestOrProductByIdAsync("TEST001");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("TEST001", result.ItemCode);
        }

        [Fact]
        public async Task GetTestOrProductByIdAsync_NonExistentItemCode_ReturnsNull()
        {
            // Arrange
            var testorProducts = new List<TestorProduct>
            {
                new() { ItemCode = "TEST001", ItemDescription = "Test One", DefraUnitPrice = 100m, FpsYear = 2024 }
            };
            var repo = CreateRepository(testorProducts);

            // Act
            var result = await repo.GetTestOrProductByIdAsync("MISSING");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetTestOrProductByIdAsync_EmptyList_ReturnsNull()
        {
            // Arrange
            var repo = CreateRepository(Enumerable.Empty<TestorProduct>());

            // Act
            var result = await repo.GetTestOrProductByIdAsync("TEST001");

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region CreateTestOrProductAsync

        [Fact]
        public async Task CreateTestOrProductAsync_ValidEntity_ReturnsSavedEntity()
        {
            // Arrange
            var context = CreateRepositoryContext(fpsYear: 2024, setupForModification: true);
            var entity = new TestorProduct { ItemCode = "NEW001", DefraUnitPrice = 100m };

            // Act
            var result = await context.Repo.CreateTestOrProductAsync(entity);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("NEW001", result.ItemCode);
            Assert.Equal(2024, result.FpsYear);
            context.MockContext.Verify(x => x.SaveChangesAsync(default), Times.Once);
        }

        [Fact]
        public async Task CreateTestOrProductAsync_MultipleEntities_SavesAll()
        {
            // Arrange
            var context = CreateRepositoryContext(fpsYear: 2024, setupForModification: true);
            var entity1 = new TestorProduct { ItemCode = "NEW001", DefraUnitPrice = 100m };
            var entity2 = new TestorProduct { ItemCode = "NEW002", DefraUnitPrice = 200m };

            // Act
            var result1 = await context.Repo.CreateTestOrProductAsync(entity1);
            var result2 = await context.Repo.CreateTestOrProductAsync(entity2);

            // Assert
            Assert.Equal(2024, result1.FpsYear);
            Assert.Equal(2024, result2.FpsYear);
            context.MockContext.Verify(x => x.SaveChangesAsync(default), Times.Exactly(2));
        }

        #endregion

        #region UpdateTestOrProductAsync

        [Fact]
        public async Task UpdateTestOrProductAsync_ValidEntity_ReturnsUpdatedEntity()
        {
            // Arrange
            var entityToUpdate = new TestorProduct { ItemCode = "TEST001", ItemDescription = "Updated", DefraUnitPrice = 150m };
            var context = CreateRepositoryContext(fpsYear: 2024, setupForModification: true);

            // Act
            var result = await context.Repo.UpdateTestOrProductAsync(entityToUpdate);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("TEST001", result.ItemCode);
            Assert.Equal(2024, result.FpsYear);
            context.MockContext.Verify(x => x.SaveChangesAsync(default), Times.Once);
        }

        [Fact]
        public async Task UpdateTestOrProductAsync_UpdatesFpsYear()
        {
            // Arrange
            var entityToUpdate = new TestorProduct { ItemCode = "TEST001", DefraUnitPrice = 150m, FpsYear = 2023 };
            var context = CreateRepositoryContext(fpsYear: 2025, setupForModification: true);

            // Act
            var result = await context.Repo.UpdateTestOrProductAsync(entityToUpdate);

            // Assert
            Assert.Equal(2025, result.FpsYear);
        }

        #endregion

        #region DeleteTestOrProductAsync

        [Fact]
        public async Task DeleteTestOrProductAsync_ExistingEntity_ReturnsTrue()
        {
            // Arrange
            var testorProducts = new List<TestorProduct>
            {
                new() { ItemCode = "TEST001", DefraUnitPrice = 100m, FpsYear = 2024 }
            };
            var context = CreateRepositoryContext(fpsYear: 2024, testorProducts: testorProducts, setupForModification: true);

            // Act
            var result = await context.Repo.DeleteTestOrProductAsync("TEST001");

            // Assert
            Assert.True(result);
            context.MockContext.Verify(x => x.SaveChangesAsync(default), Times.Once);
        }

        [Fact]
        public async Task DeleteTestOrProductAsync_NonExistentEntity_ReturnsFalse()
        {
            // Arrange
            var testorProducts = new List<TestorProduct>
            {
                new() { ItemCode = "TEST001", DefraUnitPrice = 100m, FpsYear = 2024 }
            };
            var context = CreateRepositoryContext(fpsYear: 2024, testorProducts: testorProducts);

            // Act
            var result = await context.Repo.DeleteTestOrProductAsync("MISSING");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteTestOrProductAsync_WrongFpsYear_ReturnsFalse()
        {
            // Arrange
            var testorProducts = new List<TestorProduct>
            {
                new() { ItemCode = "TEST001", DefraUnitPrice = 100m, FpsYear = 2023 }
            };
            var context = CreateRepositoryContext(fpsYear: 2024, testorProducts: testorProducts);

            // Act
            var result = await context.Repo.DeleteTestOrProductAsync("TEST001");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task DeleteTestOrProductAsync_MatchingFpsYear_ReturnsTrue()
        {
            // Arrange
            var testorProducts = new List<TestorProduct>
            {
                new() { ItemCode = "TEST001", DefraUnitPrice = 100m, FpsYear = 2024 }
            };
            var context = CreateRepositoryContext(fpsYear: 2024, testorProducts: testorProducts, setupForModification: true);

            // Act
            var result = await context.Repo.DeleteTestOrProductAsync("TEST001");

            // Assert
            Assert.True(result);
        }

        #endregion

        #region GetOwnersAsync

        [Fact]
        public async Task GetOwnersAsync_ReturnsDistinctOwners()
        {
            // Arrange
            var testorProducts = new List<TestorProduct>
            {
                new() { ItemCode = "TEST001", Owner = "AB", DefraUnitPrice = 100m, FpsYear = 2024 },
                new() { ItemCode = "TEST002", Owner = "CD", DefraUnitPrice = 100m, FpsYear = 2024 },
                new() { ItemCode = "TEST003", Owner = "AB", DefraUnitPrice = 100m, FpsYear = 2024 },
                new() { ItemCode = "TEST004", Owner = null, DefraUnitPrice = 100m, FpsYear = 2024 }
            };
            var repo = CreateRepository(testorProducts);

            // Act
            var result = await repo.GetOwnersAsync();

            // Assert
            var ownersList = result.ToList();
            Assert.Equal(2, ownersList.Count);
            Assert.Contains("AB", ownersList);
            Assert.Contains("CD", ownersList);
        }

        [Fact]
        public async Task GetOwnersAsync_EmptyList_ReturnsEmpty()
        {
            // Arrange
            var repo = CreateRepository(Enumerable.Empty<TestorProduct>());

            // Act
            var result = await repo.GetOwnersAsync();

            // Assert
            Assert.Empty(result);
        }

        #endregion
    }

    internal class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
    {
        private readonly IEnumerator<T> _inner;

        public TestAsyncEnumerator(IEnumerator<T> inner)
        {
            _inner = inner;
        }

        public ValueTask<bool> MoveNextAsync() => new ValueTask<bool>(_inner.MoveNext());

        public T Current => _inner.Current;

        public ValueTask DisposeAsync()
        {
            _inner.Dispose();
            return ValueTask.CompletedTask;
        }
    }
}
