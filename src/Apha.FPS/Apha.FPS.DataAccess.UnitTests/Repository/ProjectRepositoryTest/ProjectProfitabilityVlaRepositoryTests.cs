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
        /// <summary>
        /// Creates a <see cref="ProjectRepository"/> with mocked DbSets.
        /// Projects and Programs drive filter/sort/page behaviour.
        /// All cost tables (StaffJobs, TestRequirements, etc.) are empty so every
        /// computed cost field is 0 — keeping filter tests focused on metadata.
        /// </summary>
        private static ProjectRepository CreateRepository(
            IEnumerable<Project>? projects = null,
            IEnumerable<Program>? programs = null,
            string userEmailId = "test@example.com",
            int fpsYear = 2024)
        {
            var mockRequestContext = new Mock<IFpsRequestContext>();
            mockRequestContext.Setup(x => x.UserEmailId).Returns(userEmailId);
            mockRequestContext.Setup(x => x.FpsYear).Returns(fpsYear);

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(mockRequestContext.Object);

            mockContext.Setup(x => x.Projects)
                .Returns(RepositoryTestHelper.CreateMockDbSet(projects ?? Enumerable.Empty<Project>()).Object);
            mockContext.Setup(x => x.Programs)
                .Returns(RepositoryTestHelper.CreateMockDbSet(programs ?? Enumerable.Empty<Program>()).Object);

            // Empty cost tables — all computed costs will be 0 in these tests
            mockContext.Setup(x => x.StaffJobs)
                .Returns(RepositoryTestHelper.CreateMockDbSet(Enumerable.Empty<StaffJob>()).Object);
            mockContext.Setup(x => x.WorkGroupEmployees)
                .Returns(RepositoryTestHelper.CreateMockDbSet(Enumerable.Empty<WorkGroupEmployee>()).Object);
            mockContext.Setup(x => x.WorkgroupGrades)
                .Returns(RepositoryTestHelper.CreateMockDbSet(Enumerable.Empty<WorkgroupGrade>()).Object);
            mockContext.Setup(x => x.ProfitCentreGrades)
                .Returns(RepositoryTestHelper.CreateMockDbSet(Enumerable.Empty<ProfitCentreGrade>()).Object);
            mockContext.Setup(x => x.AdditionalCosts)
                .Returns(RepositoryTestHelper.CreateMockDbSet(Enumerable.Empty<AdditionalCost>()).Object);
            mockContext.Setup(x => x.TestRequirements)
                .Returns(RepositoryTestHelper.CreateMockDbSet(Enumerable.Empty<TestRequirement>()).Object);
            mockContext.Setup(x => x.AnimalRequests)
                .Returns(RepositoryTestHelper.CreateMockDbSet(Enumerable.Empty<AnimalRequest>()).Object);
            mockContext.Setup(x => x.Animals)
                .Returns(RepositoryTestHelper.CreateMockDbSet(Enumerable.Empty<Animal>()).Object);

            return new ProjectRepository(mockContext.Object, mockRequestContext.Object);
        }

        private static Project MakeProject(
            string code,
            string status = "Approved",
            string program = "P001",
            string customer = "ACME",
            decimal? budget = null) => new()
        {
            ParentProject  = code,
            ProjectTitle   = code,
            Program        = program,
            Customer       = customer,
            ProjectStatus  = status,
            BudgetCvl      = budget,
            Disease        = string.Empty,
            Contract       = string.Empty,
            IsDefraProject = 0,
            FpsYear        = 2024
        };

        private static Program MakeProgram(
            string no,
            string? manager = null,
            decimal? target = null) => new()
        {
            ProgramNo  = no,
            Manager    = manager,
            Target     = target,
            SectorName = "charge",
            FpsYear    = 2024
        };

        #region GetProjectProfitabilityVlaAsync

        [Fact]
        public async Task GetProjectProfitabilityVlaAsync_WithEmptyProjects_ReturnsEmptyPage()
        {
            var repo = CreateRepository(projects: new List<Project>());
            var query = new PaginationParameters<string> { Page = 1, PageSize = 15 };

            var result = await repo.GetProjectProfitabilityVlaAsync(query);

            Assert.NotNull(result);
            Assert.Empty(result.Data);
            Assert.Equal(0, result.PaginationData.TotalRecords);
        }

        [Fact]
        public async Task GetProjectProfitabilityVlaAsync_WithData_ReturnsAllRowsWhenNoFilter()
        {
            var projects = new List<Project>
            {
                MakeProject("PP001", "Approved",     "P001", "ACME"),
                MakeProject("PP002", "Completed",    "P002", "Beta"),
                MakeProject("PP003", "Not Approved", "P001", "Gamma")
            };
            var programs = new List<Program>
            {
                MakeProgram("P001", "John"),
                MakeProgram("P002", "Jane")
            };
            var repo = CreateRepository(projects, programs);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 15 };

            var result = await repo.GetProjectProfitabilityVlaAsync(query);

            Assert.Equal(3, result.Data.Count());
            Assert.Equal(3, result.PaginationData.TotalRecords);
        }

        [Fact]
        public async Task GetProjectProfitabilityVlaAsync_WithProjectStatusFilter_FiltersOnStatusField()
        {
            var projects = new List<Project>
            {
                MakeProject("PP001", "Approved",  "P001"),
                MakeProject("PP002", "Completed", "P001"),
                MakeProject("PP003", "Approved",  "P002")
            };
            var repo = CreateRepository(projects);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 15 };

            var result = await repo.GetProjectProfitabilityVlaAsync(query, projectStatus: "Approved");

            Assert.Equal(2, result.Data.Count());
            Assert.All(result.Data, v => Assert.Equal("Approved", v.Status));
        }

        [Fact]
        public async Task GetProjectProfitabilityVlaAsync_WithProgramNoFilter_FiltersOnProgramField()
        {
            var projects = new List<Project>
            {
                MakeProject("PP001", program: "P001"),
                MakeProject("PP002", program: "P002"),
                MakeProject("PP003", program: "P001")
            };
            var repo = CreateRepository(projects);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 15 };

            var result = await repo.GetProjectProfitabilityVlaAsync(query, programNo: "P001");

            Assert.Equal(2, result.Data.Count());
            Assert.All(result.Data, v => Assert.Equal("P001", v.Program));
        }

        [Fact]
        public async Task GetProjectProfitabilityVlaAsync_WithManagerFilter_FiltersOnManagerField()
        {
            var projects = new List<Project>
            {
                MakeProject("PP001", program: "P001"),
                MakeProject("PP002", program: "P002"),
                MakeProject("PP003", program: "P001")
            };
            var programs = new List<Program>
            {
                MakeProgram("P001", manager: "John Smith"),
                MakeProgram("P002", manager: "Jane Doe")
            };
            var repo = CreateRepository(projects, programs);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 15 };

            var result = await repo.GetProjectProfitabilityVlaAsync(query, manager: "John Smith");

            Assert.Equal(2, result.Data.Count());
            Assert.All(result.Data, v => Assert.Equal("John Smith", v.Manager));
        }

        [Fact]
        public async Task GetProjectProfitabilityVlaAsync_WithCustomerFilter_FiltersOnCustomerField()
        {
            var projects = new List<Project>
            {
                MakeProject("PP001", customer: "ACME Ltd"),
                MakeProject("PP002", customer: "Beta Corp"),
                MakeProject("PP003", customer: "ACME Ltd")
            };
            var repo = CreateRepository(projects);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 15 };

            var result = await repo.GetProjectProfitabilityVlaAsync(query, customer: "ACME Ltd");

            Assert.Equal(2, result.Data.Count());
            Assert.All(result.Data, v => Assert.Equal("ACME Ltd", v.Customer));
        }

        [Fact]
        public async Task GetProjectProfitabilityVlaAsync_PagingIsApplied()
        {
            var projects = Enumerable.Range(1, 10)
                .Select(i => MakeProject($"PP{i:D3}", program: "P001"))
                .ToList();
            var repo = CreateRepository(projects);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 3 };

            var result = await repo.GetProjectProfitabilityVlaAsync(query);

            Assert.Equal(3, result.Data.Count());
            Assert.Equal(10, result.PaginationData.TotalRecords);
        }

        [Fact]
        public async Task GetProjectProfitabilityVlaAsync_DefaultSort_OrdersByJobCodeAscending()
        {
            var projects = new List<Project>
            {
                MakeProject("PP003"),
                MakeProject("PP001"),
                MakeProject("PP002")
            };
            var repo = CreateRepository(projects);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 15,
                SortBy = null
            };

            var result = await repo.GetProjectProfitabilityVlaAsync(query);

            var data = result.Data.ToList();
            Assert.Equal("PP001", data[0].JobCode);
            Assert.Equal("PP002", data[1].JobCode);
            Assert.Equal("PP003", data[2].JobCode);
        }

        [Fact]
        public async Task GetProjectProfitabilityVlaAsync_WithSingleRow_ReturnsComputedFinancials()
        {
            // With no cost-table data, all cost fields are 0.
            // Profit = BudgetCvl - TotalCosts = 5000 - 0 = 5000.
            // TargetProfit comes from Program.Target (not Project.Profit).
            // OffTarget = Profit - TargetProfit = 5000 - 3000 = 2000.
            var projects = new List<Project>
            {
                new()
                {
                    ParentProject  = "PP001",
                    ProjectTitle   = "Test Project",
                    Program        = "P001",
                    Customer       = "ACME Ltd",
                    ProjectStatus  = "Approved",
                    BudgetCvl      = 5000m,
                    Disease        = string.Empty,
                    Contract       = string.Empty,
                    IsDefraProject = 0,
                    FpsYear        = 2024
                }
            };
            var programs = new List<Program>
            {
                MakeProgram("P001", manager: "John Smith", target: 3000m)
            };
            var repo = CreateRepository(projects, programs);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 15 };

            var result = await repo.GetProjectProfitabilityVlaAsync(query);

            Assert.Single(result.Data);
            var row = result.Data.First();
            Assert.Equal("PP001",       row.JobCode);
            Assert.Equal("ACME Ltd",    row.Customer);
            Assert.Equal("John Smith",  row.Manager);
            Assert.Equal("Approved",    row.Status);
            Assert.Equal(5000m,         row.Budget);
            Assert.Equal(0m,            row.StaffCosts);
            Assert.Equal(0m,            row.TotalCosts);
            Assert.Equal(5000m,         row.Profit);       // Budget - TotalCosts
            Assert.Equal(3000m,         row.TargetProfit); // Programme.Target
            Assert.Equal(2000m,         row.OffTarget);    // Profit - TargetProfit
        }

        #endregion
    }
}
