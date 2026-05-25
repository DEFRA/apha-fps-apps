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

        private static WorkGroupStaffView StaffView(string pactId, string name, string workGroupGrade) =>
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
            IEnumerable<WorkGroupStaffView>     staffViews,
            IEnumerable<MonthlyTime>            monthlyTimes)
        {
            var fpsRequestContext = Substitute.For<IFpsRequestContext>();
            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(fpsRequestContext);

            var gradeViewSet   = RepositoryTestHelper.CreateMockDbSet(gradeViews);
            var staffViewSet   = RepositoryTestHelper.CreateMockDbSet(staffViews);
            var monthlyTimeSet = RepositoryTestHelper.CreateMockDbSet(monthlyTimes);
            var workGroupSet   = RepositoryTestHelper.CreateMockDbSet(Enumerable.Empty<WorkGroup>());

            mockContext.Setup(x => x.PactWorkGroupGradeViews).Returns(gradeViewSet.Object);
            mockContext.Setup(x => x.WorkGroupStaffViews).Returns(staffViewSet.Object);
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
        // GetWgSummarisedStaffTimeUsageAsync
        // ════════════════════════════════════════════════════════════════════

        // ── Factory helpers ───────────────────────────────────────────────────

        private static WgSummarisedStaffTimeUsageView TimeUsageRow(
            string  workGroup    = "WG1",
            string  name         = "Alice",
            string  monthName    = "April",
            string  parentProject = "PP1",
            string  jobCode      = "JC1",
            string  jobTitle     = "Job Title 1",
            double? hrsPaid      = 120.0,
            double? totalTime    = 10.0,
            double? totalCost    = 500.0,
            int     fpsYear      = 2024) =>
            new()
            {
                WorkGroup     = workGroup,
                Name          = name,
                MonthName     = monthName,
                ParentProject = parentProject,
                JobCode       = jobCode,
                JobTitle      = jobTitle,
                HrsPaid       = hrsPaid,
                TotalTime     = totalTime,
                TotalCost     = totalCost,
                FpsYear       = fpsYear
            };

        private static WorkGroupRepository CreateTimeUsageRepository(
            IEnumerable<WgSummarisedStaffTimeUsageView> rows)
        {
            var fpsRequestContext = Substitute.For<IFpsRequestContext>();
            var mockContext       = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(fpsRequestContext);

            var timeUsageSet = RepositoryTestHelper.CreateMockDbSet(rows);
            var workGroupSet = RepositoryTestHelper.CreateMockDbSet(Enumerable.Empty<WorkGroup>());

            mockContext.Setup(x => x.WgSummarisedStaffTimeUsageViews).Returns(timeUsageSet.Object);
            mockContext.Setup(x => x.WorkGroups).Returns(workGroupSet.Object);

            return new WorkGroupRepository(mockContext.Object);
        }

        // ────────────────────────────────────────────────────────────────────
        #region GetWgSummarisedStaffTimeUsageAsync — basic retrieval
        // ────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task GetWgSummarisedStaffTimeUsageAsync_MatchingWorkGroup_ReturnsRows()
        {
            var repo = CreateTimeUsageRepository(
            [
                TimeUsageRow(workGroup: "WG1", name: "Alice", monthName: "April"),
                TimeUsageRow(workGroup: "WG1", name: "Alice", monthName: "May")
            ]);

            var result = (await repo.GetWgSummarisedStaffTimeUsageAsync("WG1")).ToList();

            Assert.Equal(2, result.Count);
            Assert.All(result, r => Assert.Equal("WG1", r.WorkGroup));
        }

        [Fact]
        public async Task GetWgSummarisedStaffTimeUsageAsync_NoMatchingWorkGroup_ReturnsEmpty()
        {
            var repo = CreateTimeUsageRepository(
            [
                TimeUsageRow(workGroup: "WG1")
            ]);

            var result = await repo.GetWgSummarisedStaffTimeUsageAsync("WG_MISSING");

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetWgSummarisedStaffTimeUsageAsync_EmptyView_ReturnsEmpty()
        {
            var repo = CreateTimeUsageRepository([]);

            var result = await repo.GetWgSummarisedStaffTimeUsageAsync("WG1");

            Assert.Empty(result);
        }

        [Fact]
        public async Task GetWgSummarisedStaffTimeUsageAsync_MultipleWorkGroups_ReturnsOnlyMatchingWorkGroup()
        {
            var repo = CreateTimeUsageRepository(
            [
                TimeUsageRow(workGroup: "WG1", name: "Alice"),
                TimeUsageRow(workGroup: "WG2", name: "Bob"),
                TimeUsageRow(workGroup: "WG3", name: "Carol")
            ]);

            var result = (await repo.GetWgSummarisedStaffTimeUsageAsync("WG1")).ToList();

            Assert.Single(result);
            Assert.Equal("WG1",   result[0].WorkGroup);
            Assert.Equal("Alice", result[0].Name);
        }

        [Fact]
        public async Task GetWgSummarisedStaffTimeUsageAsync_SingleRow_AllFieldsMappedCorrectly()
        {
            var repo = CreateTimeUsageRepository(
            [
                TimeUsageRow(
                    workGroup:     "WG1",
                    name:          "Alice",
                    monthName:     "April",
                    parentProject: "PROJ_A",
                    jobCode:       "JC1",
                    jobTitle:      "Analyst",
                    hrsPaid:       120.0,
                    totalTime:     10.0,
                    totalCost:     500.0,
                    fpsYear:       2024)
            ]);

            var result = (await repo.GetWgSummarisedStaffTimeUsageAsync("WG1")).ToList();

            Assert.Single(result);
            var row = result[0];
            Assert.Equal("WG1",    row.WorkGroup);
            Assert.Equal("Alice",  row.Name);
            Assert.Equal("April",  row.MonthName);
            Assert.Equal("PROJ_A", row.ParentProject);
            Assert.Equal("JC1",    row.JobCode);
            Assert.Equal("Analyst",row.JobTitle);
            Assert.Equal(120.0,    row.HrsPaid);
            Assert.Equal(10.0,     row.TotalTime);
            Assert.Equal(500.0,    row.TotalCost);
            Assert.Equal(2024,     row.FpsYear);
        }

        #endregion

        // ────────────────────────────────────────────────────────────────────
        #region GetWgSummarisedStaffTimeUsageAsync — nullable fields
        // ────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task GetWgSummarisedStaffTimeUsageAsync_NullHrsPaid_ReturnedAsNull()
        {
            var repo = CreateTimeUsageRepository(
            [
                TimeUsageRow(workGroup: "WG1", hrsPaid: null)
            ]);

            var result = (await repo.GetWgSummarisedStaffTimeUsageAsync("WG1")).ToList();

            Assert.Single(result);
            Assert.Null(result[0].HrsPaid);
        }

        [Fact]
        public async Task GetWgSummarisedStaffTimeUsageAsync_NullTotalTime_ReturnedAsNull()
        {
            var repo = CreateTimeUsageRepository(
            [
                TimeUsageRow(workGroup: "WG1", totalTime: null)
            ]);

            var result = (await repo.GetWgSummarisedStaffTimeUsageAsync("WG1")).ToList();

            Assert.Single(result);
            Assert.Null(result[0].TotalTime);
        }

        [Fact]
        public async Task GetWgSummarisedStaffTimeUsageAsync_NullTotalCost_ReturnedAsNull()
        {
            var repo = CreateTimeUsageRepository(
            [
                TimeUsageRow(workGroup: "WG1", totalCost: null)
            ]);

            var result = (await repo.GetWgSummarisedStaffTimeUsageAsync("WG1")).ToList();

            Assert.Single(result);
            Assert.Null(result[0].TotalCost);
        }

        [Fact]
        public async Task GetWgSummarisedStaffTimeUsageAsync_NullName_ReturnedAsNull()
        {
            var repo = CreateTimeUsageRepository(
            [
                new WgSummarisedStaffTimeUsageView { WorkGroup = "WG1", Name = null, MonthName = "April" }
            ]);

            var result = (await repo.GetWgSummarisedStaffTimeUsageAsync("WG1")).ToList();

            Assert.Single(result);
            Assert.Null(result[0].Name);
        }

        [Fact]
        public async Task GetWgSummarisedStaffTimeUsageAsync_NullJobCode_ReturnedAsNull()
        {
            var repo = CreateTimeUsageRepository(
            [
                new WgSummarisedStaffTimeUsageView { WorkGroup = "WG1", JobCode = null }
            ]);

            var result = (await repo.GetWgSummarisedStaffTimeUsageAsync("WG1")).ToList();

            Assert.Single(result);
            Assert.Null(result[0].JobCode);
        }

        [Fact]
        public async Task GetWgSummarisedStaffTimeUsageAsync_NullJobTitle_ReturnedAsNull()
        {
            var repo = CreateTimeUsageRepository(
            [
                new WgSummarisedStaffTimeUsageView { WorkGroup = "WG1", JobTitle = null }
            ]);

            var result = (await repo.GetWgSummarisedStaffTimeUsageAsync("WG1")).ToList();

            Assert.Single(result);
            Assert.Null(result[0].JobTitle);
        }

        #endregion

        // ────────────────────────────────────────────────────────────────────
        #region GetWgSummarisedStaffTimeUsageAsync — multiple staff / months
        // ────────────────────────────────────────────────────────────────────

        [Fact]
        public async Task GetWgSummarisedStaffTimeUsageAsync_MultipleStaffInSameWorkGroup_ReturnsAllRows()
        {
            var repo = CreateTimeUsageRepository(
            [
                TimeUsageRow(workGroup: "WG1", name: "Alice", monthName: "April"),
                TimeUsageRow(workGroup: "WG1", name: "Bob",   monthName: "April"),
                TimeUsageRow(workGroup: "WG1", name: "Carol", monthName: "April")
            ]);

            var result = (await repo.GetWgSummarisedStaffTimeUsageAsync("WG1")).ToList();

            Assert.Equal(3, result.Count);
            Assert.Contains(result, r => r.Name == "Alice");
            Assert.Contains(result, r => r.Name == "Bob");
            Assert.Contains(result, r => r.Name == "Carol");
        }

        [Fact]
        public async Task GetWgSummarisedStaffTimeUsageAsync_SameStaffMultipleMonths_ReturnsOneRowPerMonth()
        {
            var months = new[] { "April", "May", "June", "July", "August", "September",
                                 "October", "November", "December", "January", "February", "March" };
            var rows = months.Select(m => TimeUsageRow(workGroup: "WG1", name: "Alice", monthName: m));
            var repo = CreateTimeUsageRepository(rows);

            var result = (await repo.GetWgSummarisedStaffTimeUsageAsync("WG1")).ToList();

            Assert.Equal(12, result.Count);
            Assert.All(result, r => Assert.Equal("Alice", r.Name));
        }

        [Fact]
        public async Task GetWgSummarisedStaffTimeUsageAsync_SameStaffMultipleJobCodes_ReturnsOneRowPerJobCode()
        {
            var repo = CreateTimeUsageRepository(
            [
                TimeUsageRow(workGroup: "WG1", name: "Alice", jobCode: "JC_A", monthName: "April"),
                TimeUsageRow(workGroup: "WG1", name: "Alice", jobCode: "JC_B", monthName: "April"),
                TimeUsageRow(workGroup: "WG1", name: "Alice", jobCode: "JC_C", monthName: "April")
            ]);

            var result = (await repo.GetWgSummarisedStaffTimeUsageAsync("WG1")).ToList();

            Assert.Equal(3, result.Count);
            Assert.Contains(result, r => r.JobCode == "JC_A");
            Assert.Contains(result, r => r.JobCode == "JC_B");
            Assert.Contains(result, r => r.JobCode == "JC_C");
        }

        [Fact]
        public async Task GetWgSummarisedStaffTimeUsageAsync_MixedWorkGroups_FiltersCorrectly()
        {
            var repo = CreateTimeUsageRepository(
            [
                TimeUsageRow(workGroup: "WG1", name: "Alice",  monthName: "April"),
                TimeUsageRow(workGroup: "WG1", name: "Bob",    monthName: "May"),
                TimeUsageRow(workGroup: "WG2", name: "Carol",  monthName: "April"),
                TimeUsageRow(workGroup: "WG2", name: "Dave",   monthName: "May"),
                TimeUsageRow(workGroup: "WG3", name: "Eve",    monthName: "April")
            ]);

            var result = (await repo.GetWgSummarisedStaffTimeUsageAsync("WG2")).ToList();

            Assert.Equal(2, result.Count);
            Assert.All(result, r => Assert.Equal("WG2", r.WorkGroup));
            Assert.Contains(result, r => r.Name == "Carol");
            Assert.Contains(result, r => r.Name == "Dave");
        }

        [Fact]
        public async Task GetWgSummarisedStaffTimeUsageAsync_ZeroTotalTimeAndCost_ReturnedCorrectly()
        {
            var repo = CreateTimeUsageRepository(
            [
                TimeUsageRow(workGroup: "WG1", totalTime: 0.0, totalCost: 0.0)
            ]);

            var result = (await repo.GetWgSummarisedStaffTimeUsageAsync("WG1")).ToList();

            Assert.Single(result);
            Assert.Equal(0.0, result[0].TotalTime);
            Assert.Equal(0.0, result[0].TotalCost);
        }

        [Fact]
        public async Task GetWgSummarisedStaffTimeUsageAsync_LargeDataset_ReturnsAllMatchingRows()
        {
            const int totalRows = 100;
            var rows = Enumerable.Range(1, totalRows)
                .Select(i => TimeUsageRow(workGroup: "WG1", name: $"Staff{i}", monthName: "April"));
            var repo = CreateTimeUsageRepository(rows);

            var result = (await repo.GetWgSummarisedStaffTimeUsageAsync("WG1")).ToList();

            Assert.Equal(totalRows, result.Count);
        }

        #endregion
    }
}
