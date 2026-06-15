using Apha.Common.Contracts.FPS;
using Apha.Common.Helpers.Repository;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using Apha.FPS.DataAccess.Data;
using Apha.FPS.DataAccess.Repositories;
using Moq;
using Xunit;

namespace Apha.FPS.DataAccess.UnitTests.Repository.ProjectRepositoryTest
{
    public class ProjectProfitabilityVlaRepositoryTests
    {
        // TRANSFORMENGINE: factory method — only ProjectProfitabilityVlaViews DbSet needed
        //   for VLA tests; other DbSets left as Moq defaults (empty queryable).
        private static ProjectRepository CreateRepository(
            IEnumerable<ProjectProfitabilityVlaView>? vlaViews = null,
            string userEmailId = "test@example.com",
            int fpsYear = 2024)
        {
            var mockRequestContext = new Mock<IFpsRequestContext>();
            mockRequestContext.Setup(x => x.UserEmailId).Returns(userEmailId);
            mockRequestContext.Setup(x => x.FpsYear).Returns(fpsYear);

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(mockRequestContext.Object);

            var mockSet = RepositoryTestHelper.CreateMockDbSet(vlaViews ?? Enumerable.Empty<ProjectProfitabilityVlaView>());
            mockContext.Setup(x => x.ProjectProfitabilityVlaViews).Returns(mockSet.Object);

            return new ProjectRepository(mockContext.Object, mockRequestContext.Object);
        }

        #region GetProjectProfitabilityVlaAsync

        [Fact]
        public async Task GetProjectProfitabilityVlaAsync_WithEmptyView_ReturnsEmptyPage()
        {
            // Arrange
            var repo = CreateRepository(vlaViews: new List<ProjectProfitabilityVlaView>());
            var query = new PaginationParameters<ProjectProfitabilityVlaReq> { Page = 1, PageSize = 15 };

            // Act
            var result = await repo.GetProjectProfitabilityVlaAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Data);
            Assert.Equal(0, result.PaginationData.TotalRecords);
        }

        [Fact]
        public async Task GetProjectProfitabilityVlaAsync_WithData_ReturnsAllRowsWhenNoFilter()
        {
            // Arrange
            var views = new List<ProjectProfitabilityVlaView>
            {
                new() { JobCode = "PP001", Status = "Approved",     Program = "P001", Manager = "John",  Customer = "ACME",  StaffCosts = 1000m, Budget = 5000m, Profit = 4000m, TargetProfit = 3500m, OffTarget = 500m },
                new() { JobCode = "PP002", Status = "Completed",    Program = "P002", Manager = "Jane",  Customer = "Beta",  StaffCosts = 2000m, Budget = 6000m, Profit = 4000m, TargetProfit = 3000m, OffTarget = 1000m },
                new() { JobCode = "PP003", Status = "Not Approved", Program = "P001", Manager = "John",  Customer = "Gamma", StaffCosts = 500m,  Budget = 2000m, Profit = 1500m, TargetProfit = 2000m, OffTarget = -500m }
            };
            var repo = CreateRepository(vlaViews: views);
            var query = new PaginationParameters<ProjectProfitabilityVlaReq> { Page = 1, PageSize = 15 };

            // Act
            var result = await repo.GetProjectProfitabilityVlaAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Data.Count());
            Assert.Equal(3, result.PaginationData.TotalRecords);
        }

        [Fact]
        public async Task GetProjectProfitabilityVlaAsync_WithProjectStatusFilter_FiltersOnStatusField()
        {
            // Arrange
            var views = new List<ProjectProfitabilityVlaView>
            {
                new() { JobCode = "PP001", Status = "Approved",  Program = "P001", StaffCosts = 1000m },
                new() { JobCode = "PP002", Status = "Completed", Program = "P001", StaffCosts = 2000m },
                new() { JobCode = "PP003", Status = "Approved",  Program = "P002", StaffCosts = 500m }
            };
            var repo = CreateRepository(vlaViews: views);
            var query = new PaginationParameters<ProjectProfitabilityVlaReq>
            {
                Page = 1,
                PageSize = 15,
                Filter = new ProjectProfitabilityVlaReq { ProjectStatus = "Approved" }
            };

            // Act
            var result = await repo.GetProjectProfitabilityVlaAsync(query);

            // Assert — only Approved rows returned
            Assert.Equal(2, result.Data.Count());
            Assert.All(result.Data, v => Assert.Equal("Approved", v.Status));
        }

        [Fact]
        public async Task GetProjectProfitabilityVlaAsync_WithProgramNoFilter_FiltersOnProgramField()
        {
            // Arrange
            var views = new List<ProjectProfitabilityVlaView>
            {
                new() { JobCode = "PP001", Program = "P001", Status = "Approved", StaffCosts = 1000m },
                new() { JobCode = "PP002", Program = "P002", Status = "Approved", StaffCosts = 2000m },
                new() { JobCode = "PP003", Program = "P001", Status = "Completed", StaffCosts = 500m }
            };
            var repo = CreateRepository(vlaViews: views);
            var query = new PaginationParameters<ProjectProfitabilityVlaReq>
            {
                Page = 1,
                PageSize = 15,
                Filter = new ProjectProfitabilityVlaReq { ProgramNo = "P001" }
            };

            // Act
            var result = await repo.GetProjectProfitabilityVlaAsync(query);

            // Assert — only P001 rows returned
            Assert.Equal(2, result.Data.Count());
            Assert.All(result.Data, v => Assert.Equal("P001", v.Program));
        }

        [Fact]
        public async Task GetProjectProfitabilityVlaAsync_WithManagerFilter_FiltersOnManagerField()
        {
            // Arrange
            var views = new List<ProjectProfitabilityVlaView>
            {
                new() { JobCode = "PP001", Manager = "John Smith", Status = "Approved", StaffCosts = 1000m },
                new() { JobCode = "PP002", Manager = "Jane Doe",   Status = "Approved", StaffCosts = 2000m },
                new() { JobCode = "PP003", Manager = "John Smith", Status = "Completed", StaffCosts = 500m }
            };
            var repo = CreateRepository(vlaViews: views);
            var query = new PaginationParameters<ProjectProfitabilityVlaReq>
            {
                Page = 1,
                PageSize = 15,
                Filter = new ProjectProfitabilityVlaReq { Manager = "John Smith" }
            };

            // Act
            var result = await repo.GetProjectProfitabilityVlaAsync(query);

            // Assert — VLA-specific manager filter dimension
            Assert.Equal(2, result.Data.Count());
            Assert.All(result.Data, v => Assert.Equal("John Smith", v.Manager));
        }

        [Fact]
        public async Task GetProjectProfitabilityVlaAsync_WithCustomerFilter_FiltersOnCustomerField()
        {
            // Arrange
            var views = new List<ProjectProfitabilityVlaView>
            {
                new() { JobCode = "PP001", Customer = "ACME Ltd", Status = "Approved", StaffCosts = 1000m },
                new() { JobCode = "PP002", Customer = "Beta Corp", Status = "Approved", StaffCosts = 2000m },
                new() { JobCode = "PP003", Customer = "ACME Ltd", Status = "Completed", StaffCosts = 500m }
            };
            var repo = CreateRepository(vlaViews: views);
            var query = new PaginationParameters<ProjectProfitabilityVlaReq>
            {
                Page = 1,
                PageSize = 15,
                Filter = new ProjectProfitabilityVlaReq { Customer = "ACME Ltd" }
            };

            // Act
            var result = await repo.GetProjectProfitabilityVlaAsync(query);

            // Assert — VLA-specific customer filter dimension
            Assert.Equal(2, result.Data.Count());
            Assert.All(result.Data, v => Assert.Equal("ACME Ltd", v.Customer));
        }

        [Fact]
        public async Task GetProjectProfitabilityVlaAsync_PagingIsApplied()
        {
            // Arrange
            var views = Enumerable.Range(1, 10).Select(i => new ProjectProfitabilityVlaView
            {
                JobCode  = $"PP{i:D3}",
                Status   = "Approved",
                Program  = "P001",
                StaffCosts = i * 100m
            }).ToList();

            var repo = CreateRepository(vlaViews: views);
            var query = new PaginationParameters<ProjectProfitabilityVlaReq> { Page = 1, PageSize = 3 };

            // Act
            var result = await repo.GetProjectProfitabilityVlaAsync(query);

            // Assert
            Assert.Equal(3, result.Data.Count());
            Assert.Equal(10, result.PaginationData.TotalRecords);
        }

        [Fact]
        public async Task GetProjectProfitabilityVlaAsync_DefaultSort_OrdersByJobCodeAscending()
        {
            // Arrange — JobCodes inserted out of order
            var views = new List<ProjectProfitabilityVlaView>
            {
                new() { JobCode = "PP003", Status = "Approved", StaffCosts = 300m },
                new() { JobCode = "PP001", Status = "Approved", StaffCosts = 100m },
                new() { JobCode = "PP002", Status = "Approved", StaffCosts = 200m }
            };
            var repo = CreateRepository(vlaViews: views);
            var query = new PaginationParameters<ProjectProfitabilityVlaReq>
            {
                Page = 1,
                PageSize = 15,
                SortBy = null   // no explicit sort — defaults to JobCode ascending
            };

            // Act
            var result = await repo.GetProjectProfitabilityVlaAsync(query);

            // Assert
            var data = result.Data.ToList();
            Assert.Equal("PP001", data[0].JobCode);
            Assert.Equal("PP002", data[1].JobCode);
            Assert.Equal("PP003", data[2].JobCode);
        }

        [Fact]
        public async Task GetProjectProfitabilityVlaAsync_WithSingleRow_ReturnsThatRow()
        {
            // Arrange
            var views = new List<ProjectProfitabilityVlaView>
            {
                new()
                {
                    JobCode      = "PP001",
                    Status       = "Approved",
                    Program      = "P001",
                    Manager      = "John Smith",
                    Customer     = "ACME Ltd",
                    StaffCosts   = 1500m,
                    TestCost     = 200m,
                    AnimalCosts  = 300m,
                    AdditionalCosts = 100m,
                    TotalCosts   = 2100m,
                    Budget       = 5000m,
                    Profit       = 2900m,
                    TargetProfit = 3000m,
                    OffTarget    = -100m
                }
            };
            var repo = CreateRepository(vlaViews: views);
            var query = new PaginationParameters<ProjectProfitabilityVlaReq> { Page = 1, PageSize = 15 };

            // Act
            var result = await repo.GetProjectProfitabilityVlaAsync(query);

            // Assert
            Assert.Single(result.Data);
            var row = result.Data.First();
            Assert.Equal("PP001", row.JobCode);
            Assert.Equal(1500m, row.StaffCosts);
            Assert.Equal(5000m, row.Budget);
            Assert.Equal(-100m, row.OffTarget);
        }

        #endregion
    }
}
