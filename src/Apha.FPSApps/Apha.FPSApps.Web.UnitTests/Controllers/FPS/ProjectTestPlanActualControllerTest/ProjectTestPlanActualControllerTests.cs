using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.PACT;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.FPS.Controllers;
using Apha.FPSApps.Web.Areas.FPS.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using System.Text.Json;
using Xunit;

namespace Apha.FPSApps.Web.UnitTests.Controllers.FPS.ProjectTestPlanActualControllerTest
{
    public class ProjectTestPlanActualControllerTests
    {
        private readonly IMapper _mapper;
        private readonly IProjectTestPlanActualService _projTestPlanActualService;
        private readonly IProjectService _projectService;
        private readonly ITestRequirementService _testRequirementService;
        private readonly ProjectTestPlanActualController _controller;

        public ProjectTestPlanActualControllerTests()
        {
            _mapper = Substitute.For<IMapper>();
            _projTestPlanActualService = Substitute.For<IProjectTestPlanActualService>();
            _projectService = Substitute.For<IProjectService>();
            _testRequirementService = Substitute.For<ITestRequirementService>();
            _controller = new ProjectTestPlanActualController(_mapper, _projTestPlanActualService, _projectService, _testRequirementService);
        }

        private static T? Deserialize<T>(JsonResult r) => JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(r.Value), new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        private class JR { public bool success { get; set; } public string? message { get; set; } }

        private void SetupProjectList(List<ProjectDto>? projects = null)
        {
            projects ??= new List<ProjectDto> { new() { ParentProject = "AH0033", ProjectTitle = "Test Project", Program = "P001", Contract = "C001" } };
            _projectService.GetAllProjectsAsync().Returns(ApiResponseDto<List<ProjectDto>>.SuccessResponse(projects));
        }
        private void SetupProjectById(string code, ProjectDto? dto = null)
        {
            dto ??= new ProjectDto { ParentProject = code, ProjectTitle = "Test Project", Program = "P001", Contract = "C001" };
            _projectService.GetProjectByIdAsync(code).Returns(ApiResponseDto<ProjectDto>.SuccessResponse(dto));
        }
        private void SetupPlannedCost(string code, decimal cost = 500m) => _projTestPlanActualService.GetTotalPlannedCostAsync(code).Returns(ApiResponseDto<decimal>.SuccessResponse(cost));

        [Fact]
        public async Task Index_WithValidProjectCode_ReturnsViewWithPopulatedModel()
        {
            SetupProjectList(); SetupProjectById("AH0033"); SetupPlannedCost("AH0033");
            var result = await _controller.Index("AH0033");
            var model = Assert.IsType<ProjectTestPlanActualViewModel>(Assert.IsType<ViewResult>(result).Model);
            Assert.Equal("AH0033", model.SelectedProjectCode);
            Assert.Equal("Test Project", model.ProjectTitle);
            Assert.Equal(500m, model.TotalPlannedCost);
        }

        [Fact]
        public async Task Index_WithNoProjectCode_SelectsFirstProjectFromList()
        {
            var projects = new List<ProjectDto> { new() { ParentProject = "AH0001", ProjectTitle = "First", Program = "P001", Contract = "C001" }, new() { ParentProject = "AH0002", ProjectTitle = "Second", Program = "P002", Contract = "C002" } };
            SetupProjectList(projects); SetupProjectById("AH0001", projects[0]); SetupPlannedCost("AH0001");
            var model = Assert.IsType<ProjectTestPlanActualViewModel>(Assert.IsType<ViewResult>(await _controller.Index(null)).Model);
            Assert.Equal("AH0001", model.SelectedProjectCode);
        }

        [Fact]
        public async Task Index_WithInvalidProjectCode_FallsBackToFirstProject()
        {
            SetupProjectList(); SetupProjectById("AH0033"); SetupPlannedCost("AH0033");
            var model = Assert.IsType<ProjectTestPlanActualViewModel>(Assert.IsType<ViewResult>(await _controller.Index("INVALID")).Model);
            Assert.Equal("AH0033", model.SelectedProjectCode);
        }

        [Fact]
        public async Task Index_ReturnsTestPlanGridAndCompareTests2Grid()
        {
            SetupProjectList(); SetupProjectById("AH0033"); SetupPlannedCost("AH0033");
            var model = Assert.IsType<ProjectTestPlanActualViewModel>(Assert.IsType<ViewResult>(await _controller.Index("AH0033")).Model);
            Assert.Equal("testPlanGrid", model.TestPlanGrid.GridId);
            Assert.Equal("compareTests2Grid", model.CompareTests2Grid.GridId);
            Assert.False(model.CompareTests2Grid.AllowAdd);
            Assert.False(model.CompareTests2Grid.AllowEdit);
            Assert.True(model.CompareTests2Grid.AllowDelete);
        }

        [Fact]
        public async Task GetProjectInfo_WithValidProjectCode_ReturnsSuccessJson()
        {
            SetupProjectById("AH0033");
            Assert.True(Deserialize<JR>(Assert.IsType<JsonResult>(await _controller.GetProjectInfo("AH0033")))?.success);
        }

        [Theory][InlineData(null)][InlineData("")][InlineData("   ")]
        public async Task GetProjectInfo_WhenProjectCodeMissing_ReturnsFailureJson(string? code) => Assert.False(Deserialize<JR>(Assert.IsType<JsonResult>(await _controller.GetProjectInfo(code!)))?.success);

        [Fact]
        public async Task GetTotalPlannedCost_WithValidProjectCode_ReturnsSuccessJson()
        {
            SetupPlannedCost("AH0033", 750m);
            Assert.True(Deserialize<JR>(Assert.IsType<JsonResult>(await _controller.GetTotalPlannedCost("AH0033")))?.success);
        }

        [Theory][InlineData(null)][InlineData("")]
        public async Task GetTotalPlannedCost_WhenProjectCodeMissing_ReturnsFailureJson(string? code) => Assert.False(Deserialize<JR>(Assert.IsType<JsonResult>(await _controller.GetTotalPlannedCost(code!)))?.success);

        [Fact]
        public async Task GetTotalActualCost_WithValidProjectCode_ReturnsSuccessJson()
        {
            _projTestPlanActualService.GetTotalActualByProjectAsync("AH0033").Returns(ApiResponseDto<MonthlyOutputCalcsTotalsDto>.SuccessResponse(new MonthlyOutputCalcsTotalsDto { TotalVolume = 8, TotalCost = 900 }));
            Assert.True(Deserialize<JR>(Assert.IsType<JsonResult>(await _controller.GetTotalActualCost("AH0033")))?.success);
        }

        [Theory][InlineData(null)][InlineData("")]
        public async Task GetTotalActualCost_WhenProjectCodeMissing_ReturnsFailureJson(string? code) => Assert.False(Deserialize<JR>(Assert.IsType<JsonResult>(await _controller.GetTotalActualCost(code!)))?.success);

        [Fact]
        public async Task DeleteMonthlyOutputCalcs_WithValidRowKey_ReturnsSuccessJson()
        {
            _projTestPlanActualService.DeleteMonthlyOutputCalcsAsync("AH0033", "TC01", 1.0, "WG1").Returns(ApiResponseDto<bool>.SuccessResponse(true));
            Assert.True(Deserialize<JR>(Assert.IsType<JsonResult>(await _controller.DeleteMonthlyOutputCalcs("TC01|AH0033|1|WG1")))?.success);
        }

        [Theory][InlineData(null)][InlineData("")]
        public async Task DeleteMonthlyOutputCalcs_WhenRowKeyMissing_ReturnsFailureJson(string? rowKey) => Assert.False(Deserialize<JR>(Assert.IsType<JsonResult>(await _controller.DeleteMonthlyOutputCalcs(rowKey!)))?.success);

        [Fact]
        public async Task DeleteMonthlyOutputCalcs_WhenRowKeyHasWrongPartCount_ReturnsFailureJson() => Assert.False(Deserialize<JR>(Assert.IsType<JsonResult>(await _controller.DeleteMonthlyOutputCalcs("TC01|AH0033")))?.success);

        #region LoadTestPlanGrid

        [Fact]
        public async Task LoadTestPlanGrid_WithValidJobCode_ReturnsPartialViewWithGrid()
        {
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10 };
            var query   = new QueryParameters<string> { Page = 1, PageSize = 10 };
            _mapper.Map<QueryParameters<string>>(request).Returns(query);
            _testRequirementService.GetPagedTestReqmtbyProjectAsync(query, "AH0033")
                .Returns(ApiResponseDto<List<TestRequirementDto>>.SuccessResponse(new List<TestRequirementDto>()));
            _mapper.Map<List<TestPlanActualItem>>(Arg.Any<List<TestRequirementDto>>()).Returns(new List<TestPlanActualItem>());
            _mapper.Map<PaginationModel>(Arg.Any<object>()).Returns(new PaginationModel());

            var result = await _controller.LoadTestPlanGrid(request, "AH0033");

            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partial.ViewName);
        }

        [Fact]
        public async Task LoadTestPlanGrid_WithInvalidModelState_ReturnsFailureJson()
        {
            _controller.ModelState.AddModelError("Page", "Invalid");
            var request = new PaginationFilter<string> { Page = 0, PageSize = 10 };

            var result = Assert.IsType<JsonResult>(await _controller.LoadTestPlanGrid(request, "AH0033"));
            Assert.False(Deserialize<JR>(result)?.success);
        }

        [Fact]
        public async Task LoadTestPlanGrid_GridConfigHasNoEditAllowance()
        {
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10 };
            var query   = new QueryParameters<string> { Page = 1, PageSize = 10 };
            _mapper.Map<QueryParameters<string>>(request).Returns(query);
            _testRequirementService.GetPagedTestReqmtbyProjectAsync(query, "AH0033")
                .Returns(ApiResponseDto<List<TestRequirementDto>>.SuccessResponse(new List<TestRequirementDto>()));
            _mapper.Map<List<TestPlanActualItem>>(Arg.Any<List<TestRequirementDto>>()).Returns(new List<TestPlanActualItem>());
            _mapper.Map<PaginationModel>(Arg.Any<object>()).Returns(new PaginationModel());

            var partial = Assert.IsType<PartialViewResult>(await _controller.LoadTestPlanGrid(request, "AH0033"));
            var grid    = Assert.IsType<DataGridConfig<TestPlanActualItem>>(partial.Model);

            Assert.False(grid.AllowAdd);
            Assert.False(grid.AllowEdit);
            Assert.True(grid.AllowDelete);
        }

        #endregion

        #region LoadCompareTests2Grid

        [Fact]
        public async Task LoadCompareTests2Grid_WithValidProjectCode_ReturnsPartialViewWithGrid()
        {
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10 };
            var query   = new QueryParameters<string> { Page = 1, PageSize = 10 };
            _mapper.Map<QueryParameters<string>>(request).Returns(query);
            _projTestPlanActualService.GetMonthlyOutputCalcsByProjectAsync(query, "AH0033")
                .Returns(ApiResponseDto<List<MonthlyOutputCalcsViewDto>>.SuccessResponse(new List<MonthlyOutputCalcsViewDto>()));
            _mapper.Map<List<CompareTests2Item>>(Arg.Any<List<MonthlyOutputCalcsViewDto>>()).Returns(new List<CompareTests2Item>());
            _mapper.Map<PaginationModel>(Arg.Any<object>()).Returns(new PaginationModel());

            var result = await _controller.LoadCompareTests2Grid(request, "AH0033");

            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partial.ViewName);
        }

        [Fact]
        public async Task LoadCompareTests2Grid_WithInvalidModelState_ReturnsFailureJson()
        {
            _controller.ModelState.AddModelError("Page", "Invalid");
            var request = new PaginationFilter<string> { Page = 0, PageSize = 10 };

            var result = Assert.IsType<JsonResult>(await _controller.LoadCompareTests2Grid(request, "AH0033"));
            Assert.False(Deserialize<JR>(result)?.success);
        }

        [Fact]
        public async Task LoadCompareTests2Grid_GridConfigHasNoAddOrEdit()
        {
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10 };
            var query   = new QueryParameters<string> { Page = 1, PageSize = 10 };
            _mapper.Map<QueryParameters<string>>(request).Returns(query);
            _projTestPlanActualService.GetMonthlyOutputCalcsByProjectAsync(query, "AH0033")
                .Returns(ApiResponseDto<List<MonthlyOutputCalcsViewDto>>.SuccessResponse(new List<MonthlyOutputCalcsViewDto>()));
            _mapper.Map<List<CompareTests2Item>>(Arg.Any<List<MonthlyOutputCalcsViewDto>>()).Returns(new List<CompareTests2Item>());
            _mapper.Map<PaginationModel>(Arg.Any<object>()).Returns(new PaginationModel());

            var partial = Assert.IsType<PartialViewResult>(await _controller.LoadCompareTests2Grid(request, "AH0033"));
            var grid    = Assert.IsType<DataGridConfig<CompareTests2Item>>(partial.Model);

            Assert.False(grid.AllowAdd);
            Assert.False(grid.AllowEdit);
            Assert.True(grid.AllowDelete);
        }

        #endregion
    }
}