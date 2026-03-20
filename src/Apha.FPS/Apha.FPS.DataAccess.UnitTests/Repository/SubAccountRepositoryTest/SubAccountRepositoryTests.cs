using Apha.Common.Helpers.Repository;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.DataAccess.Data;
using Apha.FPS.DataAccess.Repositories;
using NSubstitute;

namespace Apha.FPS.DataAccess.UnitTests.Repository.SubAccountRepositoryTest
{
    public class SubAccountRepositoryTests
    {
        private const int DefaultTestFpsYear = 2024;

        /// <summary>
        /// Creates a SubAccountRepository with in-memory SubAccounts data.
        /// IFpsYearContext is substituted via NSubstitute.
        /// SubAccount has no FpsCalYear query filter, so year value is irrelevant.
        /// </summary>
        private static SubAccountRepository CreateRepository(IEnumerable<SubAccount> subAccounts)
        {
            var fpsYearContext = Substitute.For<IFpsYearContext>();
            fpsYearContext.FPSYear.Returns(DefaultTestFpsYear);

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(fpsYearContext);

            var subAccountsMockSet = RepositoryTestHelper.CreateMockDbSet(subAccounts);
            mockContext.Setup(x => x.SubAccounts).Returns(subAccountsMockSet.Object);

            return new SubAccountRepository(mockContext.Object);
        }

        #region GetAllSubAccountsAsync

        [Fact]
        public async Task GetAllSubAccountsAsync_ReturnsAllSubAccounts_WhenDataExists()
        {
            // Arrange
            var subAccounts = new List<SubAccount>
            {
                new() { SubAccountCode = "SA001", SubAccountName = "Sub Account One" },
                new() { SubAccountCode = "SA002", SubAccountName = "Sub Account Two" },
                new() { SubAccountCode = "SA003", SubAccountName = "Sub Account Three" }
            };
            var repo = CreateRepository(subAccounts);

            // Act
            var result = await repo.GetAllSubAccountsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count());
        }

        [Fact]
        public async Task GetAllSubAccountsAsync_ReturnsEmptyCollection_WhenNoSubAccountsExist()
        {
            // Arrange
            var repo = CreateRepository(new List<SubAccount>());

            // Act
            var result = await repo.GetAllSubAccountsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllSubAccountsAsync_ReturnsCorrectData_WhenSingleSubAccountExists()
        {
            // Arrange
            var subAccounts = new List<SubAccount>
            {
                new() { SubAccountCode = "SA001", SubAccountName = "Sub Account One" }
            };
            var repo = CreateRepository(subAccounts);

            // Act
            var result = await repo.GetAllSubAccountsAsync();

            // Assert
            var single = Assert.Single(result);
            Assert.Equal("SA001", single.SubAccountCode);
            Assert.Equal("Sub Account One", single.SubAccountName);
        }

        [Fact]
        public async Task GetAllSubAccountsAsync_ReturnsIEnumerable_NotNull()
        {
            // Arrange — verifies the return type contract is always IEnumerable, never null
            var repo = CreateRepository(new List<SubAccount>());

            // Act
            var result = await repo.GetAllSubAccountsAsync();

            // Assert
            Assert.IsAssignableFrom<IEnumerable<SubAccount>>(result);
        }

        #endregion
    }
}