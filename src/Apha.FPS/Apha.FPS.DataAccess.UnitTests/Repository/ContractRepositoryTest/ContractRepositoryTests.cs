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

        /// <summary>
        /// Creates a ContractRepository with in-memory Contracts, UserCategories, and Users data.
        /// GetAllContractsAsync joins all three DbSets and filters by Username == "dbo",
        /// so all three must be wired to exercise the filter logic in unit tests.
        /// </summary>
        private static ContractRepository CreateRepository(
            IEnumerable<Contract> contracts,
            IEnumerable<UserCategory> userCategories,
            IEnumerable<User> users)
        {
            var fpsYearContext = Substitute.For<IFpsYearContext>();
            fpsYearContext.FPSYear.Returns(DefaultTestFpsYear);

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(fpsYearContext);

            var contractsMockSet = RepositoryTestHelper.CreateMockDbSet(contracts);
            var userCategoriesMockSet = RepositoryTestHelper.CreateMockDbSet(userCategories);
            var usersMockSet = RepositoryTestHelper.CreateMockDbSet(users);

            mockContext.Setup(x => x.Contracts).Returns(contractsMockSet.Object);
            mockContext.Setup(x => x.UserCategories).Returns(userCategoriesMockSet.Object);
            mockContext.Setup(x => x.Users).Returns(usersMockSet.Object);

            return new ContractRepository(mockContext.Object);
        }

        #region GetAllContractsAsync

        [Fact]
        public async Task GetAllContractsAsync_ReturnsContracts_WhenUserIsDboAndCategoryMatches()
        {
            // Arrange
            var contracts = new List<Contract>
            {
                new() { Contractno = "C001", Category = "CAT_A" },
                new() { Contractno = "C002", Category = "CAT_B" }
            };
            var userCategories = new List<UserCategory>
            {
                new() { UserId = 1, Category = "CAT_A" },
                new() { UserId = 1, Category = "CAT_B" }
            };
            var users = new List<User>
            {
                new() { UserId = 1, Username = "dbo" }
            };
            var repo = CreateRepository(contracts, userCategories, users);

            // Act
            var result = await repo.GetAllContractsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetAllContractsAsync_ReturnsEmpty_WhenNoUserIsDbo()
        {
            // Arrange — user exists but Username is not "dbo", so JOIN filter excludes all
            var contracts = new List<Contract>
            {
                new() { Contractno = "C001", Category = "CAT_A" }
            };
            var userCategories = new List<UserCategory>
            {
                new() { UserId = 1, Category = "CAT_A" }
            };
            var users = new List<User>
            {
                new() { UserId = 1, Username = "admin" }
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
                new() { Contractno = "C001", Category = "CAT_UNMATCHED" }
            };
            var userCategories = new List<UserCategory>
            {
                new() { UserId = 1, Category = "CAT_A" }
            };
            var users = new List<User>
            {
                new() { UserId = 1, Username = "dbo" }
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
                new() { UserId = 1, Username = "dbo" }
            };
            var repo = CreateRepository(new List<Contract>(), userCategories, users);

            // Act
            var result = await repo.GetAllContractsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllContractsAsync_ReturnsOnlyDboContracts_WhenMultipleUsersExist()
        {
            // Arrange — two users share a category, only the "dbo" user should produce results
            var contracts = new List<Contract>
            {
                new() { Contractno = "C001", Category = "CAT_A" },
                new() { Contractno = "C002", Category = "CAT_B" }
            };
            var userCategories = new List<UserCategory>
            {
                new() { UserId = 1, Category = "CAT_A" }, // dbo user
                new() { UserId = 2, Category = "CAT_B" }  // non-dbo user
            };
            var users = new List<User>
            {
                new() { UserId = 1, Username = "dbo" },
                new() { UserId = 2, Username = "other" }
            };
            var repo = CreateRepository(contracts, userCategories, users);

            // Act
            var result = await repo.GetAllContractsAsync();

            // Assert
            var list = result.ToList();
            Assert.Single(list);
            Assert.Equal("C001", list[0].Contractno);
        }

        #endregion
    }
}