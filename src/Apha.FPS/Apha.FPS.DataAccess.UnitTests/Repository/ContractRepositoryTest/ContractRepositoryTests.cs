using Apha.Common.Helpers.Repository;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.DataAccess.Data;
using Apha.FPS.DataAccess.Repositories;
using NSubstitute;

namespace Apha.FPS.DataAccess.UnitTests.Repository.ContractRepositoryTest
{
    public class ContractRepositoryTests
    {
        private const int DefaultTestFpsYear = 2024;
        private const string DefaultUserEmail = "testuser@example.com";

        /// <summary>
        /// Creates a ContractRepository with in-memory Contracts, UserCategories, and Users data.
        /// GetAllContractsAsync joins all three DbSets and filters by UserEmail == requestContext.UserEmailId,
        /// so all three must be wired to exercise the filter logic in unit tests.
        /// </summary>
        private static ContractRepository CreateRepository(
            IEnumerable<Contract> contracts,
            IEnumerable<UserCategory> userCategories,
            IEnumerable<User> users)
        {
            var fpsYearContext = Substitute.For<IFpsRequestContext>();
            fpsYearContext.FpsYear.Returns(DefaultTestFpsYear);
            fpsYearContext.UserEmailId.Returns(DefaultUserEmail);

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(fpsYearContext);

            var contractsMockSet = RepositoryTestHelper.CreateMockDbSet(contracts);
            var userCategoriesMockSet = RepositoryTestHelper.CreateMockDbSet(userCategories);
            var usersMockSet = RepositoryTestHelper.CreateMockDbSet(users);

            mockContext.Setup(x => x.Contracts).Returns(contractsMockSet.Object);
            mockContext.Setup(x => x.UserCategories).Returns(userCategoriesMockSet.Object);
            mockContext.Setup(x => x.Users).Returns(usersMockSet.Object);

            return new ContractRepository(mockContext.Object, fpsYearContext);
        }

        #region GetAllContractsAsync

        [Fact]
        public async Task GetAllContractsAsync_ReturnsContracts_WhenUserEmailMatchesAndCategoryMatches()
        {
            // Arrange
            var contracts = new List<Contract>
            {
                new() { ContractNo = "C001", Category = "CAT_A" },
                new() { ContractNo = "C002", Category = "CAT_B" }
            };
            var userCategories = new List<UserCategory>
            {
                new() { UserId = 1, Category = "CAT_A" },
                new() { UserId = 1, Category = "CAT_B" }
            };
            var users = new List<User>
            {
                new() { UserId = 1, UserEmail = DefaultUserEmail }
            };
            var repo = CreateRepository(contracts, userCategories, users);

            // Act
            var result = await repo.GetAllContractsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetAllContractsAsync_ReturnsEmpty_WhenUserEmailDoesNotMatch()
        {
            // Arrange — user exists but UserEmail does not match UserEmailId, so JOIN filter excludes all
            var contracts = new List<Contract>
            {
                new() { ContractNo = "C001", Category = "CAT_A" }
            };
            var userCategories = new List<UserCategory>
            {
                new() { UserId = 1, Category = "CAT_A" }
            };
            var users = new List<User>
            {
                new() { UserId = 1, UserEmail = "otheruser@example.com" }
            };
            var repo = CreateRepository(contracts, userCategories, users);

            // Act
            var result = await repo.GetAllContractsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllContractsAsync_ReturnsEmpty_WhenCategoryDoesNotMatchUserCategory()
        {
            // Arrange — contract category has no matching UserCategory row, JOIN produces nothing
            var contracts = new List<Contract>
            {
                new() { ContractNo = "C001", Category = "CAT_UNMATCHED" }
            };
            var userCategories = new List<UserCategory>
            {
                new() { UserId = 1, Category = "CAT_A" }
            };
            var users = new List<User>
            {
                new() { UserId = 1, UserEmail = DefaultUserEmail }
            };
            var repo = CreateRepository(contracts, userCategories, users);

            // Act
            var result = await repo.GetAllContractsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllContractsAsync_ReturnsEmpty_WhenContractsIsEmpty()
        {
            // Arrange
            var userCategories = new List<UserCategory>
            {
                new() { UserId = 1, Category = "CAT_A" }
            };
            var users = new List<User>
            {
                new() { UserId = 1, UserEmail = DefaultUserEmail }
            };
            var repo = CreateRepository(new List<Contract>(), userCategories, users);

            // Act
            var result = await repo.GetAllContractsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllContractsAsync_ReturnsOnlyMatchingUserEmailContracts_WhenMultipleUsersExist()
        {
            // Arrange — two users share a category, only the matching email user should produce results
            var contracts = new List<Contract>
            {
                new() { ContractNo = "C001", Category = "CAT_A" },
                new() { ContractNo = "C002", Category = "CAT_B" }
            };
            var userCategories = new List<UserCategory>
            {
                new() { UserId = 1, Category = "CAT_A" }, // matching email user
                new() { UserId = 2, Category = "CAT_B" }  // non-matching email user
            };
            var users = new List<User>
            {
                new() { UserId = 1, UserEmail = DefaultUserEmail },
                new() { UserId = 2, UserEmail = "otheruser@example.com" }
            };
            var repo = CreateRepository(contracts, userCategories, users);

            // Act
            var result = await repo.GetAllContractsAsync();

            // Assert
            var list = result.ToList();
            Assert.Single(list);
            Assert.Equal("C001", list[0].ContractNo);
        }

        #endregion
    }
}