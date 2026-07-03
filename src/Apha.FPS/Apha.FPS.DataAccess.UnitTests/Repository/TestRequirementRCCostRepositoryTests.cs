using Apha.Common.Helpers.Repository;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.DataAccess.Data;
using Apha.FPS.DataAccess.Repositories;
using Moq;
using Xunit;

namespace Apha.FPS.DataAccess.UnitTests.Repository.TestRequirementRCCostRepositoryTest
{
    public class TestRequirementRCCostRepositoryTests
    {
        private const int DefaultFpsYear = 2025;
        private const string DefaultUserEmail = "test@example.com";
        private const string DefaultTestCode = "TEST001";
        private const string DefaultBuyer = "BUYER01";
        private const string DefaultProfitCentre = "PC001";

        private static Mock<IFpsRequestContext> CreateMockRequestContext(int year = DefaultFpsYear)
        {
            var mock = new Mock<IFpsRequestContext>();
            mock.Setup(x => x.FpsYear).Returns(year);
            mock.Setup(x => x.UserEmailId).Returns(DefaultUserEmail);
            return mock;
        }

        private static TestRequirementRCCostRepository CreateRepository(
            IEnumerable<TestRequirementRCCost>? testReqCosts = null,
            int fpsYear = DefaultFpsYear)
        {
            var mockRequestContext = CreateMockRequestContext(fpsYear);
            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(mockRequestContext.Object);

            var testReqCostSet = RepositoryTestHelper.CreateMockDbSet(
                testReqCosts ?? Enumerable.Empty<TestRequirementRCCost>());
            mockContext.Setup(x => x.TestRequirementRCCosts).Returns(testReqCostSet.Object);

            return new TestRequirementRCCostRepository(mockContext.Object);
        }

        #region GetByTestCodeAsync

        [Fact]
        public async Task GetByTestCodeAsync_WithMatchingRecords_ReturnsList()
        {
            // Arrange
            var entities = new List<TestRequirementRCCost>
            {
                CreateEntity(DefaultTestCode, "BUYER01", "PC001"),
                CreateEntity(DefaultTestCode, "BUYER02", "PC002")
            };
            var repo = CreateRepository(entities);

            // Act
            var result = await repo.GetByTestCodeAsync(DefaultTestCode, DefaultFpsYear);

            // Assert
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetByTestCodeAsync_WithNonMatchingTestCode_ReturnsEmpty()
        {
            // Arrange
            var entities = new List<TestRequirementRCCost>
            {
                CreateEntity(DefaultTestCode, DefaultBuyer, DefaultProfitCentre)
            };
            var repo = CreateRepository(entities);

            // Act
            var result = await repo.GetByTestCodeAsync("NOTEXIST", DefaultFpsYear);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetByTestCodeAsync_WithNonMatchingFpsYear_ReturnsEmpty()
        {
            // Arrange
            var entities = new List<TestRequirementRCCost>
            {
                CreateEntity(DefaultTestCode, DefaultBuyer, DefaultProfitCentre)
            };
            var repo = CreateRepository(entities);

            // Act
            var result = await repo.GetByTestCodeAsync(DefaultTestCode, 9999);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetByTestCodeAsync_MultipleRecords_ReturnsSortedByBuyerThenProfitCentre()
        {
            // Arrange
            var entities = new List<TestRequirementRCCost>
            {
                new() { TestCode = DefaultTestCode, Buyer = "BUYER02", ProfitCentre = "PC001", FpsYear = DefaultFpsYear, Price = 200m },
                new() { TestCode = DefaultTestCode, Buyer = "BUYER01", ProfitCentre = "PC002", FpsYear = DefaultFpsYear, Price = 100m },
                new() { TestCode = DefaultTestCode, Buyer = "BUYER01", ProfitCentre = "PC001", FpsYear = DefaultFpsYear, Price = 50m }
            };
            var repo = CreateRepository(entities);

            // Act
            var result = (await repo.GetByTestCodeAsync(DefaultTestCode, DefaultFpsYear)).ToList();

            // Assert
            Assert.Equal(3, result.Count);
            Assert.Equal("BUYER01", result.First().Buyer);
            Assert.Equal("PC001", result.First().ProfitCentre);
        }

        #endregion

        #region GetByKeyAsync

        [Fact]
        public async Task GetByKeyAsync_WithExistingCompositeKey_ReturnsRecord()
        {
            // Arrange
            var entities = new List<TestRequirementRCCost>
            {
                CreateEntity(DefaultTestCode, DefaultBuyer, DefaultProfitCentre)
            };
            var repo = CreateRepository(entities);

            // Act
            var result = await repo.GetByKeyAsync(DefaultTestCode, DefaultBuyer, DefaultProfitCentre, DefaultFpsYear);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(DefaultBuyer, result!.Buyer);
            Assert.Equal(DefaultProfitCentre, result.ProfitCentre);
        }

        [Fact]
        public async Task GetByKeyAsync_WithNonExistingKey_ReturnsNull()
        {
            // Arrange
            var repo = CreateRepository(Enumerable.Empty<TestRequirementRCCost>());

            // Act
            var result = await repo.GetByKeyAsync("NOTEXIST", "B999", "PC999", DefaultFpsYear);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByKeyAsync_WithWrongBuyer_ReturnsNull()
        {
            // Arrange
            var entities = new List<TestRequirementRCCost>
            {
                CreateEntity(DefaultTestCode, "BUYER01", DefaultProfitCentre)
            };
            var repo = CreateRepository(entities);

            // Act — correct TestCode, ProfitCentre, FpsYear but wrong Buyer
            var result = await repo.GetByKeyAsync(DefaultTestCode, "WRONG_BUYER", DefaultProfitCentre, DefaultFpsYear);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByKeyAsync_WithWrongProfitCentre_ReturnsNull()
        {
            // Arrange
            var entities = new List<TestRequirementRCCost>
            {
                CreateEntity(DefaultTestCode, DefaultBuyer, "PC001")
            };
            var repo = CreateRepository(entities);

            // Act — correct TestCode, Buyer, FpsYear but wrong ProfitCentre
            var result = await repo.GetByKeyAsync(DefaultTestCode, DefaultBuyer, "WRONG_PC", DefaultFpsYear);

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region ExistsAsync

        [Fact]
        public async Task ExistsAsync_WithExistingRecord_ReturnsTrue()
        {
            // Arrange
            var entities = new List<TestRequirementRCCost>
            {
                CreateEntity(DefaultTestCode, DefaultBuyer, DefaultProfitCentre)
            };
            var repo = CreateRepository(entities);

            // Act
            var result = await repo.ExistsAsync(DefaultTestCode, DefaultBuyer, DefaultProfitCentre, DefaultFpsYear);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ExistsAsync_WithNonExistingRecord_ReturnsFalse()
        {
            // Arrange
            var repo = CreateRepository(Enumerable.Empty<TestRequirementRCCost>());

            // Act
            var result = await repo.ExistsAsync("NOTEXIST", "B999", "PC999", DefaultFpsYear);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task ExistsAsync_PartialKeyMatch_ReturnsFalse()
        {
            // Arrange — same TestCode and Buyer but different ProfitCentre
            var entities = new List<TestRequirementRCCost>
            {
                CreateEntity(DefaultTestCode, DefaultBuyer, "PC001")
            };
            var repo = CreateRepository(entities);

            // Act
            var result = await repo.ExistsAsync(DefaultTestCode, DefaultBuyer, "DIFFERENT_PC", DefaultFpsYear);

            // Assert
            Assert.False(result);
        }

        #endregion

        #region Helper Methods

        private static TestRequirementRCCost CreateEntity(string testCode, string buyer, string profitCentre) =>
            new()
            {
                TestCode = testCode,
                Buyer = buyer,
                ProfitCentre = profitCentre,
                FpsYear = DefaultFpsYear,
                Price = 200m
            };

        #endregion
    }
}
