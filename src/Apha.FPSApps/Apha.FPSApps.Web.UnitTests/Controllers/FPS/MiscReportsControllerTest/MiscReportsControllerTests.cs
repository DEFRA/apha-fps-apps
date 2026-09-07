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

namespace Apha.FPSApps.Web.UnitTests.Controllers.FPS.MiscReportsControllerTest
{
    public class MiscReportsControllerTests
    {
        private const string WgPivotReport = "TestManagerWgPivot";
        private const string RcPivotReport = "TestManagerRcPivot";

        private readonly IProfitCentreService _profitCentreService;
        private readonly ITestsRequiredByWgService _testsRequiredByWgService;
        private readonly ITestsRequiredByRcService _testsRequiredByRcService;
        private readonly MiscReportsController _controller;

        public MiscReportsControllerTests()
        {
            _profitCentreService = Substitute.For<IProfitCentreService>();
            _testsRequiredByWgService = Substitute.For<ITestsRequiredByWgService>();
            _testsRequiredByRcService = Substitute.For<ITestsRequiredByRcService>();

            _profitCentreService.GetProfitCentresAsync()
                .Returns(ApiResponseDto<List<ProfitCentreDto>>.SuccessResponse(new List<ProfitCentreDto>()));
            _testsRequiredByWgService.GetTestsRequiredByWgAsync(Arg.Any<string?>())
                .Returns(ApiResponseDto<List<TestsRequiredByWgDto>>.SuccessResponse(new List<TestsRequiredByWgDto>()));
            _testsRequiredByRcService.GetTestsRequiredByRcAsync(Arg.Any<string?>())
                .Returns(ApiResponseDto<List<TestsRequiredByRcDto>>.SuccessResponse(new List<TestsRequiredByRcDto>()));

            _controller = new MiscReportsController(_profitCentreService, _testsRequiredByWgService, _testsRequiredByRcService)
            {
                ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
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
            return Assert.IsType<DataGridConfig<Dictionary<string, string?>>>(partial.Model);
        }

        private void SetWgRows(params TestsRequiredByWgDto[] rows) =>
            _testsRequiredByWgService.GetTestsRequiredByWgAsync(Arg.Any<string?>())
                .Returns(ApiResponseDto<List<TestsRequiredByWgDto>>.SuccessResponse(rows.ToList()));

        private void SetRcRows(params TestsRequiredByRcDto[] rows) =>
            _testsRequiredByRcService.GetTestsRequiredByRcAsync(Arg.Any<string?>())
                .Returns(ApiResponseDto<List<TestsRequiredByRcDto>>.SuccessResponse(rows.ToList()));

        [Fact]
        public async Task Index_SelectsWgPivotReport_ByDefault()
        {
            var model = ModelOf(await _controller.Index());

            Assert.Equal(WgPivotReport, model.SelectedReport);
            Assert.Equal("Test Manager WG Pivot", model.SelectedReportTitle);
            await _testsRequiredByWgService.Received(1).GetTestsRequiredByWgAsync(null);
            await _testsRequiredByRcService.DidNotReceive().GetTestsRequiredByRcAsync(Arg.Any<string?>());
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task Index_FallsBackToDefaultReport_WhenReportNotSupplied(string? report)
        {
            var model = ModelOf(await _controller.Index(report));

            Assert.Equal(WgPivotReport, model.SelectedReport);
            Assert.Equal("Test Manager WG Pivot", model.SelectedReportTitle);
        }

        [Fact]
        public async Task Index_HonoursExplicitlySelectedRcReport()
        {
            var model = ModelOf(await _controller.Index(RcPivotReport));

            Assert.Equal(RcPivotReport, model.SelectedReport);
            Assert.Equal("Test Manager RC Pivot", model.SelectedReportTitle);
            await _testsRequiredByRcService.Received(1).GetTestsRequiredByRcAsync(null);
            await _testsRequiredByWgService.DidNotReceive().GetTestsRequiredByWgAsync(Arg.Any<string?>());
        }

        [Fact]
        public async Task Index_ReportTitleMatchIsCaseInsensitive()
        {
            var model = ModelOf(await _controller.Index("testmanagerrcpivot"));

            Assert.Equal("Test Manager RC Pivot", model.SelectedReportTitle);
        }

        [Fact]
        public async Task Index_UsesProfitCentreColumnHeading()
        {
            var model = ModelOf(await _controller.Index());

            var column = Assert.Single(model.Grid.Columns, c => c.PropertyName == "ProfitCentre");
            Assert.Equal("Profit Centre", column.DisplayName);
        }

        [Fact]
        public async Task Index_WgReport_IncludesWorkGroupColumn()
        {
            var model = ModelOf(await _controller.Index(WgPivotReport));

            Assert.Contains(model.Grid.Columns, c => c.PropertyName == "WorkGroup");
        }

        [Fact]
        public async Task Index_RcReport_ExcludesWorkGroupColumn()
        {
            var model = ModelOf(await _controller.Index(RcPivotReport));

            Assert.DoesNotContain(model.Grid.Columns, c => c.PropertyName == "WorkGroup");
        }

        [Fact]
        public async Task Index_DefaultsPageSizeToTen()
        {
            SetWgRows(Enumerable.Range(1, 25)
                .Select(i => new TestsRequiredByWgDto { ProfitCentre = "PC", TestCode = $"TC{i:D3}" })
                .ToArray());

            var model = ModelOf(await _controller.Index());

            Assert.Equal(10, model.Grid.Pagination.PageSize);
            Assert.Equal(10, model.Grid.Data.Count());
            Assert.Equal(25, model.Grid.Pagination.TotalRecords);
        }

        [Fact]
        public async Task Index_PassesSelectedProfitCentreToService()
        {
            var model = ModelOf(await _controller.Index(WgPivotReport, "ASU"));

            Assert.Equal("ASU", model.SelectedProfitCentre);
            await _testsRequiredByWgService.Received(1).GetTestsRequiredByWgAsync("ASU");
        }

        [Fact]
        public async Task Index_UsesSelectedFpsYear_WhenPresent()
        {
            _controller.HttpContext.Items["SelectedFPSYear"] = "2024";

            var model = ModelOf(await _controller.Index());

            Assert.Equal(2024, model.FpsYear);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("not-a-year")]
        public async Task Index_FallsBackToCurrentYear_WhenSelectedFpsYearUnusable(string? value)
        {
            if (value != null)
            {
                _controller.HttpContext.Items["SelectedFPSYear"] = value;
            }

            var model = ModelOf(await _controller.Index());

            Assert.Equal(DateTime.Now.Year, model.FpsYear);
        }

        [Fact]
        public async Task Index_ReturnsEmptyProfitCentreOptions_WhenServiceFails()
        {
            _profitCentreService.GetProfitCentresAsync()
                .Returns(ApiResponseDto<List<ProfitCentreDto>>.FailureResponse(new List<ApiErrorDto>(), new ApiMetaDto()));

            var model = ModelOf(await _controller.Index());

            Assert.Empty(model.ProfitCentreOptions);
        }

        [Fact]
        public async Task Index_FiltersOutBlankProfitCentres()
        {
            _profitCentreService.GetProfitCentresAsync().Returns(
                ApiResponseDto<List<ProfitCentreDto>>.SuccessResponse(new List<ProfitCentreDto>
                {
                    new() { ProfitCentreId = "PC1" },
                    new() { ProfitCentreId = "   " }
                }));

            var model = ModelOf(await _controller.Index());

            Assert.Single(model.ProfitCentreOptions);
            Assert.Equal("PC1", model.ProfitCentreOptions[0].Value);
        }

        [Fact]
        public async Task Index_MapsWgRowValues()
        {
            SetWgRows(new TestsRequiredByWgDto
            {
                ProfitCentre = "Bact",
                WorkGroup = "BAC2",
                TestCode = "TC0036",
                ItemDescription = "Salmonella",
                ProjectedTotal = 45,
                UnitPrice = 116.30m
            });

            var model = ModelOf(await _controller.Index(WgPivotReport));

            var row = Assert.Single(model.Grid.Data);
            Assert.Equal("Bact", row["ProfitCentre"]);
            Assert.Equal("BAC2", row["WorkGroup"]);
            Assert.Equal("TC0036", row["TestCode"]);
            Assert.Equal("Salmonella", row["ItemDescription"]);
            Assert.Equal("45", row["ProjectedTotal"]);
            Assert.Equal("116.30", row["UnitPrice"]);
        }

        [Fact]
        public async Task Index_MapsRcRowValues()
        {
            SetRcRows(new TestsRequiredByRcDto
            {
                ProfitCentre = "ASU",
                TestCode = "PT0000",
                ItemDescription = "Camelid TB",
                ProjectedTotal = 24,
                UnitPrice = 550m
            });

            var model = ModelOf(await _controller.Index(RcPivotReport));

            var row = Assert.Single(model.Grid.Data);
            Assert.Equal("ASU", row["ProfitCentre"]);
            Assert.Equal("PT0000", row["TestCode"]);
            Assert.False(row.ContainsKey("WorkGroup"));
        }

        [Fact]
        public async Task Index_ReturnsNoRows_WhenReportServiceFails()
        {
            _testsRequiredByWgService.GetTestsRequiredByWgAsync(Arg.Any<string?>())
                .Returns(ApiResponseDto<List<TestsRequiredByWgDto>>.FailureResponse(new List<ApiErrorDto>(), new ApiMetaDto()));

            var model = ModelOf(await _controller.Index());

            Assert.Empty(model.Grid.Data);
        }

        [Fact]
        public async Task LoadGrid_DefaultsToWgPivotReport_WhenReportNotSupplied()
        {
            var grid = GridOf(await _controller.LoadGrid(null));

            Assert.Equal(10, grid.Pagination.PageSize);
            Assert.Contains(grid.Columns, c => c.PropertyName == "WorkGroup");
            await _testsRequiredByWgService.Received(1).GetTestsRequiredByWgAsync(null);
        }

        [Fact]
        public async Task LoadGrid_UsesSuppliedReportAndProfitCentre()
        {
            await _controller.LoadGrid("ASU", RcPivotReport);

            await _testsRequiredByRcService.Received(1).GetTestsRequiredByRcAsync("ASU");
        }

        [Fact]
        public async Task LoadGrid_FallsBackToDefaultPageSize_WhenNonPositive()
        {
            SetWgRows(Enumerable.Range(1, 15)
                .Select(i => new TestsRequiredByWgDto { TestCode = $"TC{i:D3}" })
                .ToArray());

            var grid = GridOf(await _controller.LoadGrid(null, WgPivotReport, pageSize: 0));

            Assert.Equal(10, grid.Pagination.PageSize);
            Assert.Equal(10, grid.Data.Count());
        }

        [Fact]
        public async Task LoadGrid_HonoursExplicitPageSizeAndPage()
        {
            SetWgRows(Enumerable.Range(1, 12)
                .Select(i => new TestsRequiredByWgDto { TestCode = $"TC{i:D3}" })
                .ToArray());

            var grid = GridOf(await _controller.LoadGrid(null, WgPivotReport, page: 2, pageSize: 5));

            Assert.Equal(5, grid.Pagination.PageSize);
            Assert.Equal(2, grid.Pagination.PageNumber);
            Assert.Equal(5, grid.Data.Count());
            Assert.Equal("TC006", grid.Data.First()["TestCode"]);
        }

        [Fact]
        public async Task LoadGrid_FallsBackToFirstPage_WhenPageNonPositive()
        {
            var grid = GridOf(await _controller.LoadGrid(null, WgPivotReport, page: 0));

            Assert.Equal(1, grid.Pagination.PageNumber);
        }

        [Fact]
        public async Task LoadGrid_AppliesColumnFilter()
        {
            SetWgRows(
                new TestsRequiredByWgDto { TestCode = "TC001", ProfitCentre = "Bact" },
                new TestsRequiredByWgDto { TestCode = "TC002", ProfitCentre = "ASU" });

            var grid = GridOf(await _controller.LoadGrid(null, WgPivotReport, filter: "{\"ProfitCentre\":\"bac\"}"));

            var row = Assert.Single(grid.Data);
            Assert.Equal("TC001", row["TestCode"]);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("not json")]
        [InlineData("{}")]
        [InlineData("{\"ProfitCentre\":\"  \"}")]
        public async Task LoadGrid_IgnoresUnusableFilterPayloads(string? filter)
        {
            SetWgRows(
                new TestsRequiredByWgDto { TestCode = "TC001", ProfitCentre = "Bact" },
                new TestsRequiredByWgDto { TestCode = "TC002", ProfitCentre = "ASU" });

            var grid = GridOf(await _controller.LoadGrid(null, WgPivotReport, filter: filter));

            Assert.Equal(2, grid.Data.Count());
            Assert.Null(grid.CurrentFilters);
        }

        [Fact]
        public async Task LoadGrid_SortsNumericColumnNumerically()
        {
            SetWgRows(
                new TestsRequiredByWgDto { TestCode = "TC001", ProjectedTotal = 100 },
                new TestsRequiredByWgDto { TestCode = "TC002", ProjectedTotal = 9 });

            var grid = GridOf(await _controller.LoadGrid(null, WgPivotReport, sortBy: "ProjectedTotal"));

            Assert.Equal("9", grid.Data.First()["ProjectedTotal"]);
        }

        [Fact]
        public async Task LoadGrid_SortsTextColumnDescending()
        {
            SetWgRows(
                new TestsRequiredByWgDto { ProfitCentre = "ASU" },
                new TestsRequiredByWgDto { ProfitCentre = "Bact" });

            var grid = GridOf(await _controller.LoadGrid(null, WgPivotReport, sortBy: "ProfitCentre", descending: true));

            Assert.Equal("Bact", grid.Data.First()["ProfitCentre"]);
        }

        [Fact]
        public async Task LoadGrid_SortsTextually_WhenColumnHasNonNumericValues()
        {
            SetWgRows(
                new TestsRequiredByWgDto { TestCode = "TC010" },
                new TestsRequiredByWgDto { TestCode = "TC002" });

            var grid = GridOf(await _controller.LoadGrid(null, WgPivotReport, sortBy: "TestCode"));

            Assert.Equal("TC002", grid.Data.First()["TestCode"]);
        }

        [Fact]
        public async Task LoadGrid_LeavesOrderUnchanged_WhenSortColumnIsBlank()
        {
            SetWgRows(
                new TestsRequiredByWgDto { ProfitCentre = "Bact" },
                new TestsRequiredByWgDto { ProfitCentre = "ASU" });

            var grid = GridOf(await _controller.LoadGrid(null, WgPivotReport, sortBy: " "));

            Assert.Equal("Bact", grid.Data.First()["ProfitCentre"]);
        }

        [Fact]
        public async Task LoadGrid_ExposesExpectedGridConfiguration()
        {
            var grid = GridOf(await _controller.LoadGrid(null));

            Assert.Equal("miscReportsGrid", grid.GridId);
            Assert.Equal("getMiscReportsExtraFilters", grid.ExtraFilterMethod);
            Assert.Equal("/FPS/MiscReports/LoadGrid", grid.BindGridUrl);
            Assert.False(grid.AllowAdd);
            Assert.False(grid.AllowEdit);
            Assert.False(grid.AllowDelete);
            Assert.True(grid.ShowPagination);
        }
    }
}
