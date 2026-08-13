using Apha.Common.Helpers.Repository;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using Apha.FPS.DataAccess.Data;
using Apha.FPS.DataAccess.Repositories;
using Moq;

namespace Apha.FPS.DataAccess.UnitTests.Repository.ProjectDepartmentIncomeRepositoryTest
{
    public class ProjectDepartmentIncomeRepositoryTests
    {
        private const string TestProject  = "AH0033";
        private const int    TestMonthFrom = 1;
        private const int    TestMonthTo   = 12;
        private const int    TestFpsYear   = 2024;

        // â”€â”€ Factory â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

        private static ProjectDepartmentIncomeRepository CreateRepository(
            IEnumerable<TimeCostCalcs>?      timeCostCalcs      = null,
            IEnumerable<Workgroup>?          workgroups         = null,
            IEnumerable<Project>?            projects           = null,
            IEnumerable<WorkGroupEmployee>?  workGroupEmployees = null,
            IEnumerable<CostCentre>?         costCentres        = null,
            IEnumerable<MonthlyOutput>?      monthlyOutputs     = null,
            IEnumerable<TestRequirement>?    testRequirements   = null,
            IEnumerable<AdditionalCost>?     additionalCosts    = null,
            IEnumerable<PeriodMonthlyOutput>? periodMonthlyOutputs = null,
            IEnumerable<PeriodLookup>?       periodLookups      = null,
            IEnumerable<Period>?             periods            = null,
            IEnumerable<ProjectSubContract>? projectSubContracts = null,
            int fpsYear = TestFpsYear)
        {
            var mockRequestContext = new Mock<IFpsRequestContext>();
            mockRequestContext.Setup(x => x.FpsYear).Returns(fpsYear);

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(mockRequestContext.Object);

            void Setup<T>(IEnumerable<T>? data, Action<Mock<FpsDbContext>, Mock<Microsoft.EntityFrameworkCore.DbSet<T>>> setup)
                where T : class
            {
                if (data != null)
                {
                    var mockSet = RepositoryTestHelper.CreateMockDbSet(data);
                    setup(mockContext, mockSet);
                }
            }

            Setup(timeCostCalcs,      (ctx, s) => ctx.Setup(x => x.TimeCostCalcs).Returns(s.Object));
            Setup(workgroups,         (ctx, s) => ctx.Setup(x => x.Workgroups).Returns(s.Object));
            Setup(projects,           (ctx, s) => ctx.Setup(x => x.Projects).Returns(s.Object));
            Setup(workGroupEmployees, (ctx, s) => ctx.Setup(x => x.WorkGroupEmployees).Returns(s.Object));
            Setup(costCentres,        (ctx, s) => ctx.Setup(x => x.CostCentres).Returns(s.Object));
            Setup(monthlyOutputs,     (ctx, s) => ctx.Setup(x => x.MonthlyOutputs).Returns(s.Object));
            Setup(testRequirements,   (ctx, s) => ctx.Setup(x => x.TestRequirements).Returns(s.Object));
            Setup(additionalCosts,    (ctx, s) => ctx.Setup(x => x.AdditionalCosts).Returns(s.Object));
            Setup(periodMonthlyOutputs, (ctx, s) => ctx.Setup(x => x.PeriodMonthlyOutputs).Returns(s.Object));
            Setup(periodLookups,      (ctx, s) => ctx.Setup(x => x.PeriodLookups).Returns(s.Object));
            Setup(periods,            (ctx, s) => ctx.Setup(x => x.Periods).Returns(s.Object));
            Setup(projectSubContracts,(ctx, s) => ctx.Setup(x => x.ProjectSubContracts).Returns(s.Object));

            RepositoryTestHelper.SetupSaveChanges(mockContext);

            return new ProjectDepartmentIncomeRepository(mockContext.Object, mockRequestContext.Object);
        }

        // â”€â”€ Seed helpers â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

        private static TimeCostCalcs MakeTimeCostCalc(
            string workGroup = "WG1",
            string project   = TestProject,
            int    month     = 1,
            string staffId   = "S01",
            string @class    = "Charge",
            int    fpsYear   = TestFpsYear) =>
            new()
            {
                WorkGroup  = workGroup,
                JobCode    = "JOB1",
                Project    = project,
                Month      = month,
                StaffId    = staffId,
                FpsYear    = fpsYear,
                Class      = @class,
                GradeCode  = "G1",
                Name       = "Alice",
                ChargeRate = 50m,
                Pay        = 100m,
                NonPay     = 20m,
                Overhead   = 10m,
                Time       = 8.0,
                Cost       = 130.0
            };

        private static Workgroup MakeWorkgroup(
            string workGroupName = "WG1",
            int?   fpsYear       = TestFpsYear) =>
            new()
            {
                WorkGroupName = workGroupName,
                ProfitCentre  = "PC1",
                CostCentre    = 100.0,
                FpsYear       = fpsYear
            };

        private static Project MakeProject(
            string parentProject   = TestProject,
            int    fpsYear         = TestFpsYear,
            short  isDefraProject  = 1,
            double? costCentre     = 100.0) =>
            new()
            {
                ParentProject     = parentProject,
                FpsYear           = fpsYear,
                IsDefraProject    = isDefraProject,
                CostCentre        = costCentre,
                OracleProjectCode = "OPC001",
                SubAccountCode    = "SAC001",
                ProjectTitle      = "Test Project",
                Program           = "PROG1",
                Customer          = "CUST1",
                ProjectStatus     = "Active",
                Disease           = "DIS1",
                Contract          = "CON1",
                IncomeAccountCode = "IAC1"
            };

        private static WorkGroupEmployee MakeEmployee(
            string pactId  = "S01",
            int    fpsYear = TestFpsYear) =>
            new()
            {
                PactId         = pactId,
                SpNumber       = "SP001",
                WorkGroupGrade = "G1",
                PersonStatus   = "Active",
                HrsPaid        = 40,
                Leave          = 0,
                SickSpecial    = 0,
                HrsAvail       = 40,
                MakeAvailable  = 1,
                TimeRecorder   = 1,
                FpsYear        = fpsYear
            };

        private static CostCentre MakeCostCentre(
            double costCentreNo = 100.0,
            int    fpsYear      = TestFpsYear) =>
            new()
            {
                CostCentreNo = costCentreNo,
                ProfitCentre = "OPC1",
                FpsYear      = fpsYear
            };

        private static MonthlyOutput MakeMonthlyOutput(
            string testCode  = "TC01",
            string buyer     = TestProject,
            string workGroup = "WG1",
            int    month     = 1,
            int    fpsYear   = TestFpsYear,
            double volume    = 5.0) =>
            new()
            {
                TestCode  = testCode,
                Buyer     = buyer,
                WorkGroup = workGroup,
                Month     = month,
                FpsYear   = fpsYear,
                Volume    = volume
            };

        private static TestRequirement MakeTestRequirement(
            string testCode = "TC01",
            string buyer    = TestProject,
            int    fpsYear  = TestFpsYear) =>
            new()
            {
                TestCode   = testCode,
                Buyer      = buyer,
                FpsYear    = fpsYear,
                UnitPrice  = 25m,
                NoRequired = 5.0,
                Active     = 1
            };

        private static AdditionalCost MakeAdditionalCost(
            string jobCode  = TestProject,
            string account  = "Consumables",
            int?   fpsYear  = TestFpsYear) =>
            new()
            {
                JobCode     = jobCode,
                Account     = account,
                Description = "Test item",
                ItemCost    = 100m,
                FpsYear     = fpsYear
            };

        private static ProjectSubContract MakeProjectSubContract(
            string  project  = TestProject,
            string  acctCode = "Consumables",
            double  month    = 1,
            decimal amount   = 100m,
            int     fpsYear  = TestFpsYear) =>
            new()
            {
                Project     = project,
                AcctCode    = acctCode,
                Month       = month,
                Amount      = amount,
                Description = "Test item",
                DailyRate   = 10m,
                AnimalDays  = 1,
                FpsYear     = fpsYear
            };

        private static PeriodLookup MakePeriodLookup(
            double accntsPeriod = 1,
            string monthName    = "April",
            double monthNumber  = 4,
            int    fpsYear      = TestFpsYear) =>
            new()
            {
                AccntsPeriod = accntsPeriod,
                MonthName    = monthName,
                MonthNumber  = monthNumber,
                FpsYear      = fpsYear
            };

        private static Period MakePeriod(
            string periodName    = "Period1",
            int    fpsYear       = TestFpsYear,
            double endPeriod     = 1,
            short  periodLocked  = 0) =>
            new()
            {
                PeriodName   = periodName,
                FpsYear      = fpsYear,
                EndPeriod    = endPeriod,
                PeriodLocked = periodLocked
            };

        private static PaginationParameters<string> DefaultQuery(
            int    page       = 1,
            int    pageSize   = 10,
            string? filter    = null,
            string? sortBy    = null,
            bool   descending = false) =>
            new()
            {
                Page       = page,
                PageSize   = pageSize,
                Filter     = filter,
                SortBy     = sortBy,
                Descending = descending
            };

        // â”€â”€ Constructor â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

        #region Constructor

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenRequestContextIsNull()
        {
            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(
                new Mock<IFpsRequestContext>().Object);

            Assert.Throws<ArgumentNullException>(() =>
                new ProjectDepartmentIncomeRepository(mockContext.Object, null!));
        }

        #endregion

        // â”€â”€ GetTimeIncomeAsync â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

        #region GetTimeIncomeAsync

        [Fact]
        public async Task GetTimeIncomeAsync_WithMatchingProject_ReturnsFilteredRows()
        {
            // Arrange
            var repo = CreateRepository(
                timeCostCalcs:      [MakeTimeCostCalc()],
                workgroups:         [MakeWorkgroup()],
                projects:           [MakeProject()],
                workGroupEmployees: [MakeEmployee()],
                costCentres:        [MakeCostCentre()]);

            // Act
            var result = await repo.GetTimeIncomeAsync(TestProject, TestMonthFrom, TestMonthTo);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(TestProject, result[0].Project);
        }

        [Fact]
        public async Task GetTimeIncomeAsync_NullProject_ReturnsAllProjects()
        {
            // Arrange
            var repo = CreateRepository(
                timeCostCalcs:      [MakeTimeCostCalc("WG1", "PROJ1"), MakeTimeCostCalc("WG1", "PROJ2")],
                workgroups:         [MakeWorkgroup()],
                projects:           [MakeProject("PROJ1"), MakeProject("PROJ2")],
                workGroupEmployees: [MakeEmployee()],
                costCentres:        [MakeCostCentre()]);

            // Act
            var result = await repo.GetTimeIncomeAsync(null, TestMonthFrom, TestMonthTo);

            // Assert
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetTimeIncomeAsync_EmptyResult_WhenNoTimeCostCalcs()
        {
            // Arrange
            var repo = CreateRepository(
                timeCostCalcs:      [],
                workgroups:         [MakeWorkgroup()],
                projects:           [MakeProject()],
                workGroupEmployees: [],
                costCentres:        [MakeCostCentre()]);

            // Act
            var result = await repo.GetTimeIncomeAsync(TestProject, TestMonthFrom, TestMonthTo);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetTimeIncomeAsync_ExcludesNonChargeClass()
        {
            // Arrange â€” "Budget" class should be excluded, only "Charge" is kept
            var repo = CreateRepository(
                timeCostCalcs:      [MakeTimeCostCalc(@class: "Budget")],
                workgroups:         [MakeWorkgroup()],
                projects:           [MakeProject()],
                workGroupEmployees: [MakeEmployee()],
                costCentres:        [MakeCostCentre()]);

            // Act
            var result = await repo.GetTimeIncomeAsync(TestProject, TestMonthFrom, TestMonthTo);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetTimeIncomeAsync_ExcludesMonthsOutsideRange()
        {
            // Arrange â€” month 13 is outside [1,12]
            var repo = CreateRepository(
                timeCostCalcs:      [MakeTimeCostCalc(month: 13)],
                workgroups:         [MakeWorkgroup()],
                projects:           [MakeProject()],
                workGroupEmployees: [MakeEmployee()],
                costCentres:        [MakeCostCentre()]);

            // Act
            var result = await repo.GetTimeIncomeAsync(TestProject, 1, 12);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetTimeIncomeAsync_DefraProject_MappedToYes_WhenIsDefraProjectNonZero()
        {
            // Arrange
            var repo = CreateRepository(
                timeCostCalcs:      [MakeTimeCostCalc()],
                workgroups:         [MakeWorkgroup()],
                projects:           [MakeProject(isDefraProject: 1)],
                workGroupEmployees: [MakeEmployee()],
                costCentres:        [MakeCostCentre()]);

            // Act
            var result = await repo.GetTimeIncomeAsync(TestProject, TestMonthFrom, TestMonthTo);

            // Assert
            Assert.Single(result);
            Assert.Equal("Yes", result[0].DefraProject);
        }

        [Fact]
        public async Task GetTimeIncomeAsync_DefraProject_MappedToNo_WhenIsDefraProjectZero()
        {
            // Arrange
            var repo = CreateRepository(
                timeCostCalcs:      [MakeTimeCostCalc()],
                workgroups:         [MakeWorkgroup()],
                projects:           [MakeProject(isDefraProject: 0)],
                workGroupEmployees: [MakeEmployee()],
                costCentres:        [MakeCostCentre()]);

            // Act
            var result = await repo.GetTimeIncomeAsync(TestProject, TestMonthFrom, TestMonthTo);

            // Assert
            Assert.Single(result);
            Assert.Equal("No", result[0].DefraProject);
        }

        [Fact]
        public async Task GetTimeIncomeAsync_WithNoMatchingEmployee_SpNumberIsNull()
        {
            // Arrange â€” no employee seeded so the left join produces null
            var repo = CreateRepository(
                timeCostCalcs:      [MakeTimeCostCalc()],
                workgroups:         [MakeWorkgroup()],
                projects:           [MakeProject()],
                workGroupEmployees: [],
                costCentres:        [MakeCostCentre()]);

            // Act
            var result = await repo.GetTimeIncomeAsync(TestProject, TestMonthFrom, TestMonthTo);

            // Assert
            Assert.Single(result);
            Assert.Null(result[0].SpNumber);
        }

        [Fact]
        public async Task GetTimeIncomeAsync_WithNoCostCentre_OccAndOpcAreNull()
        {
            // Arrange â€” project has no CostCentre match
            var proj = MakeProject(costCentre: null);
            var repo = CreateRepository(
                timeCostCalcs:      [MakeTimeCostCalc()],
                workgroups:         [MakeWorkgroup()],
                projects:           [proj],
                workGroupEmployees: [MakeEmployee()],
                costCentres:        []);

            // Act
            var result = await repo.GetTimeIncomeAsync(TestProject, TestMonthFrom, TestMonthTo);

            // Assert
            Assert.Single(result);
            Assert.Null(result[0].OCC);
            Assert.Null(result[0].OPC);
        }

        [Fact]
        public async Task GetTimeIncomeAsync_NullChargeRate_DefaultsToZero()
        {
            // Arrange
            var tc = MakeTimeCostCalc();
            tc.ChargeRate = null;
            var repo = CreateRepository(
                timeCostCalcs:      [tc],
                workgroups:         [MakeWorkgroup()],
                projects:           [MakeProject()],
                workGroupEmployees: [MakeEmployee()],
                costCentres:        [MakeCostCentre()]);

            // Act
            var result = await repo.GetTimeIncomeAsync(TestProject, TestMonthFrom, TestMonthTo);

            // Assert
            Assert.Single(result);
            Assert.Equal(0m, result[0].ChargeRate);
        }

        [Fact]
        public async Task GetTimeIncomeAsync_ResultOrderedByParentProject()
        {
            // Arrange
            var repo = CreateRepository(
                timeCostCalcs:      [MakeTimeCostCalc("WG1", "ZZZ"), MakeTimeCostCalc("WG1", "AAA")],
                workgroups:         [MakeWorkgroup()],
                projects:           [MakeProject("ZZZ"), MakeProject("AAA")],
                workGroupEmployees: [MakeEmployee()],
                costCentres:        [MakeCostCentre()]);

            // Act
            var result = await repo.GetTimeIncomeAsync(null, TestMonthFrom, TestMonthTo);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal("AAA", result[0].Project);
            Assert.Equal("ZZZ", result[1].Project);
        }

        #endregion

        // â”€â”€ GetPagedTimeIncomeAsync â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

        #region GetPagedTimeIncomeAsync

        [Fact]
        public async Task GetPagedTimeIncomeAsync_ReturnsPagedResult()
        {
            // Arrange
            var repo = CreateRepository(
                timeCostCalcs:      [MakeTimeCostCalc()],
                workgroups:         [MakeWorkgroup()],
                projects:           [MakeProject()],
                workGroupEmployees: [MakeEmployee()],
                costCentres:        [MakeCostCentre()]);

            // Act
            var result = await repo.GetPagedTimeIncomeAsync(DefaultQuery(), TestProject, TestMonthFrom, TestMonthTo);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Data);
            Assert.Equal(1, result.PaginationData.TotalRecords);
        }

        [Fact]
        public async Task GetPagedTimeIncomeAsync_EmptyData_ReturnsEmptyPage()
        {
            // Arrange
            var repo = CreateRepository(
                timeCostCalcs:      [],
                workgroups:         [MakeWorkgroup()],
                projects:           [MakeProject()],
                workGroupEmployees: [],
                costCentres:        [MakeCostCentre()]);

            // Act
            var result = await repo.GetPagedTimeIncomeAsync(DefaultQuery(), TestProject, TestMonthFrom, TestMonthTo);

            // Assert
            Assert.Empty(result.Data);
            Assert.Equal(0, result.PaginationData.TotalRecords);
        }

        [Fact]
        public async Task GetPagedTimeIncomeAsync_PagingApplied_ReturnsCorrectPage()
        {
            // Arrange - 11 TC entries across 11 projects; page 2 of size 10 should return the 11th record.
            // GetPagedTimeIncomeAsync clamps pageSize to Math.Max(pageSize, 10), so pageSize 10 is the minimum.
            var projectCodes = Enumerable.Range(1, 11).Select(i => $"PROJ{i:D2}").ToArray();
            var tcs          = projectCodes.Select(p => MakeTimeCostCalc(project: p)).ToArray();
            var projs        = projectCodes.Select(p => MakeProject(p)).ToArray();
            var repo = CreateRepository(
                timeCostCalcs:      tcs,
                workgroups:         [MakeWorkgroup()],
                projects:           projs,
                workGroupEmployees: [MakeEmployee()],
                costCentres:        [MakeCostCentre()]);

            // Act
            var result = await repo.GetPagedTimeIncomeAsync(DefaultQuery(page: 2, pageSize: 10), null, TestMonthFrom, TestMonthTo);

            // Assert
            Assert.Single(result.Data);
            Assert.Equal(11, result.PaginationData.TotalRecords);
        }

        #endregion

        

        #region GetTestIncomeAsync

        [Fact]
        public async Task GetTestIncomeAsync_WithMatchingProject_ReturnsFilteredRows()
        {
            // Arrange
            var repo = CreateRepository(
                workgroups:       [MakeWorkgroup()],
                projects:         [MakeProject()],
                costCentres:      [MakeCostCentre()],
                monthlyOutputs:   [MakeMonthlyOutput()],
                testRequirements: [MakeTestRequirement()]);

            // Act
            var result = await repo.GetTestIncomeAsync(TestProject, TestMonthFrom, TestMonthTo);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(TestProject, result[0].Project);
        }

        [Fact]
        public async Task GetTestIncomeAsync_NullProject_ReturnsAllProjects()
        {
            // Arrange
            var repo = CreateRepository(
                workgroups:       [MakeWorkgroup()],
                projects:         [MakeProject("PROJ1"), MakeProject("PROJ2")],
                costCentres:      [MakeCostCentre()],
                monthlyOutputs:   [MakeMonthlyOutput(buyer: "PROJ1"), MakeMonthlyOutput(buyer: "PROJ2")],
                testRequirements: [MakeTestRequirement(buyer: "PROJ1"), MakeTestRequirement(buyer: "PROJ2")]);

            // Act
            var result = await repo.GetTestIncomeAsync(null, TestMonthFrom, TestMonthTo);

            // Assert
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetTestIncomeAsync_EmptyResult_WhenNoMonthlyOutputs()
        {
            // Arrange
            var repo = CreateRepository(
                workgroups:       [MakeWorkgroup()],
                projects:         [MakeProject()],
                costCentres:      [MakeCostCentre()],
                monthlyOutputs:   [],
                testRequirements: [MakeTestRequirement()]);

            // Act
            var result = await repo.GetTestIncomeAsync(TestProject, TestMonthFrom, TestMonthTo);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetTestIncomeAsync_ExcludesMonthsOutsideRange()
        {
            // Arrange â€” month 13 outside [1,12]
            var repo = CreateRepository(
                workgroups:       [MakeWorkgroup()],
                projects:         [MakeProject()],
                costCentres:      [MakeCostCentre()],
                monthlyOutputs:   [MakeMonthlyOutput(month: 13)],
                testRequirements: [MakeTestRequirement()]);

            // Act
            var result = await repo.GetTestIncomeAsync(TestProject, 1, 12);

            // Assert
            Assert.Empty(result);
        }

        #endregion

        // â”€â”€ GetPagedTestIncomeAsync â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

        #region GetPagedTestIncomeAsync

        [Fact]
        public async Task GetPagedTestIncomeAsync_ReturnsPagedResult()
        {
            // Arrange
            var repo = CreateRepository(
                workgroups:       [MakeWorkgroup()],
                projects:         [MakeProject()],
                costCentres:      [MakeCostCentre()],
                monthlyOutputs:   [MakeMonthlyOutput()],
                testRequirements: [MakeTestRequirement()]);

            // Act
            var result = await repo.GetPagedTestIncomeAsync(DefaultQuery(), TestProject, TestMonthFrom, TestMonthTo);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result.Data);
            Assert.Equal(1, result.PaginationData.TotalRecords);
        }

        [Fact]
        public async Task GetPagedTestIncomeAsync_EmptyData_ReturnsEmptyPage()
        {
            // Arrange
            var repo = CreateRepository(
                workgroups:       [MakeWorkgroup()],
                projects:         [MakeProject()],
                costCentres:      [MakeCostCentre()],
                monthlyOutputs:   [],
                testRequirements: [MakeTestRequirement()]);

            // Act
            var result = await repo.GetPagedTestIncomeAsync(DefaultQuery(), TestProject, TestMonthFrom, TestMonthTo);

            // Assert
            Assert.Empty(result.Data);
        }

        #endregion

        // â”€â”€ GetTestSnapshotIncomeAsync â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

        #region GetTestSnapshotIncomeAsync

        [Fact]
        public async Task GetTestSnapshotIncomeAsync_WithData_ReturnsRows()
        {
            // Arrange
            var repo = CreateRepository(
                workgroups:           [MakeWorkgroup()],
                projects:             [MakeProject()],
                costCentres:          [MakeCostCentre()],
                periodMonthlyOutputs: [new PeriodMonthlyOutput
                {
                    Id                = 1,
                    Period            = 2,
                    Project           = TestProject,
                    OracleProjectCode = "OPC001",
                    SubAccountCode    = "SAC001",
                    IsDefraProject    = "Yes",
                    Month             = 1,
                    WorkGroup         = "WG1",
                    Spc               = "PC1",
                    TestCode          = "TC01",
                    Volume            = 5.0,
                    TestPrice         = 25m,
                    TotalCost         = 125m
                }],
                periods: [MakePeriod(endPeriod: 2)]);

            // Act
            var result = await repo.GetTestSnapshotIncomeAsync(TestProject, 1, 2);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(TestProject, result[0].Project);
        }

        [Fact]
        public async Task GetTestSnapshotIncomeAsync_EmptyResult_WhenNoPeriodData()
        {
            // Arrange
            var repo = CreateRepository(
                workgroups:           [MakeWorkgroup()],
                projects:             [MakeProject()],
                costCentres:          [MakeCostCentre()],
                periodMonthlyOutputs: [],
                periods: []);

            // Act
            var result = await repo.GetTestSnapshotIncomeAsync(TestProject, 1, 2);

            // Assert
            Assert.Empty(result);
        }

        #endregion

        // â”€â”€ GetAnimalIncomeAsync â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

        #region GetAnimalIncomeAsync

        [Fact]
        public async Task GetAnimalIncomeAsync_WithMatchingProject_ReturnsAnimalRows()
        {
            // Arrange â€” "LargeAnimals" is in AnimalAcctCodes
            var repo = CreateRepository(
                workgroups:       [MakeWorkgroup()],
                projects:         [MakeProject()],
                costCentres:      [MakeCostCentre()],
                monthlyOutputs:   [MakeMonthlyOutput()],
                testRequirements: [MakeTestRequirement()],
                projectSubContracts: [MakeProjectSubContract(acctCode: "LargeAnimals")]);

            // Act
            var result = await repo.GetAnimalIncomeAsync(TestProject, TestMonthFrom, TestMonthTo);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetAnimalIncomeAsync_ExcludesNonAnimalAccountCodes()
        {
            // Arrange â€” "Consumables" is NOT in AnimalAcctCodes
            var repo = CreateRepository(
                workgroups:       [MakeWorkgroup()],
                projects:         [MakeProject()],
                costCentres:      [MakeCostCentre()],
                monthlyOutputs:   [MakeMonthlyOutput()],
                testRequirements: [MakeTestRequirement()],
                projectSubContracts: [MakeProjectSubContract(acctCode: "Consumables")]);

            // Act
            var result = await repo.GetAnimalIncomeAsync(TestProject, TestMonthFrom, TestMonthTo);

            // Assert â€” no animal rows since account code is not an animal code
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAnimalIncomeAsync_NullProject_ReturnsAllProjects()
        {
            // Arrange
            var repo = CreateRepository(
                workgroups:       [MakeWorkgroup()],
                projects:         [MakeProject("PROJ1"), MakeProject("PROJ2")],
                costCentres:      [MakeCostCentre()],
                projectSubContracts: [MakeProjectSubContract(project: "PROJ1", acctCode: "SmallAnimals"),
                                      MakeProjectSubContract(project: "PROJ2", acctCode: "Mice")]);

            // Act
            var result = await repo.GetAnimalIncomeAsync(null, TestMonthFrom, TestMonthTo);

            // Assert
            Assert.NotNull(result);
        }

        #endregion

        // â”€â”€ GetPagedAnimalIncomeAsync â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

        #region GetPagedAnimalIncomeAsync

        [Fact]
        public async Task GetPagedAnimalIncomeAsync_EmptyData_ReturnsEmptyPage()
        {
            // Arrange
            var repo = CreateRepository(
                workgroups:       [MakeWorkgroup()],
                projects:         [MakeProject()],
                costCentres:      [MakeCostCentre()],
                projectSubContracts: []);

            // Act
            var result = await repo.GetPagedAnimalIncomeAsync(DefaultQuery(), TestProject, TestMonthFrom, TestMonthTo);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Data);
        }

        #endregion

        // â”€â”€ GetAdditionalIncomeAsync â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

        #region GetAdditionalIncomeAsync

        [Fact]
        public async Task GetAdditionalIncomeAsync_ExcludesAnimalAccountCodes()
        {
            // Arrange â€” LargeAnimals is an animal code, NOT additional
            var repo = CreateRepository(
                workgroups:       [MakeWorkgroup()],
                projects:         [MakeProject()],
                costCentres:      [MakeCostCentre()],
                monthlyOutputs:   [MakeMonthlyOutput()],
                testRequirements: [MakeTestRequirement()],
                projectSubContracts: [MakeProjectSubContract(acctCode: "LargeAnimals")]);

            // Act
            var result = await repo.GetAdditionalIncomeAsync(TestProject, TestMonthFrom, TestMonthTo);

            // Assert â€” excluded because account code is in AnimalAcctCodes
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAdditionalIncomeAsync_IncludesNonAnimalAccountCodes()
        {
            // Arrange â€” "Consumables" is NOT in AnimalAcctCodes â†’ additional
            var repo = CreateRepository(
                workgroups:       [MakeWorkgroup()],
                projects:         [MakeProject()],
                costCentres:      [MakeCostCentre()],
                monthlyOutputs:   [MakeMonthlyOutput()],
                testRequirements: [MakeTestRequirement()],
                projectSubContracts: [MakeProjectSubContract(acctCode: "Consumables")]);

            // Act
            var result = await repo.GetAdditionalIncomeAsync(TestProject, TestMonthFrom, TestMonthTo);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetAdditionalIncomeAsync_EmptyResult_WhenNoAdditionalCosts()
        {
            // Arrange
            var repo = CreateRepository(
                workgroups:       [MakeWorkgroup()],
                projects:         [MakeProject()],
                costCentres:      [MakeCostCentre()],
                projectSubContracts: []);

            // Act
            var result = await repo.GetAdditionalIncomeAsync(TestProject, TestMonthFrom, TestMonthTo);

            // Assert
            Assert.Empty(result);
        }

        #endregion

        // â”€â”€ GetPagedAdditionalIncomeAsync â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

        #region GetPagedAdditionalIncomeAsync

        [Fact]
        public async Task GetPagedAdditionalIncomeAsync_EmptyData_ReturnsEmptyPage()
        {
            // Arrange
            var repo = CreateRepository(
                workgroups:       [MakeWorkgroup()],
                projects:         [MakeProject()],
                costCentres:      [MakeCostCentre()],
                projectSubContracts: []);

            // Act
            var result = await repo.GetPagedAdditionalIncomeAsync(DefaultQuery(), TestProject, TestMonthFrom, TestMonthTo);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Data);
        }

        #endregion

        // â”€â”€ GetTotalsAsync â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

        #region GetTotalsAsync

        [Fact]
        public async Task GetTotalsAsync_WithMatchingProject_ReturnsTotals()
        {
            // Arrange â€” seed all cost-type data so totals can be aggregated
            var repo = CreateRepository(
                timeCostCalcs:    [MakeTimeCostCalc()],
                workgroups:       [MakeWorkgroup()],
                projects:         [MakeProject()],
                workGroupEmployees: [MakeEmployee()],
                costCentres:      [MakeCostCentre()],
                monthlyOutputs:   [MakeMonthlyOutput()],
                testRequirements: [MakeTestRequirement()],
                projectSubContracts: [MakeProjectSubContract(acctCode: "Consumables")]);

            // Act
            var result = await repo.GetTotalsAsync(TestProject, TestMonthFrom, TestMonthTo);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetTotalsAsync_EmptyData_ReturnsEmpty()
        {
            // Arrange
            var repo = CreateRepository(
                timeCostCalcs:    [],
                workgroups:       [],
                projects:         [],
                workGroupEmployees: [],
                costCentres:      [],
                monthlyOutputs:   [],
                testRequirements: [],
                projectSubContracts: []);

            // Act
            var result = await repo.GetTotalsAsync(TestProject, TestMonthFrom, TestMonthTo);

            // Assert
            Assert.Empty(result);
        }

        #endregion

        // â”€â”€ GetTimeIncomeCurrentAsync â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

        #region GetTimeIncomeCurrentAsync

        [Fact]
        public async Task GetTimeIncomeCurrentAsync_WithMatchingProject_ReturnsRows()
        {
            // Arrange
            var repo = CreateRepository(
                timeCostCalcs:      [MakeTimeCostCalc()],
                workgroups:         [MakeWorkgroup()],
                projects:           [MakeProject()],
                workGroupEmployees: [MakeEmployee()],
                costCentres:        [MakeCostCentre()]);

            // Act
            var result = await repo.GetTimeIncomeCurrentAsync(TestProject, TestMonthFrom, TestMonthTo);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(TestProject, result[0].Project);
        }

        [Fact]
        public async Task GetTimeIncomeCurrentAsync_NullProject_ReturnsAllProjects()
        {
            // Arrange
            var repo = CreateRepository(
                timeCostCalcs:      [MakeTimeCostCalc("WG1", "PROJ1"), MakeTimeCostCalc("WG1", "PROJ2")],
                workgroups:         [MakeWorkgroup()],
                projects:           [MakeProject("PROJ1"), MakeProject("PROJ2")],
                workGroupEmployees: [MakeEmployee()],
                costCentres:        [MakeCostCentre()]);

            // Act
            var result = await repo.GetTimeIncomeCurrentAsync(null, TestMonthFrom, TestMonthTo);

            // Assert
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetTimeIncomeCurrentAsync_EmptyResult_WhenNoData()
        {
            // Arrange
            var repo = CreateRepository(
                timeCostCalcs:      [],
                workgroups:         [MakeWorkgroup()],
                projects:           [MakeProject()],
                workGroupEmployees: [],
                costCentres:        [MakeCostCentre()]);

            // Act
            var result = await repo.GetTimeIncomeCurrentAsync(TestProject, TestMonthFrom, TestMonthTo);

            // Assert
            Assert.Empty(result);
        }

        #endregion

        // â”€â”€ GetTestIncomeCurrentAsync â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

        #region GetTestIncomeCurrentAsync

        [Fact]
        public async Task GetTestIncomeCurrentAsync_WithMatchingProject_ReturnsRows()
        {
            // Arrange
            var repo = CreateRepository(
                workgroups:       [MakeWorkgroup()],
                projects:         [MakeProject()],
                costCentres:      [MakeCostCentre()],
                monthlyOutputs:   [MakeMonthlyOutput()],
                testRequirements: [MakeTestRequirement()]);

            // Act
            var result = await repo.GetTestIncomeCurrentAsync(TestProject, TestMonthFrom, TestMonthTo);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
        }

        [Fact]
        public async Task GetTestIncomeCurrentAsync_EmptyResult_WhenNoData()
        {
            // Arrange
            var repo = CreateRepository(
                workgroups:       [MakeWorkgroup()],
                projects:         [MakeProject()],
                costCentres:      [MakeCostCentre()],
                monthlyOutputs:   [],
                testRequirements: [MakeTestRequirement()]);

            // Act
            var result = await repo.GetTestIncomeCurrentAsync(TestProject, TestMonthFrom, TestMonthTo);

            // Assert
            Assert.Empty(result);
        }

        #endregion

        // â”€â”€ GetAnimalIncomeCurrentAsync â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

        #region GetAnimalIncomeCurrentAsync

        [Fact]
        public async Task GetAnimalIncomeCurrentAsync_ExcludesNonAnimalAccountCodes()
        {
            // Arrange â€” "Consumables" is NOT an animal code
            var repo = CreateRepository(
                workgroups:       [MakeWorkgroup()],
                projects:         [MakeProject()],
                costCentres:      [MakeCostCentre()],
                monthlyOutputs:   [MakeMonthlyOutput()],
                testRequirements: [MakeTestRequirement()],
                projectSubContracts: [MakeProjectSubContract(acctCode: "Consumables")]);

            // Act
            var result = await repo.GetAnimalIncomeCurrentAsync(TestProject, TestMonthFrom, TestMonthTo);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAnimalIncomeCurrentAsync_EmptyResult_WhenNoData()
        {
            // Arrange
            var repo = CreateRepository(
                workgroups:       [MakeWorkgroup()],
                projects:         [MakeProject()],
                costCentres:      [MakeCostCentre()],
                projectSubContracts: []);

            // Act
            var result = await repo.GetAnimalIncomeCurrentAsync(TestProject, TestMonthFrom, TestMonthTo);

            // Assert
            Assert.Empty(result);
        }

        #endregion

        // â”€â”€ GetAdditionalIncomeCurrentAsync â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

        #region GetAdditionalIncomeCurrentAsync

        [Fact]
        public async Task GetAdditionalIncomeCurrentAsync_ExcludesAnimalAccountCodes()
        {
            // Arrange â€” LargeAnimals is an animal code â†’ excluded from additional
            var repo = CreateRepository(
                workgroups:       [MakeWorkgroup()],
                projects:         [MakeProject()],
                costCentres:      [MakeCostCentre()],
                monthlyOutputs:   [MakeMonthlyOutput()],
                testRequirements: [MakeTestRequirement()],
                projectSubContracts: [MakeProjectSubContract(acctCode: "LargeAnimals")]);

            // Act
            var result = await repo.GetAdditionalIncomeCurrentAsync(TestProject, TestMonthFrom, TestMonthTo);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAdditionalIncomeCurrentAsync_IncludesNonAnimalAccountCodes()
        {
            // Arrange â€” "Consumables" is NOT an animal code â†’ included
            var repo = CreateRepository(
                workgroups:       [MakeWorkgroup()],
                projects:         [MakeProject()],
                costCentres:      [MakeCostCentre()],
                monthlyOutputs:   [MakeMonthlyOutput()],
                testRequirements: [MakeTestRequirement()],
                projectSubContracts: [MakeProjectSubContract(acctCode: "Consumables")]);

            // Act
            var result = await repo.GetAdditionalIncomeCurrentAsync(TestProject, TestMonthFrom, TestMonthTo);

            // Assert
            Assert.NotNull(result);
        }

        #endregion

        // â”€â”€ GetTotalsCurrentAsync â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

        #region GetTotalsCurrentAsync

        [Fact]
        public async Task GetTotalsCurrentAsync_EmptyData_ReturnsEmpty()
        {
            // Arrange
            var repo = CreateRepository(
                timeCostCalcs:    [],
                workgroups:       [],
                projects:         [],
                workGroupEmployees: [],
                costCentres:      [],
                monthlyOutputs:   [],
                testRequirements: [],
                projectSubContracts: []);

            // Act
            var result = await repo.GetTotalsCurrentAsync(TestProject, TestMonthFrom, TestMonthTo);

            // Assert
            Assert.Empty(result);
        }

        #endregion

        // â”€â”€ GetPeriodsAsync â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

        #region GetPeriodsAsync

        [Fact]
        public async Task GetPeriodsAsync_ReturnsAllPeriods_WhenNoFilter()
        {
            // Arrange
            var repo = CreateRepository(
                periodLookups: [
                    MakePeriodLookup(1, "April", 4),
                    MakePeriodLookup(2, "May",   5),
                    MakePeriodLookup(3, "June",  6)
                ]);

            // Act
            var result = await repo.GetPeriodsAsync();

            // Assert
            Assert.Equal(3, result.Count);
        }

        [Fact]
        public async Task GetPeriodsAsync_FiltersByAccntsPeriod_WhenParamProvided()
        {
            // Arrange
            var repo = CreateRepository(
                periodLookups: [
                    MakePeriodLookup(1, "April", 4),
                    MakePeriodLookup(2, "May",   5)
                ]);

            // Act
            var result = await repo.GetPeriodsAsync(accntsPeriod: 1.0);

            // Assert
            Assert.Single(result);
            Assert.Equal("April", result[0].MonthName);
        }

        [Fact]
        public async Task GetPeriodsAsync_EmptyResult_WhenNoPeriods()
        {
            // Arrange
            var repo = CreateRepository(periodLookups: []);

            // Act
            var result = await repo.GetPeriodsAsync();

            // Assert
            Assert.Empty(result);
        }

        #endregion

        // â”€â”€ GetSnapshotPeriodsAsync â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

        #region GetSnapshotPeriodsAsync

        [Fact]
        public async Task GetSnapshotPeriodsAsync_ReturnsPeriodsSortedByEndPeriod()
        {
            // Arrange
            var repo = CreateRepository(
                periods: [
                    MakePeriod("Period2", TestFpsYear, 2),
                    MakePeriod("Period1", TestFpsYear, 1),
                    MakePeriod("Period3", TestFpsYear, 3)
                ]);

            // Act
            var result = await repo.GetSnapshotPeriodsAsync();

            // Assert
            Assert.Equal(3, result.Count);
            Assert.Equal(1, result[0].EndPeriod);
            Assert.Equal(2, result[1].EndPeriod);
            Assert.Equal(3, result[2].EndPeriod);
        }

        [Fact]
        public async Task GetSnapshotPeriodsAsync_FiltersByFpsYear()
        {
            // Arrange â€” one period for current year, one for another year
            var repo = CreateRepository(
                periods: [
                    MakePeriod("Period1", TestFpsYear, 1),
                    MakePeriod("Period1", TestFpsYear + 1, 1)
                ]);

            // Act
            var result = await repo.GetSnapshotPeriodsAsync();

            // Assert â€” only current year
            Assert.Single(result);
            Assert.Equal(TestFpsYear, result[0].FpsYear);
        }

        [Fact]
        public async Task GetSnapshotPeriodsAsync_EmptyResult_WhenNoPeriods()
        {
            // Arrange
            var repo = CreateRepository(periods: []);

            // Act
            var result = await repo.GetSnapshotPeriodsAsync();

            // Assert
            Assert.Empty(result);
        }

        #endregion

        // â”€â”€ UpdatePeriodLockedAsync â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

        #region UpdatePeriodLockedAsync

        [Fact]
        public async Task UpdatePeriodLockedAsync_MatchingPeriod_UpdatesPeriodLockedAndReturns1()
        {
            // Arrange
            var period = MakePeriod("Period1", TestFpsYear, 1, periodLocked: 0);
            var mockRequestContext = new Mock<IFpsRequestContext>();
            mockRequestContext.Setup(x => x.FpsYear).Returns(TestFpsYear);

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(mockRequestContext.Object);
            var periodsMockSet = RepositoryTestHelper.CreateMockDbSet(new[] { period });
            RepositoryTestHelper.SetupDbSetOperations(periodsMockSet);
            mockContext.Setup(x => x.Periods).Returns(periodsMockSet.Object);
            RepositoryTestHelper.SetupSaveChanges(mockContext, returnValue: 1);

            var repo = new ProjectDepartmentIncomeRepository(mockContext.Object, mockRequestContext.Object);

            // Act
            var result = await repo.UpdatePeriodLockedAsync("Period1", true);

            // Assert
            Assert.Equal(1, result);
        }

        [Fact]
        public async Task UpdatePeriodLockedAsync_NonMatchingPeriod_Returns0()
        {
            // Arrange
            var period = MakePeriod("Period1", TestFpsYear, 1, periodLocked: 0);
            var mockRequestContext = new Mock<IFpsRequestContext>();
            mockRequestContext.Setup(x => x.FpsYear).Returns(TestFpsYear);

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(mockRequestContext.Object);
            var periodsMockSet = RepositoryTestHelper.CreateMockDbSet(new[] { period });
            RepositoryTestHelper.SetupDbSetOperations(periodsMockSet);
            mockContext.Setup(x => x.Periods).Returns(periodsMockSet.Object);
            RepositoryTestHelper.SetupSaveChanges(mockContext, returnValue: 0);

            var repo = new ProjectDepartmentIncomeRepository(mockContext.Object, mockRequestContext.Object);

            // Act
            var result = await repo.UpdatePeriodLockedAsync("NoSuchPeriod", true);

            // Assert
            Assert.Equal(0, result);
        }

        #endregion
    }
}





