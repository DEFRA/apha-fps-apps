using Apha.Common.Helpers.Repository;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using Apha.FPS.DataAccess.Data;
using Apha.FPS.DataAccess.Repositories;
using Moq;
using FpsProgram = Apha.FPS.Core.Entities.Program;
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

        #region Search and Filter (query.Search / query.Filter)

        [Fact]
        public async Task GetProjectProfitabilityVlaAsync_WithSearchTerm_FiltersOnParentProject()
        {
            var projects = new List<Project>
            {
                MakeProject("ALPHA01"),
                MakeProject("BETA001"),
                MakeProject("ALPHA02")
            };
            var repo = CreateRepository(projects);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 15, Search = "ALPHA" };

            var result = await repo.GetProjectProfitabilityVlaAsync(query);

            Assert.Equal(2, result.Data.Count());
            Assert.All(result.Data, v => Assert.Contains("ALPHA", v.JobCode));
        }

        [Fact]
        public async Task GetProjectProfitabilityVlaAsync_WithFilterJobCode_FiltersOnParentProject()
        {
            var projects = new List<Project>
            {
                MakeProject("PP001"),
                MakeProject("PP002"),
                MakeProject("XQ003")
            };
            var repo = CreateRepository(projects);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 15,
                Filter = """{"JobCode":"PP"}"""
            };

            var result = await repo.GetProjectProfitabilityVlaAsync(query);

            Assert.Equal(2, result.Data.Count());
            Assert.All(result.Data, v => Assert.StartsWith("PP", v.JobCode));
        }

        [Fact]
        public async Task GetProjectProfitabilityVlaAsync_WithFilterParentProject_FiltersOnParentProject()
        {
            var projects = new List<Project>
            {
                MakeProject("PP001"),
                MakeProject("PP002"),
                MakeProject("XQ003")
            };
            var repo = CreateRepository(projects);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 15,
                Filter = """{"ParentProject":"PP"}"""
            };

            var result = await repo.GetProjectProfitabilityVlaAsync(query);

            Assert.Equal(2, result.Data.Count());
        }

        [Fact]
        public async Task GetProjectProfitabilityVlaAsync_WithEmptyFilter_ReturnsAllRows()
        {
            var projects = new List<Project>
            {
                MakeProject("PP001"),
                MakeProject("PP002")
            };
            var repo = CreateRepository(projects);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 15, Filter = "" };

            var result = await repo.GetProjectProfitabilityVlaAsync(query);

            Assert.Equal(2, result.Data.Count());
        }

        #endregion

        #region Financials — null budget / null target / no matching program

        [Fact]
        public async Task GetProjectProfitabilityVlaAsync_NullBudget_TreatedAsZero()
        {
            var projects = new List<Project>
            {
                MakeProject("PP001", budget: null)
            };
            var repo = CreateRepository(projects);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 15 };

            var result = await repo.GetProjectProfitabilityVlaAsync(query);

            var row = result.Data.First();
            Assert.Equal(0m, row.Budget ?? 0m);
            Assert.Equal(0m, row.Profit);    // 0 - 0
            Assert.Equal(0m, row.OffTarget); // 0 - 0
        }

        [Fact]
        public async Task GetProjectProfitabilityVlaAsync_NullProgrammeTarget_TreatedAsZero()
        {
            var projects = new List<Project> { MakeProject("PP001", budget: 1000m) };
            var programs = new List<Program> { MakeProgram("P001", target: null) };
            var repo = CreateRepository(projects, programs);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 15 };

            var result = await repo.GetProjectProfitabilityVlaAsync(query);

            var row = result.Data.First();
            Assert.Equal(0m, row.TargetProfit);
            Assert.Equal(1000m, row.OffTarget); // profit(1000) - target(0)
        }

        [Fact]
        public async Task GetProjectProfitabilityVlaAsync_ProjectWithNoMatchingProgram_ManagerAndTargetAreDefault()
        {
            // Project refers to P999 which has no entry in Programs → pg is null
            var projects = new List<Project> { MakeProject("PP001", program: "P999", budget: 500m) };
            var repo = CreateRepository(projects, programs: new List<Program>());
            var query = new PaginationParameters<string> { Page = 1, PageSize = 15 };

            var result = await repo.GetProjectProfitabilityVlaAsync(query);

            var row = result.Data.First();
            Assert.Null(row.Manager);
            Assert.Equal(0m, row.TargetProfit);
            Assert.Equal(500m, row.Profit);
        }

        #endregion

        #region ApplyVlaSorting — all sort keys

        [Theory]
        [InlineData("jobcode",         false)]
        [InlineData("jobcode",         true)]
        [InlineData("program",         false)]
        [InlineData("program",         true)]
        [InlineData("customer",        false)]
        [InlineData("customer",        true)]
        [InlineData("manager",         false)]
        [InlineData("manager",         true)]
        [InlineData("status",          false)]
        [InlineData("status",          true)]
        [InlineData("staffcosts",      false)]
        [InlineData("staffcosts",      true)]
        [InlineData("testcost",        false)]
        [InlineData("testcost",        true)]
        [InlineData("animalcosts",     false)]
        [InlineData("animalcosts",     true)]
        [InlineData("additionalcosts", false)]
        [InlineData("additionalcosts", true)]
        [InlineData("totalcosts",      false)]
        [InlineData("totalcosts",      true)]
        [InlineData("budget",          false)]
        [InlineData("budget",          true)]
        [InlineData("profit",          false)]
        [InlineData("profit",          true)]
        [InlineData("targetprofit",    false)]
        [InlineData("targetprofit",    true)]
        [InlineData("offtarget",       false)]
        [InlineData("offtarget",       true)]
        public async Task GetProjectProfitabilityVlaAsync_SortKey_DoesNotThrowAndReturnsSameCount(
            string sortBy, bool descending)
        {
            var projects = new List<Project>
            {
                MakeProject("PP001", budget: 1000m),
                MakeProject("PP002", budget: 2000m),
                MakeProject("PP003", budget: 3000m)
            };
            var repo = CreateRepository(projects);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 15,
                SortBy = sortBy,
                Descending = descending
            };

            var result = await repo.GetProjectProfitabilityVlaAsync(query);

            Assert.Equal(3, result.Data.Count());
        }

        [Fact]
        public async Task GetProjectProfitabilityVlaAsync_SortByJobcodeAscending_OrdersCorrectly()
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
                SortBy = "jobcode",
                Descending = false
            };

            var result = await repo.GetProjectProfitabilityVlaAsync(query);

            var codes = result.Data.Select(v => v.JobCode).ToList();
            Assert.Equal(new[] { "PP001", "PP002", "PP003" }, codes);
        }

        [Fact]
        public async Task GetProjectProfitabilityVlaAsync_SortByJobcodeDescending_OrdersCorrectly()
        {
            var projects = new List<Project>
            {
                MakeProject("PP001"),
                MakeProject("PP003"),
                MakeProject("PP002")
            };
            var repo = CreateRepository(projects);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 15,
                SortBy = "jobcode",
                Descending = true
            };

            var result = await repo.GetProjectProfitabilityVlaAsync(query);

            var codes = result.Data.Select(v => v.JobCode).ToList();
            Assert.Equal(new[] { "PP003", "PP002", "PP001" }, codes);
        }

        [Fact]
        public async Task GetProjectProfitabilityVlaAsync_SortByBudgetDescending_OrdersCorrectly()
        {
            var projects = new List<Project>
            {
                MakeProject("PP001", budget: 1000m),
                MakeProject("PP002", budget: 3000m),
                MakeProject("PP003", budget: 2000m)
            };
            var repo = CreateRepository(projects);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 15,
                SortBy = "budget",
                Descending = true
            };

            var result = await repo.GetProjectProfitabilityVlaAsync(query);

            var budgets = result.Data.Select(v => v.Budget).ToList();
            Assert.Equal(new decimal?[] { 3000m, 2000m, 1000m }, budgets);
        }

        [Fact]
        public async Task GetProjectProfitabilityVlaAsync_UnrecognisedSortKey_FallsBackToJobcodeAscending()
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
                SortBy = "unknownkey"
            };

            var result = await repo.GetProjectProfitabilityVlaAsync(query);

            var codes = result.Data.Select(v => v.JobCode).ToList();
            Assert.Equal(new[] { "PP001", "PP002", "PP003" }, codes);
        }

        #endregion

        #region Null-model filter guard

        [Fact]
        public async Task GetProjectProfitabilityVlaAsync_NullModelFilter_ReturnsAllRows()
        {
            // JSON "null" deserialises to null — exercises the filterModel == null guard in ParseFilterDict
            var projects = new List<Project>
            {
                MakeProject("PP001"),
                MakeProject("PP002")
            };
            var repo  = CreateRepository(projects);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 15, Filter = "null" };

            var result = await repo.GetProjectProfitabilityVlaAsync(query);

            Assert.Equal(2, result.Data.Count());
        }

        #endregion

        #region ComputeProfitabilityForVlaAsync grouping lambdas

        private static ProjectRepository CreateRepositoryWithVlaCostData(
            IEnumerable<Project> projects,
            IEnumerable<Program>? programs = null,
            IEnumerable<TestRequirement>? testRequirements = null,
            IEnumerable<AnimalRequest>? animalRequests = null,
            IEnumerable<Animal>? animals = null,
            string userEmailId = "test@example.com",
            int fpsYear = 2024)
        {
            var mockRequestContext = new Mock<IFpsRequestContext>();
            mockRequestContext.Setup(x => x.UserEmailId).Returns(userEmailId);
            mockRequestContext.Setup(x => x.FpsYear).Returns(fpsYear);

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(mockRequestContext.Object);

            mockContext.Setup(x => x.Projects)
                .Returns(RepositoryTestHelper.CreateMockDbSet(projects).Object);
            mockContext.Setup(x => x.Programs)
                .Returns(RepositoryTestHelper.CreateMockDbSet(programs ?? Enumerable.Empty<Program>()).Object);
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
                .Returns(RepositoryTestHelper.CreateMockDbSet(testRequirements ?? Enumerable.Empty<TestRequirement>()).Object);
            mockContext.Setup(x => x.AnimalRequests)
                .Returns(RepositoryTestHelper.CreateMockDbSet(animalRequests ?? Enumerable.Empty<AnimalRequest>()).Object);
            mockContext.Setup(x => x.Animals)
                .Returns(RepositoryTestHelper.CreateMockDbSet(animals ?? Enumerable.Empty<Animal>()).Object);

            return new ProjectRepository(mockContext.Object, mockRequestContext.Object);
        }

        [Fact]
        public async Task GetProjectProfitabilityVlaAsync_WithTestRequirements_ComputesTestCost()
        {
            // Exercises testCostsRaw GroupBy/Select lambdas in ComputeProfitabilityForVlaAsync
            var projects = new List<Project>
            {
                new() { ParentProject = "PP001", Program = "P001", Customer = "ACME",
                        ProjectStatus = "Approved", BudgetCvl = 1000m,
                        Disease = string.Empty, Contract = string.Empty,
                        IsDefraProject = 0, FpsYear = 2024 }
            };
            var testReqs = new List<TestRequirement>
            {
                new() { Buyer = "PP001", NoRequired = 4d, UnitPrice = 25m, TestCode = "T1", FpsYear = 2024 },
                new() { Buyer = "PP001", NoRequired = 1d, UnitPrice = 50m, TestCode = "T2", FpsYear = 2024 }
            };

            var repo  = CreateRepositoryWithVlaCostData(projects, testRequirements: testReqs);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 15 };

            var result = await repo.GetProjectProfitabilityVlaAsync(query);

            var row = Assert.Single(result.Data);
            // 4×25 + 1×50 = 150
            Assert.Equal(150m, row.TestCost);
        }

        [Fact]
        public async Task GetProjectProfitabilityVlaAsync_WithAnimalRequests_ComputesAnimalCosts()
        {
            // Exercises animalCostsRaw GroupBy/ToDictionary lambdas in ComputeProfitabilityForVlaAsync
            var projects = new List<Project>
            {
                new() { ParentProject = "PP001", Program = "P001", Customer = "ACME",
                        ProjectStatus = "Approved", BudgetCvl = 2000m,
                        Disease = string.Empty, Contract = string.Empty,
                        IsDefraProject = 0, FpsYear = 2024 }
            };
            var animalReqs = new List<AnimalRequest>
            {
                new() { JobCode = "PP001", AnimalType = "PIG", NumberOfAnimals = 3d, NumberOfDays = 4d, FpsYear = 2024 }
            };
            var animals = new List<Animal>
            {
                new() { AnimalType = "PIG", DailyRate = 8m, DefraDailyRate = 16m, FpsYear = 2024 }
            };

            var repo  = CreateRepositoryWithVlaCostData(projects, animalRequests: animalReqs, animals: animals);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 15 };

            var result = await repo.GetProjectProfitabilityVlaAsync(query);

            var row = Assert.Single(result.Data);
            // IsDefraProject=0 → DailyRate: 3 × 4 × £8 = £96
            Assert.Equal(96m, row.AnimalCosts);
        }

        [Fact]
        public async Task GetProjectProfitabilityVlaAsync_WithStaffCosts_ComputesStaffMap()
        {
            // Exercises staffCosts Where/GroupBy/ToDictionary lambdas in ComputeProfitabilityForVlaAsync
            var projects = new List<Project>
            {
                new() { ParentProject = "PP001", Program = "P001", Customer = "ACME",
                        ProjectStatus = "Approved", BudgetCvl = 5000m,
                        Disease = string.Empty, Contract = string.Empty,
                        IsDefraProject = 0, FpsYear = 2024 }
            };
            var programs = new List<Program>
            {
                new() { ProgramNo = "P001", Target = 0m, SectorName = "charge", FpsYear = 2024 }
            };
            var staffJobs = new List<StaffJob>
            {
                new() { StaffId = "S001", JobCode = "PP001", PlannedHours = 8d, FpsYear = 2024 }
            };
            var workGroupEmployees = new List<WorkGroupEmployee>
            {
                new() { PactId = "S001", WorkGroupGrade = "WG1", SpNumber = "SP1",
                        PersonStatus = "A", FpsYear = 2024 }
            };
            var workgroupGrades = new List<WorkgroupGrade>
            {
                new() { WgGrade = "WG1", ProfitCentreGrade = "PCG1", GradeCode = "GC1", Workgroup = "WG", FpsYear = 2024 }
            };
            var profitCentreGrades = new List<ProfitCentreGrade>
            {
                new() { PcGrade = "PCG1", ChargeRate = 25m, DefraChargeRate = 40m,
                        DivisionGrade = "DG1", GradeCode = "GC1", ProfitCentre = "PC1", FpsYear = 2024 }
            };

            var mockRequestContext = new Mock<IFpsRequestContext>();
            mockRequestContext.Setup(x => x.UserEmailId).Returns("test@example.com");
            mockRequestContext.Setup(x => x.FpsYear).Returns(2024);
            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(mockRequestContext.Object);
            mockContext.Setup(x => x.Projects).Returns(RepositoryTestHelper.CreateMockDbSet(projects).Object);
            mockContext.Setup(x => x.Programs).Returns(RepositoryTestHelper.CreateMockDbSet(programs).Object);
            mockContext.Setup(x => x.StaffJobs).Returns(RepositoryTestHelper.CreateMockDbSet(staffJobs).Object);
            mockContext.Setup(x => x.WorkGroupEmployees).Returns(RepositoryTestHelper.CreateMockDbSet(workGroupEmployees).Object);
            mockContext.Setup(x => x.WorkgroupGrades).Returns(RepositoryTestHelper.CreateMockDbSet(workgroupGrades).Object);
            mockContext.Setup(x => x.ProfitCentreGrades).Returns(RepositoryTestHelper.CreateMockDbSet(profitCentreGrades).Object);
            mockContext.Setup(x => x.AdditionalCosts).Returns(RepositoryTestHelper.CreateMockDbSet(Enumerable.Empty<AdditionalCost>()).Object);
            mockContext.Setup(x => x.TestRequirements).Returns(RepositoryTestHelper.CreateMockDbSet(Enumerable.Empty<TestRequirement>()).Object);
            mockContext.Setup(x => x.AnimalRequests).Returns(RepositoryTestHelper.CreateMockDbSet(Enumerable.Empty<AnimalRequest>()).Object);
            mockContext.Setup(x => x.Animals).Returns(RepositoryTestHelper.CreateMockDbSet(Enumerable.Empty<Animal>()).Object);
            var repo = new ProjectRepository(mockContext.Object, mockRequestContext.Object);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 15 };

            var result = await repo.GetProjectProfitabilityVlaAsync(query);

            var row = Assert.Single(result.Data);
            // SectorName="charge" → sectorCharge=1 → staffCosts included
            // 8 hours × £25 chargeRate = £200
            Assert.Equal(200m, row.StaffCosts);
        }

        #endregion
    }
}
