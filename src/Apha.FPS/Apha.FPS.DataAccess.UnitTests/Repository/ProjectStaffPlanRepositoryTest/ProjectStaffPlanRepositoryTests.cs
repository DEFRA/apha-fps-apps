using Apha.Common.Helpers.Repository;
using Apha.FPS.Core.Entities;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
using Apha.FPS.DataAccess.Data;
using Apha.FPS.DataAccess.Repositories;
using Moq;

namespace Apha.FPS.DataAccess.UnitTests.Repository.ProjectStaffPlanRepositoryTest
{
    public class ProjectStaffPlanRepositoryTests
    {
        private static ProjectStaffPlanRepository CreateRepository(
            IEnumerable<ProjectStaffPlanView>? views = null)
        {
            var mockRequestContext = new Mock<IFpsRequestContext>();
            mockRequestContext.Setup(x => x.FpsYear).Returns(2024);

            var mockContext = RepositoryTestHelper.CreateMockDbContext<FpsDbContext>(mockRequestContext.Object);

            var mockSet = RepositoryTestHelper.CreateMockDbSet(views ?? Enumerable.Empty<ProjectStaffPlanView>());
            mockContext.Setup(x => x.ProjectStaffPlanViews).Returns(mockSet.Object);

            return new ProjectStaffPlanRepository(mockContext.Object);
        }

        private static PaginationParameters<string> DefaultQuery(
            int page = 1, int pageSize = 10,
            string? filter = null, string? sortBy = null, bool descending = false)
            => new PaginationParameters<string>
            {
                Page       = page,
                PageSize   = pageSize,
                Filter     = filter,
                SortBy     = sortBy,
                Descending = descending
            };

        private static List<ProjectStaffPlanView> SampleData() =>
        [
            new() { ParentProject = "P001", ProgramNo = "PROG1", Name = "Alice Smith",  StaffId = "S001", WorkGroup = "WG_CSU",  GradeCode = "GR1", PlannedHours = 100, Cost = 500m, PayCost = 400m },
            new() { ParentProject = "P001", ProgramNo = "PROG1", Name = "Bob Jones",    StaffId = "S002", WorkGroup = "WG_BSU",  GradeCode = "GR2", PlannedHours = 80,  Cost = 400m, PayCost = 320m },
            new() { ParentProject = "P002", ProgramNo = "PROG2", Name = "Carol White",  StaffId = "S003", WorkGroup = "WG_CSU",  GradeCode = "GR1", PlannedHours = 60,  Cost = 300m, PayCost = 240m },
            new() { ParentProject = "P003", ProgramNo = "PROG3", Name = "Dave Brown",   StaffId = "S004", WorkGroup = "WG_OTHER",GradeCode = "GR3", PlannedHours = 40,  Cost = 200m, PayCost = 160m }
        ];

        #region GetPagedAsync — Happy path

        [Fact]
        public async Task GetPagedAsync_ReturnsAllRows_WhenNoFilter()
        {
            var repo   = CreateRepository(SampleData());
            var result = await repo.GetPagedAsync(DefaultQuery());

            Assert.NotNull(result);
            Assert.Equal(4, result.Data.Count());
        }

        [Fact]
        public async Task GetPagedAsync_ReturnsEmpty_WhenNoData()
        {
            var repo   = CreateRepository([]);
            var result = await repo.GetPagedAsync(DefaultQuery());

            Assert.NotNull(result);
            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task GetPagedAsync_DefaultSort_OrdersByProgramNoThenParentProjectThenName()
        {
            var repo   = CreateRepository(SampleData());
            var result = await repo.GetPagedAsync(DefaultQuery());

            var list = result.Data.ToList();
            Assert.Equal("PROG1", list[0].ProgramNo);
            Assert.Equal("PROG1", list[1].ProgramNo);
            Assert.Equal("PROG2", list[2].ProgramNo);
            Assert.Equal("PROG3", list[3].ProgramNo);
        }

        #endregion

        #region GetPagedAsync — Paging

        [Fact]
        public async Task GetPagedAsync_Paging_ReturnsCorrectPage()
        {
            var repo   = CreateRepository(SampleData());
            var result = await repo.GetPagedAsync(DefaultQuery(page: 1, pageSize: 2));

            Assert.Equal(2, result.Data.Count());
            Assert.Equal(4, result.PaginationData.TotalRecords);
        }

        [Fact]
        public async Task GetPagedAsync_Paging_SecondPage_ReturnsRemainingRows()
        {
            var repo   = CreateRepository(SampleData());
            var result = await repo.GetPagedAsync(DefaultQuery(page: 2, pageSize: 2));

            Assert.Equal(2, result.Data.Count());
        }

        [Fact]
        public async Task GetPagedAsync_Paging_PageBeyondData_ReturnsEmpty()
        {
            var repo   = CreateRepository(SampleData());
            var result = await repo.GetPagedAsync(DefaultQuery(page: 99, pageSize: 10));

            Assert.Empty(result.Data);
        }

        #endregion

        #region GetPagedAsync — Filtering by ParentProject

        [Fact]
        public async Task GetPagedAsync_FilterByParentProject_ReturnsMatchingRows()
        {
            var repo   = CreateRepository(SampleData());
            var result = await repo.GetPagedAsync(DefaultQuery(filter: "{\"ParentProject\":\"P001\"}"));

            Assert.Equal(2, result.Data.Count());
            Assert.All(result.Data, r => Assert.Contains("P001", r.ParentProject, StringComparison.OrdinalIgnoreCase));
        }

        [Fact]
        public async Task GetPagedAsync_FilterByParentProject_NoMatch_ReturnsEmpty()
        {
            var repo   = CreateRepository(SampleData());
            var result = await repo.GetPagedAsync(DefaultQuery(filter: "{\"ParentProject\":\"ZZZZ\"}"));

            Assert.Empty(result.Data);
        }

        #endregion

        #region GetPagedAsync — Filtering by ProgramNo

        [Fact]
        public async Task GetPagedAsync_FilterByProgramNo_ReturnsMatchingRows()
        {
            var repo   = CreateRepository(SampleData());
            var result = await repo.GetPagedAsync(DefaultQuery(filter: "{\"ProgramNo\":\"PROG1\"}"));

            Assert.Equal(2, result.Data.Count());
            Assert.All(result.Data, r => Assert.Contains("PROG1", r.ProgramNo!, StringComparison.OrdinalIgnoreCase));
        }

        #endregion

        #region GetPagedAsync — Filtering by Name

        [Fact]
        public async Task GetPagedAsync_FilterByName_ReturnsMatchingRows()
        {
            var repo   = CreateRepository(SampleData());
            var result = await repo.GetPagedAsync(DefaultQuery(filter: "{\"Name\":\"Alice\"}"));

            Assert.Single(result.Data);
            Assert.Contains("Alice", result.Data.First().Name!, StringComparison.OrdinalIgnoreCase);
        }

        #endregion

        #region GetPagedAsync — Filtering by WorkGroup

        [Fact]
        public async Task GetPagedAsync_FilterByWorkGroup_ReturnsMatchingRows()
        {
            var repo   = CreateRepository(SampleData());
            var result = await repo.GetPagedAsync(DefaultQuery(filter: "{\"WorkGroup\":\"WG_CSU\"}"));

            Assert.Equal(2, result.Data.Count());
            Assert.All(result.Data, r => Assert.Contains("WG_CSU", r.WorkGroup!, StringComparison.OrdinalIgnoreCase));
        }

        #endregion

        #region GetPagedAsync — Filtering by GradeCode

        [Fact]
        public async Task GetPagedAsync_FilterByGradeCode_ReturnsMatchingRows()
        {
            var repo   = CreateRepository(SampleData());
            var result = await repo.GetPagedAsync(DefaultQuery(filter: "{\"GradeCode\":\"GR1\"}"));

            Assert.Equal(2, result.Data.Count());
            Assert.All(result.Data, r => Assert.Contains("GR1", r.GradeCode!, StringComparison.OrdinalIgnoreCase));
        }

        #endregion

        #region GetPagedAsync — Filtering by StaffId

        [Fact]
        public async Task GetPagedAsync_FilterByStaffId_ReturnsMatchingRows()
        {
            var repo   = CreateRepository(SampleData());
            var result = await repo.GetPagedAsync(DefaultQuery(filter: "{\"StaffId\":\"S001\"}"));

            Assert.Single(result.Data);
            Assert.Equal("S001", result.Data.First().StaffId);
        }

        #endregion

        #region GetPagedAsync — Null / empty filter

        [Fact]
        public async Task GetPagedAsync_NullFilter_ReturnsAllRows()
        {
            var repo   = CreateRepository(SampleData());
            var result = await repo.GetPagedAsync(DefaultQuery(filter: null));

            Assert.Equal(4, result.Data.Count());
        }

        [Fact]
        public async Task GetPagedAsync_EmptyFilter_ReturnsAllRows()
        {
            var repo   = CreateRepository(SampleData());
            var result = await repo.GetPagedAsync(DefaultQuery(filter: ""));

            Assert.Equal(4, result.Data.Count());
        }

        [Fact]
        public async Task GetPagedAsync_InvalidJsonFilter_ThrowsJsonException()
        {
            var repo = CreateRepository(SampleData());

            await Assert.ThrowsAsync<Newtonsoft.Json.JsonReaderException>(
                () => repo.GetPagedAsync(DefaultQuery(filter: "not-valid-json")));
        }

        #endregion

        #region GetPagedAsync — Sorting

        [Fact]
        public async Task GetPagedAsync_SortByParentProjectAscending_ReturnsOrderedResults()
        {
            var repo   = CreateRepository(SampleData());
            var result = await repo.GetPagedAsync(DefaultQuery(sortBy: "parentproject", descending: false));

            var list = result.Data.ToList();
            for (int i = 1; i < list.Count; i++)
                Assert.True(string.Compare(list[i - 1].ParentProject, list[i].ParentProject, StringComparison.OrdinalIgnoreCase) <= 0);
        }

        [Fact]
        public async Task GetPagedAsync_SortByParentProjectDescending_ReturnsOrderedResults()
        {
            var repo   = CreateRepository(SampleData());
            var result = await repo.GetPagedAsync(DefaultQuery(sortBy: "parentproject", descending: true));

            var list = result.Data.ToList();
            for (int i = 1; i < list.Count; i++)
                Assert.True(string.Compare(list[i - 1].ParentProject, list[i].ParentProject, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        [Fact]
        public async Task GetPagedAsync_SortByNameAscending_ReturnsOrderedResults()
        {
            var repo   = CreateRepository(SampleData());
            var result = await repo.GetPagedAsync(DefaultQuery(sortBy: "name", descending: false));

            var list = result.Data.ToList();
            for (int i = 1; i < list.Count; i++)
                Assert.True(string.Compare(list[i - 1].Name, list[i].Name, StringComparison.OrdinalIgnoreCase) <= 0);
        }

        [Fact]
        public async Task GetPagedAsync_SortByStaffId_ReturnsOrderedResults()
        {
            var repo   = CreateRepository(SampleData());
            var result = await repo.GetPagedAsync(DefaultQuery(sortBy: "staffid", descending: false));

            var list = result.Data.ToList();
            for (int i = 1; i < list.Count; i++)
                Assert.True(string.Compare(list[i - 1].StaffId, list[i].StaffId, StringComparison.OrdinalIgnoreCase) <= 0);
        }

        [Fact]
        public async Task GetPagedAsync_SortByPlannedHoursDescending_ReturnsOrderedResults()
        {
            var repo   = CreateRepository(SampleData());
            var result = await repo.GetPagedAsync(DefaultQuery(sortBy: "plannedhours", descending: true));

            var list = result.Data.ToList();
            for (int i = 1; i < list.Count; i++)
                Assert.True(list[i - 1].PlannedHours >= list[i].PlannedHours);
        }

        [Fact]
        public async Task GetPagedAsync_SortByCostDescending_ReturnsOrderedResults()
        {
            var repo   = CreateRepository(SampleData());
            var result = await repo.GetPagedAsync(DefaultQuery(sortBy: "cost", descending: true));

            var list = result.Data.ToList();
            for (int i = 1; i < list.Count; i++)
                Assert.True(list[i - 1].Cost >= list[i].Cost);
        }

        [Fact]
        public async Task GetPagedAsync_SortByPayCostAscending_ReturnsOrderedResults()
        {
            var repo   = CreateRepository(SampleData());
            var result = await repo.GetPagedAsync(DefaultQuery(sortBy: "paycost", descending: false));

            var list = result.Data.ToList();
            for (int i = 1; i < list.Count; i++)
                Assert.True(list[i - 1].PayCost <= list[i].PayCost);
        }

        [Fact]
        public async Task GetPagedAsync_SortByWorkGroup_ReturnsOrderedResults()
        {
            var repo   = CreateRepository(SampleData());
            var result = await repo.GetPagedAsync(DefaultQuery(sortBy: "workgroup", descending: false));

            var list = result.Data.ToList();
            for (int i = 1; i < list.Count; i++)
                Assert.True(string.Compare(list[i - 1].WorkGroup, list[i].WorkGroup, StringComparison.OrdinalIgnoreCase) <= 0);
        }

        [Fact]
        public async Task GetPagedAsync_SortByGradeCode_ReturnsOrderedResults()
        {
            var repo   = CreateRepository(SampleData());
            var result = await repo.GetPagedAsync(DefaultQuery(sortBy: "gradecode", descending: false));

            var list = result.Data.ToList();
            for (int i = 1; i < list.Count; i++)
                Assert.True(string.Compare(list[i - 1].GradeCode, list[i].GradeCode, StringComparison.OrdinalIgnoreCase) <= 0);
        }

        [Fact]
        public async Task GetPagedAsync_UnknownSortBy_FallsBackToDefaultSort()
        {
            var repo   = CreateRepository(SampleData());
            var result = await repo.GetPagedAsync(DefaultQuery(sortBy: "unknownfield"));

            Assert.NotNull(result);
            Assert.Equal(4, result.Data.Count());
        }

        #endregion

        #region GetPagedAsync — PaginationData

        [Fact]
        public async Task GetPagedAsync_PaginationData_ReflectsTotalRecords()
        {
            var repo   = CreateRepository(SampleData());
            var result = await repo.GetPagedAsync(DefaultQuery(page: 1, pageSize: 10));

            Assert.Equal(4, result.PaginationData.TotalRecords);
            Assert.Equal(1, result.PaginationData.PageNumber);
            Assert.Equal(10, result.PaginationData.PageSize);
        }

        [Fact]
        public async Task GetPagedAsync_PaginationData_ReflectsFilteredCount()
        {
            var repo   = CreateRepository(SampleData());
            var result = await repo.GetPagedAsync(DefaultQuery(page: 1, pageSize: 10, filter: "{\"WorkGroup\":\"WG_CSU\"}"));

            Assert.Equal(2, result.PaginationData.TotalRecords);
        }

        #endregion
    }
}
