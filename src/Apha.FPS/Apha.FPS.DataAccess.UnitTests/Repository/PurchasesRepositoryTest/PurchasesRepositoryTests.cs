using Apha.Common.Helpers.Repository;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.DataAccess.Data;
using Apha.FPS.DataAccess.Repositories;
using Moq;

namespace Apha.FPS.DataAccess.UnitTests.Repository.PurchasesRepositoryTest
{
    public class PurchasesRepositoryTests
    {
        private const string DefaultUserEmail = "test@example.com";
        private const int    DefaultFpsYear   = 2024;

        private static Mock<IFpsRequestContext> CreateMockRequestContext(
            int fpsYear = DefaultFpsYear, string userEmail = DefaultUserEmail)
        {
            var mock = new Mock<IFpsRequestContext>();
            mock.Setup(x => x.FpsYear).Returns(fpsYear);
            mock.Setup(x => x.UserEmailId).Returns(userEmail);
            return mock;
        }

        private static PurchasesRepository CreateRepository(
            IEnumerable<BidView>?  bidViews  = null,
            IEnumerable<Purchase>? purchases = null,
            int    fpsYear   = DefaultFpsYear,
            string userEmail = DefaultUserEmail)
        {
            var mockCtx = CreateMockRequestContext(fpsYear, userEmail);
            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(mockCtx.Object);

            var bidViewSet = RepositoryTestHelper.CreateMockDbSet(bidViews ?? Enumerable.Empty<BidView>());
            mockContext.Setup(x => x.BidViews).Returns(bidViewSet.Object);

            var purchaseSet = RepositoryTestHelper.CreateMockDbSet(purchases ?? Enumerable.Empty<Purchase>());
            mockContext.Setup(x => x.Purchases).Returns(purchaseSet.Object);

            return new PurchasesRepository(mockContext.Object, mockCtx.Object);
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_WithNullRequestContext_ThrowsArgumentNullException()
        {
            var dummyCtx = new Mock<IFpsRequestContext>().Object;
            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(dummyCtx);
            Assert.Throws<ArgumentNullException>(() => new PurchasesRepository(mockContext.Object, null!));
        }

        #endregion

        #region IsAuthorizedAsync Tests

        [Fact]
        public async Task IsAuthorizedAsync_WithMatchingData_ReturnsTrue()
        {
            // Arrange
            var bidViews = new List<BidView>
            {
                new() { WorkgroupName = "WG01", FpsYear = DefaultFpsYear, UserEmail = DefaultUserEmail }
            };
            var repo = CreateRepository(bidViews: bidViews);

            // Act
            var result = await repo.IsAuthorizedAsync("WG01", DefaultUserEmail);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task IsAuthorizedAsync_WithDifferentWorkgroup_ReturnsFalse()
        {
            // Arrange
            var bidViews = new List<BidView>
            {
                new() { WorkgroupName = "WG01", FpsYear = DefaultFpsYear, UserEmail = DefaultUserEmail }
            };
            var repo = CreateRepository(bidViews: bidViews);

            // Act
            var result = await repo.IsAuthorizedAsync("WG99", DefaultUserEmail);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task IsAuthorizedAsync_WithDifferentFpsYear_ReturnsFalse()
        {
            // Arrange
            var bidViews = new List<BidView>
            {
                new() { WorkgroupName = "WG01", FpsYear = DefaultFpsYear - 1, UserEmail = DefaultUserEmail }
            };
            var repo = CreateRepository(bidViews: bidViews);

            // Act
            var result = await repo.IsAuthorizedAsync("WG01", DefaultUserEmail);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task IsAuthorizedAsync_WithNullUserEmail_ReturnsFalse()
        {
            // Arrange
            var bidViews = new List<BidView>
            {
                new() { WorkgroupName = "WG01", FpsYear = DefaultFpsYear, UserEmail = null }
            };
            var repo = CreateRepository(bidViews: bidViews);

            // Act
            var result = await repo.IsAuthorizedAsync("WG01", DefaultUserEmail);

            // Assert
            Assert.False(result);
        }

        #endregion

        #region GetPurchasesAsync Tests

        [Fact]
        public async Task GetPurchasesAsync_WithMatchingData_ReturnsPurchases()
        {
            // Arrange
            var purchases = new List<Purchase>
            {
                new() { WorkgroupName = "WG01", Account = "ACC1", ItemDescription = "Item A", Amount = 100m, FpsYear = DefaultFpsYear },
                new() { WorkgroupName = "WG01", Account = "ACC1", ItemDescription = "Item B", Amount = 200m, FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(purchases: purchases);

            // Act
            var result = await repo.GetPurchasesAsync("WG01", "ACC1");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetPurchasesAsync_ReturnsOrderedByItemDescription()
        {
            // Arrange
            var purchases = new List<Purchase>
            {
                new() { WorkgroupName = "WG01", Account = "ACC1", ItemDescription = "Zebra", Amount = 100m, FpsYear = DefaultFpsYear },
                new() { WorkgroupName = "WG01", Account = "ACC1", ItemDescription = "Alpha", Amount = 200m, FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(purchases: purchases);

            // Act
            var result = await repo.GetPurchasesAsync("WG01", "ACC1");

            // Assert
            Assert.Equal("Alpha", result[0].ItemDescription);
            Assert.Equal("Zebra", result[1].ItemDescription);
        }

        [Fact]
        public async Task GetPurchasesAsync_FiltersOutDifferentFpsYear()
        {
            // Arrange
            var purchases = new List<Purchase>
            {
                new() { WorkgroupName = "WG01", Account = "ACC1", ItemDescription = "Item A", Amount = 100m, FpsYear = DefaultFpsYear },
                new() { WorkgroupName = "WG01", Account = "ACC1", ItemDescription = "Item B", Amount = 200m, FpsYear = DefaultFpsYear - 1 }
            };
            var repo = CreateRepository(purchases: purchases);

            // Act
            var result = await repo.GetPurchasesAsync("WG01", "ACC1");

            // Assert
            Assert.Single(result);
            Assert.Equal("Item A", result[0].ItemDescription);
        }

        [Fact]
        public async Task GetPurchasesAsync_FiltersOutDifferentWorkgroup()
        {
            // Arrange
            var purchases = new List<Purchase>
            {
                new() { WorkgroupName = "WG01", Account = "ACC1", ItemDescription = "Item A", Amount = 100m, FpsYear = DefaultFpsYear },
                new() { WorkgroupName = "WG02", Account = "ACC1", ItemDescription = "Item A", Amount = 100m, FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(purchases: purchases);

            // Act
            var result = await repo.GetPurchasesAsync("WG01", "ACC1");

            // Assert
            Assert.Single(result);
        }

        [Fact]
        public async Task GetPurchasesAsync_WithNoData_ReturnsEmptyList()
        {
            // Arrange
            var repo = CreateRepository(purchases: new List<Purchase>());

            // Act
            var result = await repo.GetPurchasesAsync("WG01", "ACC1");

            // Assert
            Assert.Empty(result);
        }

        #endregion

        #region GetPurchaseByIdAsync Tests

        [Fact]
        public async Task GetPurchaseByIdAsync_WithMatchingKeys_ReturnsPurchase()
        {
            // Arrange
            var purchases = new List<Purchase>
            {
                new() { WorkgroupName = "WG01", Account = "ACC1", ItemDescription = "Item A", Amount = 100m, FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(purchases: purchases);

            // Act
            var result = await repo.GetPurchaseByIdAsync("WG01", "ACC1", "Item A");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Item A", result.ItemDescription);
        }

        [Fact]
        public async Task GetPurchaseByIdAsync_WithNonMatchingKeys_ReturnsNull()
        {
            // Arrange
            var purchases = new List<Purchase>
            {
                new() { WorkgroupName = "WG01", Account = "ACC1", ItemDescription = "Item A", Amount = 100m, FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(purchases: purchases);

            // Act
            var result = await repo.GetPurchaseByIdAsync("WG01", "ACC1", "NOTEXIST");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetPurchaseByIdAsync_FiltersOutDifferentFpsYear()
        {
            // Arrange
            var purchases = new List<Purchase>
            {
                new() { WorkgroupName = "WG01", Account = "ACC1", ItemDescription = "Item A", Amount = 100m, FpsYear = DefaultFpsYear - 1 }
            };
            var repo = CreateRepository(purchases: purchases);

            // Act
            var result = await repo.GetPurchaseByIdAsync("WG01", "ACC1", "Item A");

            // Assert
            Assert.Null(result);
        }

        #endregion
    }
}
