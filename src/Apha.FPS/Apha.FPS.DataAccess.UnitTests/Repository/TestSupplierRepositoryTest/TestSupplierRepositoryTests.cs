using Apha.Common.Helpers.Repository;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using Apha.FPS.DataAccess.Data;
using Apha.FPS.DataAccess.Repositories;
using Moq;

namespace Apha.FPS.DataAccess.UnitTests.Repository.TestSupplierRepositoryTest
{
    public class TestSupplierRepositoryTests
    {
        private const int DefaultFpsYear = 2024;
        private const string DefaultTestCode = "TEST001";
        private const string DefaultBuyer = "BUYER001";
        private const string DefaultUserEmail = "test@example.com";

        private static Mock<IFpsRequestContext> CreateMockFpsYearContext(int year = DefaultFpsYear)
        {
            var mock = new Mock<IFpsRequestContext>();
            mock.Setup(x => x.FpsYear).Returns(year);
            mock.Setup(x => x.UserEmailId).Returns(DefaultUserEmail);
            return mock;
        }

        private static TestSupplierRepository CreateRepository(
            IEnumerable<TestRequirement>? testRequirements = null,
            IEnumerable<Project>? projects = null,
            IEnumerable<TestOrProduct>? testOrProducts = null,
            IEnumerable<TestCapability>? testCapabilities = null,
            IEnumerable<MonthlyOutput>? monthlyOutputs = null,
            IEnumerable<TestRequirementLog>? testRequirementLogs = null,
            int fpsYear = DefaultFpsYear)
        {
            var mockContext = CreateMockFpsYearContext(fpsYear);
            var dbContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(mockContext.Object);

            if (testRequirements != null)
            {
                var mockSet = RepositoryTestHelper.CreateMockDbSet(testRequirements);
                dbContext.Setup(x => x.TestRequirements).Returns(mockSet.Object);
            }

            if (projects != null)
            {
                var mockSet = RepositoryTestHelper.CreateMockDbSet(projects);
                dbContext.Setup(x => x.Projects).Returns(mockSet.Object);
            }

            if (testOrProducts != null)
            {
                var mockSet = RepositoryTestHelper.CreateMockDbSet(testOrProducts);
                dbContext.Setup(x => x.TestOrProducts).Returns(mockSet.Object);
            }

            if (testCapabilities != null)
            {
                var mockSet = RepositoryTestHelper.CreateMockDbSet(testCapabilities);
                dbContext.Setup(x => x.TestCapabilities).Returns(mockSet.Object);
            }

            if (monthlyOutputs != null)
            {
                var mockSet = RepositoryTestHelper.CreateMockDbSet(monthlyOutputs);
                dbContext.Setup(x => x.MonthlyOutputs).Returns(mockSet.Object);
            }

            if (testRequirementLogs != null)
            {
                var mockSet = RepositoryTestHelper.CreateMockDbSet(testRequirementLogs);
                dbContext.Setup(x => x.TestRequirementLogs).Returns(mockSet.Object);
            }

            return new TestSupplierRepository(dbContext.Object);
        }

        #region GetByIdAsync

        [Fact]
        public async Task GetByIdAsync_WithMatchingRecord_ReturnsTestRequirement()
        {
            var requirements = new List<TestRequirement>
            {
                new() { TestCode = DefaultTestCode, Buyer = DefaultBuyer, FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(testRequirements: requirements);

            var result = await repo.GetByIdAsync(DefaultTestCode, DefaultBuyer);

            Assert.NotNull(result);
            Assert.Equal(DefaultTestCode, result.TestCode);
            Assert.Equal(DefaultBuyer, result.Buyer);
        }

        [Fact]
        public async Task GetByIdAsync_WithNoMatchingRecord_ReturnsNull()
        {
            var repo = CreateRepository(testRequirements: new List<TestRequirement>());

            var result = await repo.GetByIdAsync(DefaultTestCode, DefaultBuyer);

            Assert.Null(result);
        }

        [Fact]
        public async Task GetByIdAsync_WithDifferentBuyer_ReturnsNull()
        {
            var requirements = new List<TestRequirement>
            {
                new() { TestCode = DefaultTestCode, Buyer = "OTHER_BUYER", FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(testRequirements: requirements);

            var result = await repo.GetByIdAsync(DefaultTestCode, DefaultBuyer);

            Assert.Null(result);
        }

        #endregion

        #region GetTestOrProductsAsync

        [Fact]
        public async Task GetTestOrProductsAsync_WithItems_ReturnsOrderedList()
        {
            var products = new List<TestOrProduct>
            {
                new() { ItemCode = "ZZZ", ItemDescription = "Last" },
                new() { ItemCode = "AAA", ItemDescription = "First" }
            };
            var repo = CreateRepository(testOrProducts: products);

            var result = await repo.GetTestOrProductsAsync();

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("AAA", result[0].ItemCode);
        }

        [Fact]
        public async Task GetTestOrProductsAsync_WithNoItems_ReturnsEmptyList()
        {
            var repo = CreateRepository(testOrProducts: new List<TestOrProduct>());

            var result = await repo.GetTestOrProductsAsync();

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        #endregion

        #region ProjectExistsAsync

        [Fact]
        public async Task ProjectExistsAsync_WhenProjectExists_ReturnsTrue()
        {
            var projects = new List<Project>
            {
                new() { ParentProject = "PROJ001" }
            };
            var repo = CreateRepository(projects: projects);

            var result = await repo.ProjectExistsAsync("PROJ001");

            Assert.True(result);
        }

        [Fact]
        public async Task ProjectExistsAsync_WhenProjectDoesNotExist_ReturnsFalse()
        {
            var repo = CreateRepository(projects: new List<Project>());

            var result = await repo.ProjectExistsAsync("PROJ001");

            Assert.False(result);
        }

        #endregion

        #region TestBuyerCodeExistsAsync

        [Fact]
        public async Task TestBuyerCodeExistsAsync_WhenCapabilityExists_ReturnsTrue()
        {
            var capabilities = new List<TestCapability>
            {
                new() { TestCode = DefaultTestCode, WorkGroup = "WG001" }
            };
            var repo = CreateRepository(testCapabilities: capabilities);

            var result = await repo.TestBuyerCodeExistsAsync(DefaultTestCode, "WG001");

            Assert.True(result);
        }

        [Fact]
        public async Task TestBuyerCodeExistsAsync_WhenCapabilityDoesNotExist_ReturnsFalse()
        {
            var repo = CreateRepository(testCapabilities: new List<TestCapability>());

            var result = await repo.TestBuyerCodeExistsAsync(DefaultTestCode, "WG001");

            Assert.False(result);
        }

        #endregion

        #region MonthlyOutputExistsAsync

        [Fact]
        public async Task MonthlyOutputExistsAsync_WhenOutputExists_ReturnsTrue()
        {
            var outputs = new List<MonthlyOutput>
            {
                new() { TestCode = DefaultTestCode, Buyer = DefaultBuyer }
            };
            var repo = CreateRepository(monthlyOutputs: outputs);

            var result = await repo.MonthlyOutputExistsAsync(DefaultTestCode, DefaultBuyer);

            Assert.True(result);
        }

        [Fact]
        public async Task MonthlyOutputExistsAsync_WhenNoOutputExists_ReturnsFalse()
        {
            var repo = CreateRepository(monthlyOutputs: new List<MonthlyOutput>());

            var result = await repo.MonthlyOutputExistsAsync(DefaultTestCode, DefaultBuyer);

            Assert.False(result);
        }

        [Fact]
        public async Task MonthlyOutputExistsAsync_WithDifferentBuyer_ReturnsFalse()
        {
            var outputs = new List<MonthlyOutput>
            {
                new() { TestCode = DefaultTestCode, Buyer = "OTHER_BUYER" }
            };
            var repo = CreateRepository(monthlyOutputs: outputs);

            var result = await repo.MonthlyOutputExistsAsync(DefaultTestCode, DefaultBuyer);

            Assert.False(result);
        }

        #endregion
    }
}
