using Apha.Common.Helpers.Repository;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using Apha.FPS.DataAccess.Data;
using Apha.FPS.DataAccess.Repositories;
using Moq;
using FpsProgram = Apha.FPS.Core.Entities.Program;

namespace Apha.FPS.DataAccess.UnitTests.Repository.ProjectRepositoryTest
{
    public class ProjectProfitabilityRepositoryTests
    {
        private static ProjectRepository CreateRepository(
            IEnumerable<ProjectView>? projectViews = null,
            IEnumerable<Program>? programs = null,
            string userEmailId = "test@example.com",
            int fpsYear = 2024)
        {
            var mockRequestContext = new Mock<IFpsRequestContext>();
            mockRequestContext.Setup(x => x.UserEmailId).Returns(userEmailId);
            mockRequestContext.Setup(x => x.FpsYear).Returns(fpsYear);

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(mockRequestContext.Object);

            mockContext.Setup(x => x.ProjectViews)
                .Returns(RepositoryTestHelper.CreateMockDbSet(projectViews ?? Enumerable.Empty<ProjectView>()).Object);
            mockContext.Setup(x => x.Programs)
                .Returns(RepositoryTestHelper.CreateMockDbSet(programs ?? Enumerable.Empty<Program>()).Object);
            mockContext.Setup(x => x.Projects)
                .Returns(RepositoryTestHelper.CreateMockDbSet(Enumerable.Empty<Project>()).Object);

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

        private static ProjectView MakeView(
            string code,
            string status = "Approved",
            string program = "P001",
            decimal? budget = null,
            decimal? profit = null) => new()
        {
            ParentProject = code,
            ProjectTitle  = code,
            Program       = program,
            ProjectStatus = status,
            BudgetCvl     = budget,
            Profit        = profit,
            UserEmail     = "test@example.com"
        };

        // ── GetProjectProfitabilityAsync ──────────────────────────────────────

        [Fact]
        public async Task GetProjectProfitabilityAsync_WithNoMatchingProgram_ReturnsEmptyPage()
        {
            var repo  = CreateRepository(projectViews: new List<ProjectView>());
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetProjectProfitabilityAsync(query, "P999", "all");

            Assert.NotNull(result);
            Assert.Empty(result.Data);
            Assert.Equal(0, result.PaginationData.TotalRecords);
        }

        [Fact]
        public async Task GetProjectProfitabilityAsync_WithValidProgram_ReturnsProjectsForProgram()
        {
            var projectViews = new List<ProjectView>
            {
                MakeView("PP001", program: "P001"),
                MakeView("PP002", program: "P001"),
                MakeView("PP003", program: "P002")
            };
            var programs = new List<Program>
            {
                new() { ProgramNo = "P001", Target = 10000m },
                new() { ProgramNo = "P002", Target = 15000m }
            };
            var repo  = CreateRepository(projectViews, programs);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetProjectProfitabilityAsync(query, "P001", "all");

            Assert.Equal(2, result.Data.Count());
            Assert.All(result.Data, item => Assert.Equal("P001", item.ProgramNo));
        }

        [Fact]
        public async Task GetProjectProfitabilityAsync_WorkTypeFilter_Approved_FiltersCorrectly()
        {
            var projectViews = new List<ProjectView>
            {
                MakeView("PP001", status: "Approved",     program: "P001"),
                MakeView("PP002", status: "Not Approved", program: "P001")
            };
            var programs = new List<Program> { new() { ProgramNo = "P001", Target = 10000m } };
            var repo  = CreateRepository(projectViews, programs);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetProjectProfitabilityAsync(query, "P001", "approved");

            Assert.Single(result.Data);
            Assert.Equal("PP001", result.Data.First().JobCode);
        }

        [Fact]
        public async Task GetProjectProfitabilityAsync_WorkTypeFilter_NotApproved_FiltersCorrectly()
        {
            var projectViews = new List<ProjectView>
            {
                MakeView("PP001", status: "Approved",     program: "P001"),
                MakeView("PP002", status: "Not Approved", program: "P001")
            };
            var programs = new List<Program> { new() { ProgramNo = "P001", Target = 10000m } };
            var repo  = CreateRepository(projectViews, programs);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetProjectProfitabilityAsync(query, "P001", "not-approved");

            Assert.Single(result.Data);
            Assert.Equal("PP002", result.Data.First().JobCode);
        }

        [Fact]
        public async Task GetProjectProfitabilityAsync_ProgrammeTargetIsMappedFromProgram()
        {
            var projectViews = new List<ProjectView>
            {
                MakeView("PP001", program: "P001", budget: 5000m, profit: 500m)
            };
            var programs = new List<Program> { new() { ProgramNo = "P001", Target = 12000m } };
            var repo  = CreateRepository(projectViews, programs);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetProjectProfitabilityAsync(query, "P001", "all");

            var item = Assert.Single(result.Data);
            Assert.Equal(12000m, item.ProgrammeTarget);
        }

        [Fact]
        public async Task GetProjectProfitabilityAsync_PagingIsApplied()
        {
            var projectViews = Enumerable.Range(1, 5).Select(i =>
                MakeView($"PP00{i}", program: "P001")).ToList();
            var programs = new List<Program> { new() { ProgramNo = "P001", Target = 10000m } };
            var repo  = CreateRepository(projectViews, programs);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 2 };

            var result = await repo.GetProjectProfitabilityAsync(query, "P001", "all");

            Assert.Equal(2, result.Data.Count());
            Assert.Equal(5, result.PaginationData.TotalRecords);
        }

        // ── ApplyProfitabilityFilter — JobCode branch ─────────────────────────

        [Fact]
        public async Task GetProjectProfitabilityAsync_FilterByJobCode_ReturnsMatchingProjects()
        {
            var projectViews = new List<ProjectView>
            {
                MakeView("PP001", program: "P001"),
                MakeView("PP002", program: "P001"),
                MakeView("XX003", program: "P001")
            };
            var programs = new List<Program> { new() { ProgramNo = "P001", Target = 10000m } };
            var repo  = CreateRepository(projectViews, programs);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                Filter = "{\"JobCode\":\"PP\"}"
            };

            var result = await repo.GetProjectProfitabilityAsync(query, "P001", "all");

            Assert.Equal(2, result.PaginationData.TotalRecords);
            Assert.All(result.Data, item => Assert.Contains("PP", item.JobCode));
        }

        // ── ApplyProfitabilityFilter — ProjectStatus branch ───────────────────

        [Fact]
        public async Task GetProjectProfitabilityAsync_FilterByProjectStatus_ReturnsMatchingProjects()
        {
            var projectViews = new List<ProjectView>
            {
                MakeView("PP001", status: "Completed", program: "P001"),
                MakeView("PP002", status: "Pending",   program: "P001"),
                MakeView("PP003", status: "Completed", program: "P001")
            };
            var programs = new List<Program> { new() { ProgramNo = "P001", Target = 10000m } };
            var repo  = CreateRepository(projectViews, programs);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                Filter = "{\"ProjectStatus\":\"Completed\"}"
            };

            var result = await repo.GetProjectProfitabilityAsync(query, "P001", "all");

            Assert.Equal(2, result.PaginationData.TotalRecords);
            Assert.All(result.Data, item => Assert.Equal("Completed", item.ProjectStatus));
        }

        // ── ApplyProfitabilityFilter — null/empty filter (early-exit branch) ──

        [Fact]
        public async Task GetProjectProfitabilityAsync_NullFilter_ReturnsAllProjects()
        {
            var projectViews = new List<ProjectView>
            {
                MakeView("PP001", program: "P001"),
                MakeView("PP002", program: "P001")
            };
            var programs = new List<Program> { new() { ProgramNo = "P001", Target = 10000m } };
            var repo  = CreateRepository(projectViews, programs);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = null };

            var result = await repo.GetProjectProfitabilityAsync(query, "P001", "all");

            Assert.Equal(2, result.PaginationData.TotalRecords);
        }

        // ── Sorting ───────────────────────────────────────────────────────────

        [Fact]
        public async Task GetProjectProfitabilityAsync_DefaultSort_OrdersByJobCodeAscending()
        {
            var projectViews = new List<ProjectView>
            {
                MakeView("CC003", program: "P001"),
                MakeView("AA001", program: "P001"),
                MakeView("BB002", program: "P001")
            };
            var programs = new List<Program> { new() { ProgramNo = "P001", Target = 10000m } };
            var repo  = CreateRepository(projectViews, programs);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetProjectProfitabilityAsync(query, "P001", "all");

            var data = result.Data.ToList();
            Assert.Equal("AA001", data[0].JobCode);
            Assert.Equal("BB002", data[1].JobCode);
            Assert.Equal("CC003", data[2].JobCode);
        }

        [Fact]
        public async Task GetProjectProfitabilityAsync_SortByTotalCostsDescending()
        {
            var projectViews = new List<ProjectView>
            {
                MakeView("PP001", program: "P001", budget: 1000m),
                MakeView("PP002", program: "P001", budget: 3000m),
                MakeView("PP003", program: "P001", budget: 2000m)
            };
            var programs = new List<Program> { new() { ProgramNo = "P001", Target = 10000m } };
            var repo  = CreateRepository(projectViews, programs);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                SortBy = "budgetcvl",
                Descending = true
            };

            var result = await repo.GetProjectProfitabilityAsync(query, "P001", "all");

            var data = result.Data.ToList();
            Assert.Equal("PP002", data[0].JobCode);
            Assert.Equal("PP003", data[1].JobCode);
            Assert.Equal("PP001", data[2].JobCode);
        }

        [Fact]
        public async Task GetProjectProfitabilityAsync_SortByProjectStatus_Ascending()
        {
            var projectViews = new List<ProjectView>
            {
                MakeView("PP001", status: "Not Approved", program: "P001"),
                MakeView("PP002", status: "Approved",     program: "P001")
            };
            var programs = new List<Program> { new() { ProgramNo = "P001", Target = 10000m } };
            var repo  = CreateRepository(projectViews, programs);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                SortBy = "projectstatus",
                Descending = false
            };

            var result = await repo.GetProjectProfitabilityAsync(query, "P001", "all");

            var data = result.Data.ToList();
            Assert.Equal("Approved", data[0].ProjectStatus);
            Assert.Equal("Not Approved", data[1].ProjectStatus);
        }

        [Theory]
        [InlineData("jobcode")]
        [InlineData("totalcosts")]
        [InlineData("budgetcvl")]
        [InlineData("jcprofit")]
        [InlineData("offtarget")]
        [InlineData("projectstatus")]
        [InlineData("jctotalstaffcosts")]
        [InlineData("jctotaltestcosts")]
        [InlineData("jctotalanimalcosts")]
        [InlineData("jctotaladditionalcosts")]
        [InlineData("targetprofit")]
        [InlineData("unknownkey")]
        public async Task GetProjectProfitabilityAsync_AllSortKeys_DoNotThrowAndReturnSameCount(string sortBy)
        {
            var projectViews = Enumerable.Range(1, 3)
                .Select(i => MakeView($"PP00{i}", program: "P001", budget: i * 1000m, profit: i * 100m))
                .ToList();
            var programs = new List<Program> { new() { ProgramNo = "P001", Target = 5000m } };
            var repo  = CreateRepository(projectViews, programs);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                SortBy = sortBy, Descending = false
            };

            var result = await repo.GetProjectProfitabilityAsync(query, "P001", "all");

            Assert.Equal(3, result.Data.Count());
        }

        [Fact]
        public async Task GetProjectProfitabilityAsync_EmptyFilter_ReturnsAllProjects()
        {
            var projectViews = new List<ProjectView>
            {
                MakeView("PP001", program: "P001"),
                MakeView("PP002", program: "P001")
            };
            var programs = new List<Program> { new() { ProgramNo = "P001", Target = 10000m } };
            var repo  = CreateRepository(projectViews, programs);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = string.Empty };

            var result = await repo.GetProjectProfitabilityAsync(query, "P001", "all");

            Assert.Equal(2, result.PaginationData.TotalRecords);
        }
    }
}
