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
            IEnumerable<WorkGroupView>?  wgViews          = null,
            IEnumerable<BidView>?        bidViews         = null,
            IEnumerable<Bid>?            bids             = null,
            IEnumerable<AccountCategory>? accountCategories = null,
            int    fpsYear   = DefaultFpsYear,
            string userEmail = DefaultUserEmail)
        {
            var mockCtx = CreateMockRequestContext(fpsYear, userEmail);
            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(mockCtx.Object);

            var wgViewSet = RepositoryTestHelper.CreateMockDbSet(wgViews ?? Enumerable.Empty<WorkGroupView>());
            mockContext.Setup(x => x.WorkGroupViews).Returns(wgViewSet.Object);

            var bidViewSet = RepositoryTestHelper.CreateMockDbSet(bidViews ?? Enumerable.Empty<BidView>());
            mockContext.Setup(x => x.BidViews).Returns(bidViewSet.Object);

            var bidSet = RepositoryTestHelper.CreateMockDbSet(bids ?? Enumerable.Empty<Bid>());
            mockContext.Setup(x => x.Bids).Returns(bidSet.Object);

            var catSet = RepositoryTestHelper.CreateMockDbSet(accountCategories ?? Enumerable.Empty<AccountCategory>());
            mockContext.Setup(x => x.AccountCategories).Returns(catSet.Object);

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

        #region IsAuthorizedAsync Tests

        [Fact]
        public async Task IsAuthorizedAsync_WithMatchingWorkgroupAndUser_ReturnsTrue()
        {
            // Arrange
            var views = new List<WorkGroupView>
            {
                new() { WorkGroupName = "WG01", FpsYear = DefaultFpsYear, UserEmail = DefaultUserEmail }
            };
            var repo = CreateRepository(wgViews: views);

            // Act
            var result = await repo.IsAuthorizedAsync("WG01");

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task IsAuthorizedAsync_WithDifferentWorkgroup_ReturnsFalse()
        {
            // Arrange
            var views = new List<WorkGroupView>
            {
                new() { WorkGroupName = "WG01", FpsYear = DefaultFpsYear, UserEmail = DefaultUserEmail }
            };
            var repo = CreateRepository(wgViews: views);

            // Act
            var result = await repo.IsAuthorizedAsync("WG99");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task IsAuthorizedAsync_WithNullUserEmail_ReturnsFalse()
        {
            // Arrange
            var views = new List<WorkGroupView>
            {
                new() { WorkGroupName = "WG01", FpsYear = DefaultFpsYear, UserEmail = null }
            };
            var repo = CreateRepository(wgViews: views);

            // Act
            var result = await repo.IsAuthorizedAsync("WG01");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task IsAuthorizedAsync_WithNoMatchingData_ReturnsFalse()
        {
            // Arrange
            var repo = CreateRepository(wgViews: new List<WorkGroupView>());

            // Act
            var result = await repo.IsAuthorizedAsync("WG01");

            // Assert
            Assert.False(result);
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
    }
}
