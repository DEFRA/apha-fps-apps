using Apha.Common.Helpers.Repository;
using Apha.FPS.Core.Enities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.DataAccess.Data;
using Apha.FPS.DataAccess.Repositories;
using NSubstitute;

namespace Apha.FPS.DataAccess.UnitTests.Repository.AccountCodeRepositoryTest
{
    public class AccountCodeRepositoryTests
    {
        /// <summary>
        /// Creates an AccountCodeRepository with in-memory AccountCodes data.
        /// IFpsYearContext is substituted via NSubstitute.
        /// AccountCode has no FpsCalYear query filter, so year value is irrelevant.
        /// </summary>
        private static AccountCodeRepository CreateRepository(IEnumerable<AccountCode> accountCodes)
        {
            var fpsYearContext = Substitute.For<IFpsRequestContext>();
            fpsYearContext.FpsYear.Returns(2024);

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(fpsYearContext);

            var accountCodesMockSet = RepositoryTestHelper.CreateMockDbSet(accountCodes);
            mockContext.Setup(x => x.AccountCodes).Returns(accountCodesMockSet.Object);

            return new AccountCodeRepository(mockContext.Object);
        }

        #region GetAllAccountCodeAsync

        [Fact]
        public async Task GetAllAccountCodeAsync_ReturnsAllAccountCodes_WhenDataExists()
        {
            // Arrange
            var accountCodes = new List<AccountCode>
            {
                new() { Code = "AC001", Description = "Account One" },
                new() { Code = "AC002", Description = "Account Two" },
                new() { Code = "AC003", Description = "Account Three" }
            };
            var repo = CreateRepository(accountCodes);

            // Act
            var result = await repo.GetAllAccountCodeAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count());
        }

        [Fact]
        public async Task GetAllAccountCodeAsync_ReturnsEmptyCollection_WhenNoDataExists()
        {
            // Arrange
            var repo = CreateRepository(new List<AccountCode>());

            // Act
            var result = await repo.GetAllAccountCodeAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllAccountCodeAsync_ReturnsCorrectData_WhenSingleRecordExists()
        {
            // Arrange
            var accountCodes = new List<AccountCode>
            {
                new() { Code = "AC001", Description = "Account One" }
            };
            var repo = CreateRepository(accountCodes);

            // Act
            var result = await repo.GetAllAccountCodeAsync();

            // Assert
            var single = Assert.Single(result);
            Assert.Equal("AC001", single.Code);
            Assert.Equal("Account One", single.Description);
        }

        [Fact]
        public async Task GetAllAccountCodeAsync_ReturnsIEnumerable_NotNull()
        {
            // Arrange — verifies the return type contract is always IEnumerable, never null
            var repo = CreateRepository(new List<AccountCode>());

            // Act
            var result = await repo.GetAllAccountCodeAsync();

            // Assert
            Assert.IsAssignableFrom<IEnumerable<AccountCode>>(result);
        }

        #endregion
    }
}