using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Web.Areas.FPS.Controllers;
using Apha.FPSApps.Web.Areas.FPS.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;

namespace Apha.FPSApps.Web.UnitTests.Controllers.FPS.MiscReportsTestControllerTest
{
    public class MiscReportsTestControllerTests
    {
        private const string WgPivotReport = "TestManagerWgPivot";
        private const string RcPivotReport = "TestManagerRcPivot";

        private readonly IProfitCentreService _profitCentreService;
        private readonly ITestsRequiredByWgService _wgService;
        private readonly ITestsRequiredByRcService _rcService;
        private readonly DefaultHttpContext _httpContext;
        private readonly MiscReportsTestController _controller;

        public MiscReportsTestControllerTests()
        {
            _profitCentreService = Substitute.For<IProfitCentreService>();
            _wgService = Substitute.For<ITestsRequiredByWgService>();
            _rcService = Substitute.For<ITestsRequiredByRcService>();

            _profitCentreService.GetProfitCentresAsync()
                .Returns(ApiResponseDto<List<ProfitCentreDto>>.SuccessResponse(new List<ProfitCentreDto>()));
            _wgService.GetTestsRequiredByWgByYearAsync(Arg.Any<string?>(), Arg.Any<int>())
                .Returns(ApiResponseDto<List<TestsRequiredByWgDto>>.SuccessResponse(new List<TestsRequiredByWgDto>()));
            _rcService.GetTestsRequiredByRcByYearAsync(Arg.Any<string?>(), Arg.Any<int>())
                .Returns(ApiResponseDto<List<TestsRequiredByRcDto>>.SuccessResponse(new List<TestsRequiredByRcDto>()));

            _httpContext = new DefaultHttpContext();
            _httpContext.Items["SelectedFPSYear"] = "2025";

            _controller = new MiscReportsTestController(_profitCentreService, _wgService, _rcService)
            {
                ControllerContext = new ControllerContext { HttpContext = _httpContext }
            };
        }

        private static MiscReportsViewModel ModelOf(IActionResult result)
        {
            var view = Assert.IsType<ViewResult>(result);
            return Assert.IsType<MiscReportsViewModel>(view.Model);
        }

        private static DataGridConfig<Dictionary<string, string?>> GridOf(IActionResult result)
        {
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partial.ViewName);
            return Assert.IsType<DataGridConfig<Dictionary<string, string?>>>(partial.Model);
        }

        private void SetWgRows(params TestsRequiredByWgDto[] rows) =>
            _wgService.GetTestsRequiredByWgByYearAsync(Arg.Any<string?>(), Arg.Any<int>())
                .Returns(ApiResponseDto<List<TestsRequiredByWgDto>>.SuccessResponse(rows.ToList()));

        private void SetRcRows(params TestsRequiredByRcDto[] rows) =>
            _rcService.GetTestsRequiredByRcByYearAsync(Arg.Any<string?>(), Arg.Any<int>())
                .Returns(ApiResponseDto<List<TestsRequiredByRcDto>>.SuccessResponse(rows.ToList()));

        #region Index

        [Fact]
        public async Task Index_ByDefault_LoadsWgPivotForSelectedYear()
        {
            var model = ModelOf(await _controller.Index());

            Assert.Equal(WgPivotReport, model.SelectedReport);
            Assert.Equal(2025, model.FpsYear);
            Assert.Null(model.SelectedProfitCentre);
            Assert.Equal("miscReportsTestGrid", model.Grid.GridId);
            Assert.Equal("/FPS/MiscReportsTest/LoadGrid", model.Grid.BindGridUrl);
            Assert.Equal("getMiscReportsTestExtraFilters", model.Grid.ExtraFilterMethod);
            Assert.Equal("Test Manager WG Pivot", model.Grid.Title);
            await _wgService.Received(1).GetTestsRequiredByWgByYearAsync(null, 2025);
            await _rcService.DidNotReceive().GetTestsRequiredByRcByYearAsync(Arg.Any<string?>(), Arg.Any<int>());
        }

        [Fact]
        public async Task Index_WithRcReport_LoadsRcPivotForSelectedYearAndProfitCentre()
        {
            var model = ModelOf(await _controller.Index(RcPivotReport, "ASU"));

            Assert.Equal(RcPivotReport, model.SelectedReport);
            Assert.Equal("ASU", model.SelectedProfitCentre);
            Assert.Equal("Test Manager RC Pivot", model.Grid.Title);
            Assert.DoesNotContain(model.Grid.Columns, c => c.PropertyName == "WorkGroup");
            await _rcService.Received(1).GetTestsRequiredByRcByYearAsync("ASU", 2025);
            await _wgService.DidNotReceive().GetTestsRequiredByWgByYearAsync(Arg.Any<string?>(), Arg.Any<int>());
        }

        [Fact]
        public async Task Index_WithWgReport_IncludesWorkGroupColumn()
        {
            var model = ModelOf(await _controller.Index(WgPivotReport));

            Assert.Contains(model.Grid.Columns, c => c.PropertyName == "WorkGroup");
            Assert.Equal(6, model.Grid.Columns.Count);
        }

        [Fact]
        public async Task Index_WhenNoYearInContext_FallsBackToCurrentYear()
        {
            _httpContext.Items.Remove("SelectedFPSYear");

            var model = ModelOf(await _controller.Index());

            Assert.Equal(DateTime.Now.Year, model.FpsYear);
            await _wgService.Received(1).GetTestsRequiredByWgByYearAsync(null, DateTime.Now.Year);
        }

        [Fact]
        public async Task Index_WhenYearInContextIsNotNumeric_FallsBackToCurrentYear()
        {
            _httpContext.Items["SelectedFPSYear"] = "abc";

            var model = ModelOf(await _controller.Index());

            Assert.Equal(DateTime.Now.Year, model.FpsYear);
        }

        [Fact]
        public async Task Index_WhenYearInContextIsNull_FallsBackToCurrentYear()
        {
            _httpContext.Items["SelectedFPSYear"] = null;

            var model = ModelOf(await _controller.Index());

            Assert.Equal(DateTime.Now.Year, model.FpsYear);
        }

        [Fact]
        public async Task Index_PopulatesProfitCentreOptions_SkippingBlankIds()
        {
            _profitCentreService.GetProfitCentresAsync().Returns(
                ApiResponseDto<List<ProfitCentreDto>>.SuccessResponse(new List<ProfitCentreDto>
                {
                    new() { ProfitCentreId = "ASU" },
                    new() { ProfitCentreId = " " },
                    new() { ProfitCentreId = null! },
                    new() { ProfitCentreId = "Bact" }
                }));

            var model = ModelOf(await _controller.Index());

            Assert.Equal(new[] { "ASU", "Bact" }, model.ProfitCentreOptions.Select(o => o.Value));
        }

        [Fact]
        public async Task Index_WhenProfitCentreServiceFails_ReturnsEmptyOptions()
        {
            _profitCentreService.GetProfitCentresAsync().Returns(
                ApiResponseDto<List<ProfitCentreDto>>.FailureResponse(new List<ApiErrorDto>(), new ApiMetaDto()));

            var model = ModelOf(await _controller.Index());

            Assert.Empty(model.ProfitCentreOptions);
        }

        [Fact]
        public async Task Index_MapsWgRowsToGridData()
        {
            SetWgRows(new TestsRequiredByWgDto
            {
                ProfitCentre = "ASU", WorkGroup = "WG1", TestCode = "TC1",
                ItemDescription = "Desc", ProjectedTotal = 12, UnitPrice = 3.5m
            });

            var model = ModelOf(await _controller.Index());

            var row = Assert.Single(model.Grid.Data);
            Assert.Equal("ASU", row["ProfitCentre"]);
            Assert.Equal("WG1", row["WorkGroup"]);
            Assert.Equal("TC1", row["TestCode"]);
            Assert.Equal("Desc", row["ItemDescription"]);
            Assert.Equal("12", row["ProjectedTotal"]);
            Assert.Equal("3.5", row["UnitPrice"]);
        }

        [Fact]
        public async Task Index_MapsRcRowsToGridData_WithNullNumbers()
        {
            SetRcRows(new TestsRequiredByRcDto { ProfitCentre = "ASU", TestCode = "TC1", ItemDescription = "Desc" });

            var model = ModelOf(await _controller.Index(RcPivotReport));

            var row = Assert.Single(model.Grid.Data);
            Assert.Equal("ASU", row["ProfitCentre"]);
            Assert.False(row.ContainsKey("WorkGroup"));
            Assert.Null(row["ProjectedTotal"]);
            Assert.Null(row["UnitPrice"]);
        }

        [Fact]
        public async Task Index_MapsRcRowsWithValues()
        {
            SetRcRows(new TestsRequiredByRcDto { ProfitCentre = "ASU", TestCode = "TC1", ProjectedTotal = 7, UnitPrice = 1.25m });

            var model = ModelOf(await _controller.Index(RcPivotReport));

            var row = Assert.Single(model.Grid.Data);
            Assert.Equal("7", row["ProjectedTotal"]);
            Assert.Equal("1.25", row["UnitPrice"]);
        }

        [Fact]
        public async Task Index_WhenWgServiceFails_ReturnsEmptyGrid()
        {
            _wgService.GetTestsRequiredByWgByYearAsync(Arg.Any<string?>(), Arg.Any<int>())
                .Returns(ApiResponseDto<List<TestsRequiredByWgDto>>.FailureResponse(new List<ApiErrorDto>(), new ApiMetaDto()));

            var model = ModelOf(await _controller.Index());

            Assert.Empty(model.Grid.Data);
            Assert.Equal(0, model.Grid.Pagination.TotalRecords);
        }

        [Fact]
        public async Task Index_WhenRcServiceFails_ReturnsEmptyGrid()
        {
            _rcService.GetTestsRequiredByRcByYearAsync(Arg.Any<string?>(), Arg.Any<int>())
                .Returns(ApiResponseDto<List<TestsRequiredByRcDto>>.FailureResponse(new List<ApiErrorDto>(), new ApiMetaDto()));

            var model = ModelOf(await _controller.Index(RcPivotReport));

            Assert.Empty(model.Grid.Data);
        }

        #endregion

        #region LoadGrid

        [Fact]
        public async Task LoadGrid_WithExplicitYear_UsesThatYear()
        {
            GridOf(await _controller.LoadGrid("ASU", WgPivotReport, fpsYear: 2024));

            await _wgService.Received(1).GetTestsRequiredByWgByYearAsync("ASU", 2024);
        }

        [Theory]
        [InlineData(null)]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task LoadGrid_WithoutValidYear_UsesYearFromContext(int? fpsYear)
        {
            GridOf(await _controller.LoadGrid(null, WgPivotReport, fpsYear));

            await _wgService.Received(1).GetTestsRequiredByWgByYearAsync(null, 2025);
        }

        [Fact]
        public async Task LoadGrid_WithBlankReport_DefaultsToWgPivot()
        {
            var grid = GridOf(await _controller.LoadGrid("ASU", " ", 2025));

            Assert.Equal("Test Manager WG Pivot", grid.Title);
            await _wgService.Received(1).GetTestsRequiredByWgByYearAsync("ASU", 2025);
        }

        [Fact]
        public async Task LoadGrid_WithRcReport_LoadsRcRows()
        {
            SetRcRows(new TestsRequiredByRcDto { TestCode = "TC1" });

            var grid = GridOf(await _controller.LoadGrid("ASU", RcPivotReport, 2025));

            Assert.Single(grid.Data);
            await _rcService.Received(1).GetTestsRequiredByRcByYearAsync("ASU", 2025);
        }

        [Fact]
        public async Task LoadGrid_PaginatesRows()
        {
            SetWgRows(Enumerable.Range(1, 25).Select(i => new TestsRequiredByWgDto { TestCode = $"TC{i:D3}" }).ToArray());

            var grid = GridOf(await _controller.LoadGrid(null, WgPivotReport, 2025, page: 3, pageSize: 10));

            Assert.Equal(5, grid.Data.Count());
            Assert.Equal("TC021", grid.Data.First()["TestCode"]);
            Assert.Equal(25, grid.Pagination.TotalRecords);
            Assert.Equal(3, grid.Pagination.PageNumber);
            Assert.Equal(10, grid.Pagination.PageSize);
        }

        [Fact]
        public async Task LoadGrid_WithInvalidPageAndPageSize_UsesDefaults()
        {
            SetWgRows(Enumerable.Range(1, 15).Select(i => new TestsRequiredByWgDto { TestCode = $"TC{i:D3}" }).ToArray());

            var grid = GridOf(await _controller.LoadGrid(null, WgPivotReport, 2025, page: 0, pageSize: 0));

            Assert.Equal(10, grid.Data.Count());
            Assert.Equal(1, grid.Pagination.PageNumber);
            Assert.Equal(10, grid.Pagination.PageSize);
        }

        #endregion

        #region Filtering

        [Fact]
        public async Task LoadGrid_WithFilter_ReturnsMatchingRowsCaseInsensitive()
        {
            SetWgRows(
                new TestsRequiredByWgDto { TestCode = "TC001", ProfitCentre = "Bact" },
                new TestsRequiredByWgDto { TestCode = "TC002", ProfitCentre = "ASU" });

            var grid = GridOf(await _controller.LoadGrid(null, WgPivotReport, 2025, filter: "{\"ProfitCentre\":\"bac\"}"));

            var row = Assert.Single(grid.Data);
            Assert.Equal("TC001", row["TestCode"]);
            Assert.NotNull(grid.CurrentFilters);
        }

        [Fact]
        public async Task LoadGrid_WithFilterOnNullCell_ExcludesRow()
        {
            SetWgRows(
                new TestsRequiredByWgDto { TestCode = "TC001", WorkGroup = null },
                new TestsRequiredByWgDto { TestCode = "TC002", WorkGroup = "WG1" });

            var grid = GridOf(await _controller.LoadGrid(null, WgPivotReport, 2025, filter: "{\"WorkGroup\":\"WG\"}"));

            Assert.Equal("TC002", Assert.Single(grid.Data)["TestCode"]);
        }

        [Fact]
        public async Task LoadGrid_WithFilterOnUnknownColumn_ExcludesAllRows()
        {
            SetWgRows(new TestsRequiredByWgDto { TestCode = "TC001" });

            var grid = GridOf(await _controller.LoadGrid(null, WgPivotReport, 2025, filter: "{\"Unknown\":\"x\"}"));

            Assert.Empty(grid.Data);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("{}")]
        [InlineData("{\"TestCode\":\"  \"}")]
        [InlineData("not-json")]
        [InlineData("null")]
        public async Task LoadGrid_WithEmptyOrInvalidFilter_ReturnsAllRows(string? filter)
        {
            SetWgRows(new TestsRequiredByWgDto { TestCode = "TC001" }, new TestsRequiredByWgDto { TestCode = "TC002" });

            var grid = GridOf(await _controller.LoadGrid(null, WgPivotReport, 2025, filter: filter));

            Assert.Equal(2, grid.Data.Count());
            Assert.Null(grid.CurrentFilters);
        }

        [Fact]
        public async Task LoadGrid_WithFilterAndNoRows_ReturnsEmpty()
        {
            var grid = GridOf(await _controller.LoadGrid(null, WgPivotReport, 2025, filter: "{\"TestCode\":\"TC\"}"));

            Assert.Empty(grid.Data);
        }

        #endregion

        #region Sorting

        [Fact]
        public async Task LoadGrid_SortByNumericColumn_SortsNumerically()
        {
            SetWgRows(
                new TestsRequiredByWgDto { TestCode = "TC001", ProjectedTotal = 100 },
                new TestsRequiredByWgDto { TestCode = "TC002", ProjectedTotal = 9 },
                new TestsRequiredByWgDto { TestCode = "TC003", ProjectedTotal = null });

            var asc = GridOf(await _controller.LoadGrid(null, WgPivotReport, 2025, sortBy: "ProjectedTotal"));
            var desc = GridOf(await _controller.LoadGrid(null, WgPivotReport, 2025, sortBy: "ProjectedTotal", descending: true));

            Assert.Equal(new[] { "TC003", "TC002", "TC001" }, asc.Data.Select(r => r["TestCode"]));
            Assert.Equal(new[] { "TC001", "TC002", "TC003" }, desc.Data.Select(r => r["TestCode"]));
        }

        [Fact]
        public async Task LoadGrid_SortByTextColumn_SortsCaseInsensitive()
        {
            SetWgRows(
                new TestsRequiredByWgDto { TestCode = "TC1", ProfitCentre = "bact" },
                new TestsRequiredByWgDto { TestCode = "TC2", ProfitCentre = "ASU" });

            var asc = GridOf(await _controller.LoadGrid(null, WgPivotReport, 2025, sortBy: "ProfitCentre"));
            var desc = GridOf(await _controller.LoadGrid(null, WgPivotReport, 2025, sortBy: "ProfitCentre", descending: true));

            Assert.Equal(new[] { "ASU", "bact" }, asc.Data.Select(r => r["ProfitCentre"]));
            Assert.Equal(new[] { "bact", "ASU" }, desc.Data.Select(r => r["ProfitCentre"]));
        }

        [Fact]
        public async Task LoadGrid_SortByMixedColumn_FallsBackToTextSort()
        {
            SetWgRows(
                new TestsRequiredByWgDto { TestCode = "TC010" },
                new TestsRequiredByWgDto { TestCode = "TC002" });

            var grid = GridOf(await _controller.LoadGrid(null, WgPivotReport, 2025, sortBy: "TestCode"));

            Assert.Equal(new[] { "TC002", "TC010" }, grid.Data.Select(r => r["TestCode"]));
        }

        [Fact]
        public async Task LoadGrid_SortByColumnWithOnlyEmptyValues_UsesTextSort()
        {
            SetWgRows(new TestsRequiredByWgDto { TestCode = "B" }, new TestsRequiredByWgDto { TestCode = "A" });

            var grid = GridOf(await _controller.LoadGrid(null, WgPivotReport, 2025, sortBy: "WorkGroup"));

            Assert.Equal(2, grid.Data.Count());
            Assert.Equal("WorkGroup", grid.Pagination.SortColumn);
        }

        [Fact]
        public async Task LoadGrid_SortByUnknownColumn_KeepsRows()
        {
            SetWgRows(new TestsRequiredByWgDto { TestCode = "B" }, new TestsRequiredByWgDto { TestCode = "A" });

            var grid = GridOf(await _controller.LoadGrid(null, WgPivotReport, 2025, sortBy: "Missing"));

            Assert.Equal(2, grid.Data.Count());
        }

        [Fact]
        public async Task LoadGrid_SortWithNoRows_ReturnsEmpty()
        {
            var grid = GridOf(await _controller.LoadGrid(null, WgPivotReport, 2025, sortBy: "TestCode", descending: true));

            Assert.Empty(grid.Data);
            Assert.True(grid.Pagination.SortDirection);
        }

        #endregion
    }
}
