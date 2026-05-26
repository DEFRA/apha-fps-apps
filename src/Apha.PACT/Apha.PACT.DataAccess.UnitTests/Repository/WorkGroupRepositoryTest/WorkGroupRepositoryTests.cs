using Apha.Common.Helpers.Repository;
using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Interfaces;
using Apha.PACT.Core.Pagination;
using Apha.PACT.DataAccess.Data;
using Apha.PACT.DataAccess.Repository;
using Microsoft.EntityFrameworkCore;
using Moq;
using NSubstitute;

namespace Apha.PACT.DataAccess.UnitTests.Repository.WorkGroupRepositoryTest
{
    public class WorkGroupRepositoryTests
    {
        // ── Factories ────────────────────────────────────────────────────────

        /// <summary>
        /// WorkGroupRepository has no IFpsYearContext dependency and only reads data.
        /// All query logic (AsNoTracking, OrderBy, ToListAsync) is exercised through the mock DbSet.
        /// </summary>
        private static (
            WorkGroupRepository Repo,
            Mock<DbSet<WorkGroup>> WorkGroupsDbSet,
            Mock<FpsDbContext> Context)
            CreateRepositoryWithMocks(IEnumerable<WorkGroup> workGroups)
        {
            var fpsYearContext = Substitute.For<IFpsRequestContext>();
            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(fpsYearContext);
            var workGroupsMockSet = RepositoryTestHelper.CreateMockDbSet(workGroups);

            mockContext.Setup(x => x.WorkGroups).Returns(workGroupsMockSet.Object);

            var repo = new WorkGroupRepository(mockContext.Object);
            return (repo, workGroupsMockSet, mockContext);
        }

        private static WorkGroupRepository CreateRepository(IEnumerable<WorkGroup> workGroups)
            => CreateRepositoryWithMocks(workGroups).Repo;

        // ── Time-code repository factory (three-set join) ────────────────────

        private static PactWorkGroupGradeView GradeView(string wgGrade, string workGroup) =>
            new() { WgGrade = wgGrade, WorkGroup = workGroup };

        private static PactStaffView StaffView(string pactId, string name, string workGroupGrade) =>
            new() { PactId = pactId, Name = name, WorkGroupGrade = workGroupGrade };

        private static MonthlyTime TimeRecord(
            string pactStaffId,
            string parentProject,
            string timeCode,
            double month,
            double? hours = 8.0) =>
            new()
            {
                PactStaffId   = pactStaffId,
                ParentProject = parentProject,
                TimeCode      = timeCode,
                Month         = month,
                Hours         = hours
            };

        private static WorkGroupRepository CreateTimeCodeRepository(
            IEnumerable<PactWorkGroupGradeView> gradeViews,
            IEnumerable<PactStaffView>     staffViews,
            IEnumerable<MonthlyTime>            monthlyTimes)
        {
            var fpsRequestContext = Substitute.For<IFpsRequestContext>();
            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(fpsRequestContext);

            var gradeViewSet   = RepositoryTestHelper.CreateMockDbSet(gradeViews);
            var staffViewSet   = RepositoryTestHelper.CreateMockDbSet(staffViews);
            var monthlyTimeSet = RepositoryTestHelper.CreateMockDbSet(monthlyTimes);
            var workGroupSet   = RepositoryTestHelper.CreateMockDbSet(Enumerable.Empty<WorkGroup>());

            mockContext.Setup(x => x.PactWorkGroupGradeViews).Returns(gradeViewSet.Object);
            mockContext.Setup(x => x.PactStaffViews).Returns(staffViewSet.Object);
            mockContext.Setup(x => x.MonthlyTimes).Returns(monthlyTimeSet.Object);
            mockContext.Setup(x => x.WorkGroups).Returns(workGroupSet.Object);

            return new WorkGroupRepository(mockContext.Object);
        }

        /// <summary>Minimal single-row joined dataset.</summary>
        private static WorkGroupRepository CreateDefaultTimeCodeRepository(
            string workGroup     = "WG1",
            string wgGrade       = "G1",
            string pactId        = "S1",
            string name          = "Alice",
            string parentProject = "PP1",
            string timeCode      = "TC1",
            double month         = 1,
            double? hours        = 8.0) =>
            CreateTimeCodeRepository(
                [GradeView(wgGrade, workGroup)],
                [StaffView(pactId, name, wgGrade)],
                [TimeRecord(pactId, parentProject, timeCode, month, hours)]);

        #region GetAllWorkGroupsAsync

        [Fact]
        public async Task GetAllWorkGroupsAsync_WithData_ReturnsOrderedList()
        {
            var workGroups = new List<WorkGroup>
            {
                new() { WorkGroupName = "ZGroup", ProfitCentre = "PC2" },
                new() { WorkGroupName = "AGroup", ProfitCentre = "PC1" },
                new() { WorkGroupName = "MGroup", ProfitCentre = "PC3" }
            };
            var repo = CreateRepository(workGroups);

            var result = (await repo.GetAllWorkGroupsAsync()).ToList();

            Assert.Equal(3, result.Count);
            // OrderBy(w => w.WorkGroupName) applied in repository
            Assert.Equal("AGroup", result[0].WorkGroupName);
            Assert.Equal("MGroup", result[1].WorkGroupName);
            Assert.Equal("ZGroup", result[2].WorkGroupName);
        }

        [Fact]
        public async Task GetAllWorkGroupsAsync_EmptyData_ReturnsEmptyList()
        {
            var repo = CreateRepository([]);

            var result = await repo.GetAllWorkGroupsAsync();

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllWorkGroupsAsync_SingleEntry_ReturnsSingleItem()
        {
            var workGroups = new List<WorkGroup>
            {
                new() { WorkGroupName = "WG1", ProfitCentre = "PC1" }
            };
            var repo = CreateRepository(workGroups);

            var result = (await repo.GetAllWorkGroupsAsync()).ToList();

            Assert.Single(result);
            Assert.Equal("WG1", result[0].WorkGroupName);
        }

        #endregion

        // ────────────────────────────────────────────────────────────────────
        #region GetWorkGroupTimeCodeAsync — join / projection
        // ────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task GetWorkGroupTimeCodeAsync_MatchingJoin_ReturnsProjectedRow()
        {
            var repo  = CreateDefaultTimeCodeRepository();
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetWorkGroupTimeCodeAsync(query, null, null);

            Assert.Single(result.Data);
            var row = result.Data.First();
            Assert.Equal("S1",    row.PACTStaffID);
            Assert.Equal("PP1",   row.ParentProject);
            Assert.Equal("WG1",   row.WorkGroup);
            Assert.Equal("Alice", row.Name);
            Assert.Equal("TC1",   row.TimeCode);
            Assert.Equal(1,       row.Month);
            Assert.Equal(8.0,     row.Hours);
        }

        [Fact]
        public async Task GetWorkGroupTimeCodeAsync_HoursIsNull_DefaultsToZero()
        {
            var repo  = CreateDefaultTimeCodeRepository(hours: null);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetWorkGroupTimeCodeAsync(query, null, null);

            Assert.Single(result.Data);
            Assert.Equal(0, result.Data.First().Hours);
        }

        [Fact]
        public async Task GetWorkGroupTimeCodeAsync_GradeMismatch_ReturnsEmpty()
        {
            var repo = CreateTimeCodeRepository(
                [GradeView("G1", "WG1")],
                [StaffView("S1", "Alice", "G_DIFFERENT")],
                [TimeRecord("S1", "PP1", "TC1", 1)]);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetWorkGroupTimeCodeAsync(query, null, null);

            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetWorkGroupTimeCodeAsync_StaffIdMismatch_ReturnsEmpty()
        {
            var repo = CreateTimeCodeRepository(
                [GradeView("G1", "WG1")],
                [StaffView("S1", "Alice", "G1")],
                [TimeRecord("S_DIFFERENT", "PP1", "TC1", 1)]);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetWorkGroupTimeCodeAsync(query, null, null);

            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetWorkGroupTimeCodeAsync_EmptyAllSets_ReturnsEmpty()
        {
            var repo  = CreateTimeCodeRepository([], [], []);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetWorkGroupTimeCodeAsync(query, null, null);

            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetWorkGroupTimeCodeAsync_MultipleStaffSameGrade_ReturnsRowPerStaff()
        {
            var repo = CreateTimeCodeRepository(
                [GradeView("G1", "WG1")],
                [StaffView("S1", "Alice", "G1"), StaffView("S2", "Bob", "G1")],
                [TimeRecord("S1", "PP1", "TC1", 1), TimeRecord("S2", "PP2", "TC2", 2)]);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetWorkGroupTimeCodeAsync(query, null, null);

            Assert.Equal(2, result.Data.Count);
        }

        #endregion

        // ────────────────────────────────────────────────────────────────────
        #region GetWorkGroupTimeCodeAsync — workGroup filter
        // ────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task GetWorkGroupTimeCodeAsync_WorkGroupFilter_ReturnsMatchingRows()
        {
            var repo = CreateTimeCodeRepository(
                [GradeView("G1", "WG1"), GradeView("G2", "WG2")],
                [StaffView("S1", "Alice", "G1"), StaffView("S2", "Bob", "G2")],
                [TimeRecord("S1", "PP1", "TC1", 1), TimeRecord("S2", "PP2", "TC2", 2)]);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetWorkGroupTimeCodeAsync(query, "WG1", null);

            Assert.Single(result.Data);
            Assert.Equal("WG1", result.Data.First().WorkGroup);
        }

        [Fact]
        public async Task GetWorkGroupTimeCodeAsync_WorkGroupFilter_NoMatch_ReturnsEmpty()
        {
            var repo  = CreateDefaultTimeCodeRepository(workGroup: "WG1");
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetWorkGroupTimeCodeAsync(query, "WG_MISSING", null);

            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetWorkGroupTimeCodeAsync_NullWorkGroupFilter_ReturnsAllRows()
        {
            var repo = CreateTimeCodeRepository(
                [GradeView("G1", "WG1"), GradeView("G2", "WG2")],
                [StaffView("S1", "Alice", "G1"), StaffView("S2", "Bob", "G2")],
                [TimeRecord("S1", "PP1", "TC1", 1), TimeRecord("S2", "PP2", "TC2", 2)]);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetWorkGroupTimeCodeAsync(query, null, null);

            Assert.Equal(2, result.Data.Count);
        }

        [Fact]
        public async Task GetWorkGroupTimeCodeAsync_WhitespaceWorkGroupFilter_ReturnsAllRows()
        {
            var repo = CreateTimeCodeRepository(
                [GradeView("G1", "WG1"), GradeView("G2", "WG2")],
                [StaffView("S1", "Alice", "G1"), StaffView("S2", "Bob", "G2")],
                [TimeRecord("S1", "PP1", "TC1", 1), TimeRecord("S2", "PP2", "TC2", 2)]);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetWorkGroupTimeCodeAsync(query, "   ", null);

            Assert.Equal(2, result.Data.Count);
        }

        #endregion

        // ────────────────────────────────────────────────────────────────────
        #region GetWorkGroupTimeCodeAsync — monthNumber filter
        // ────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task GetWorkGroupTimeCodeAsync_MonthNumberFilter_ReturnsMatchingRows()
        {
            var repo = CreateTimeCodeRepository(
                [GradeView("G1", "WG1")],
                [StaffView("S1", "Alice", "G1")],
                [TimeRecord("S1", "PP1", "TC1", 1), TimeRecord("S1", "PP1", "TC2", 2)]);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetWorkGroupTimeCodeAsync(query, null, 1);

            Assert.Single(result.Data);
            Assert.Equal(1, result.Data.First().Month);
        }

        [Fact]
        public async Task GetWorkGroupTimeCodeAsync_MonthNumberFilter_NoMatch_ReturnsEmpty()
        {
            var repo  = CreateDefaultTimeCodeRepository(month: 3);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetWorkGroupTimeCodeAsync(query, null, 99);

            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetWorkGroupTimeCodeAsync_NullMonthNumber_ReturnsAllRows()
        {
            var repo = CreateTimeCodeRepository(
                [GradeView("G1", "WG1")],
                [StaffView("S1", "Alice", "G1")],
                [TimeRecord("S1", "PP1", "TC1", 1), TimeRecord("S1", "PP1", "TC2", 2)]);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetWorkGroupTimeCodeAsync(query, null, null);

            Assert.Equal(2, result.Data.Count);
        }

        #endregion

        // ────────────────────────────────────────────────────────────────────
        #region GetWorkGroupTimeCodeAsync — column filter (ApplyWorkGroupTimeCodeFilter)
        // ────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task GetWorkGroupTimeCodeAsync_NullFilter_ReturnsAllRows()
        {
            var repo  = CreateDefaultTimeCodeRepository();
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = null };

            var result = await repo.GetWorkGroupTimeCodeAsync(query, null, null);

            Assert.Single(result.Data);
        }

        [Fact]
        public async Task GetWorkGroupTimeCodeAsync_EmptyJsonFilter_ReturnsAllRows()
        {
            var repo  = CreateDefaultTimeCodeRepository();
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = "{}" };

            var result = await repo.GetWorkGroupTimeCodeAsync(query, null, null);

            Assert.Single(result.Data);
        }

        [Fact]
        public async Task GetWorkGroupTimeCodeAsync_FilterByPACTStaffID_PartialMatch_ReturnsRow()
        {
            var repo = CreateTimeCodeRepository(
                [GradeView("G1", "WG1")],
                [StaffView("STAFF001", "Alice", "G1"), StaffView("STAFF002", "Bob", "G1")],
                [TimeRecord("STAFF001", "PP1", "TC1", 1), TimeRecord("STAFF002", "PP2", "TC2", 2)]);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                Filter = "{\"PACTStaffID\":\"001\"}"
            };

            var result = await repo.GetWorkGroupTimeCodeAsync(query, null, null);

            Assert.Single(result.Data);
            Assert.Equal("STAFF001", result.Data.First().PACTStaffID);
        }

        [Fact]
        public async Task GetWorkGroupTimeCodeAsync_FilterByPACTStaffID_NoMatch_ReturnsEmpty()
        {
            var repo  = CreateDefaultTimeCodeRepository(pactId: "STAFF001");
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                Filter = "{\"PACTStaffID\":\"NOMATCH\"}"
            };

            var result = await repo.GetWorkGroupTimeCodeAsync(query, null, null);

            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetWorkGroupTimeCodeAsync_FilterByName_PartialMatch_ReturnsRow()
        {
            var repo = CreateTimeCodeRepository(
                [GradeView("G1", "WG1")],
                [StaffView("S1", "Alice Smith", "G1"), StaffView("S2", "Bob Jones", "G1")],
                [TimeRecord("S1", "PP1", "TC1", 1), TimeRecord("S2", "PP2", "TC2", 2)]);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                Filter = "{\"Name\":\"Alice\"}"
            };

            var result = await repo.GetWorkGroupTimeCodeAsync(query, null, null);

            Assert.Single(result.Data);
            Assert.Equal("Alice Smith", result.Data.First().Name);
        }

        [Fact]
        public async Task GetWorkGroupTimeCodeAsync_FilterByName_NoMatch_ReturnsEmpty()
        {
            var repo  = CreateDefaultTimeCodeRepository(name: "Alice");
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                Filter = "{\"Name\":\"NOMATCH\"}"
            };

            var result = await repo.GetWorkGroupTimeCodeAsync(query, null, null);

            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetWorkGroupTimeCodeAsync_FilterByWorkGroup_PartialMatch_ReturnsRow()
        {
            var repo = CreateTimeCodeRepository(
                [GradeView("G1", "WG_ALPHA"), GradeView("G2", "WG_BETA")],
                [StaffView("S1", "Alice", "G1"), StaffView("S2", "Bob", "G2")],
                [TimeRecord("S1", "PP1", "TC1", 1), TimeRecord("S2", "PP2", "TC2", 2)]);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                Filter = "{\"WorkGroup\":\"ALPHA\"}"
            };

            var result = await repo.GetWorkGroupTimeCodeAsync(query, null, null);

            Assert.Single(result.Data);
            Assert.Equal("WG_ALPHA", result.Data.First().WorkGroup);
        }

        [Fact]
        public async Task GetWorkGroupTimeCodeAsync_FilterByWorkGroup_NoMatch_ReturnsEmpty()
        {
            var repo  = CreateDefaultTimeCodeRepository(workGroup: "WG1");
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                Filter = "{\"WorkGroup\":\"NOMATCH\"}"
            };

            var result = await repo.GetWorkGroupTimeCodeAsync(query, null, null);

            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetWorkGroupTimeCodeAsync_FilterByParentProject_PartialMatch_ReturnsRow()
        {
            var repo = CreateTimeCodeRepository(
                [GradeView("G1", "WG1")],
                [StaffView("S1", "Alice", "G1"), StaffView("S2", "Bob", "G1")],
                [TimeRecord("S1", "PROJECT_A", "TC1", 1), TimeRecord("S2", "PROJECT_B", "TC2", 2)]);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                Filter = "{\"ParentProject\":\"PROJECT_A\"}"
            };

            var result = await repo.GetWorkGroupTimeCodeAsync(query, null, null);

            Assert.Single(result.Data);
            Assert.Equal("PROJECT_A", result.Data.First().ParentProject);
        }

        [Fact]
        public async Task GetWorkGroupTimeCodeAsync_FilterByParentProject_NoMatch_ReturnsEmpty()
        {
            var repo  = CreateDefaultTimeCodeRepository(parentProject: "PP1");
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                Filter = "{\"ParentProject\":\"NOMATCH\"}"
            };

            var result = await repo.GetWorkGroupTimeCodeAsync(query, null, null);

            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetWorkGroupTimeCodeAsync_FilterByTimeCode_PartialMatch_ReturnsRow()
        {
            var repo = CreateTimeCodeRepository(
                [GradeView("G1", "WG1")],
                [StaffView("S1", "Alice", "G1"), StaffView("S2", "Bob", "G1")],
                [TimeRecord("S1", "PP1", "TIME_ALPHA", 1), TimeRecord("S2", "PP2", "TIME_BETA", 2)]);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                Filter = "{\"TimeCode\":\"ALPHA\"}"
            };

            var result = await repo.GetWorkGroupTimeCodeAsync(query, null, null);

            Assert.Single(result.Data);
            Assert.Equal("TIME_ALPHA", result.Data.First().TimeCode);
        }

        [Fact]
        public async Task GetWorkGroupTimeCodeAsync_FilterByTimeCode_NoMatch_ReturnsEmpty()
        {
            var repo  = CreateDefaultTimeCodeRepository(timeCode: "TC1");
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                Filter = "{\"TimeCode\":\"NOMATCH\"}"
            };

            var result = await repo.GetWorkGroupTimeCodeAsync(query, null, null);

            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetWorkGroupTimeCodeAsync_MultipleColumnFilters_AllApplied()
        {
            var repo = CreateTimeCodeRepository(
                [GradeView("G1", "WG1")],
                [StaffView("S1", "Alice Smith", "G1"), StaffView("S2", "Bob Jones", "G1")],
                [TimeRecord("S1", "PP_SHARED", "TC1", 1), TimeRecord("S2", "PP_SHARED", "TC2", 2)]);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                Filter = "{\"PACTStaffID\":\"S1\",\"Name\":\"Alice\"}"
            };

            var result = await repo.GetWorkGroupTimeCodeAsync(query, null, null);

            Assert.Single(result.Data);
            Assert.Equal("S1", result.Data.First().PACTStaffID);
        }

        #endregion

        // ────────────────────────────────────────────────────────────────────
        #region GetWorkGroupTimeCodeAsync — sorting
        // ────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task GetWorkGroupTimeCodeAsync_NoSortBy_DefaultsToOrderByName()
        {
            var repo = CreateTimeCodeRepository(
                [GradeView("G1", "WG1")],
                [StaffView("S1", "Zara", "G1"), StaffView("S2", "Amy", "G1")],
                [TimeRecord("S1", "PP1", "TC1", 1), TimeRecord("S2", "PP2", "TC2", 2)]);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetWorkGroupTimeCodeAsync(query, null, null);

            Assert.Equal("Amy",  result.Data.ElementAt(0).Name);
            Assert.Equal("Zara", result.Data.ElementAt(1).Name);
        }

        [Fact]
        public async Task GetWorkGroupTimeCodeAsync_SortByNameAscending_ReturnsSortedRows()
        {
            var repo = CreateTimeCodeRepository(
                [GradeView("G1", "WG1")],
                [StaffView("S1", "Zara", "G1"), StaffView("S2", "Amy", "G1")],
                [TimeRecord("S1", "PP1", "TC1", 1), TimeRecord("S2", "PP2", "TC2", 2)]);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                SortBy = "Name", Descending = false
            };

            var result = await repo.GetWorkGroupTimeCodeAsync(query, null, null);

            Assert.Equal("Amy",  result.Data.ElementAt(0).Name);
            Assert.Equal("Zara", result.Data.ElementAt(1).Name);
        }

        [Fact]
        public async Task GetWorkGroupTimeCodeAsync_SortByNameDescending_ReturnsSortedRows()
        {
            var repo = CreateTimeCodeRepository(
                [GradeView("G1", "WG1")],
                [StaffView("S1", "Zara", "G1"), StaffView("S2", "Amy", "G1")],
                [TimeRecord("S1", "PP1", "TC1", 1), TimeRecord("S2", "PP2", "TC2", 2)]);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                SortBy = "Name", Descending = true
            };

            var result = await repo.GetWorkGroupTimeCodeAsync(query, null, null);

            Assert.Equal("Zara", result.Data.ElementAt(0).Name);
            Assert.Equal("Amy",  result.Data.ElementAt(1).Name);
        }

        [Fact]
        public async Task GetWorkGroupTimeCodeAsync_SortByHoursAscending_ReturnsSortedRows()
        {
            var repo = CreateTimeCodeRepository(
                [GradeView("G1", "WG1")],
                [StaffView("S1", "Alice", "G1"), StaffView("S2", "Bob", "G1")],
                [TimeRecord("S1", "PP1", "TC1", 1, hours: 5), TimeRecord("S2", "PP2", "TC2", 1, hours: 2)]);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                SortBy = "Hours", Descending = false
            };

            var result = await repo.GetWorkGroupTimeCodeAsync(query, null, null);

            Assert.Equal(2.0, result.Data.ElementAt(0).Hours);
            Assert.Equal(5.0, result.Data.ElementAt(1).Hours);
        }

        [Fact]
        public async Task GetWorkGroupTimeCodeAsync_SortByHoursDescending_ReturnsSortedRows()
        {
            var repo = CreateTimeCodeRepository(
                [GradeView("G1", "WG1")],
                [StaffView("S1", "Alice", "G1"), StaffView("S2", "Bob", "G1")],
                [TimeRecord("S1", "PP1", "TC1", 1, hours: 5), TimeRecord("S2", "PP2", "TC2", 1, hours: 2)]);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                SortBy = "Hours", Descending = true
            };

            var result = await repo.GetWorkGroupTimeCodeAsync(query, null, null);

            Assert.Equal(5.0, result.Data.ElementAt(0).Hours);
            Assert.Equal(2.0, result.Data.ElementAt(1).Hours);
        }

        #endregion

        // ────────────────────────────────────────────────────────────────────
        #region GetWorkGroupTimeCodeAsync — paging
        // ────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task GetWorkGroupTimeCodeAsync_PaginationFirstPage_ReturnsCorrectSlice()
        {
            var repo = CreateTimeCodeRepository(
                [GradeView("G1", "WG1")],
                [StaffView("S1", "A", "G1"), StaffView("S2", "B", "G1"), StaffView("S3", "C", "G1")],
                [TimeRecord("S1", "PP1", "TC1", 1), TimeRecord("S2", "PP2", "TC2", 2), TimeRecord("S3", "PP3", "TC3", 3)]);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 2 };

            var result = await repo.GetWorkGroupTimeCodeAsync(query, null, null);

            Assert.Equal(2, result.Data.Count);
            Assert.Equal(3, result.PaginationData.TotalRecords);
            Assert.Equal(2, result.PaginationData.TotalPages);
        }

        [Fact]
        public async Task GetWorkGroupTimeCodeAsync_PaginationSecondPage_ReturnsRemainingItems()
        {
            var repo = CreateTimeCodeRepository(
                [GradeView("G1", "WG1")],
                [StaffView("S1", "A", "G1"), StaffView("S2", "B", "G1"), StaffView("S3", "C", "G1")],
                [TimeRecord("S1", "PP1", "TC1", 1), TimeRecord("S2", "PP2", "TC2", 2), TimeRecord("S3", "PP3", "TC3", 3)]);
            var query = new PaginationParameters<string> { Page = 2, PageSize = 2 };

            var result = await repo.GetWorkGroupTimeCodeAsync(query, null, null);

            Assert.Single(result.Data);
            Assert.Equal(3, result.PaginationData.TotalRecords);
        }

        #endregion

        // ────────────────────────────────────────────────────────────────────
        #region GetWorkGroupTimeCodeAsync — combined filters
        // ────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task GetWorkGroupTimeCodeAsync_WorkGroupAndMonthFilter_NarrowsResults()
        {
            var repo = CreateTimeCodeRepository(
                [GradeView("G1", "WG1"), GradeView("G2", "WG2")],
                [StaffView("S1", "Alice", "G1"), StaffView("S2", "Bob", "G2")],
                [TimeRecord("S1", "PP1", "TC1", 1), TimeRecord("S1", "PP1", "TC1", 2), TimeRecord("S2", "PP2", "TC2", 1)]);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetWorkGroupTimeCodeAsync(query, "WG1", 1);

            Assert.Single(result.Data);
            Assert.Equal("WG1", result.Data.First().WorkGroup);
            Assert.Equal(1,     result.Data.First().Month);
        }

        [Fact]
        public async Task GetWorkGroupTimeCodeAsync_AllFiltersApplied_ReturnsCorrectRow()
        {
            var repo = CreateTimeCodeRepository(
                [GradeView("G1", "WG_ALPHA"), GradeView("G2", "WG_BETA")],
                [StaffView("S1", "Alice Smith", "G1"), StaffView("S2", "Bob Jones", "G2")],
                [TimeRecord("S1", "PROJ_A", "TC_X", 3, hours: 7.5), TimeRecord("S2", "PROJ_B", "TC_Y", 4, hours: 4)]);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                Filter = "{\"PACTStaffID\":\"S1\",\"Name\":\"Alice\",\"ParentProject\":\"PROJ\",\"TimeCode\":\"TC_X\"}"
            };

            var result = await repo.GetWorkGroupTimeCodeAsync(query, "WG_ALPHA", 3);

            Assert.Single(result.Data);
            var row = result.Data.First();
            Assert.Equal("S1",       row.PACTStaffID);
            Assert.Equal("PROJ_A",   row.ParentProject);
            Assert.Equal("WG_ALPHA", row.WorkGroup);
            Assert.Equal("TC_X",     row.TimeCode);
            Assert.Equal(7.5,        row.Hours);
        }

        #endregion

        // ════════════════════════════════════════════════════════════════════
        // GetWorkGroupValidTimeCodeAsync
        // ════════════════════════════════════════════════════════════════════

        // ── Factory helpers ──────────────────────────────────────────────────

        private static TimeCodeValid ValidTimeCode(
            string workGroup     = "WG1",
            string timeCode      = "TC1",
            string parentProject = "PP1",
            bool   active        = true) =>
            new()
            {
                WorkGroup     = workGroup,
                TimeCode      = timeCode,
                ParentProject = parentProject,
                Active        = active
            };

        private static Project ProjectRecord(
            string parentProject = "PP1",
            string manager       = "MGR1") =>
            new()
            {
                ParentProject = parentProject,
                Manager       = manager,
                ProjectTitle  = "Title",
                Program       = "Prog",
                Customer      = "Cust",
                ProjectStatus = "Open",
                Disease       = "D",
                Contract      = "C",
                IsDefraProject = 0
            };

        private static WorkGroupRepository CreateValidTimeCodeRepository(
            IEnumerable<TimeCodeValid> timeCodeValids,
            IEnumerable<Project>       projects)
        {
            var fpsRequestContext = Substitute.For<IFpsRequestContext>();
            var mockContext       = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(fpsRequestContext);

            var timeCodeSet  = RepositoryTestHelper.CreateMockDbSet(timeCodeValids);
            var projectSet   = RepositoryTestHelper.CreateMockDbSet(projects);
            var workGroupSet = RepositoryTestHelper.CreateMockDbSet(Enumerable.Empty<WorkGroup>());

            mockContext.Setup(x => x.TimeCodeValids).Returns(timeCodeSet.Object);
            mockContext.Setup(x => x.Projects).Returns(projectSet.Object);
            mockContext.Setup(x => x.WorkGroups).Returns(workGroupSet.Object);

            return new WorkGroupRepository(mockContext.Object);
        }

        private static WorkGroupRepository CreateDefaultValidTimeCodeRepository(
            string workGroup     = "WG1",
            string timeCode      = "TC1",
            string parentProject = "PP1",
            bool   active        = true,
            string manager       = "MGR1") =>
            CreateValidTimeCodeRepository(
                [ValidTimeCode(workGroup, timeCode, parentProject, active)],
                [ProjectRecord(parentProject, manager)]);

        // ────────────────────────────────────────────────────────────────────
        #region GetWorkGroupValidTimeCodeAsync — join / projection
        // ────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task GetWorkGroupValidTimeCodeAsync_MatchingJoin_ReturnsProjectedRow()
        {
            var repo  = CreateDefaultValidTimeCodeRepository();
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetWorkGroupValidTimeCodeAsync(query, "WG1");

            Assert.Single(result.Data);
            var row = result.Data.First();
            Assert.Equal("WG1",  row.WorkGroup);
            Assert.Equal("TC1",  row.TimeCode);
            Assert.Equal("PP1",  row.ParentProject);
            Assert.Equal("MGR1", row.Manager);
            Assert.True(row.Active);
        }

        [Fact]
        public async Task GetWorkGroupValidTimeCodeAsync_ActiveFalse_ProjectedCorrectly()
        {
            var repo  = CreateDefaultValidTimeCodeRepository(active: false);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetWorkGroupValidTimeCodeAsync(query, "WG1");

            Assert.Single(result.Data);
            Assert.False(result.Data.First().Active);
        }

        [Fact]
        public async Task GetWorkGroupValidTimeCodeAsync_ManagerIsNull_ProjectedAsNull()
        {
            var repo = CreateValidTimeCodeRepository(
                [ValidTimeCode("WG1", "TC1", "PP1")],
                [ProjectRecord("PP1", null!)]);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetWorkGroupValidTimeCodeAsync(query, "WG1");

            Assert.Single(result.Data);
            Assert.Null(result.Data.First().Manager);
        }

        [Fact]
        public async Task GetWorkGroupValidTimeCodeAsync_ParentProjectMismatch_ReturnsEmpty()
        {
            var repo = CreateValidTimeCodeRepository(
                [ValidTimeCode("WG1", "TC1", "PP1")],
                [ProjectRecord("PP_DIFFERENT", "MGR1")]);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetWorkGroupValidTimeCodeAsync(query, "WG1");

            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetWorkGroupValidTimeCodeAsync_EmptyTimeCodeValids_ReturnsEmpty()
        {
            var repo  = CreateValidTimeCodeRepository([], [ProjectRecord()]);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetWorkGroupValidTimeCodeAsync(query, "WG1");

            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetWorkGroupValidTimeCodeAsync_EmptyProjects_ReturnsEmpty()
        {
            var repo  = CreateValidTimeCodeRepository([ValidTimeCode()], []);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetWorkGroupValidTimeCodeAsync(query, "WG1");

            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetWorkGroupValidTimeCodeAsync_MultipleMatchingRows_ReturnsAll()
        {
            var repo = CreateValidTimeCodeRepository(
                [ValidTimeCode("WG1", "TC1", "PP1"), ValidTimeCode("WG1", "TC2", "PP1")],
                [ProjectRecord("PP1", "MGR1")]);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetWorkGroupValidTimeCodeAsync(query, "WG1");

            Assert.Equal(2, result.Data.Count);
        }

        #endregion

        // ────────────────────────────────────────────────────────────────────
        #region GetWorkGroupValidTimeCodeAsync — workGroup filter
        // ────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task GetWorkGroupValidTimeCodeAsync_WorkGroupFilter_ReturnsOnlyMatchingRows()
        {
            var repo = CreateValidTimeCodeRepository(
                [ValidTimeCode("WG1", "TC1", "PP1"), ValidTimeCode("WG2", "TC2", "PP1")],
                [ProjectRecord("PP1", "MGR1")]);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetWorkGroupValidTimeCodeAsync(query, "WG1");

            Assert.Single(result.Data);
            Assert.Equal("WG1", result.Data.First().WorkGroup);
        }

        [Fact]
        public async Task GetWorkGroupValidTimeCodeAsync_WorkGroupFilter_NoMatch_ReturnsEmpty()
        {
            var repo  = CreateDefaultValidTimeCodeRepository(workGroup: "WG1");
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetWorkGroupValidTimeCodeAsync(query, "WG_MISSING");

            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetWorkGroupValidTimeCodeAsync_EmptyWorkGroupFilter_ReturnsAllRows()
        {
            var repo = CreateValidTimeCodeRepository(
                [ValidTimeCode("WG1", "TC1", "PP1"), ValidTimeCode("WG2", "TC2", "PP2")],
                [ProjectRecord("PP1", "M1"), ProjectRecord("PP2", "M2")]);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetWorkGroupValidTimeCodeAsync(query, "");

            Assert.Equal(2, result.Data.Count);
        }

        [Fact]
        public async Task GetWorkGroupValidTimeCodeAsync_WhitespaceWorkGroupFilter_ReturnsAllRows()
        {
            var repo = CreateValidTimeCodeRepository(
                [ValidTimeCode("WG1", "TC1", "PP1"), ValidTimeCode("WG2", "TC2", "PP2")],
                [ProjectRecord("PP1", "M1"), ProjectRecord("PP2", "M2")]);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetWorkGroupValidTimeCodeAsync(query, "   ");

            Assert.Equal(2, result.Data.Count);
        }

        #endregion

        // ────────────────────────────────────────────────────────────────────
        #region GetWorkGroupValidTimeCodeAsync — column filter (ApplyWorkGroupValidTimeCodeFilter)
        // ────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task GetWorkGroupValidTimeCodeAsync_NullFilter_ReturnsAllRows()
        {
            var repo  = CreateDefaultValidTimeCodeRepository();
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = null };

            var result = await repo.GetWorkGroupValidTimeCodeAsync(query, "WG1");

            Assert.Single(result.Data);
        }

        [Fact]
        public async Task GetWorkGroupValidTimeCodeAsync_EmptyJsonFilter_ReturnsAllRows()
        {
            var repo  = CreateDefaultValidTimeCodeRepository();
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10, Filter = "{}" };

            var result = await repo.GetWorkGroupValidTimeCodeAsync(query, "WG1");

            Assert.Single(result.Data);
        }

        [Fact]
        public async Task GetWorkGroupValidTimeCodeAsync_FilterByWorkGroup_PartialMatch_ReturnsRow()
        {
            var repo = CreateValidTimeCodeRepository(
                [ValidTimeCode("WG_ALPHA", "TC1", "PP1"), ValidTimeCode("WG_BETA", "TC2", "PP1")],
                [ProjectRecord("PP1", "MGR1")]);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                Filter = "{\"WorkGroup\":\"ALPHA\"}"
            };

            var result = await repo.GetWorkGroupValidTimeCodeAsync(query, "");

            Assert.Single(result.Data);
            Assert.Equal("WG_ALPHA", result.Data.First().WorkGroup);
        }

        [Fact]
        public async Task GetWorkGroupValidTimeCodeAsync_FilterByWorkGroup_NoMatch_ReturnsEmpty()
        {
            var repo  = CreateDefaultValidTimeCodeRepository(workGroup: "WG1");
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                Filter = "{\"WorkGroup\":\"NOMATCH\"}"
            };

            var result = await repo.GetWorkGroupValidTimeCodeAsync(query, "");

            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetWorkGroupValidTimeCodeAsync_FilterByTimeCode_PartialMatch_ReturnsRow()
        {
            var repo = CreateValidTimeCodeRepository(
                [ValidTimeCode("WG1", "TIME_ALPHA", "PP1"), ValidTimeCode("WG1", "TIME_BETA", "PP1")],
                [ProjectRecord("PP1", "MGR1")]);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                Filter = "{\"TimeCode\":\"ALPHA\"}"
            };

            var result = await repo.GetWorkGroupValidTimeCodeAsync(query, "");

            Assert.Single(result.Data);
            Assert.Equal("TIME_ALPHA", result.Data.First().TimeCode);
        }

        [Fact]
        public async Task GetWorkGroupValidTimeCodeAsync_FilterByTimeCode_NoMatch_ReturnsEmpty()
        {
            var repo  = CreateDefaultValidTimeCodeRepository(timeCode: "TC1");
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                Filter = "{\"TimeCode\":\"NOMATCH\"}"
            };

            var result = await repo.GetWorkGroupValidTimeCodeAsync(query, "");

            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetWorkGroupValidTimeCodeAsync_FilterByParentProject_PartialMatch_ReturnsRow()
        {
            var repo = CreateValidTimeCodeRepository(
                [ValidTimeCode("WG1", "TC1", "PROJECT_ALPHA"), ValidTimeCode("WG1", "TC2", "PROJECT_BETA")],
                [ProjectRecord("PROJECT_ALPHA", "MGR1"), ProjectRecord("PROJECT_BETA", "MGR2")]);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                Filter = "{\"ParentProject\":\"ALPHA\"}"
            };

            var result = await repo.GetWorkGroupValidTimeCodeAsync(query, "");

            Assert.Single(result.Data);
            Assert.Equal("PROJECT_ALPHA", result.Data.First().ParentProject);
        }

        [Fact]
        public async Task GetWorkGroupValidTimeCodeAsync_FilterByParentProject_NoMatch_ReturnsEmpty()
        {
            var repo  = CreateDefaultValidTimeCodeRepository(parentProject: "PP1");
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                Filter = "{\"ParentProject\":\"NOMATCH\"}"
            };

            var result = await repo.GetWorkGroupValidTimeCodeAsync(query, "");

            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetWorkGroupValidTimeCodeAsync_FilterByManager_PartialMatch_ReturnsRow()
        {
            var repo = CreateValidTimeCodeRepository(
                [ValidTimeCode("WG1", "TC1", "PP1"), ValidTimeCode("WG1", "TC2", "PP2")],
                [ProjectRecord("PP1", "MANAGER_JONES"), ProjectRecord("PP2", "MANAGER_SMITH")]);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                Filter = "{\"Manager\":\"JONES\"}"
            };

            var result = await repo.GetWorkGroupValidTimeCodeAsync(query, "");

            Assert.Single(result.Data);
            Assert.Equal("MANAGER_JONES", result.Data.First().Manager);
        }

        [Fact]
        public async Task GetWorkGroupValidTimeCodeAsync_FilterByManager_NoMatch_ReturnsEmpty()
        {
            var repo  = CreateDefaultValidTimeCodeRepository(manager: "MGR1");
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                Filter = "{\"Manager\":\"NOMATCH\"}"
            };

            var result = await repo.GetWorkGroupValidTimeCodeAsync(query, "");

            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetWorkGroupValidTimeCodeAsync_MultipleColumnFilters_AllApplied()
        {
            var repo = CreateValidTimeCodeRepository(
                [ValidTimeCode("WG1", "TC_X", "PROJ_A"), ValidTimeCode("WG1", "TC_Y", "PROJ_B")],
                [ProjectRecord("PROJ_A", "MANAGER_A"), ProjectRecord("PROJ_B", "MANAGER_B")]);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                Filter = "{\"TimeCode\":\"TC_X\",\"Manager\":\"MANAGER_A\"}"
            };

            var result = await repo.GetWorkGroupValidTimeCodeAsync(query, "");

            Assert.Single(result.Data);
            Assert.Equal("TC_X",      result.Data.First().TimeCode);
            Assert.Equal("MANAGER_A", result.Data.First().Manager);
        }

        #endregion

        // ────────────────────────────────────────────────────────────────────
        #region GetWorkGroupValidTimeCodeAsync — sorting
        // ────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task GetWorkGroupValidTimeCodeAsync_NoSortBy_DefaultsToOrderByParentProject()
        {
            var repo = CreateValidTimeCodeRepository(
                [ValidTimeCode("WG1", "TC1", "ZZ_PROJ"), ValidTimeCode("WG1", "TC2", "AA_PROJ")],
                [ProjectRecord("ZZ_PROJ", "M1"), ProjectRecord("AA_PROJ", "M2")]);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetWorkGroupValidTimeCodeAsync(query, "");

            Assert.Equal("AA_PROJ", result.Data.ElementAt(0).ParentProject);
            Assert.Equal("ZZ_PROJ", result.Data.ElementAt(1).ParentProject);
        }

        [Fact]
        public async Task GetWorkGroupValidTimeCodeAsync_SortByWorkGroupAscending_ReturnsSortedRows()
        {
            var repo = CreateValidTimeCodeRepository(
                [ValidTimeCode("WG_Z", "TC1", "PP1"), ValidTimeCode("WG_A", "TC2", "PP1")],
                [ProjectRecord("PP1", "MGR1")]);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                SortBy = "WorkGroup", Descending = false
            };

            var result = await repo.GetWorkGroupValidTimeCodeAsync(query, "");

            Assert.Equal("WG_A", result.Data.ElementAt(0).WorkGroup);
            Assert.Equal("WG_Z", result.Data.ElementAt(1).WorkGroup);
        }

        [Fact]
        public async Task GetWorkGroupValidTimeCodeAsync_SortByWorkGroupDescending_ReturnsSortedRows()
        {
            var repo = CreateValidTimeCodeRepository(
                [ValidTimeCode("WG_Z", "TC1", "PP1"), ValidTimeCode("WG_A", "TC2", "PP1")],
                [ProjectRecord("PP1", "MGR1")]);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                SortBy = "WorkGroup", Descending = true
            };

            var result = await repo.GetWorkGroupValidTimeCodeAsync(query, "");

            Assert.Equal("WG_Z", result.Data.ElementAt(0).WorkGroup);
            Assert.Equal("WG_A", result.Data.ElementAt(1).WorkGroup);
        }

        [Fact]
        public async Task GetWorkGroupValidTimeCodeAsync_SortByTimeCodeAscending_ReturnsSortedRows()
        {
            var repo = CreateValidTimeCodeRepository(
                [ValidTimeCode("WG1", "TC_Z", "PP1"), ValidTimeCode("WG1", "TC_A", "PP1")],
                [ProjectRecord("PP1", "MGR1")]);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                SortBy = "TimeCode", Descending = false
            };

            var result = await repo.GetWorkGroupValidTimeCodeAsync(query, "");

            Assert.Equal("TC_A", result.Data.ElementAt(0).TimeCode);
            Assert.Equal("TC_Z", result.Data.ElementAt(1).TimeCode);
        }

        [Fact]
        public async Task GetWorkGroupValidTimeCodeAsync_SortByTimeCodeDescending_ReturnsSortedRows()
        {
            var repo = CreateValidTimeCodeRepository(
                [ValidTimeCode("WG1", "TC_Z", "PP1"), ValidTimeCode("WG1", "TC_A", "PP1")],
                [ProjectRecord("PP1", "MGR1")]);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                SortBy = "TimeCode", Descending = true
            };

            var result = await repo.GetWorkGroupValidTimeCodeAsync(query, "");

            Assert.Equal("TC_Z", result.Data.ElementAt(0).TimeCode);
            Assert.Equal("TC_A", result.Data.ElementAt(1).TimeCode);
        }

        [Fact]
        public async Task GetWorkGroupValidTimeCodeAsync_SortByParentProjectAscending_ReturnsSortedRows()
        {
            var repo = CreateValidTimeCodeRepository(
                [ValidTimeCode("WG1", "TC1", "ZZ_PROJ"), ValidTimeCode("WG1", "TC2", "AA_PROJ")],
                [ProjectRecord("ZZ_PROJ", "M1"), ProjectRecord("AA_PROJ", "M2")]);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                SortBy = "ParentProject", Descending = false
            };

            var result = await repo.GetWorkGroupValidTimeCodeAsync(query, "");

            Assert.Equal("AA_PROJ", result.Data.ElementAt(0).ParentProject);
            Assert.Equal("ZZ_PROJ", result.Data.ElementAt(1).ParentProject);
        }

        [Fact]
        public async Task GetWorkGroupValidTimeCodeAsync_SortByParentProjectDescending_ReturnsSortedRows()
        {
            var repo = CreateValidTimeCodeRepository(
                [ValidTimeCode("WG1", "TC1", "ZZ_PROJ"), ValidTimeCode("WG1", "TC2", "AA_PROJ")],
                [ProjectRecord("ZZ_PROJ", "M1"), ProjectRecord("AA_PROJ", "M2")]);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                SortBy = "ParentProject", Descending = true
            };

            var result = await repo.GetWorkGroupValidTimeCodeAsync(query, "");

            Assert.Equal("ZZ_PROJ", result.Data.ElementAt(0).ParentProject);
            Assert.Equal("AA_PROJ", result.Data.ElementAt(1).ParentProject);
        }

        [Fact]
        public async Task GetWorkGroupValidTimeCodeAsync_SortByManagerAscending_ReturnsSortedRows()
        {
            var repo = CreateValidTimeCodeRepository(
                [ValidTimeCode("WG1", "TC1", "PP1"), ValidTimeCode("WG1", "TC2", "PP2")],
                [ProjectRecord("PP1", "MGR_Z"), ProjectRecord("PP2", "MGR_A")]);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                SortBy = "Manager", Descending = false
            };

            var result = await repo.GetWorkGroupValidTimeCodeAsync(query, "");

            Assert.Equal("MGR_A", result.Data.ElementAt(0).Manager);
            Assert.Equal("MGR_Z", result.Data.ElementAt(1).Manager);
        }

        [Fact]
        public async Task GetWorkGroupValidTimeCodeAsync_SortByManagerDescending_ReturnsSortedRows()
        {
            var repo = CreateValidTimeCodeRepository(
                [ValidTimeCode("WG1", "TC1", "PP1"), ValidTimeCode("WG1", "TC2", "PP2")],
                [ProjectRecord("PP1", "MGR_Z"), ProjectRecord("PP2", "MGR_A")]);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                SortBy = "Manager", Descending = true
            };

            var result = await repo.GetWorkGroupValidTimeCodeAsync(query, "");

            Assert.Equal("MGR_Z", result.Data.ElementAt(0).Manager);
            Assert.Equal("MGR_A", result.Data.ElementAt(1).Manager);
        }

        [Fact]
        public async Task GetWorkGroupValidTimeCodeAsync_UnknownSortByAscending_DefaultsToParentProjectAsc()
        {
            var repo = CreateValidTimeCodeRepository(
                [ValidTimeCode("WG1", "TC1", "ZZ_PROJ"), ValidTimeCode("WG1", "TC2", "AA_PROJ")],
                [ProjectRecord("ZZ_PROJ", "M1"), ProjectRecord("AA_PROJ", "M2")]);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                SortBy = "UnknownColumn", Descending = false
            };

            var result = await repo.GetWorkGroupValidTimeCodeAsync(query, "");

            Assert.Equal("AA_PROJ", result.Data.ElementAt(0).ParentProject);
            Assert.Equal("ZZ_PROJ", result.Data.ElementAt(1).ParentProject);
        }

        [Fact]
        public async Task GetWorkGroupValidTimeCodeAsync_UnknownSortByDescending_DefaultsToParentProjectDesc()
        {
            var repo = CreateValidTimeCodeRepository(
                [ValidTimeCode("WG1", "TC1", "ZZ_PROJ"), ValidTimeCode("WG1", "TC2", "AA_PROJ")],
                [ProjectRecord("ZZ_PROJ", "M1"), ProjectRecord("AA_PROJ", "M2")]);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                SortBy = "UnknownColumn", Descending = true
            };

            var result = await repo.GetWorkGroupValidTimeCodeAsync(query, "");

            Assert.Equal("ZZ_PROJ", result.Data.ElementAt(0).ParentProject);
            Assert.Equal("AA_PROJ", result.Data.ElementAt(1).ParentProject);
        }

        #endregion

        // ────────────────────────────────────────────────────────────────────
        #region GetWorkGroupValidTimeCodeAsync — paging
        // ────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task GetWorkGroupValidTimeCodeAsync_PaginationFirstPage_ReturnsCorrectSlice()
        {
            var repo = CreateValidTimeCodeRepository(
                [
                    ValidTimeCode("WG1", "TC1", "PP1"),
                    ValidTimeCode("WG1", "TC2", "PP1"),
                    ValidTimeCode("WG1", "TC3", "PP1")
                ],
                [ProjectRecord("PP1", "MGR1")]);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 2 };

            var result = await repo.GetWorkGroupValidTimeCodeAsync(query, "");

            Assert.Equal(2, result.Data.Count);
            Assert.Equal(3, result.PaginationData.TotalRecords);
            Assert.Equal(2, result.PaginationData.TotalPages);
        }

        [Fact]
        public async Task GetWorkGroupValidTimeCodeAsync_PaginationSecondPage_ReturnsRemainingItems()
        {
            var repo = CreateValidTimeCodeRepository(
                [
                    ValidTimeCode("WG1", "TC1", "PP1"),
                    ValidTimeCode("WG1", "TC2", "PP1"),
                    ValidTimeCode("WG1", "TC3", "PP1")
                ],
                [ProjectRecord("PP1", "MGR1")]);
            var query = new PaginationParameters<string> { Page = 2, PageSize = 2 };

            var result = await repo.GetWorkGroupValidTimeCodeAsync(query, "");

            Assert.Single(result.Data);
            Assert.Equal(3, result.PaginationData.TotalRecords);
        }

        #endregion

        // ────────────────────────────────────────────────────────────────────
        #region GetWorkGroupValidTimeCodeAsync — combined filters
        // ────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task GetWorkGroupValidTimeCodeAsync_WorkGroupAndColumnFilter_NarrowsResults()
        {
            var repo = CreateValidTimeCodeRepository(
                [ValidTimeCode("WG1", "TC_X", "PP1"), ValidTimeCode("WG1", "TC_Y", "PP1"), ValidTimeCode("WG2", "TC_X", "PP1")],
                [ProjectRecord("PP1", "MGR1")]);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                Filter = "{\"TimeCode\":\"TC_X\"}"
            };

            var result = await repo.GetWorkGroupValidTimeCodeAsync(query, "WG1");

            Assert.Single(result.Data);
            Assert.Equal("WG1",  result.Data.First().WorkGroup);
            Assert.Equal("TC_X", result.Data.First().TimeCode);
        }

        [Fact]
        public async Task GetWorkGroupValidTimeCodeAsync_AllFiltersApplied_ReturnsCorrectRow()
        {
            var repo = CreateValidTimeCodeRepository(
                [
                    ValidTimeCode("WG_ALPHA", "TC_X", "PROJ_A", true),
                    ValidTimeCode("WG_BETA",  "TC_Y", "PROJ_B", false)
                ],
                [ProjectRecord("PROJ_A", "MANAGER_JONES"), ProjectRecord("PROJ_B", "MANAGER_SMITH")]);
            var query = new PaginationParameters<string>
            {
                Page = 1, PageSize = 10,
                Filter = "{\"TimeCode\":\"TC_X\",\"ParentProject\":\"PROJ_A\",\"Manager\":\"JONES\"}"
            };

            var result = await repo.GetWorkGroupValidTimeCodeAsync(query, "WG_ALPHA");

            Assert.Single(result.Data);
            var row = result.Data.First();
            Assert.Equal("WG_ALPHA",       row.WorkGroup);
            Assert.Equal("TC_X",           row.TimeCode);
            Assert.Equal("PROJ_A",         row.ParentProject);
            Assert.Equal("MANAGER_JONES",  row.Manager);
            Assert.True(row.Active);
        }

        #endregion

        // ════════════════════════════════════════════════════════════════════
        // GetWorkGroupsByProfitCentreAsync
        // ════════════════════════════════════════════════════════════════════

        private static WorkGroupRepository CreateWorkGroupsByPcRepository(
            IEnumerable<WorkGroup> workGroups,
            int fpsYear = 2024)
        {
            var fpsRequestContext = Substitute.For<IFpsRequestContext>();
            fpsRequestContext.FpsYear.Returns(fpsYear);

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(fpsRequestContext);
            RepositoryTestHelper.SetupSaveChanges(mockContext);

            var mockSet = RepositoryTestHelper.CreateMockDbSet(workGroups);
            mockContext.Setup(x => x.WorkGroups).Returns(mockSet.Object);

            return new WorkGroupRepository(mockContext.Object);
        }

        #region GetWorkGroupsByProfitCentreAsync

        // NOTE: GetWorkGroupsByProfitCentreAsync uses EF.Property<T> for dynamic sorting.
        // EF.Property<T> is only valid inside a real EF provider query and throws
        // InvalidOperationException when evaluated by LINQ-to-Objects (mock DbSet).
        // Only the empty-result case (WHERE filters out all rows before sort) is testable
        // under the mock-only constraint. All other cases require an integration test.

        [Fact]
        public async Task GetWorkGroupsByProfitCentreAsync_NoMatchingProfitCentre_ReturnsEmpty()
        {
            var workGroups = new List<WorkGroup>
            {
                new() { WorkGroupName = "WG1", ProfitCentre = "PC01", FpsYear = 2024 }
            };
            var repo  = CreateWorkGroupsByPcRepository(workGroups);
            var query = new PaginationParameters<string> { Page = 1, PageSize = 10 };

            var result = await repo.GetWorkGroupsByProfitCentreAsync(query, "PC_MISSING");

            Assert.Empty(result.Data);
        }

        #endregion

        // ════════════════════════════════════════════════════════════════════
        // SetSendEmailForProfitCentreWorkGroupsAsync
        // ════════════════════════════════════════════════════════════════════

        private static WorkGroupRepository CreateSendEmailRepository(
            IEnumerable<WorkGroup>    workGroups,
            IEnumerable<ProfitCentre> profitCentres,
            int fpsYear = 2024)
        {
            var fpsRequestContext = Substitute.For<IFpsRequestContext>();
            fpsRequestContext.FpsYear.Returns(fpsYear);

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(fpsRequestContext);
            RepositoryTestHelper.SetupSaveChanges(mockContext);

            var wgSet = RepositoryTestHelper.CreateMockDbSet(workGroups);
            var pcSet = RepositoryTestHelper.CreateMockDbSet(profitCentres);

            mockContext.Setup(x => x.WorkGroups).Returns(wgSet.Object);
            mockContext.Setup(x => x.ProfitCentres).Returns(pcSet.Object);

            return new WorkGroupRepository(mockContext.Object);
        }

        #region SetSendEmailForProfitCentreWorkGroupsAsync

        // NOTE: SetSendEmailForProfitCentreWorkGroupsAsync uses ExecuteUpdateAsync (EF Core bulk update),
        // which is not supported by LINQ-to-Objects mock IQueryable. Integration tests with a real
        // EF provider are required to cover this method.

        #endregion

        // ════════════════════════════════════════════════════════════════════
        // SetSendEmailForAllWorkGroupsAsync
        // ════════════════════════════════════════════════════════════════════

        #region SetSendEmailForAllWorkGroupsAsync

        // NOTE: SetSendEmailForAllWorkGroupsAsync uses ExecuteUpdateAsync (EF Core bulk update),
        // which is not supported by LINQ-to-Objects mock IQueryable. Integration tests with a real
        // EF provider are required to cover this method.

        #endregion

        // ════════════════════════════════════════════════════════════════════
        // UpdateWorkGroupEmailAsync
        // ════════════════════════════════════════════════════════════════════

        #region UpdateWorkGroupEmailAsync

        // NOTE: UpdateWorkGroupEmailAsync uses ExecuteUpdateAsync (EF Core bulk update),
        // which is not supported by LINQ-to-Objects mock IQueryable. Integration tests with a real
        // EF provider are required to cover this method.

        #endregion

        // ════════════════════════════════════════════════════════════════════
        // GetProfitCentreAsync
        // ════════════════════════════════════════════════════════════════════

        private static WorkGroupRepository CreateProfitCentreViewRepository(
            IEnumerable<PactProfitCentreView>? profitCentreViews = null,
            IEnumerable<WorkGroup>? workGroups = null,
            IEnumerable<TimeCodeValid>? timeCodeValids = null,
            IEnumerable<PactWorkGroupGradeView>? workGroupGradeViews = null,
            IEnumerable<PactStaffView>? workGroupStaffViews = null,
            IEnumerable<JobCode>? jobCodes = null,
            IEnumerable<TestorProduct>? testorProducts = null,
            IEnumerable<TestCapability>? testCapabilities = null,
            IEnumerable<TestRequirement>? testRequirements = null)
        {
            var fpsYearContext = Substitute.For<IFpsRequestContext>();
            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(fpsYearContext);

            mockContext.Setup(x => x.PactProfitCentreViews)
                .Returns(RepositoryTestHelper.CreateMockDbSet(profitCentreViews ?? []).Object);
            mockContext.Setup(x => x.WorkGroups)
                .Returns(RepositoryTestHelper.CreateMockDbSet(workGroups ?? []).Object);
            mockContext.Setup(x => x.TimeCodeValids)
                .Returns(RepositoryTestHelper.CreateMockDbSet(timeCodeValids ?? []).Object);
            mockContext.Setup(x => x.PactWorkGroupGradeViews)
                .Returns(RepositoryTestHelper.CreateMockDbSet(workGroupGradeViews ?? []).Object);
            mockContext.Setup(x => x.PactStaffViews)
                .Returns(RepositoryTestHelper.CreateMockDbSet(workGroupStaffViews ?? []).Object);
            mockContext.Setup(x => x.JobCodes)
                .Returns(RepositoryTestHelper.CreateMockDbSet(jobCodes ?? []).Object);
            mockContext.Setup(x => x.TestorProducts)
                .Returns(RepositoryTestHelper.CreateMockDbSet(testorProducts ?? []).Object);
            mockContext.Setup(x => x.TestCapabilities)
                .Returns(RepositoryTestHelper.CreateMockDbSet(testCapabilities ?? []).Object);
            mockContext.Setup(x => x.TestRequirements)
                .Returns(RepositoryTestHelper.CreateMockDbSet(testRequirements ?? []).Object);

            return new WorkGroupRepository(mockContext.Object);
        }

        #region GetProfitCentreAsync

        [Fact]
        public async Task GetProfitCentreAsync_MatchingProfitCentre_ReturnsEntity()
        {
            var views = new List<PactProfitCentreView>
            {
                new() { ProfitCentre = "PC1", ProfitCentreName = "Centre One" },
                new() { ProfitCentre = "PC2", ProfitCentreName = "Centre Two" }
            };
            var repo = CreateProfitCentreViewRepository(profitCentreViews: views);

            var result = await repo.GetProfitCentreAsync("PC1");

            Assert.NotNull(result);
            Assert.Equal("PC1", result.ProfitCentre);
            Assert.Equal("Centre One", result.ProfitCentreName);
        }

        [Fact]
        public async Task GetProfitCentreAsync_NoMatchingProfitCentre_ReturnsNull()
        {
            var views = new List<PactProfitCentreView>
            {
                new() { ProfitCentre = "PC1" }
            };
            var repo = CreateProfitCentreViewRepository(profitCentreViews: views);

            var result = await repo.GetProfitCentreAsync("MISSING");

            Assert.Null(result);
        }

        [Fact]
        public async Task GetProfitCentreAsync_EmptyData_ReturnsNull()
        {
            var repo = CreateProfitCentreViewRepository(profitCentreViews: []);

            var result = await repo.GetProfitCentreAsync("PC1");

            Assert.Null(result);
        }

        #endregion

        // ════════════════════════════════════════════════════════════════════
        // GetWorkGroupsForEmailAsync
        // ════════════════════════════════════════════════════════════════════

        #region GetWorkGroupsForEmailAsync

        [Fact]
        public async Task GetWorkGroupsForEmailAsync_FiltersBySendEmailAndProfitCentre_ReturnsMatchingOrderedList()
        {
            var workGroups = new List<WorkGroup>
            {
                new() { WorkGroupName = "ZWG", ProfitCentre = "PC1", SendEmail = 1 },
                new() { WorkGroupName = "AWG", ProfitCentre = "PC1", SendEmail = 1 },
                new() { WorkGroupName = "MWG", ProfitCentre = "PC1", SendEmail = 0 },
                new() { WorkGroupName = "BWG", ProfitCentre = "PC2", SendEmail = 1 }
            };
            var repo = CreateProfitCentreViewRepository(workGroups: workGroups);

            var result = (await repo.GetWorkGroupsForEmailAsync("PC1")).ToList();

            Assert.Equal(2, result.Count);
            Assert.Equal("AWG", result[0].WorkGroupName);
            Assert.Equal("ZWG", result[1].WorkGroupName);
            Assert.All(result, wg => Assert.Equal("PC1", wg.ProfitCentre));
            Assert.All(result, wg => Assert.Equal((short)1, wg.SendEmail));
        }

        [Fact]
        public async Task GetWorkGroupsForEmailAsync_NoSendEmailWorkGroups_ReturnsEmptyList()
        {
            var workGroups = new List<WorkGroup>
            {
                new() { WorkGroupName = "WG1", ProfitCentre = "PC1", SendEmail = 0 },
                new() { WorkGroupName = "WG2", ProfitCentre = "PC1", SendEmail = 0 }
            };
            var repo = CreateProfitCentreViewRepository(workGroups: workGroups);

            var result = await repo.GetWorkGroupsForEmailAsync("PC1");

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetWorkGroupsForEmailAsync_NoProfitCentreMatch_ReturnsEmptyList()
        {
            var workGroups = new List<WorkGroup>
            {
                new() { WorkGroupName = "WG1", ProfitCentre = "PC2", SendEmail = 1 }
            };
            var repo = CreateProfitCentreViewRepository(workGroups: workGroups);

            var result = await repo.GetWorkGroupsForEmailAsync("PC1");

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetWorkGroupsForEmailAsync_EmptyData_ReturnsEmptyList()
        {
            var repo = CreateProfitCentreViewRepository(workGroups: []);

            var result = await repo.GetWorkGroupsForEmailAsync("PC1");

            Assert.Empty(result);
        }

        #endregion

        // ════════════════════════════════════════════════════════════════════
        // GetTimeSheetTemplateAsync
        // ════════════════════════════════════════════════════════════════════

        #region GetTimeSheetTemplateAsync — Layout 1 (Flat-file)

        [Fact]
        public async Task GetTimeSheetTemplateAsync_Layout1_ReturnsOneRowPerStaffTimeCodeParentProject()
        {
            var timeCodeValids = new List<TimeCodeValid>
            {
                new() { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1", Active = true, FpsYear = 2024 }
            };
            var gradeViews = new List<PactWorkGroupGradeView>
            {
                new() { WorkGroup = "WG1", WgGrade = "GR1" }
            };
            var workGroupStaffViews = new List<PactStaffView>
            {
                new() { Name = "Alice", WorkGroupGrade = "GR1", PersonStatus = "A" }
            };
            var repo = CreateProfitCentreViewRepository(
                timeCodeValids: timeCodeValids,
                workGroupGradeViews: gradeViews,
                workGroupStaffViews: workGroupStaffViews);

            var result = (await repo.GetTimeSheetTemplateAsync("WG1", 3, layout: 1)).ToList();

            Assert.Single(result);
            Assert.Equal("Alice", result[0].StaffName);
            Assert.Equal("TC1", result[0].TimeCode);
            Assert.Equal("PRJ1", result[0].ParentProject);
            Assert.Equal((short)3, result[0].Month);
            Assert.Null(result[0].Hours);
        }

        [Fact]
        public async Task GetTimeSheetTemplateAsync_Layout1_ExcludesInactiveTimeCodes()
        {
            var timeCodeValids = new List<TimeCodeValid>
            {
                new() { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1", Active = true,  FpsYear = 2024 },
                new() { TimeCode = "TC2", WorkGroup = "WG1", ParentProject = "PRJ1", Active = false, FpsYear = 2024 }
            };
            var gradeViews = new List<PactWorkGroupGradeView>
            {
                new() { WorkGroup = "WG1", WgGrade = "GR1" }
            };
            var workGroupStaffViews = new List<PactStaffView>
            {
                new() { Name = "Alice", WorkGroupGrade = "GR1", PersonStatus = "A" }
            };
            var repo = CreateProfitCentreViewRepository(
                timeCodeValids: timeCodeValids,
                workGroupGradeViews: gradeViews,
                workGroupStaffViews: workGroupStaffViews);

            var result = (await repo.GetTimeSheetTemplateAsync("WG1", 3, layout: 1)).ToList();

            Assert.Single(result);
            Assert.Equal("TC1", result[0].TimeCode);
        }

        [Fact]
        public async Task GetTimeSheetTemplateAsync_Layout1_NoMatchingWorkGroup_ReturnsEmptyList()
        {
            var timeCodeValids = new List<TimeCodeValid>
            {
                new() { TimeCode = "TC1", WorkGroup = "WG2", ParentProject = "PRJ1", Active = true, FpsYear = 2024 }
            };
            var gradeViews = new List<PactWorkGroupGradeView>
            {
                new() { WorkGroup = "WG2", WgGrade = "GR1" }
            };
            var workGroupStaffViews = new List<PactStaffView>
            {
                new() { Name = "Alice", WorkGroupGrade = "GR1", PersonStatus = "A" }
            };
            var repo = CreateProfitCentreViewRepository(
                timeCodeValids: timeCodeValids,
                workGroupGradeViews: gradeViews,
                workGroupStaffViews: workGroupStaffViews);

            var result = await repo.GetTimeSheetTemplateAsync("WG1", 3, layout: 1);

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetTimeSheetTemplateAsync_Layout1_MultipleStaffSameTimeCode_ReturnsRowPerStaff()
        {
            var timeCodeValids = new List<TimeCodeValid>
            {
                new() { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1", Active = true, FpsYear = 2024 }
            };
            var gradeViews = new List<PactWorkGroupGradeView>
            {
                new() { WorkGroup = "WG1", WgGrade = "GR1" },
                new() { WorkGroup = "WG1", WgGrade = "GR2" }
            };
            var workGroupStaffViews = new List<PactStaffView>
            {
                new() { Name = "Alice", WorkGroupGrade = "GR1", PersonStatus = "A" },
                new() { Name = "Bob",   WorkGroupGrade = "GR2", PersonStatus = "A" }
            };
            var repo = CreateProfitCentreViewRepository(
                timeCodeValids: timeCodeValids,
                workGroupGradeViews: gradeViews,
                workGroupStaffViews: workGroupStaffViews);

            var result = (await repo.GetTimeSheetTemplateAsync("WG1", 3, layout: 1)).ToList();

            Assert.Equal(2, result.Count);
        }

        #endregion

        #region GetTimeSheetTemplateAsync — Layout 2 (Cross-tab)

        [Fact]
        public async Task GetTimeSheetTemplateAsync_Layout2_ReturnsGroupedRowPerTimeCodeParentProject()
        {
            var timeCodeValids = new List<TimeCodeValid>
            {
                new() { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1", JobCode = "JC1", Active = true, FpsYear = 2024 }
            };
            var gradeViews = new List<PactWorkGroupGradeView>
            {
                new() { WorkGroup = "WG1", WgGrade = "GR1" }
            };
            var workGroupStaffViews = new List<PactStaffView>
            {
                new() { Name = "Alice", WorkGroupGrade = "GR1", PersonStatus = "A" }
            };
            var jobCodes = new List<JobCode>
            {
                new() { JobCodeId = "JC1", JobCodeName = "Job One", FpsYear = 2024 }
            };
            var repo = CreateProfitCentreViewRepository(
                timeCodeValids: timeCodeValids,
                workGroupGradeViews: gradeViews,
                workGroupStaffViews: workGroupStaffViews,
                jobCodes: jobCodes);

            var result = (await repo.GetTimeSheetTemplateAsync("WG1", 4, layout: 2)).ToList();

            Assert.Single(result);
            Assert.Equal("TC1", result[0].TimeCode);
            Assert.Equal("PRJ1", result[0].ParentProject);
            Assert.Equal((short)4, result[0].Month);
            Assert.Null(result[0].Hours);
        }

        [Fact]
        public async Task GetTimeSheetTemplateAsync_Layout2_ExcludesInactiveStaff()
        {
            var timeCodeValids = new List<TimeCodeValid>
            {
                new() { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1", JobCode = "JC1", Active = true, FpsYear = 2024 }
            };
            var gradeViews = new List<PactWorkGroupGradeView>
            {
                new() { WorkGroup = "WG1", WgGrade = "GR1" },
                new() { WorkGroup = "WG1", WgGrade = "GR2" }
            };
            var workGroupStaffViews = new List<PactStaffView>
            {
                new() { Name = "Alice",    WorkGroupGrade = "GR1", PersonStatus = "A" },
                new() { Name = "Inactive", WorkGroupGrade = "GR2", PersonStatus = "I" }
            };
            var jobCodes = new List<JobCode>
            {
                new() { JobCodeId = "JC1", JobCodeName = "Job One", FpsYear = 2024 }
            };
            var repo = CreateProfitCentreViewRepository(
                timeCodeValids: timeCodeValids,
                workGroupGradeViews: gradeViews,
                workGroupStaffViews: workGroupStaffViews,
                jobCodes: jobCodes);

            var result = (await repo.GetTimeSheetTemplateAsync("WG1", 4, layout: 2)).ToList();

            Assert.Single(result);
            Assert.Equal("Alice", result[0].StaffName);
        }

        [Fact]
        public async Task GetTimeSheetTemplateAsync_Layout2_MultipleStaffSameTimeCode_GroupsIntoOneRowWithCommaNames()
        {
            var timeCodeValids = new List<TimeCodeValid>
            {
                new() { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1", JobCode = "JC1", Active = true, FpsYear = 2024 }
            };
            var gradeViews = new List<PactWorkGroupGradeView>
            {
                new() { WorkGroup = "WG1", WgGrade = "GR1" },
                new() { WorkGroup = "WG1", WgGrade = "GR2" }
            };
            var workGroupStaffViews = new List<PactStaffView>
            {
                new() { Name = "Alice", WorkGroupGrade = "GR1", PersonStatus = "A" },
                new() { Name = "Bob",   WorkGroupGrade = "GR2", PersonStatus = "A" }
            };
            var jobCodes = new List<JobCode>
            {
                new() { JobCodeId = "JC1", JobCodeName = "Job One", FpsYear = 2024 }
            };
            var repo = CreateProfitCentreViewRepository(
                timeCodeValids: timeCodeValids,
                workGroupGradeViews: gradeViews,
                workGroupStaffViews: workGroupStaffViews,
                jobCodes: jobCodes);

            var result = (await repo.GetTimeSheetTemplateAsync("WG1", 4, layout: 2)).ToList();

            Assert.Single(result);
            Assert.Contains("Alice", result[0].StaffName);
            Assert.Contains("Bob", result[0].StaffName);
        }

        [Fact]
        public async Task GetTimeSheetTemplateAsync_Layout2_UsesJobCodeNameWhenJobCodePresent()
        {
            var timeCodeValids = new List<TimeCodeValid>
            {
                new() { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1", JobCode = "JC1", Active = true, FpsYear = 2024 }
            };
            var gradeViews = new List<PactWorkGroupGradeView>
            {
                new() { WorkGroup = "WG1", WgGrade = "GR1" }
            };
            var workGroupStaffViews = new List<PactStaffView>
            {
                new() { Name = "Alice", WorkGroupGrade = "GR1", PersonStatus = "A" }
            };
            var jobCodes = new List<JobCode>
            {
                new() { JobCodeId = "JC1", JobCodeName = "Job One", FpsYear = 2024 }
            };
            var repo = CreateProfitCentreViewRepository(
                timeCodeValids: timeCodeValids,
                workGroupGradeViews: gradeViews,
                workGroupStaffViews: workGroupStaffViews,
                jobCodes: jobCodes);

            var result = (await repo.GetTimeSheetTemplateAsync("WG1", 4, layout: 2)).ToList();

            Assert.Single(result);
            Assert.Equal("Job One", result[0].Description);
        }

        [Fact]
        public async Task GetTimeSheetTemplateAsync_Layout2_UsesItemDescriptionWhenNoJobCode()
        {
            var timeCodeValids = new List<TimeCodeValid>
            {
                new() { TimeCode = "TC1", WorkGroup = "WG1", ParentProject = "PRJ1", TestCode = "TST1", Active = true, FpsYear = 2024 }
            };
            var gradeViews = new List<PactWorkGroupGradeView>
            {
                new() { WorkGroup = "WG1", WgGrade = "GR1" }
            };
            var workGroupStaffViews = new List<PactStaffView>
            {
                new() { Name = "Alice", WorkGroupGrade = "GR1", PersonStatus = "A" }
            };
            var testorProducts = new List<TestorProduct>
            {
                new() { ItemCode = "TST1", ItemDescription = "Blood Test" }
            };
            var repo = CreateProfitCentreViewRepository(
                timeCodeValids: timeCodeValids,
                workGroupGradeViews: gradeViews,
                workGroupStaffViews: workGroupStaffViews,
                testorProducts: testorProducts);

            var result = (await repo.GetTimeSheetTemplateAsync("WG1", 4, layout: 2)).ToList();

            Assert.Single(result);
            Assert.Equal("Blood Test", result[0].Description);
        }

        [Fact]
        public async Task GetTimeSheetTemplateAsync_Layout2_EmptyData_ReturnsEmptyList()
        {
            var repo = CreateProfitCentreViewRepository();

            var result = await repo.GetTimeSheetTemplateAsync("WG1", 4, layout: 2);

            Assert.Empty(result);
        }

        #endregion

        // ════════════════════════════════════════════════════════════════════
        // GetOutputSheetTemplateAsync
        // ════════════════════════════════════════════════════════════════════

        #region GetOutputSheetTemplateAsync

        [Fact]
        public async Task GetOutputSheetTemplateAsync_MatchingWorkGroup_ReturnsOrderedRows()
        {
            var testCapabilities = new List<TestCapability>
            {
                new() { TestCode = "B", WorkGroup = "WG1", PlanPortfolio = "PP1", FpsYear = 2024 },
                new() { TestCode = "A", WorkGroup = "WG1", PlanPortfolio = "PP1", FpsYear = 2024 }
            };
            var testRequirements = new List<TestRequirement>
            {
                new() { TestCode = "B", Buyer = "BuyerB", Active = 1, FpsYear = 2024 },
                new() { TestCode = "A", Buyer = "BuyerA", Active = 1, FpsYear = 2024 }
            };
            var testorProducts = new List<TestorProduct>
            {
                new() { ItemCode = "B", ItemDescription = "Blood Test" },
                new() { ItemCode = "A", ItemDescription = "Anthrax Test" }
            };
            var repo = CreateProfitCentreViewRepository(
                testCapabilities: testCapabilities,
                testRequirements: testRequirements,
                testorProducts: testorProducts);

            var result = (await repo.GetOutputSheetTemplateAsync("WG1", month: 5)).ToList();

            Assert.Equal(2, result.Count);
            Assert.Equal("A", result[0].TestCode);
            Assert.Equal("B", result[1].TestCode);
        }

        [Fact]
        public async Task GetOutputSheetTemplateAsync_FiltersOutInactiveTestRequirements()
        {
            var testCapabilities = new List<TestCapability>
            {
                new() { TestCode = "TC1", WorkGroup = "WG1", PlanPortfolio = "PP1", FpsYear = 2024 },
                new() { TestCode = "TC2", WorkGroup = "WG1", PlanPortfolio = "PP1", FpsYear = 2024 }
            };
            var testRequirements = new List<TestRequirement>
            {
                new() { TestCode = "TC1", Buyer = "B1", Active = 1, FpsYear = 2024 },
                new() { TestCode = "TC2", Buyer = "B2", Active = 0, FpsYear = 2024 }
            };
            var testorProducts = new List<TestorProduct>
            {
                new() { ItemCode = "TC1", ItemDescription = "Test 1" },
                new() { ItemCode = "TC2", ItemDescription = "Test 2" }
            };
            var repo = CreateProfitCentreViewRepository(
                testCapabilities: testCapabilities,
                testRequirements: testRequirements,
                testorProducts: testorProducts);

            var result = (await repo.GetOutputSheetTemplateAsync("WG1", month: 5)).ToList();

            Assert.Single(result);
            Assert.Equal("TC1", result[0].TestCode);
        }

        [Fact]
        public async Task GetOutputSheetTemplateAsync_SetsMonthAndNullVolume()
        {
            var testCapabilities = new List<TestCapability>
            {
                new() { TestCode = "TC1", WorkGroup = "WG1", PlanPortfolio = "PP1", FpsYear = 2024 }
            };
            var testRequirements = new List<TestRequirement>
            {
                new() { TestCode = "TC1", Buyer = "B1", Active = 1, FpsYear = 2024 }
            };
            var testorProducts = new List<TestorProduct>
            {
                new() { ItemCode = "TC1", ItemDescription = "Test 1" }
            };
            var repo = CreateProfitCentreViewRepository(
                testCapabilities: testCapabilities,
                testRequirements: testRequirements,
                testorProducts: testorProducts);

            var result = (await repo.GetOutputSheetTemplateAsync("WG1", month: 7)).ToList();

            Assert.Single(result);
            Assert.Equal((short)7, result[0].Month);
            Assert.Null(result[0].Volume);
        }

        [Fact]
        public async Task GetOutputSheetTemplateAsync_NoMatchingWorkGroup_ReturnsEmptyList()
        {
            var testCapabilities = new List<TestCapability>
            {
                new() { TestCode = "TC1", WorkGroup = "WG2", PlanPortfolio = "PP1", FpsYear = 2024 }
            };
            var testRequirements = new List<TestRequirement>
            {
                new() { TestCode = "TC1", Buyer = "B1", Active = 1, FpsYear = 2024 }
            };
            var testorProducts = new List<TestorProduct>
            {
                new() { ItemCode = "TC1", ItemDescription = "Test 1" }
            };
            var repo = CreateProfitCentreViewRepository(
                testCapabilities: testCapabilities,
                testRequirements: testRequirements,
                testorProducts: testorProducts);

            var result = await repo.GetOutputSheetTemplateAsync("WG1", month: 5);

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetOutputSheetTemplateAsync_EmptyData_ReturnsEmptyList()
        {
            var repo = CreateProfitCentreViewRepository();

            var result = await repo.GetOutputSheetTemplateAsync("WG1", month: 5);

            Assert.Empty(result);
        }

        #endregion
    }
}
