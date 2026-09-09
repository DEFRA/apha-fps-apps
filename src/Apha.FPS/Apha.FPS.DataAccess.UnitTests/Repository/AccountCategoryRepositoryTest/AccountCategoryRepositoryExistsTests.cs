using Apha.Common.Helpers.Repository;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.DataAccess.Data;
using Apha.FPS.DataAccess.Repositories;
using Moq;

namespace Apha.FPS.DataAccess.UnitTests.Repositories.AccountCategoryRepositoryTest
{
    /// <summary>
    /// Exercises the REAL <see cref="AccountCategoryRepository"/> (not a mocked interface) so the
    /// newly added <see cref="AccountCategoryRepository.ExistsByAccShortNameAsync"/> logic is covered.
    /// The shared TestAsyncQueryProvider rewrites EF.Functions.ILike into a client-side
    /// ToLower().Contains() call, allowing the case-insensitive existence check to be evaluated in-memory.
    /// </summary>
    public class AccountCategoryRepositoryExistsTests
    {
        private const int DefaultFpsYear = 2024;

        private static Mock<IFpsRequestContext> CreateRequestContextMock()
        {
            var mock = new Mock<IFpsRequestContext>();
            mock.Setup(x => x.FpsYear).Returns(DefaultFpsYear);
            return mock;
        }

        private static AccountCategory BuildCategory(string accShortName, int fpsYear = DefaultFpsYear) =>
            new()
            {
                AccShortName = accShortName,
                AccountDescription = "Description",
                AccountType = "Pay",
                FpsYear = fpsYear
            };

        private static AccountCategoryRepository CreateRepository(IEnumerable<AccountCategory> categories)
        {
            return CreateRepository(categories, Enumerable.Empty<AdditionalCost>(), Enumerable.Empty<Bid>());
        }

        private static AccountCategoryRepository CreateRepository(
            IEnumerable<AccountCategory> categories,
            IEnumerable<AdditionalCost> additionalCosts,
            IEnumerable<Bid> bids)
        {
            var requestCtx = CreateRequestContextMock();
            var dbContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(requestCtx.Object);

            var set = RepositoryTestHelper.CreateMockDbSet(categories);
            RepositoryTestHelper.SetupDbSetOperations(set);
            dbContext.Setup(x => x.AccountCategories).Returns(set.Object);

            var additionalCostSet = RepositoryTestHelper.CreateMockDbSet(additionalCosts);
            RepositoryTestHelper.SetupDbSetOperations(additionalCostSet);
            dbContext.Setup(x => x.Set<AdditionalCost>()).Returns(additionalCostSet.Object);

            var bidSet = RepositoryTestHelper.CreateMockDbSet(bids);
            RepositoryTestHelper.SetupDbSetOperations(bidSet);
            dbContext.Setup(x => x.Bids).Returns(bidSet.Object);

            RepositoryTestHelper.SetupSaveChanges(dbContext);
            return new AccountCategoryRepository(dbContext.Object, requestCtx.Object);
        }

        [Fact]
        public async Task ExistsByAccShortNameAsync_ExactMatch_ReturnsTrue()
        {
            // Arrange
            var repository = CreateRepository(new[] { BuildCategory("PAY") });

            // Act
            var result = await repository.ExistsByAccShortNameAsync("PAY");

            // Assert
            Assert.True(result);
        }

        [Theory]
        [InlineData("pay")]
        [InlineData("Pay")]
        [InlineData("pAY")]
        public async Task ExistsByAccShortNameAsync_DifferentCase_ReturnsTrue(string candidate)
        {
            // Arrange - stored value is "PAY"; duplicate detection must be case-insensitive
            var repository = CreateRepository(new[] { BuildCategory("PAY") });

            // Act
            var result = await repository.ExistsByAccShortNameAsync(candidate);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ExistsByAccShortNameAsync_NoMatch_ReturnsFalse()
        {
            // Arrange
            var repository = CreateRepository(new[] { BuildCategory("PAY") });

            // Act
            var result = await repository.ExistsByAccShortNameAsync("NPRC");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ExistsByAccShortNameAsync_EmptyTable_ReturnsFalse()
        {
            // Arrange
            var repository = CreateRepository(Enumerable.Empty<AccountCategory>());

            // Act
            var result = await repository.ExistsByAccShortNameAsync("PAY");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ExistsByAccShortNameAsync_MatchInDifferentFpsYear_ReturnsFalse()
        {
            // Arrange - same name but a different FpsYear should not count as a duplicate
            var repository = CreateRepository(new[] { BuildCategory("PAY", fpsYear: DefaultFpsYear - 1) });

            // Act
            var result = await repository.ExistsByAccShortNameAsync("PAY");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task GetForeignKeyReferencesAsync_NullOrWhiteSpace_ReturnsEmptyList()
        {
            // Arrange
            var repository = CreateRepository(Enumerable.Empty<AccountCategory>());

            // Act
            var result = await repository.GetForeignKeyReferencesAsync("   ");

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetForeignKeyReferencesAsync_NoReferences_ReturnsEmptyList()
        {
            // Arrange
            var repository = CreateRepository(
                Enumerable.Empty<AccountCategory>(),
                Enumerable.Empty<AdditionalCost>(),
                Enumerable.Empty<Bid>());

            // Act
            var result = await repository.GetForeignKeyReferencesAsync("PAY");

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetForeignKeyReferencesAsync_OnlyAdditionalCostReference_ReturnsAdditionalCostsTable()
        {
            // Arrange
            var additionalCosts = new[]
            {
                new AdditionalCost { JobCode = "J1", Account = "PAY", Description = "Desc", FpsYear = DefaultFpsYear }
            };
            var repository = CreateRepository(
                Enumerable.Empty<AccountCategory>(),
                additionalCosts,
                Enumerable.Empty<Bid>());

            // Act
            var result = await repository.GetForeignKeyReferencesAsync("PAY");

            // Assert
            Assert.Single(result);
            Assert.Contains("tbladditionalcosts", result);
        }

        [Fact]
        public async Task GetForeignKeyReferencesAsync_OnlyBidReference_ReturnsBidTable()
        {
            // Arrange
            var bids = new[]
            {
                new Bid { WorkGroupName = "WG1", Account = "PAY", FpsYear = DefaultFpsYear }
            };
            var repository = CreateRepository(
                Enumerable.Empty<AccountCategory>(),
                Enumerable.Empty<AdditionalCost>(),
                bids);

            // Act
            var result = await repository.GetForeignKeyReferencesAsync("PAY");

            // Assert
            Assert.Single(result);
            Assert.Contains("tblbid", result);
        }

        [Fact]
        public async Task GetForeignKeyReferencesAsync_BothReferences_ReturnsBothTables()
        {
            // Arrange
            var additionalCosts = new[]
            {
                new AdditionalCost { JobCode = "J1", Account = "PAY", Description = "Desc", FpsYear = DefaultFpsYear }
            };
            var bids = new[]
            {
                new Bid { WorkGroupName = "WG1", Account = "PAY", FpsYear = DefaultFpsYear }
            };
            var repository = CreateRepository(
                Enumerable.Empty<AccountCategory>(),
                additionalCosts,
                bids);

            // Act
            var result = await repository.GetForeignKeyReferencesAsync("PAY");

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Contains("tbladditionalcosts", result);
            Assert.Contains("tblbid", result);
        }
    }
}
