using Apha.Common.Helpers.Repository;
using Apha.PIMS.Core.Entities;
using Apha.PIMS.Core.Pagination;
using Apha.PIMS.DataAccess.Data;
using Apha.PIMS.DataAccess.Repository;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Apha.PIMS.DataAccess.UnitTests.Repository.ProjectYearCostsRepositoryTest
{
    public class ProjectYearCostsRepositoryTests
    {
        /// <summary>
        /// Creates a ProjectYearCostsRepository with in-memory data for all DbSets.
        /// All parameters are optional — omitted sets are initialised as empty.
        /// </summary>
        private static ProjectYearCostsRepository CreateRepository(
            IEnumerable<ProjSubContract>?   projSubContracts   = null,
            IEnumerable<AdditionalCosts>?   additionalCosts    = null,
            IEnumerable<ProjectAnimalPlan>? projectAnimalPlans = null,
            IEnumerable<TestReqmt>?         testReqmts         = null,
            IEnumerable<MonthlyOutput>?     monthlyOutputs     = null,
            IEnumerable<TimeCostCalcs>?     timeCostCalcs      = null,
            IEnumerable<ProjectStaffPlan>?  projectStaffPlans  = null,
            IEnumerable<ProjectMonthFinal>? projectMonthFinals = null,
            IEnumerable<FpsYearTotal>?      fpsYearTotals      = null,
            IEnumerable<Projects>?          yearlyProjects     = null)
        {
            var mockContext = RepositoryTestHelper.CreateMockDbContext<PimsDbContext>();

            var projSubContractsMockSet   = RepositoryTestHelper.CreateMockDbSet(projSubContracts   ?? Enumerable.Empty<ProjSubContract>());
            var additionalCostsMockSet    = RepositoryTestHelper.CreateMockDbSet(additionalCosts    ?? Enumerable.Empty<AdditionalCosts>());
            var projectAnimalPlansMockSet = RepositoryTestHelper.CreateMockDbSet(projectAnimalPlans ?? Enumerable.Empty<ProjectAnimalPlan>());
            var testReqmtsMockSet         = RepositoryTestHelper.CreateMockDbSet(testReqmts         ?? Enumerable.Empty<TestReqmt>());
            var monthlyOutputsMockSet     = RepositoryTestHelper.CreateMockDbSet(monthlyOutputs     ?? Enumerable.Empty<MonthlyOutput>());
            var timeCostCalcsMockSet      = RepositoryTestHelper.CreateMockDbSet(timeCostCalcs      ?? Enumerable.Empty<TimeCostCalcs>());
            var projectStaffPlansMockSet  = RepositoryTestHelper.CreateMockDbSet(projectStaffPlans  ?? Enumerable.Empty<ProjectStaffPlan>());
            var projectMonthFinalsMockSet = RepositoryTestHelper.CreateMockDbSet(projectMonthFinals ?? Enumerable.Empty<ProjectMonthFinal>());
            var fpsYearTotalsMockSet      = RepositoryTestHelper.CreateMockDbSet(fpsYearTotals      ?? Enumerable.Empty<FpsYearTotal>());
            var yearlyProjectsMockSet     = RepositoryTestHelper.CreateMockDbSet(yearlyProjects     ?? Enumerable.Empty<Projects>());

            RepositoryTestHelper.SetupSaveChanges(mockContext);

            mockContext.Setup(x => x.ProjSubContracts).Returns(projSubContractsMockSet.Object);
            mockContext.Setup(x => x.AdditionalCosts).Returns(additionalCostsMockSet.Object);
            mockContext.Setup(x => x.ProjectAnimalPlans).Returns(projectAnimalPlansMockSet.Object);
            mockContext.Setup(x => x.TestReqmts).Returns(testReqmtsMockSet.Object);
            mockContext.Setup(x => x.MonthlyOutputs).Returns(monthlyOutputsMockSet.Object);
            mockContext.Setup(x => x.TimeCostCalcs).Returns(timeCostCalcsMockSet.Object);
            mockContext.Setup(x => x.ProjectStaffPlans).Returns(projectStaffPlansMockSet.Object);
            mockContext.Setup(x => x.ProjectMonthFinals).Returns(projectMonthFinalsMockSet.Object);
            mockContext.Setup(x => x.FpsYearTotals).Returns(fpsYearTotalsMockSet.Object);
            mockContext.Setup(x => x.MyTlkpProjects).Returns(yearlyProjectsMockSet.Object);

            return new ProjectYearCostsRepository(mockContext.Object);
        }

        // ─── helper builders ──────────────────────────────────────────────────────

        private static ProjSubContract MakeSubContract(
            string project, short year, string acctcode, double month = 1,
            int counter = 1, decimal amount = 0m, string? description = null, string? supplier = null)
            => new()
            {
                Project        = project,
                Year           = year,
                Acctcode       = acctcode,
                Month          = month,
                Subcontcounter = counter,
                Amount         = amount,
                Description    = description,
                Supplier       = supplier
            };

        private static TimeCostCalcs MakeTimeCost(
            string project, short year, string jobcode, double month = 1,
            string workgroup = "WG1", string? name = null, string? gradecode = null,
            string staffid = "S1", decimal pay = 0m, decimal nonpay = 0m,
            double cost = 0d, decimal overhead = 0m)
            => new()
            {
                Project   = project,
                Year      = year,
                Jobcode   = jobcode,
                Month     = month,
                Workgroup = workgroup,
                Name      = name,
                Gradecode = gradecode,
                Staffid   = staffid,
                Pay       = pay,
                Nonpay    = nonpay,
                Cost      = cost,
                Overhead  = overhead
            };

        #region GetAdditionalActualsAsync

        [Fact]
        public async Task GetAdditionalActualsAsync_ReturnsOnlyNonAnimalRecordsForProjectAndYear()
        {
            // Arrange
            var data = new List<ProjSubContract>
            {
                MakeSubContract("PP001", 2024, "TRAVEL",       month: 1, counter: 1),
                MakeSubContract("PP001", 2024, "LargeAnimals", month: 1, counter: 2), // excluded
                MakeSubContract("PP001", 2024, "SmallAnimals", month: 1, counter: 3), // excluded
                MakeSubContract("PP001", 2024, "Mice",         month: 1, counter: 4), // excluded
                MakeSubContract("PP001", 2024, "EQUIP",        month: 2, counter: 5),
                MakeSubContract("PP002", 2024, "TRAVEL",       month: 1, counter: 6)  // different project
            };
            var repo = CreateRepository(projSubContracts: data);
            var paging = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await repo.GetAdditionalActualsAsync("PP001", 2024, paging);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Data.Count);
            Assert.All(result.Data, r => Assert.Equal("PP001", r.Project));
            Assert.DoesNotContain(result.Data, r => r.Acctcode is "LargeAnimals" or "SmallAnimals" or "Mice");
        }

        [Fact]
        public async Task GetAdditionalActualsAsync_FiltersOutDifferentYear()
        {
            // Arrange
            var data = new List<ProjSubContract>
            {
                MakeSubContract("PP001", 2024, "TRAVEL", counter: 1),
                MakeSubContract("PP001", 2023, "TRAVEL", counter: 2)
            };
            var repo = CreateRepository(projSubContracts: data);
            var paging = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await repo.GetAdditionalActualsAsync("PP001", 2024, paging);

            // Assert
            Assert.Single(result.Data);
            Assert.Equal((short)2024, result.Data.First().Year);
        }

        [Fact]
        public async Task GetAdditionalActualsAsync_ReturnsEmptyWhenNoMatchingRecords()
        {
            // Arrange
            var repo = CreateRepository(projSubContracts: new List<ProjSubContract>());
            var paging = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await repo.GetAdditionalActualsAsync("PP001", 2024, paging);

            // Assert
            Assert.Empty(result.Data);
            Assert.Equal(0, result.PaginationData.TotalRecords);
        }

        [Fact]
        public async Task GetAdditionalActualsAsync_SearchByAcctcode_ReturnsMatchingRecords()
        {
            // Arrange
            var data = new List<ProjSubContract>
            {
                MakeSubContract("PP001", 2024, "TRAVEL", counter: 1),
                MakeSubContract("PP001", 2024, "EQUIP",  counter: 2)
            };
            var repo = CreateRepository(projSubContracts: data);
            var paging = new PaginationParameters<string> { Page = 1, PageSize = 10, Search = "travel" };

            // Act
            var result = await repo.GetAdditionalActualsAsync("PP001", 2024, paging);

            // Assert
            Assert.Single(result.Data);
            Assert.Equal("TRAVEL", result.Data.First().Acctcode);
        }

        [Fact]
        public async Task GetAdditionalActualsAsync_SearchByDescription_ReturnsMatchingRecords()
        {
            // Arrange
            var data = new List<ProjSubContract>
            {
                MakeSubContract("PP001", 2024, "TRAVEL", counter: 1, description: "Train ticket"),
                MakeSubContract("PP001", 2024, "EQUIP",  counter: 2, description: "Lab equipment")
            };
            var repo = CreateRepository(projSubContracts: data);
            var paging = new PaginationParameters<string> { Page = 1, PageSize = 10, Search = "lab" };

            // Act
            var result = await repo.GetAdditionalActualsAsync("PP001", 2024, paging);

            // Assert
            Assert.Single(result.Data);
            Assert.Equal("EQUIP", result.Data.First().Acctcode);
        }

        [Fact]
        public async Task GetAdditionalActualsAsync_SearchBySupplier_ReturnsMatchingRecords()
        {
            // Arrange
            var data = new List<ProjSubContract>
            {
                MakeSubContract("PP001", 2024, "TRAVEL", counter: 1, supplier: "SupplierA"),
                MakeSubContract("PP001", 2024, "EQUIP",  counter: 2, supplier: "SupplierB")
            };
            var repo = CreateRepository(projSubContracts: data);
            var paging = new PaginationParameters<string> { Page = 1, PageSize = 10, Search = "supplierb" };

            // Act
            var result = await repo.GetAdditionalActualsAsync("PP001", 2024, paging);

            // Assert
            Assert.Single(result.Data);
            Assert.Equal("SupplierB", result.Data.First().Supplier);
        }

        [Fact]
        public async Task GetAdditionalActualsAsync_SearchMatchingNoRecords_ReturnsEmpty()
        {
            // Arrange
            var data = new List<ProjSubContract>
            {
                MakeSubContract("PP001", 2024, "TRAVEL", counter: 1)
            };
            var repo = CreateRepository(projSubContracts: data);
            var paging = new PaginationParameters<string> { Page = 1, PageSize = 10, Search = "ZZZZ" };

            // Act
            var result = await repo.GetAdditionalActualsAsync("PP001", 2024, paging);

            // Assert
            Assert.Empty(result.Data);
        }

        [Theory]
        [InlineData("acctcode",    false, "EQUIP")]
        [InlineData("acctcode",    true,  "TRAVEL")]
        [InlineData("description", false, "Alpha")]
        [InlineData("description", true,  "Zebra")]
        [InlineData("supplier",    false, "SupplierA")]
        [InlineData("supplier",    true,  "SupplierB")]
        public async Task GetAdditionalActualsAsync_SortingByStringFields_ReturnsSortedResults(
            string sortBy, bool descending, string expectedFirstValue)
        {
            // Arrange
            var data = new List<ProjSubContract>
            {
                MakeSubContract("PP001", 2024, "TRAVEL", counter: 1, description: "Zebra", supplier: "SupplierB"),
                MakeSubContract("PP001", 2024, "EQUIP",  counter: 2, description: "Alpha", supplier: "SupplierA")
            };
            var repo = CreateRepository(projSubContracts: data);
            var paging = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = sortBy, Descending = descending };

            // Act
            var result = await repo.GetAdditionalActualsAsync("PP001", 2024, paging);

            // Assert
            var first = result.Data.First();
            var actual = sortBy switch
            {
                "acctcode"    => first.Acctcode,
                "description" => first.Description,
                "supplier"    => first.Supplier,
                _             => null
            };
            Assert.Equal(expectedFirstValue, actual);
        }

        [Theory]
        [InlineData("amount", false, 100)]
        [InlineData("amount", true,  200)]
        [InlineData("month",  false, 1)]
        [InlineData("month",  true,  2)]
        public async Task GetAdditionalActualsAsync_SortingByNumericFields_ReturnsSortedResults(
            string sortBy, bool descending, int expectedFirstValue)
        {
            // Arrange
            var data = new List<ProjSubContract>
            {
                MakeSubContract("PP001", 2024, "TRAVEL", month: 2, counter: 1, amount: 200m),
                MakeSubContract("PP001", 2024, "EQUIP",  month: 1, counter: 2, amount: 100m)
            };
            var repo = CreateRepository(projSubContracts: data);
            var paging = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = sortBy, Descending = descending };

            // Act
            var result = await repo.GetAdditionalActualsAsync("PP001", 2024, paging);

            // Assert
            var first = result.Data.First();
            var actual = sortBy switch
            {
                "amount" => (int)(first.Amount ?? 0m),
                "month"  => (int)(first.Month  ?? 0d),
                _        => 0
            };
            Assert.Equal(expectedFirstValue, actual);
        }

        [Fact]
        public async Task GetAdditionalActualsAsync_DefaultSorting_OrdersByMonthThenAcctcodeThenSubcontcounter()
        {
            // Arrange — default sort is Month ASC → Acctcode ASC → Subcontcounter ASC
            var data = new List<ProjSubContract>
            {
                MakeSubContract("PP001", 2024, "TRAVEL", month: 2, counter: 1), // month=2
                MakeSubContract("PP001", 2024, "BETA",   month: 1, counter: 1), // month=1, acctcode=BETA
                MakeSubContract("PP001", 2024, "ALPHA",  month: 1, counter: 2)  // month=1, acctcode=ALPHA (alphabetically before BETA)
            };
            var repo = CreateRepository(projSubContracts: data);
            var paging = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await repo.GetAdditionalActualsAsync("PP001", 2024, paging);

            // Assert
            var items = result.Data.ToList();
            Assert.Equal("ALPHA",  items[0].Acctcode); // month=1, acctcode=ALPHA (A < B)
            Assert.Equal("BETA",   items[1].Acctcode); // month=1, acctcode=BETA
            Assert.Equal("TRAVEL", items[2].Acctcode); // month=2
        }

        [Fact]
        public async Task GetAnimalActualsAsync_DefaultSorting_OrdersByMonthThenAcctcodeThenSubcontcounter()
        {
            // Arrange — default sort is Month ASC → Acctcode ASC → Subcontcounter ASC
            var data = new List<ProjSubContract>
            {
                MakeSubContract("PP001", 2024, "SmallAnimals", month: 2, counter: 1), // month=2
                MakeSubContract("PP001", 2024, "Mice",         month: 1, counter: 1), // month=1, acctcode=Mice (M > L)
                MakeSubContract("PP001", 2024, "LargeAnimals", month: 1, counter: 2)  // month=1, acctcode=LargeAnimals (L < M alphabetically)
            };
            var repo = CreateRepository(projSubContracts: data);
            var paging = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            // Act
            var result = await repo.GetAnimalActualsAsync("PP001", 2024, paging);

            // Assert
            var items = result.Data.ToList();
            Assert.Equal("LargeAnimals", items[0].Acctcode); // month=1, acctcode=L (L < M)
            Assert.Equal("Mice",         items[1].Acctcode); // month=1, acctcode=M
            Assert.Equal("SmallAnimals", items[2].Acctcode); // month=2
        }

        [Theory]
        [InlineData("buyer", false)]
        [InlineData("buyer", true)]
        public async Task GetTestPlansAsync_SortingByBuyer_ReturnsAllMatchingRecords(
            string sortBy, bool descending)
        {
            // Arrange — the repository WHERE clause already filters to a single Buyer (= project),
            // so all returned records share the same Buyer value; sorting by buyer is valid but
            // produces no reordering. This test verifies it does not throw and returns all records.
            var data = new List<TestReqmt>
            {
                new() { Buyer = "PP001", Year = 2024, Testcode = "TC002" },
                new() { Buyer = "PP001", Year = 2024, Testcode = "TC001" }
            };
            var repo = CreateRepository(testReqmts: data);
            var paging = new PaginationParameters<string> { Page = 1, PageSize = 10, SortBy = sortBy, Descending = descending };

            // Act
            var result = await repo.GetTestPlansAsync("PP001", 2024, paging);

            // Assert
            Assert.Equal(2, result.Data.Count);
            Assert.All(result.Data, r => Assert.Equal("PP001", r.Buyer));
        }

        #endregion
    }
}