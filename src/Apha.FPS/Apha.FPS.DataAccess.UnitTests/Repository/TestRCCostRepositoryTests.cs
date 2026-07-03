using Apha.Common.Helpers.Repository;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.DataAccess.Data;
using Apha.FPS.DataAccess.Repositories;
using Moq;
using Xunit;

namespace Apha.FPS.DataAccess.UnitTests.Repository.TestRCCostRepositoryTest
{
    public class TestRCCostRepositoryTests
    {
        private const int DefaultFpsYear = 2025;
        private const string DefaultUserEmail = "test@example.com";
        private const string DefaultTestCode = "TEST001";
        private const string DefaultProfitCentre = "PC001";

        private static Mock<IFpsRequestContext> CreateMockRequestContext(int year = DefaultFpsYear)
        {
            var mock = new Mock<IFpsRequestContext>();
            mock.Setup(x => x.FpsYear).Returns(year);
            mock.Setup(x => x.UserEmailId).Returns(DefaultUserEmail);
            return mock;
        }

        private static TestRCCostRepository CreateRepository(
            IEnumerable<TestRCCost>? testRCCosts = null,
            int fpsYear = DefaultFpsYear)
        {
            var mockRequestContext = CreateMockRequestContext(fpsYear);
            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(mockRequestContext.Object);

            var testRCCostSet = RepositoryTestHelper.CreateMockDbSet(
                testRCCosts ?? Enumerable.Empty<TestRCCost>());
            mockContext.Setup(x => x.TestRCCosts).Returns(testRCCostSet.Object);

            return new TestRCCostRepository(mockContext.Object);
        }

        #region GetByTestCodeAsync

        [Fact]
        public async Task GetByTestCodeAsync_WithMatchingRecords_ReturnsList()
        {
            // Arrange
            var entities = new List<TestRCCost>
            {
                CreateEntity(DefaultTestCode, "PC001"),
                CreateEntity(DefaultTestCode, "PC002")
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
            var entities = new List<TestRCCost> { CreateEntity(DefaultTestCode, DefaultProfitCentre) };
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
            var entities = new List<TestRCCost> { CreateEntity(DefaultTestCode, DefaultProfitCentre) };
            var repo = CreateRepository(entities);

            // Act
            var result = await repo.GetByTestCodeAsync(DefaultTestCode, 9999);

            // Assert
            Assert.Empty(result);
        }

        #endregion

        #region GetByKeyAsync

        [Fact]
        public async Task GetByKeyAsync_WithExistingCompositeKey_ReturnsRecord()
        {
            // Arrange
            var entities = new List<TestRCCost> { CreateEntity(DefaultTestCode, DefaultProfitCentre) };
            var repo = CreateRepository(entities);

            // Act
            var result = await repo.GetByKeyAsync(DefaultTestCode, DefaultProfitCentre, DefaultFpsYear);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(DefaultProfitCentre, result!.ProfitCentre);
        }

        [Fact]
        public async Task GetByKeyAsync_WithNonExistingKey_ReturnsNull()
        {
            // Arrange
            var repo = CreateRepository(Enumerable.Empty<TestRCCost>());

            // Act
            var result = await repo.GetByKeyAsync("NOTEXIST", "PC999", DefaultFpsYear);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByKeyAsync_WithWrongProfitCentre_ReturnsNull()
        {
            // Arrange
            var entities = new List<TestRCCost> { CreateEntity(DefaultTestCode, "PC001") };
            var repo = CreateRepository(entities);

            // Act
            var result = await repo.GetByKeyAsync(DefaultTestCode, "WRONG_PC", DefaultFpsYear);

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region ExistsAsync

        [Fact]
        public async Task ExistsAsync_WithExistingRecord_ReturnsTrue()
        {
            // Arrange
            var entities = new List<TestRCCost> { CreateEntity(DefaultTestCode, DefaultProfitCentre) };
            var repo = CreateRepository(entities);

            // Act
            var result = await repo.ExistsAsync(DefaultTestCode, DefaultProfitCentre, DefaultFpsYear);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ExistsAsync_WithNonExistingRecord_ReturnsFalse()
        {
            // Arrange
            var repo = CreateRepository(Enumerable.Empty<TestRCCost>());

            // Act
            var result = await repo.ExistsAsync("NOTEXIST", "PC999", DefaultFpsYear);

            // Assert
            Assert.False(result);
        }

        #endregion

        #region GetByTestCodeAsync — Multiple profit centres sorted

        [Fact]
        public async Task GetByTestCodeAsync_MultipleRecords_ReturnsSortedByProfitCentre()
        {
            // Arrange
            var entities = new List<TestRCCost>
            {
                new() { TestCode = DefaultTestCode, ProfitCentre = "PC003", FpsYear = DefaultFpsYear, Price = 300m },
                new() { TestCode = DefaultTestCode, ProfitCentre = "PC001", FpsYear = DefaultFpsYear, Price = 100m },
                new() { TestCode = DefaultTestCode, ProfitCentre = "PC002", FpsYear = DefaultFpsYear, Price = 200m }
            };
            var repo = CreateRepository(entities);

            // Act
            var result = (await repo.GetByTestCodeAsync(DefaultTestCode, DefaultFpsYear)).ToList();

            // Assert
            Assert.Equal(3, result.Count);
            Assert.Equal("PC001", result.First().ProfitCentre);
            Assert.Equal("PC003", result.Last().ProfitCentre);
        }

        #endregion

        #region Helper Methods

        private static TestRCCost CreateEntity(string testCode, string profitCentre) =>
            new()
            {
                TestCode = testCode,
                ProfitCentre = profitCentre,
                FpsYear = DefaultFpsYear,
                Price = 150m
            };

        #endregion
    }
}
