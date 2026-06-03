using Apha.Common.Helpers.Repository;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using Apha.FPS.DataAccess.Data;
using Apha.FPS.DataAccess.Repositories;
using Microsoft.EntityFrameworkCore;
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
                RepositoryTestHelper.SetupDbSetOperations(mockSet);
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

            RepositoryTestHelper.SetupSaveChanges(dbContext);

            return new TestSupplierRepository(dbContext.Object);
        }

        /// <summary>
        /// Returns the repo plus all relevant mock sets for mutation tests (Add/Update/Delete).
        /// </summary>
        private static (
            TestSupplierRepository Repo,
            Mock<DbSet<TestRequirement>> RequirementsSet,
            Mock<DbSet<TestRequirementLog>> LogsSet,
            Mock<FpsDbContext> Context)
            CreateRepositoryWithMocks(IEnumerable<TestRequirement>? requirements = null)
        {
            var mockYearContext = CreateMockFpsYearContext();
            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(mockYearContext.Object);

            var requirementsSet = RepositoryTestHelper.CreateMockDbSet(requirements ?? []);
            RepositoryTestHelper.SetupDbSetOperations(requirementsSet);
            mockContext.Setup(x => x.TestRequirements).Returns(requirementsSet.Object);

            var logsSet = RepositoryTestHelper.CreateMockDbSet(new List<TestRequirementLog>());
            mockContext.Setup(x => x.TestRequirementLogs).Returns(logsSet.Object);

            RepositoryTestHelper.SetupSaveChanges(mockContext);

            return (new TestSupplierRepository(mockContext.Object), requirementsSet, logsSet, mockContext);
        }

        // ── Shared test data ─────────────────────────────────────────────────

        private static List<TestRequirement> TwoRequirements() =>
        [
            new() { TestCode = DefaultTestCode, Buyer = "BUYER_A", NoRequired = 2.0, UnitPrice = 10m, FpsYear = DefaultFpsYear },
            new() { TestCode = DefaultTestCode, Buyer = "BUYER_B", NoRequired = 3.0, UnitPrice = 5m,  FpsYear = DefaultFpsYear }
        ];

        private static List<Project> TwoProjects() =>
        [
            new() { ParentProject = "BUYER_A", Manager = "Alice", ProjectStatus = "active",   FpsYear = DefaultFpsYear },
            new() { ParentProject = "BUYER_B", Manager = "Bob",   ProjectStatus = "rejected", FpsYear = DefaultFpsYear }
        ];

        private static PaginationParameters<string> DefaultPage(string? sortBy = null, bool desc = false) =>
            new() { Page = 1, PageSize = 10, SortBy = sortBy, Descending = desc };

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

        #region GetPagedByTestCodeAsync

        [Fact]
        public async Task GetPagedByTestCodeAsync_ShowRejectedTrue_IncludesRejectedRows()
        {
            var repo = CreateRepository(testRequirements: TwoRequirements(), projects: TwoProjects());

            var result = await repo.GetPagedByTestCodeAsync(DefaultPage(), DefaultTestCode, showRejected: true);

            Assert.Equal(2, result.Data.Count());
        }

        [Fact]
        public async Task GetPagedByTestCodeAsync_ShowRejectedFalse_ExcludesRejectedRows()
        {
            var repo = CreateRepository(testRequirements: TwoRequirements(), projects: TwoProjects());

            var result = await repo.GetPagedByTestCodeAsync(DefaultPage(), DefaultTestCode, showRejected: false);

            Assert.Single(result.Data);
            Assert.DoesNotContain(result.Data, v => v.ProjectStatus == "rejected");
        }

        [Fact]
        public async Task GetPagedByTestCodeAsync_CalculatesTestCostCorrectly()
        {
            var repo = CreateRepository(testRequirements: TwoRequirements(), projects: TwoProjects());

            var result = await repo.GetPagedByTestCodeAsync(DefaultPage(), DefaultTestCode, showRejected: true);

            var viewA = result.Data.First(v => v.JobCode == "BUYER_A");
            Assert.Equal(20m, viewA.TestCost); // (decimal)2.0 * 10m
        }

        [Fact]
        public async Task GetPagedByTestCodeAsync_NoMatchingTestCode_ReturnsEmptyData()
        {
            var repo = CreateRepository(testRequirements: TwoRequirements(), projects: TwoProjects());

            var result = await repo.GetPagedByTestCodeAsync(DefaultPage(), "UNKNOWN", showRejected: true);

            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetPagedByTestCodeAsync_MapsViewFieldsCorrectly()
        {
            var repo = CreateRepository(testRequirements: TwoRequirements(), projects: TwoProjects());

            var result = await repo.GetPagedByTestCodeAsync(DefaultPage(), DefaultTestCode, showRejected: true);

            var viewA = result.Data.First(v => v.JobCode == "BUYER_A");
            Assert.Equal(DefaultTestCode, viewA.TestCode);
            Assert.Equal("Alice", viewA.ProjectManager);
            Assert.Equal(2.0, viewA.NoTests);
            Assert.Equal(10m, viewA.TestPrice);
            Assert.Equal("active", viewA.ProjectStatus);
        }

        [Theory]
        [InlineData("testcode",      false)]
        [InlineData("testcode",      true)]
        [InlineData("jobcode",       false)]
        [InlineData("jobcode",       true)]
        [InlineData("projectmanager",false)]
        [InlineData("projectmanager",true)]
        [InlineData("notests",       false)]
        [InlineData("notests",       true)]
        [InlineData("testprice",     false)]
        [InlineData("testprice",     true)]
        [InlineData("testcost",      false)]
        [InlineData("testcost",      true)]
        [InlineData("projectstatus", false)]
        [InlineData("projectstatus", true)]
        [InlineData(null,            false)]
        [InlineData("unknown_col",   false)]
        public async Task GetPagedByTestCodeAsync_WithSortBy_DoesNotThrow(string? sortBy, bool desc)
        {
            var repo = CreateRepository(testRequirements: TwoRequirements(), projects: TwoProjects());
            var query = DefaultPage(sortBy, desc);

            var result = await repo.GetPagedByTestCodeAsync(query, DefaultTestCode, showRejected: true);

            Assert.NotNull(result);
            Assert.Equal(2, result.Data.Count());
        }

        [Fact]
        public async Task GetPagedByTestCodeAsync_NullNoRequiredAndUnitPrice_TestCostIsZero()
        {
            var requirements = new List<TestRequirement>
            {
                new() { TestCode = DefaultTestCode, Buyer = "BUYER_A", NoRequired = null, UnitPrice = null, FpsYear = DefaultFpsYear }
            };
            var projects = new List<Project>
            {
                new() { ParentProject = "BUYER_A", Manager = "Alice", ProjectStatus = "active", FpsYear = DefaultFpsYear }
            };
            var repo = CreateRepository(testRequirements: requirements, projects: projects);

            var result = await repo.GetPagedByTestCodeAsync(DefaultPage(), DefaultTestCode, showRejected: true);

            Assert.Single(result.Data);
            Assert.Equal(0m, result.Data.First().TestCost);
        }

        [Fact]
        public async Task GetPagedByTestCodeAsync_Paging_ReturnsCorrectPage()
        {
            var requirements = Enumerable.Range(1, 5).Select(i => new TestRequirement
            {
                TestCode = DefaultTestCode, Buyer = $"BUYER_{i:D2}", NoRequired = 1.0, UnitPrice = 1m, FpsYear = DefaultFpsYear
            }).ToList();
            var projects = Enumerable.Range(1, 5).Select(i => new Project
            {
                ParentProject = $"BUYER_{i:D2}", Manager = "Mgr", ProjectStatus = "active", FpsYear = DefaultFpsYear
            }).ToList();

            var repo = CreateRepository(testRequirements: requirements, projects: projects);
            var query = new PaginationParameters<string> { Page = 2, PageSize = 2 };

            var result = await repo.GetPagedByTestCodeAsync(query, DefaultTestCode, showRejected: true);

            Assert.Equal(2, result.Data.Count());
            Assert.Equal(5, result.PaginationData.TotalRecords);
            Assert.Equal(2, result.PaginationData.PageNumber);
        }

        #endregion

        #region AddAsync

        [Fact]
        public async Task AddAsync_AddsEntityAndLog_AndReturnsEntity()
        {
            var (repo, requirementsSet, logsSet, mockContext) = CreateRepositoryWithMocks();
            var entity = new TestRequirement
            {
                TestCode = DefaultTestCode, Buyer = DefaultBuyer,
                NoRequired = 2.0, UnitPrice = 5m, Active = 1, FpsYear = DefaultFpsYear
            };

            var result = await repo.AddAsync(entity);

            Assert.Same(entity, result);
            requirementsSet.Verify(x => x.Add(It.Is<TestRequirement>(e => e.TestCode == DefaultTestCode)), Times.Once);
            logsSet.Verify(x => x.Add(It.Is<TestRequirementLog>(l =>
                l.TestCode == DefaultTestCode &&
                l.Buyer == DefaultBuyer &&
                l.InsertDelete == "I")), Times.Once);
            RepositoryTestHelper.VerifySaveChanges(mockContext);
        }

        [Fact]
        public async Task AddAsync_LogEntry_ContainsCorrectFieldValues()
        {
            var (repo, _, logsSet, _) = CreateRepositoryWithMocks();
            var entity = new TestRequirement
            {
                TestCode = DefaultTestCode, Buyer = DefaultBuyer,
                UnitPrice = 12.50m, NoRequired = 3.0,
                ProjectBuyerCode = "PBC", TestBuyerCode = "TBC",
                Active = 1, FpsYear = DefaultFpsYear
            };

            await repo.AddAsync(entity);

            logsSet.Verify(x => x.Add(It.Is<TestRequirementLog>(l =>
                l.UnitPrice == 12.50m &&
                l.NoRequired == 3.0 &&
                l.ProjectBuyerCode == "PBC" &&
                l.TestBuyerCode == "TBC" &&
                l.Active == 1 &&
                l.FpsYear == DefaultFpsYear)), Times.Once);
        }

        #endregion

        #region UpdateAsync

        [Fact]
        public async Task UpdateAsync_UpdatesEntityAndLog_AndReturnsEntity()
        {
            var (repo, requirementsSet, logsSet, mockContext) = CreateRepositoryWithMocks();
            var entity = new TestRequirement
            {
                TestCode = DefaultTestCode, Buyer = DefaultBuyer,
                NoRequired = 4.0, UnitPrice = 8m, Active = 1, FpsYear = DefaultFpsYear
            };

            var result = await repo.UpdateAsync(entity);

            Assert.Same(entity, result);
            requirementsSet.Verify(x => x.Update(It.Is<TestRequirement>(e => e.TestCode == DefaultTestCode)), Times.Once);
            logsSet.Verify(x => x.Add(It.Is<TestRequirementLog>(l =>
                l.TestCode == DefaultTestCode &&
                l.Buyer == DefaultBuyer &&
                l.InsertDelete == "I")), Times.Once);
            RepositoryTestHelper.VerifySaveChanges(mockContext);
        }

        #endregion

        #region DeleteAsync

        [Fact]
        public async Task DeleteAsync_WhenEntityExists_RemovesEntityAndAddsLog_ReturnsTrue()
        {
            var existing = new TestRequirement
            {
                TestCode = DefaultTestCode, Buyer = DefaultBuyer, FpsYear = DefaultFpsYear
            };
            var (repo, requirementsSet, logsSet, mockContext) = CreateRepositoryWithMocks([existing]);

            var result = await repo.DeleteAsync(DefaultTestCode, DefaultBuyer);

            Assert.True(result);
            requirementsSet.Verify(x => x.Remove(It.Is<TestRequirement>(e =>
                e.TestCode == DefaultTestCode && e.Buyer == DefaultBuyer)), Times.Once);
            logsSet.Verify(x => x.Add(It.Is<TestRequirementLog>(l =>
                l.TestCode == DefaultTestCode &&
                l.Buyer == DefaultBuyer &&
                l.InsertDelete == "D")), Times.Once);
            RepositoryTestHelper.VerifySaveChanges(mockContext);
        }

        [Fact]
        public async Task DeleteAsync_WhenEntityDoesNotExist_ReturnsFalse()
        {
            var (repo, requirementsSet, logsSet, mockContext) = CreateRepositoryWithMocks([]);

            var result = await repo.DeleteAsync(DefaultTestCode, DefaultBuyer);

            Assert.False(result);
            requirementsSet.Verify(x => x.Remove(It.IsAny<TestRequirement>()), Times.Never);
            logsSet.Verify(x => x.Add(It.IsAny<TestRequirementLog>()), Times.Never);
            RepositoryTestHelper.VerifySaveChanges(mockContext, times: 0);
        }

        [Fact]
        public async Task DeleteAsync_WhenBuyerDoesNotMatch_ReturnsFalse()
        {
            var existing = new TestRequirement
            {
                TestCode = DefaultTestCode, Buyer = "DIFFERENT_BUYER", FpsYear = DefaultFpsYear
            };
            var (repo, _, _, _) = CreateRepositoryWithMocks([existing]);

            var result = await repo.DeleteAsync(DefaultTestCode, DefaultBuyer);

            Assert.False(result);
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
