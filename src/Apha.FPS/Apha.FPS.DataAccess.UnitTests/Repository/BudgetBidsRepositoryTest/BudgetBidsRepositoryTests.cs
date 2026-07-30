using Apha.Common.Helpers.Repository;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.DataAccess.Data;
using Apha.FPS.DataAccess.Repositories;
using Moq;

namespace Apha.FPS.DataAccess.UnitTests.Repository.BudgetBidsRepositoryTest
{
    public class BudgetBidsRepositoryTests
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

        private static BudgetBidsRepository CreateRepository(
            IEnumerable<BidView>?         bidViews          = null,
            IEnumerable<Bid>?             bids              = null,
            IEnumerable<AccountCategory>? accountCategories = null,
            IEnumerable<Purchase>?        purchases         = null,
            IEnumerable<Workgroup>?       workgroups        = null,
            int    fpsYear   = DefaultFpsYear,
            string userEmail = DefaultUserEmail)
        {
            var mockCtx = CreateMockRequestContext(fpsYear, userEmail);
            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(mockCtx.Object);

            var bidViewSet = RepositoryTestHelper.CreateMockDbSet(bidViews ?? Enumerable.Empty<BidView>());
            mockContext.Setup(x => x.BidViews).Returns(bidViewSet.Object);

            var bidSet = RepositoryTestHelper.CreateMockDbSet(bids ?? Enumerable.Empty<Bid>());
            mockContext.Setup(x => x.Bids).Returns(bidSet.Object);

            var catSet = RepositoryTestHelper.CreateMockDbSet(accountCategories ?? Enumerable.Empty<AccountCategory>());
            mockContext.Setup(x => x.AccountCategories).Returns(catSet.Object);

            var purchaseSet = RepositoryTestHelper.CreateMockDbSet(purchases ?? Enumerable.Empty<Purchase>());
            mockContext.Setup(x => x.Purchases).Returns(purchaseSet.Object);

            var workgroupSet = RepositoryTestHelper.CreateMockDbSet(workgroups ?? Enumerable.Empty<Workgroup>());
            mockContext.Setup(x => x.Workgroups).Returns(workgroupSet.Object);

            return new BudgetBidsRepository(mockContext.Object, mockCtx.Object);
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_WithNullRequestContext_ThrowsArgumentNullException()
        {
            var dummyCtx = new Mock<IFpsRequestContext>().Object;
            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(dummyCtx);
            Assert.Throws<ArgumentNullException>(() => new BudgetBidsRepository(mockContext.Object, null!));
        }

        #endregion

        #region GetBidViewAsync Tests

        [Fact]
        public async Task GetBidViewAsync_WithMatchingData_ReturnsBidViews()
        {
            // Arrange
            var bidViews = new List<BidView>
            {
                new() { WorkGroupName = "WG01", Account = "ACC1", GenBid = 100m, FpsYear = DefaultFpsYear, UserEmail = DefaultUserEmail },
                new() { WorkGroupName = "WG01", Account = "ACC2", GenBid = 200m, FpsYear = DefaultFpsYear, UserEmail = DefaultUserEmail }
            };
            var repo = CreateRepository(bidViews: bidViews);

            // Act
            var result = await repo.GetBidViewAsync("WG01");

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetBidViewAsync_FiltersOutDifferentWorkgroup()
        {
            // Arrange
            var bidViews = new List<BidView>
            {
                new() { WorkGroupName = "WG01", Account = "ACC1", GenBid = 100m, FpsYear = DefaultFpsYear, UserEmail = DefaultUserEmail },
                new() { WorkGroupName = "WG02", Account = "ACC1", GenBid = 200m, FpsYear = DefaultFpsYear, UserEmail = DefaultUserEmail }
            };
            var repo = CreateRepository(bidViews: bidViews);

            // Act
            var result = await repo.GetBidViewAsync("WG01");

            // Assert
            Assert.Single(result);
            Assert.Equal("WG01", result[0].WorkGroupName);
        }

        [Fact]
        public async Task GetBidViewAsync_FiltersOutNullUserEmail()
        {
            // Arrange
            var bidViews = new List<BidView>
            {
                new() { WorkGroupName = "WG01", Account = "ACC1", GenBid = 100m, FpsYear = DefaultFpsYear, UserEmail = DefaultUserEmail },
                new() { WorkGroupName = "WG01", Account = "ACC2", GenBid = 200m, FpsYear = DefaultFpsYear, UserEmail = null }
            };
            var repo = CreateRepository(bidViews: bidViews);

            // Act
            var result = await repo.GetBidViewAsync("WG01");

            // Assert
            Assert.Single(result);
        }

        [Fact]
        public async Task GetBidViewAsync_DeduplicatesByAccount()
        {
            // Arrange
            var bidViews = new List<BidView>
            {
                new() { WorkGroupName = "WG01", Account = "ACC1", GenBid = 100m, FpsYear = DefaultFpsYear, UserEmail = DefaultUserEmail },
                new() { WorkGroupName = "WG01", Account = "ACC1", GenBid = 100m, FpsYear = DefaultFpsYear, UserEmail = DefaultUserEmail }
            };
            var repo = CreateRepository(bidViews: bidViews);

            // Act
            var result = await repo.GetBidViewAsync("WG01");

            // Assert
            Assert.Single(result);
        }

        [Fact]
        public async Task GetBidViewAsync_WithNoData_ReturnsEmptyList()
        {
            // Arrange
            var repo = CreateRepository(bidViews: new List<BidView>());

            // Act
            var result = await repo.GetBidViewAsync("WG01");

            // Assert
            Assert.Empty(result);
        }

        #endregion

        #region GetBidByIdAsync Tests

        [Fact]
        public async Task GetBidByIdAsync_WithMatchingKeys_ReturnsBid()
        {
            // Arrange
            var bids = new List<Bid>
            {
                new() { WorkGroupName = "WG01", Account = "ACC1", GenBid = 100m, FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(bids: bids);

            // Act
            var result = await repo.GetBidByIdAsync("WG01", "ACC1");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("WG01", result.WorkGroupName);
            Assert.Equal("ACC1", result.Account);
        }

        [Fact]
        public async Task GetBidByIdAsync_WithNonMatchingKeys_ReturnsNull()
        {
            // Arrange
            var bids = new List<Bid>
            {
                new() { WorkGroupName = "WG01", Account = "ACC1", GenBid = 100m, FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(bids: bids);

            // Act
            var result = await repo.GetBidByIdAsync("WG01", "NOTEXIST");

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region HasRelatedPurchasesAsync Tests

        [Fact]
        public async Task HasRelatedPurchasesAsync_WhenMatchingPurchaseExists_ReturnsTrue()
        {
            // Arrange
            var purchases = new List<Purchase>
            {
                new() { WorkGroupName = "WG01", Account = "ACC1", ItemDescription = "Item A", Amount = 100m, FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(purchases: purchases);

            // Act
            var result = await repo.HasRelatedPurchasesAsync("WG01", "ACC1");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task HasRelatedPurchasesAsync_WhenNoPurchasesExist_ReturnsFalse()
        {
            // Arrange
            var repo = CreateRepository(purchases: new List<Purchase>());

            // Act
            var result = await repo.HasRelatedPurchasesAsync("WG01", "ACC1");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task HasRelatedPurchasesAsync_WhenDifferentWorkgroup_ReturnsFalse()
        {
            // Arrange
            var purchases = new List<Purchase>
            {
                new() { WorkGroupName = "WG99", Account = "ACC1", ItemDescription = "Item A", Amount = 100m, FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(purchases: purchases);

            // Act
            var result = await repo.HasRelatedPurchasesAsync("WG01", "ACC1");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task HasRelatedPurchasesAsync_WhenDifferentAccount_ReturnsFalse()
        {
            // Arrange
            var purchases = new List<Purchase>
            {
                new() { WorkGroupName = "WG01", Account = "ACCOTHER", ItemDescription = "Item A", Amount = 100m, FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(purchases: purchases);

            // Act
            var result = await repo.HasRelatedPurchasesAsync("WG01", "ACC1");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task HasRelatedPurchasesAsync_WhenMultiplePurchasesForSameKey_ReturnsTrue()
        {
            // Arrange
            var purchases = new List<Purchase>
            {
                new() { WorkGroupName = "WG01", Account = "ACC1", ItemDescription = "Item A", Amount = 100m, FpsYear = DefaultFpsYear },
                new() { WorkGroupName = "WG01", Account = "ACC1", ItemDescription = "Item B", Amount = 200m, FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(purchases: purchases);

            // Act
            var result = await repo.HasRelatedPurchasesAsync("WG01", "ACC1");

            // Assert
            Assert.True(result);
        }

        #endregion

        #region AddBidAsync / UpdateBidAsync / DeleteBidAsync — ownership guard

        [Fact]
        public async Task AddBidAsync_WhenUserNotInBidViews_ThrowsUnauthorizedAccessException()
        {
            // Arrange — no BidViews entry for this user, so ThrowIfNotOwnerAsync fires
            var repo = CreateRepository(bidViews: new List<BidView>());
            var bid  = new Bid { WorkGroupName = "WG01", Account = "ACC1", GenBid = 100m };

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => repo.AddBidAsync(bid));
        }

        [Fact]
        public async Task UpdateBidAsync_WhenUserNotInBidViews_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var repo = CreateRepository(bidViews: new List<BidView>());
            var bid  = new Bid { WorkGroupName = "WG01", Account = "ACC1", GenBid = 200m };

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => repo.UpdateBidAsync(bid));
        }

        [Fact]
        public async Task DeleteBidAsync_WhenUserNotInBidViews_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            var repo = CreateRepository(bidViews: new List<BidView>());

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => repo.DeleteBidAsync("WG01", "ACC1"));
        }

        #endregion

        #region GetAccountCategoriesAsync Tests

        [Fact]
        public async Task GetAccountCategoriesAsync_ReturnsRcSpecificCategories()
        {
            // Arrange
            var categories = new List<AccountCategory>
            {
                new() { AccShortName = "ACC1", RcSpecific = -1 },
                new() { AccShortName = "ACC2", RcSpecific = 0 },
                new() { AccShortName = "ACC3", RcSpecific = -1 }
            };
            var repo = CreateRepository(accountCategories: categories);

            // Act
            var result = await repo.GetAccountCategoriesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.All(result, c => Assert.Equal(-1, c.RcSpecific));
        }

        [Fact]
        public async Task GetAccountCategoriesAsync_ReturnsOrderedByAccShortName()
        {
            // Arrange
            var categories = new List<AccountCategory>
            {
                new() { AccShortName = "ZZZ", RcSpecific = -1 },
                new() { AccShortName = "AAA", RcSpecific = -1 },
                new() { AccShortName = "MMM", RcSpecific = -1 }
            };
            var repo = CreateRepository(accountCategories: categories);

            // Act
            var result = await repo.GetAccountCategoriesAsync();

            // Assert
            Assert.Equal("AAA", result[0].AccShortName);
            Assert.Equal("MMM", result[1].AccShortName);
            Assert.Equal("ZZZ", result[2].AccShortName);
        }

        [Fact]
        public async Task GetAccountCategoriesAsync_WithNoCategories_ReturnsEmptyList()
        {
            // Arrange
            var repo = CreateRepository(accountCategories: new List<AccountCategory>());

            // Act
            var result = await repo.GetAccountCategoriesAsync();

            // Assert
            Assert.Empty(result);
        }

        #endregion

        #region GetGenericBidsPagedAsync Tests

        private static (List<Bid> bids, List<Workgroup> workgroups, List<AccountCategory> categories) BuildGenericBidData()
        {
            var bids = new List<Bid>
            {
                new() { WorkGroupName = "WG01", Account = "ACC1", GenBid = 100m, FpsYear = DefaultFpsYear },
                new() { WorkGroupName = "WG02", Account = "ACC2", GenBid = 200m, FpsYear = DefaultFpsYear }
            };
            var workgroups = new List<Workgroup>
            {
                new() { WorkGroupName = "WG01", ProfitCentre = "PC1" },
                new() { WorkGroupName = "WG02", ProfitCentre = "PC2" }
            };
            var categories = new List<AccountCategory>
            {
                new() { AccShortName = "ACC1", AccountType = "TYPE1" },
                new() { AccShortName = "ACC2", AccountType = "TYPE2" }
            };
            return (bids, workgroups, categories);
        }

        [Fact]
        public async Task GetGenericBidsPagedAsync_JoinsBidsWorkgroupsAndCategories()
        {
            // Arrange
            var (bids, workgroups, categories) = BuildGenericBidData();
            var repo = CreateRepository(bids: bids, workgroups: workgroups, accountCategories: categories);
            var query = new Apha.FPS.Core.Pagination.PaginationParameters<string>(page: 1, pageSize: 10);

            // Act
            var result = await repo.GetGenericBidsPagedAsync(query);

            // Assert
            Assert.Equal(2, result.PaginationData.TotalRecords);
            var first = result.Data.First(x => x.WorkGroupName == "WG01");
            Assert.Equal("PC1", first.ProfitCentre);
            Assert.Equal("TYPE1", first.AccountType);
        }

        [Fact]
        public async Task GetGenericBidsPagedAsync_ExcludesBidsWithoutMatchingWorkgroup()
        {
            // Arrange
            var bids = new List<Bid>
            {
                new() { WorkGroupName = "WG01", Account = "ACC1", GenBid = 100m, FpsYear = DefaultFpsYear },
                new() { WorkGroupName = "WGX", Account = "ACC1", GenBid = 150m, FpsYear = DefaultFpsYear }
            };
            var workgroups = new List<Workgroup> { new() { WorkGroupName = "WG01", ProfitCentre = "PC1" } };
            var categories = new List<AccountCategory> { new() { AccShortName = "ACC1", AccountType = "TYPE1" } };
            var repo = CreateRepository(bids: bids, workgroups: workgroups, accountCategories: categories);
            var query = new Apha.FPS.Core.Pagination.PaginationParameters<string>(page: 1, pageSize: 10);

            // Act
            var result = await repo.GetGenericBidsPagedAsync(query);

            // Assert
            Assert.Equal(1, result.PaginationData.TotalRecords);
            Assert.Equal("WG01", result.Data.Single().WorkGroupName);
        }

        [Fact]
        public async Task GetGenericBidsPagedAsync_SortsByAccountDescending()
        {
            // Arrange
            var (bids, workgroups, categories) = BuildGenericBidData();
            var repo = CreateRepository(bids: bids, workgroups: workgroups, accountCategories: categories);
            var query = new Apha.FPS.Core.Pagination.PaginationParameters<string>(sortBy: "account", descending: true, page: 1, pageSize: 10);

            // Act
            var result = await repo.GetGenericBidsPagedAsync(query);

            // Assert
            Assert.Equal("ACC2", result.Data.First().Account);
        }

        [Fact]
        public async Task GetGenericBidsPagedAsync_AppliesPaging()
        {
            // Arrange
            var (bids, workgroups, categories) = BuildGenericBidData();
            var repo = CreateRepository(bids: bids, workgroups: workgroups, accountCategories: categories);
            var query = new Apha.FPS.Core.Pagination.PaginationParameters<string>(page: 1, pageSize: 1);

            // Act
            var result = await repo.GetGenericBidsPagedAsync(query);

            // Assert
            Assert.Single(result.Data);
            Assert.Equal(2, result.PaginationData.TotalRecords);
            Assert.Equal(2, result.PaginationData.TotalPages);
        }

        [Fact]
        public async Task GetGenericBidsPagedAsync_WithNoData_ReturnsEmpty()
        {
            // Arrange
            var repo = CreateRepository(
                bids: new List<Bid>(),
                workgroups: new List<Workgroup>(),
                accountCategories: new List<AccountCategory>());
            var query = new Apha.FPS.Core.Pagination.PaginationParameters<string>(page: 1, pageSize: 10);

            // Act
            var result = await repo.GetGenericBidsPagedAsync(query);

            // Assert
            Assert.Empty(result.Data);
            Assert.Equal(0, result.PaginationData.TotalRecords);
        }

        #endregion
    }
}
