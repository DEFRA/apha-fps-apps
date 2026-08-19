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
            var requestCtx = CreateRequestContextMock();
            var dbContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(requestCtx.Object);

            var set = RepositoryTestHelper.CreateMockDbSet(categories);
            RepositoryTestHelper.SetupDbSetOperations(set);
            dbContext.Setup(x => x.AccountCategories).Returns(set.Object);

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
    }
}
