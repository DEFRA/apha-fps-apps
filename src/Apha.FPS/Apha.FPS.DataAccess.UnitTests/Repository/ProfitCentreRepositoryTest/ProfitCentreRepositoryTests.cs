using Apha.Common.Helpers.Repository;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.DataAccess.Data;
using Apha.FPS.DataAccess.Repositories;
using NSubstitute;

namespace Apha.FPS.DataAccess.UnitTests.Repository.ProfitCentreRepositoryTest
{
    public class ProfitCentreRepositoryTests
    {
        private static ProfitCentreRepository CreateRepository(IEnumerable<ProfitCentreView> profitCentres)
        {
            var requestContext = Substitute.For<IFpsRequestContext>();
            requestContext.FpsYear.Returns(2024);
            requestContext.UserEmailId.Returns("test@example.com");

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(requestContext);

            var mockSet = RepositoryTestHelper.CreateMockDbSet(profitCentres);
            mockContext.Setup(x => x.ProfitCentreViews).Returns(mockSet.Object);

            return new ProfitCentreRepository(mockContext.Object, requestContext);
        }

        #region GetProfitCentresAsync Tests

        [Fact]
        public async Task GetProfitCentresAsync_ReturnsAllProfitCentres_WhenDataExists()
        {
            // Arrange
            var profitCentres = new List<ProfitCentreView>
            {
                new() { ProfitCentreId = "PC01", ProfitCentreName = "Profit Centre One", Division = "DIV1", UserEmail = "test@example.com" },
                new() { ProfitCentreId = "PC02", ProfitCentreName = "Profit Centre Two", Division = "DIV1", UserEmail = "test@example.com" },
                new() { ProfitCentreId = "PC03", ProfitCentreName = "Profit Centre Three", Division = "DIV2", UserEmail = "test@example.com" }
            };
            var repo = CreateRepository(profitCentres);

            // Act
            var result = await repo.GetProfitCentresAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count);
        }

        [Fact]
        public async Task GetProfitCentresAsync_ReturnsEmpty_WhenNoDataExists()
        {
            // Arrange
            var repo = CreateRepository(new List<ProfitCentreView>());

            // Act
            var result = await repo.GetProfitCentresAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetProfitCentresAsync_ReturnsOrderedByProfitCentreId()
        {
            // Arrange
            var profitCentres = new List<ProfitCentreView>
            {
                new() { ProfitCentreId = "PC03", ProfitCentreName = "Centre Three", Division = "DIV1", UserEmail = "test@example.com" },
                new() { ProfitCentreId = "PC01", ProfitCentreName = "Centre One",   Division = "DIV1", UserEmail = "test@example.com" },
                new() { ProfitCentreId = "PC02", ProfitCentreName = "Centre Two",   Division = "DIV1", UserEmail = "test@example.com" }
            };
            var repo = CreateRepository(profitCentres);

            // Act
            var result = await repo.GetProfitCentresAsync();

            // Assert
            var resultList = result.ToList();
            Assert.Equal("PC01", resultList[0].ProfitCentreId);
            Assert.Equal("PC02", resultList[1].ProfitCentreId);
            Assert.Equal("PC03", resultList[2].ProfitCentreId);
        }

        [Fact]
        public async Task GetProfitCentresAsync_ReturnsSingle_WhenOneItemExists()
        {
            // Arrange
            var profitCentres = new List<ProfitCentreView>
            {
                new() { ProfitCentreId = "PC01", ProfitCentreName = "Centre One", Division = "DIV1", UserEmail = "test@example.com" }
            };
            var repo = CreateRepository(profitCentres);

            // Act
            var result = await repo.GetProfitCentresAsync();

            // Assert
            var single = Assert.Single(result);
            Assert.Equal("PC01", single.ProfitCentreId);
        }

        #endregion

        #region GetAllProfitCentresAsync Tests

        private static ProfitCentreRepository CreateRepositoryWithProfitCentres(IEnumerable<ProfitCentre> profitCentres)
        {
            var requestContext = Substitute.For<IFpsRequestContext>();
            requestContext.FpsYear.Returns(2024);
            requestContext.UserEmailId.Returns("test@example.com");

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(requestContext);

            var mockSet = RepositoryTestHelper.CreateMockDbSet(profitCentres);
            mockContext.Setup(x => x.ProfitCentres).Returns(mockSet.Object);
            RepositoryTestHelper.SetupSaveChanges(mockContext);

            return new ProfitCentreRepository(mockContext.Object, requestContext);
        }

        [Fact]
        public async Task GetAllProfitCentresAsync_WithData_ReturnsAllOrderedById()
        {
            // Arrange
            var profitCentres = new List<ProfitCentre>
            {
                new() { ProfitCentreId = "PC03", ProfitCentreName = "Centre Three", Division = "DIV1" },
                new() { ProfitCentreId = "PC01", ProfitCentreName = "Centre One",   Division = "DIV1" },
                new() { ProfitCentreId = "PC02", ProfitCentreName = "Centre Two",   Division = "DIV1" }
            };
            var repo = CreateRepositoryWithProfitCentres(profitCentres);

            // Act
            var result = (await repo.GetAllProfitCentresAsync()).ToList();

            // Assert
            Assert.Equal(3, result.Count);
            Assert.Equal("PC01", result[0].ProfitCentreId);
            Assert.Equal("PC02", result[1].ProfitCentreId);
            Assert.Equal("PC03", result[2].ProfitCentreId);
        }

        [Fact]
        public async Task GetAllProfitCentresAsync_WithNoData_ReturnsEmpty()
        {
            // Arrange
            var repo = CreateRepositoryWithProfitCentres(new List<ProfitCentre>());

            // Act
            var result = await repo.GetAllProfitCentresAsync();

            // Assert
            Assert.Empty(result);
        }

        #endregion

        #region GetProfitCentreByIdAsync Tests

        [Fact]
        public async Task GetProfitCentreByIdAsync_WithExistingId_ReturnsProfitCentre()
        {
            // Arrange
            var profitCentres = new List<ProfitCentre>
            {
                new() { ProfitCentreId = "PC01", ProfitCentreName = "Centre One", Division = "DIV1" },
                new() { ProfitCentreId = "PC02", ProfitCentreName = "Centre Two", Division = "DIV1" }
            };
            var repo = CreateRepositoryWithProfitCentres(profitCentres);

            // Act
            var result = await repo.GetProfitCentreByIdAsync("PC01");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("PC01", result.ProfitCentreId);
            Assert.Equal("Centre One", result.ProfitCentreName);
        }

        [Fact]
        public async Task GetProfitCentreByIdAsync_WithNonExistentId_ReturnsNull()
        {
            // Arrange
            var profitCentres = new List<ProfitCentre>
            {
                new() { ProfitCentreId = "PC01", ProfitCentreName = "Centre One", Division = "DIV1" }
            };
            var repo = CreateRepositoryWithProfitCentres(profitCentres);

            // Act
            var result = await repo.GetProfitCentreByIdAsync("PC_MISSING");

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region UpdateProfitCentreSettingsAsync Tests

        [Fact]
        public async Task UpdateProfitCentreSettingsAsync_WithValidData_ReturnsTrue()
        {
            // Arrange
            var profitCentres = new List<ProfitCentre>
            {
                new() { ProfitCentreId = "PC01", ProfitCentreName = "Centre One", Division = "DIV1", Timesheet = 0, OutputSheet = 0, TimesheetLayout = 1 }
            };
            var repo = CreateRepositoryWithProfitCentres(profitCentres);

            // Act
            var result = await repo.UpdateProfitCentreSettingsAsync("PC01", -1, -1, 2);

            // Assert
            Assert.True(result);
        }

        #endregion
    }
}
