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
        private const string DefaultTestCode = "TST001";
        private const string DefaultBuyer = "B001";
        private const string DefaultProjectStatus = "Active";
        private const int DefaultFpsYear = 2024;

        private static TestSupplierRepository CreateRepository(
            IEnumerable<TestRequirement>? testRequirements = null,
            IEnumerable<Project>? projects = null,
            int fpsYear = DefaultFpsYear)
        {
            var mockContext = new Mock<IFpsRequestContext>();
            mockContext.Setup(x => x.FpsYear).Returns(fpsYear);
            mockContext.Setup(x => x.UserEmailId).Returns("test@example.com");

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

            return new TestSupplierRepository(dbContext.Object);
        }

        private static TestRequirement BuildTestRequirement(
            string testCode = DefaultTestCode,
            string buyer = DefaultBuyer,
            short active = 1,
            decimal? unitPrice = 50m,
            double? noRequired = 3) =>
            new()
            {
                TestCode = testCode,
                Buyer = buyer,
                Active = active,
                UnitPrice = unitPrice,
                NoRequired = noRequired,
                FpsYear = DefaultFpsYear
            };

        private static Project BuildProject(
            string parentProject = DefaultBuyer,
            string? manager = "MGR01",
            string projectStatus = DefaultProjectStatus) =>
            new()
            {
                ParentProject = parentProject,
                ProjectTitle = "Test Project",
                Program = "P001",
                Customer = "DEFRA",
                ProjectStatus = projectStatus,
                Manager = manager,
                FpsYear = DefaultFpsYear
            };

        #region GetPagedByTestCodeAsync Tests

        [Fact]
        public async Task GetPagedByTestCodeAsync_WithMatchingTestCode_ReturnsMatchingRows()
        {
            var requirements = new List<TestRequirement>
            {
                BuildTestRequirement(testCode: DefaultTestCode, buyer: "B001"),
                BuildTestRequirement(testCode: DefaultTestCode, buyer: "B002"),
                BuildTestRequirement(testCode: "OTHER", buyer: "B003")
            };
            var projects = new List<Project>
            {
                BuildProject("B001"),
                BuildProject("B002"),
                BuildProject("B003")
            };

            var repo = CreateRepository(requirements, projects);
            var query = new PaginationParameters<string>(page: 1, pageSize: 10);

            var result = await repo.GetPagedByTestCodeAsync(query, DefaultTestCode, showRejected: false);

            Assert.NotNull(result);
            Assert.Equal(2, result.Data.Count());
        }

        [Fact]
        public async Task GetPagedByTestCodeAsync_ShowRejectedFalse_ExcludesInactiveRows()
        {
            var requirements = new List<TestRequirement>
            {
                BuildTestRequirement(buyer: "B001", active: 1),
                BuildTestRequirement(buyer: "B002", active: 0)
            };
            var projects = new List<Project>
            {
                BuildProject("B001"),
                BuildProject("B002")
            };

            var repo = CreateRepository(requirements, projects);
            var query = new PaginationParameters<string>(page: 1, pageSize: 10);

            var result = await repo.GetPagedByTestCodeAsync(query, DefaultTestCode, showRejected: false);

            Assert.Single(result.Data);
            Assert.Equal("B001", result.Data.First().Buyer);
        }

        [Fact]
        public async Task GetPagedByTestCodeAsync_ShowRejectedTrue_IncludesInactiveRows()
        {
            var requirements = new List<TestRequirement>
            {
                BuildTestRequirement(buyer: "B001", active: 1),
                BuildTestRequirement(buyer: "B002", active: 0)
            };
            var projects = new List<Project>
            {
                BuildProject("B001"),
                BuildProject("B002")
            };

            var repo = CreateRepository(requirements, projects);
            var query = new PaginationParameters<string>(page: 1, pageSize: 10);

            var result = await repo.GetPagedByTestCodeAsync(query, DefaultTestCode, showRejected: true);

            Assert.Equal(2, result.Data.Count());
        }

        [Fact]
        public async Task GetPagedByTestCodeAsync_NoMatchingTestCode_ReturnsEmptyResult()
        {
            var requirements = new List<TestRequirement>
            {
                BuildTestRequirement(testCode: "OTHER", buyer: "B001")
            };
            var projects = new List<Project> { BuildProject("B001") };

            var repo = CreateRepository(requirements, projects);
            var query = new PaginationParameters<string>(page: 1, pageSize: 10);

            var result = await repo.GetPagedByTestCodeAsync(query, DefaultTestCode, showRejected: false);

            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetPagedByTestCodeAsync_ComputesTestCostClientSide()
        {
            var requirements = new List<TestRequirement>
            {
                BuildTestRequirement(buyer: "B001", unitPrice: 10m, noRequired: 5)
            };
            var projects = new List<Project> { BuildProject("B001") };

            var repo = CreateRepository(requirements, projects);
            var query = new PaginationParameters<string>(page: 1, pageSize: 10);

            var result = await repo.GetPagedByTestCodeAsync(query, DefaultTestCode, showRejected: false);

            var row = result.Data.Single();
            Assert.Equal(50m, row.TestCost);
        }

        [Fact]
        public async Task GetPagedByTestCodeAsync_NullUnitPrice_TestCostIsNull()
        {
            var requirements = new List<TestRequirement>
            {
                BuildTestRequirement(buyer: "B001", unitPrice: null, noRequired: 5)
            };
            var projects = new List<Project> { BuildProject("B001") };

            var repo = CreateRepository(requirements, projects);
            var query = new PaginationParameters<string>(page: 1, pageSize: 10);

            var result = await repo.GetPagedByTestCodeAsync(query, DefaultTestCode, showRejected: false);

            Assert.Null(result.Data.Single().TestCost);
        }

        [Fact]
        public async Task GetPagedByTestCodeAsync_WithPaging_ReturnsCorrectPage()
        {
            var requirements = Enumerable.Range(1, 5)
                .Select(i => BuildTestRequirement(buyer: $"B{i:D3}"))
                .ToList();
            var projects = Enumerable.Range(1, 5)
                .Select(i => BuildProject($"B{i:D3}"))
                .ToList();

            var repo = CreateRepository(requirements, projects);
            var query = new PaginationParameters<string>(page: 1, pageSize: 3);

            var result = await repo.GetPagedByTestCodeAsync(query, DefaultTestCode, showRejected: false);

            Assert.Equal(3, result.Data.Count());
            Assert.Equal(5, result.PaginationData.TotalRecords);
        }

        [Fact]
        public async Task GetPagedByTestCodeAsync_SortByBuyerDescending_OrdersCorrectly()
        {
            var requirements = new List<TestRequirement>
            {
                BuildTestRequirement(buyer: "B001"),
                BuildTestRequirement(buyer: "B003"),
                BuildTestRequirement(buyer: "B002")
            };
            var projects = new List<Project>
            {
                BuildProject("B001"),
                BuildProject("B002"),
                BuildProject("B003")
            };

            var repo = CreateRepository(requirements, projects);
            var query = new PaginationParameters<string>(sortBy: "Buyer", descending: true, page: 1, pageSize: 10);

            var result = await repo.GetPagedByTestCodeAsync(query, DefaultTestCode, showRejected: false);

            var buyers = result.Data.Select(r => r.Buyer).ToList();
            Assert.Equal("B003", buyers[0]);
            Assert.Equal("B001", buyers[2]);
        }

        [Fact]
        public async Task GetPagedByTestCodeAsync_SortByTestCostDescending_OrdersCorrectly()
        {
            var requirements = new List<TestRequirement>
            {
                BuildTestRequirement(buyer: "B001", unitPrice: 5m, noRequired: 2),
                BuildTestRequirement(buyer: "B002", unitPrice: 20m, noRequired: 3),
                BuildTestRequirement(buyer: "B003", unitPrice: 10m, noRequired: 1)
            };
            var projects = new List<Project>
            {
                BuildProject("B001"),
                BuildProject("B002"),
                BuildProject("B003")
            };

            var repo = CreateRepository(requirements, projects);
            var query = new PaginationParameters<string>(sortBy: "TestCost", descending: true, page: 1, pageSize: 10);

            var result = await repo.GetPagedByTestCodeAsync(query, DefaultTestCode, showRejected: false);

            var data = result.Data.ToList();
            Assert.Equal("B002", data[0].Buyer); // 60 = 20 * 3
            // B001 (10) and B003 (10) share the same cost — just confirm B002 is at the top
            Assert.NotEqual("B002", data[1].Buyer);
        }

        [Fact]
        public async Task GetPagedByTestCodeAsync_FilterByBuyer_ReturnsMatchingRows()
        {
            var requirements = new List<TestRequirement>
            {
                BuildTestRequirement(buyer: "ALPHA"),
                BuildTestRequirement(buyer: "BETA")
            };
            var projects = new List<Project>
            {
                BuildProject("ALPHA"),
                BuildProject("BETA")
            };

            var repo = CreateRepository(requirements, projects);
            var filterJson = "{\"Buyer\":\"ALPHA\"}";
            var query = new PaginationParameters<string> { Filter = filterJson, Page = 1, PageSize = 10 };

            var result = await repo.GetPagedByTestCodeAsync(query, DefaultTestCode, showRejected: false);

            Assert.Single(result.Data);
            Assert.Equal("ALPHA", result.Data.First().Buyer);
        }

        [Fact]
        public async Task GetPagedByTestCodeAsync_EmptyTestRequirements_ReturnsEmptyResult()
        {
            var repo = CreateRepository(
                new List<TestRequirement>(),
                new List<Project>());
            var query = new PaginationParameters<string>(page: 1, pageSize: 10);

            var result = await repo.GetPagedByTestCodeAsync(query, DefaultTestCode, showRejected: false);

            Assert.Empty(result.Data);
            Assert.Equal(0, result.PaginationData.TotalRecords);
        }

        #endregion
    }
}
