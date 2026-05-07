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
        private static ProfitCentreRepository CreateRepository(IEnumerable<ProfitCentre> profitCentres)
        {
            var requestContext = Substitute.For<IFpsRequestContext>();
            requestContext.FpsYear.Returns(2024);

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(requestContext);

            var mockSet = RepositoryTestHelper.CreateMockDbSet(profitCentres);
            mockContext.Setup(x => x.ProfitCentres).Returns(mockSet.Object);

            return new ProfitCentreRepository(mockContext.Object);
        }

        #region GetProfitCentresAsync Tests

        [Fact]
        public async Task GetProfitCentresAsync_ReturnsAllProfitCentres_WhenDataExists()
        {
            // Arrange
            var profitCentres = new List<ProfitCentre>
            {
                new() { ProfitCentreId = "PC01", ProfitCentreName = "Profit Centre One", Division = "DIV1" },
                new() { ProfitCentreId = "PC02", ProfitCentreName = "Profit Centre Two", Division = "DIV1" },
                new() { ProfitCentreId = "PC03", ProfitCentreName = "Profit Centre Three", Division = "DIV2" }
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
            var repo = CreateRepository(new List<ProfitCentre>());

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
            var profitCentres = new List<ProfitCentre>
            {
                new() { ProfitCentreId = "PC03", ProfitCentreName = "Centre Three", Division = "DIV1" },
                new() { ProfitCentreId = "PC01", ProfitCentreName = "Centre One",   Division = "DIV1" },
                new() { ProfitCentreId = "PC02", ProfitCentreName = "Centre Two",   Division = "DIV1" }
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
            var profitCentres = new List<ProfitCentre>
            {
                new() { ProfitCentreId = "PC01", ProfitCentreName = "Centre One", Division = "DIV1" }
            };
            var repo = CreateRepository(profitCentres);

            // Act
            var result = await repo.GetProfitCentresAsync();

            // Assert
            var single = Assert.Single(result);
            Assert.Equal("PC01", single.ProfitCentreId);
        }

        #endregion
    }
}
