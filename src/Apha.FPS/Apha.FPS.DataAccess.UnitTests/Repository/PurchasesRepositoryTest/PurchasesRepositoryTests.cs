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

        #region GetPurchasesAsync Tests

        [Fact]
        public async Task GetPurchasesAsync_WithMatchingData_ReturnsPurchases()
        {
            // Arrange
            var bidViews = new List<BidView>
            {
                new() { WorkGroupName = "WG01", Account = "ACC1", UserEmail = DefaultUserEmail }
            };
            var purchases = new List<Purchase>
            {
                new() { WorkGroupName = "WG01", Account = "ACC1", ItemDescription = "Item A", Amount = 100m, FpsYear = DefaultFpsYear },
                new() { WorkGroupName = "WG01", Account = "ACC1", ItemDescription = "Item B", Amount = 200m, FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(bidViews: bidViews, purchases: purchases);

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
            var bidViews = new List<BidView>
            {
                new() { WorkGroupName = "WG01", Account = "ACC1", UserEmail = DefaultUserEmail }
            };
            var purchases = new List<Purchase>
            {
                new() { WorkGroupName = "WG01", Account = "ACC1", ItemDescription = "Zebra", Amount = 100m, FpsYear = DefaultFpsYear },
                new() { WorkGroupName = "WG01", Account = "ACC1", ItemDescription = "Alpha", Amount = 200m, FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(bidViews: bidViews, purchases: purchases);

            // Act
            var result = await repo.GetPurchasesAsync("WG01", "ACC1");

            // Assert
            Assert.Equal("Alpha", result[0].ItemDescription);
            Assert.Equal("Zebra", result[1].ItemDescription);
        }

        [Fact]
        public async Task GetPurchasesAsync_FiltersOutDifferentWorkgroup()
        {
            // Arrange
            var bidViews = new List<BidView>
            {
                new() { WorkGroupName = "WG01", Account = "ACC1", UserEmail = DefaultUserEmail }
            };
            var purchases = new List<Purchase>
            {
                new() { WorkGroupName = "WG01", Account = "ACC1", ItemDescription = "Item A", Amount = 100m, FpsYear = DefaultFpsYear },
                new() { WorkGroupName = "WG02", Account = "ACC1", ItemDescription = "Item A", Amount = 100m, FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(bidViews: bidViews, purchases: purchases);

            // Act
            var result = await repo.GetPurchasesAsync("WG01", "ACC1");

            // Assert
            Assert.Single(result);
        }

        [Fact]
        public async Task GetPurchasesAsync_WithNoData_ReturnsEmptyList()
        {
            // Arrange
            var bidViews = new List<BidView>
            {
                new() { WorkGroupName = "WG01", Account = "ACC1", UserEmail = DefaultUserEmail }
            };
            var repo = CreateRepository(bidViews: bidViews, purchases: new List<Purchase>());

            // Act
            var result = await repo.GetPurchasesAsync("WG01", "ACC1");

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetPurchasesAsync_WhenUserNotInBidViews_ReturnsEmptyList()
        {
            // Arrange — no BidViews entry for this user, so authorisedAccounts is empty
            var purchases = new List<Purchase>
            {
                new() { WorkGroupName = "WG01", Account = "ACC1", ItemDescription = "Item A", Amount = 100m, FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(bidViews: new List<BidView>(), purchases: purchases);

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
                new() { WorkGroupName = "WG01", Account = "ACC1", ItemDescription = "Item A", Amount = 100m, FpsYear = DefaultFpsYear }
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
                new() { WorkGroupName = "WG01", Account = "ACC1", ItemDescription = "Item A", Amount = 100m, FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(purchases: purchases);

            // Act
            var result = await repo.GetPurchaseByIdAsync("WG01", "ACC1", "NOTEXIST");

            // Assert
            Assert.Null(result);
        }

        #endregion
    }
}
