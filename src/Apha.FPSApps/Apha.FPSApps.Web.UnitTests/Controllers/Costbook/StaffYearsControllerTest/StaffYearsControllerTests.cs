using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.CostBook;
using Apha.FPSApps.Application.Interfaces.Costbook;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.CostBook.Controllers;
using Apha.FPSApps.Web.Areas.CostBook.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace Apha.FPSApps.Web.UnitTests.Controllers.Costbook.StaffYearsControllerTest
{
    public class StaffYearsControllerTests
    {
        private readonly ICostBookProjectSummaryService _projectSummaryService;
        private readonly ICostBookYearlyDetailsService  _yearlyDetailsService;
        private readonly IMapper                        _mapper;
        private readonly StaffYearsController           _controller;

        public StaffYearsControllerTests()
        {
            _projectSummaryService = Substitute.For<ICostBookProjectSummaryService>();
            _yearlyDetailsService  = Substitute.For<ICostBookYearlyDetailsService>();
            _mapper                = Substitute.For<IMapper>();

            _controller = new StaffYearsController(
                _projectSummaryService,
                _yearlyDetailsService,
                _mapper);
        }

        // ── helpers ──────────────────────────────────────────────────────────

        private static StaffYearsPivotDto BuildPivot(int rowCount = 2, int yearCount = 3)
        {
            var years = Enumerable.Range(2022, yearCount).ToList();
            var rows  = Enumerable.Range(1, rowCount).Select(i => new StaffYearsRowDto
            {
                Project       = "P001",
                Grade         = $"G{i}",
                Total         = 10d * i,
                YearlyAmounts = years.ToDictionary(y => y, y => (double)(i + y % 10))
            }).ToList();

            return new StaffYearsPivotDto { Years = years, Rows = rows, TotalCount = rowCount };
        }

        private void SetupDefaultMapper()
        {
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                   .Returns(new QueryParameters<string> { Page = 1, PageSize = 5 });
        }

        private void SetupHeaderSuccess(string projectId)
        {
            var header = new ProjectHeaderDto { ProjectId = projectId, ProjectTitle = "Test Project" };
            _yearlyDetailsService.GetProjectHeaderAsync(projectId)
                .Returns(ApiResponseDto<ProjectHeaderDto>.SuccessResponse(header));
        }

        private void SetupPivotSuccess(string projectId, StaffYearsPivotDto pivot)
        {
            _projectSummaryService.GetStaffYearsPivotAsync(projectId, Arg.Any<QueryParameters<string>>())
                .Returns(ApiResponseDto<StaffYearsPivotDto>.SuccessResponse(pivot));
        }

        // ── Index ─────────────────────────────────────────────────────────────

        [Fact]
        public async Task Index_WhenHeaderSucceeds_ReturnsViewWithCorrectViewModel()
        {
            // Arrange
            const string projectId = "P001";
            var pivot = BuildPivot();
            SetupHeaderSuccess(projectId);
            SetupPivotSuccess(projectId, pivot);

            // Act
            var result = await _controller.Index(projectId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<StaffYearsViewModel>(viewResult.Model);
            Assert.Equal(projectId, model.ProjectId);
            Assert.Equal(projectId, model.ProjectHeaderDto.ProjectId);
            Assert.NotNull(model.Grid);
        }

        [Fact]
        public async Task Index_WhenHeaderFails_RedirectsToProjectsIndex()
        {
            // Arrange
            const string projectId = "P001";
            var errors = new List<ApiErrorDto> { new() { Message = "Not found", Code = "NOT_FOUND" } };
            _yearlyDetailsService.GetProjectHeaderAsync(projectId)
                .Returns(ApiResponseDto<ProjectHeaderDto>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _controller.Index(projectId);

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index",    redirect.ActionName);
            Assert.Equal("Projects", redirect.ControllerName);
        }

        [Fact]
        public async Task Index_WhenHeaderDataIsNull_RedirectsToProjectsIndex()
        {
            // Arrange
            const string projectId = "P001";
            _yearlyDetailsService.GetProjectHeaderAsync(projectId)
                .Returns(ApiResponseDto<ProjectHeaderDto>.SuccessResponse(null!));

            // Act
            var result = await _controller.Index(projectId);

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index",    redirect.ActionName);
            Assert.Equal("Projects", redirect.ControllerName);
        }

        [Fact]
        public async Task Index_GridContainsDynamicYearColumns()
        {
            // Arrange
            const string projectId = "P001";
            var pivot = BuildPivot(rowCount: 1, yearCount: 3);
            SetupHeaderSuccess(projectId);
            SetupPivotSuccess(projectId, pivot);

            // Act
            var result = await _controller.Index(projectId);

            // Assert
            var model = Assert.IsType<StaffYearsViewModel>(
                Assert.IsType<ViewResult>(result).Model!);

            // 3 fixed columns (Project, Grade, Total) + 3 year columns
            Assert.Equal(6, model.Grid.Columns.Count);
            Assert.Contains(model.Grid.Columns, c => c.PropertyName == "Y1");
            Assert.Contains(model.Grid.Columns, c => c.PropertyName == "Y2");
            Assert.Contains(model.Grid.Columns, c => c.PropertyName == "Y3");
        }

        [Fact]
        public async Task Index_GridRowsAreMappedCorrectly()
        {
            // Arrange
            const string projectId = "P001";
            var pivot = BuildPivot(rowCount: 2, yearCount: 2);
            SetupHeaderSuccess(projectId);
            SetupPivotSuccess(projectId, pivot);

            // Act
            var result = await _controller.Index(projectId);

            // Assert
            var model = Assert.IsType<StaffYearsViewModel>(
                Assert.IsType<ViewResult>(result).Model!);

            Assert.Equal(2, model.Grid.Data.Count);
            Assert.All(model.Grid.Data, row => Assert.Equal("P001", row.Project));
        }

        [Fact]
        public async Task Index_WhenPivotServiceFails_GridHasEmptyRows()
        {
            // Arrange
            const string projectId = "P001";
            SetupHeaderSuccess(projectId);
            var errors = new List<ApiErrorDto> { new() { Message = "API error", Code = "ERR" } };
            _projectSummaryService.GetStaffYearsPivotAsync(projectId, Arg.Any<QueryParameters<string>>())
                .Returns(ApiResponseDto<StaffYearsPivotDto>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _controller.Index(projectId);

            // Assert
            var model = Assert.IsType<StaffYearsViewModel>(
                Assert.IsType<ViewResult>(result).Model!);
            Assert.Empty(model.Grid.Data);
        }

        [Fact]
        public async Task Index_GridBindUrlContainsEncodedProjectId()
        {
            // Arrange
            const string projectId = "P001";
            SetupHeaderSuccess(projectId);
            SetupPivotSuccess(projectId, BuildPivot());

            // Act
            var result = await _controller.Index(projectId);

            // Assert
            var model = Assert.IsType<StaffYearsViewModel>(
                Assert.IsType<ViewResult>(result).Model!);
            Assert.Contains(Uri.EscapeDataString(projectId), model.Grid.BindGridUrl);
        }

        [Fact]
        public async Task Index_YearColumnsAreCappedAtTen()
        {
            // Arrange
            const string projectId = "P001";
            var pivot = BuildPivot(rowCount: 1, yearCount: 15); // 15 years, but max is 10
            SetupHeaderSuccess(projectId);
            SetupPivotSuccess(projectId, pivot);

            // Act
            var result = await _controller.Index(projectId);

            // Assert
            var model = Assert.IsType<StaffYearsViewModel>(
                Assert.IsType<ViewResult>(result).Model!);

            var yearColumns = model.Grid.Columns
                .Where(c => c.PropertyName.StartsWith("Y", StringComparison.Ordinal) && c.PropertyName != "Total")
                .ToList();
            Assert.Equal(10, yearColumns.Count);
        }

        [Fact]
        public async Task Index_YearValuesAreRoundedToTwoDecimalPlaces()
        {
            // Arrange
            const string projectId = "P001";
            var pivot = new StaffYearsPivotDto
            {
                Years      = [2022],
                TotalCount = 1,
                Rows       =
                [
                    new() { Project = "P001", Grade = "G1", Total = 1.23456d, YearlyAmounts = new() { [2022] = 3.14159d } }
                ]
            };
            SetupHeaderSuccess(projectId);
            SetupPivotSuccess(projectId, pivot);

            // Act
            var result = await _controller.Index(projectId);

            // Assert
            var model = Assert.IsType<StaffYearsViewModel>(
                Assert.IsType<ViewResult>(result).Model!);

            var row = model.Grid.Data[0];
            Assert.Equal(1.23m, row.Total);
            Assert.Equal(3.14m, row.Y1);
        }

        [Fact]
        public async Task Index_GridKeyPropertyIsGrade()
        {
            // Arrange
            const string projectId = "P001";
            SetupHeaderSuccess(projectId);
            SetupPivotSuccess(projectId, BuildPivot());

            // Act
            var result = await _controller.Index(projectId);

            // Assert
            var model = Assert.IsType<StaffYearsViewModel>(
                Assert.IsType<ViewResult>(result).Model!);
            Assert.Equal("Grade", model.Grid.KeyProperty);
        }

        [Fact]
        public async Task Index_GridDoesNotAllowAddEditDelete()
        {
            // Arrange
            const string projectId = "P001";
            SetupHeaderSuccess(projectId);
            SetupPivotSuccess(projectId, BuildPivot());

            // Act
            var result = await _controller.Index(projectId);

            // Assert
            var model = Assert.IsType<StaffYearsViewModel>(
                Assert.IsType<ViewResult>(result).Model!);
            Assert.False(model.Grid.AllowAdd);
            Assert.False(model.Grid.AllowEdit);
            Assert.False(model.Grid.AllowDelete);
        }

        [Fact]
        public async Task Index_PaginationIsPopulatedFromPivotTotalCount()
        {
            // Arrange
            const string projectId = "P001";
            var pivot = BuildPivot(rowCount: 2);
            pivot.TotalCount = 50;
            SetupHeaderSuccess(projectId);
            SetupPivotSuccess(projectId, pivot);

            // Act
            var result = await _controller.Index(projectId);

            // Assert
            var model = Assert.IsType<StaffYearsViewModel>(
                Assert.IsType<ViewResult>(result).Model!);
            Assert.Equal(50, model.Grid.Pagination.TotalRecords);
        }

        // ── LoadGrid ──────────────────────────────────────────────────────────

        [Fact]
        public async Task LoadGrid_WithValidRequest_ReturnsPartialViewWithDataGrid()
        {
            // Arrange
            const string projectId = "P001";
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            var pivot   = BuildPivot();
            SetupDefaultMapper();
            SetupPivotSuccess(projectId, pivot);

            // Act
            var result = await _controller.LoadGrid(projectId, request);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partial.ViewName);
            var grid = Assert.IsType<DataGridConfig<StaffYearsPivotRow>>(partial.Model!);
            Assert.Equal("staffYearsGrid", grid.GridId);
        }

        [Fact]
        public async Task LoadGrid_WithPivotData_ReturnsCorrectRowCount()
        {
            // Arrange
            const string projectId = "P001";
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            var pivot   = BuildPivot(rowCount: 3);
            SetupDefaultMapper();
            SetupPivotSuccess(projectId, pivot);

            // Act
            var result = await _controller.LoadGrid(projectId, request);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var grid    = Assert.IsType<DataGridConfig<StaffYearsPivotRow>>(partial.Model!);
            Assert.Equal(3, grid.Data.Count);
        }

        [Fact]
        public async Task LoadGrid_WhenServiceFails_ReturnsPartialViewWithEmptyRows()
        {
            // Arrange
            const string projectId = "P001";
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            var errors  = new List<ApiErrorDto> { new() { Message = "Error", Code = "ERR" } };
            SetupDefaultMapper();
            _projectSummaryService.GetStaffYearsPivotAsync(projectId, Arg.Any<QueryParameters<string>>())
                .Returns(ApiResponseDto<StaffYearsPivotDto>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _controller.LoadGrid(projectId, request);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var grid    = Assert.IsType<DataGridConfig<StaffYearsPivotRow>>(partial.Model!);
            Assert.Empty(grid.Data);
        }

        [Fact]
        public async Task LoadGrid_PaginationIsPopulatedFromPivotTotalCount()
        {
            // Arrange
            const string projectId = "P001";
            var request = new PaginationFilter<string> { Page = 2, PageSize = 5, Filter = "{}" };
            var pivot   = BuildPivot(rowCount: 2);
            pivot.TotalCount = 20;
            SetupDefaultMapper();
            SetupPivotSuccess(projectId, pivot);

            // Act
            var result = await _controller.LoadGrid(projectId, request);

            // Assert
            var grid = Assert.IsType<DataGridConfig<StaffYearsPivotRow>>(
                Assert.IsType<PartialViewResult>(result).Model!);
            Assert.Equal(20, grid.Pagination.TotalRecords);
        }

        [Fact]
        public async Task LoadGrid_WithFilterJson_PopulatesCurrentFilters()
        {
            // Arrange
            const string projectId = "P001";
            var request = new PaginationFilter<string>
            {
                Page     = 1,
                PageSize = 10,
                Filter   = """{"Grade":"G1"}"""
            };
            SetupDefaultMapper();
            SetupPivotSuccess(projectId, BuildPivot());

            // Act
            var result = await _controller.LoadGrid(projectId, request);

            // Assert
            var grid = Assert.IsType<DataGridConfig<StaffYearsPivotRow>>(
                Assert.IsType<PartialViewResult>(result).Model!);
            Assert.NotNull(grid.CurrentFilters);
            Assert.True(grid.CurrentFilters.ContainsKey("Grade"));
            Assert.Equal("G1", grid.CurrentFilters["Grade"]);
        }

        [Fact]
        public async Task LoadGrid_YearColumnsDisplayNameMatchesYear()
        {
            // Arrange
            const string projectId = "P001";
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            var pivot   = BuildPivot(yearCount: 2); // years 2022, 2023
            SetupDefaultMapper();
            SetupPivotSuccess(projectId, pivot);

            // Act
            var result = await _controller.LoadGrid(projectId, request);

            // Assert
            var grid = Assert.IsType<DataGridConfig<StaffYearsPivotRow>>(
                Assert.IsType<PartialViewResult>(result).Model!);

            var yearCols = grid.Columns
                .Where(c => c.PropertyName.StartsWith("Y", StringComparison.Ordinal) && c.PropertyName != "Total")
                .ToList();
            Assert.Equal("2022", yearCols[0].DisplayName);
            Assert.Equal("2023", yearCols[1].DisplayName);
        }

        [Fact]
        public async Task LoadGrid_WithInvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            const string projectId = "P001";
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            _controller.ModelState.AddModelError("Page", "Required");

            // Act
            var result = await _controller.LoadGrid(projectId, request);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task LoadGrid_YearColumnsAreCappedAtTen()
        {
            // Arrange
            const string projectId = "P001";
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10, Filter = "{}" };
            var pivot   = BuildPivot(rowCount: 1, yearCount: 15);
            SetupDefaultMapper();
            SetupPivotSuccess(projectId, pivot);

            // Act
            var result = await _controller.LoadGrid(projectId, request);

            // Assert
            var grid = Assert.IsType<DataGridConfig<StaffYearsPivotRow>>(
                Assert.IsType<PartialViewResult>(result).Model!);

            var yearColumns = grid.Columns
                .Where(c => c.PropertyName.StartsWith("Y", StringComparison.Ordinal) && c.PropertyName != "Total")
                .ToList();
            Assert.Equal(10, yearColumns.Count);
        }
    }
}