using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.FPS.Controllers;
using Apha.FPSApps.Web.Areas.FPS.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using System.Text.Json;
using Xunit;

namespace Apha.FPSApps.Web.UnitTests.Controllers.FPS.ProjectStaffPlanActualControllerTest
{
    public class ProjectStaffPlanActualControllerTests
    {
        private readonly IMapper _mapper;
        private readonly ITimeCostCalcsService _projPlanVsActualsStaffService;
        private readonly IProjectService _projectService;
        private readonly IStaffJobService _staffJobService;
        private readonly ProjectStaffPlanActualController _controller;

        public ProjectStaffPlanActualControllerTests()
        {
            _mapper = Substitute.For<IMapper>();
            _projPlanVsActualsStaffService = Substitute.For<ITimeCostCalcsService>();
            _projectService = Substitute.For<IProjectService>();
            _staffJobService = Substitute.For<IStaffJobService>();
            _controller = new ProjectStaffPlanActualController(
                _mapper,
                _projPlanVsActualsStaffService,
                _projectService,
                _staffJobService);
        }

        private static T? GetJsonResultValue<T>(JsonResult jsonResult)
        {
            var json = JsonSerializer.Serialize(jsonResult.Value);
            return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        private class JsonResponse
        {
            public bool success { get; set; }
            public string? message { get; set; }
            public object? data { get; set; }
            public object? errors { get; set; }
        }

        private void SetupProjectList(List<ProjectDto>? projects = null)
        {
            projects ??= new List<ProjectDto>
            {
                new() { ParentProject = "AH0033", ProjectTitle = "Test Project", Program = "P001", Contract = "C001" }
            };
            _projectService.GetAllProjectsAsync()
                .Returns(ApiResponseDto<List<ProjectDto>>.SuccessResponse(projects));
        }

        private void SetupProjectById(string projectCode, ProjectDto? dto = null)
        {
            dto ??= new ProjectDto { ParentProject = projectCode, ProjectTitle = "Test Project", Program = "P001", Contract = "C001" };
            _projectService.GetProjectByIdAsync(projectCode)
                .Returns(ApiResponseDto<ProjectDto>.SuccessResponse(dto));
        }

        private void SetupTotalStaffCost(string jobCode, decimal cost = 500m)
        {
            _staffJobService.GetTotalStaffCostAsync(jobCode)
                .Returns(ApiResponseDto<decimal>.SuccessResponse(cost));
        }

        #region Index — Happy path

        [Fact]
        public async Task Index_WithValidProjectCode_ReturnsViewWithPopulatedModel()
        {
            // Arrange
            var projectCode = "AH0033";
            SetupProjectList();
            SetupProjectById(projectCode);
            SetupTotalStaffCost(projectCode);

            // Act
            var result = await _controller.Index(projectCode);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectStaffPlanActualViewModel>(viewResult.Model);
            Assert.Equal(projectCode, model.SelectedProjectCode);
            Assert.Equal("Test Project", model.ProjectTitle);
            Assert.Equal("P001", model.Program);
            Assert.Equal("C001", model.Contract);
            Assert.Equal(0, model.TotalActualHrs);
            Assert.Equal(0, model.TotalActualCost);
            Assert.Equal(0, model.PercentOfPlan);
        }

        [Fact]
        public async Task Index_WithNoProjectCode_SelectsFirstProjectFromList()
        {
            // Arrange
            var projects = new List<ProjectDto>
            {
                new() { ParentProject = "AH0001", ProjectTitle = "First Project",  Program = "P001", Contract = "C001" },
                new() { ParentProject = "AH0002", ProjectTitle = "Second Project", Program = "P002", Contract = "C002" }
            };
            SetupProjectList(projects);
            SetupProjectById("AH0001", new ProjectDto { ParentProject = "AH0001", ProjectTitle = "First Project", Program = "P001", Contract = "C001" });
            SetupTotalStaffCost("AH0001");

            // Act
            var result = await _controller.Index(null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectStaffPlanActualViewModel>(
                Assert.IsType<ViewResult>(result).Model);
            Assert.Equal("AH0001", model.SelectedProjectCode);
        }

        [Fact]
        public async Task Index_WithUnrecognisedProjectCode_FallsBackToFirstInList()
        {
            // Arrange
            var projects = new List<ProjectDto>
            {
                new() { ParentProject = "AH0001", ProjectTitle = "First Project" }
            };
            SetupProjectList(projects);
            SetupProjectById("AH0001");
            SetupTotalStaffCost("AH0001");

            // Act
            var result = await _controller.Index("UNKNOWN");

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectStaffPlanActualViewModel>(viewResult.Model);
            Assert.Equal("AH0001", model.SelectedProjectCode);
        }

        [Fact]
        public async Task Index_WithEmptyProjectList_ReturnsViewWithEmptyModel()
        {
            // Arrange
            _projectService.GetAllProjectsAsync()
                .Returns(ApiResponseDto<List<ProjectDto>>.SuccessResponse(new List<ProjectDto>()));
            _staffJobService.GetTotalStaffCostAsync(string.Empty)
                .Returns(ApiResponseDto<decimal>.SuccessResponse(0m));

            // Act
            var result = await _controller.Index(null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProjectStaffPlanActualViewModel>(viewResult.Model);
            Assert.Equal(string.Empty, model.SelectedProjectCode);
            Assert.Empty(model.ProjectList);
        }

        [Fact]
        public async Task Index_AlwaysConfiguresBothGrids()
        {
            // Arrange
            SetupProjectList();
            SetupProjectById("AH0033");
            SetupTotalStaffCost("AH0033");

            // Act
            var result = await _controller.Index("AH0033");

            // Assert
            var model = Assert.IsType<ProjectStaffPlanActualViewModel>(
                Assert.IsType<ViewResult>(result).Model);
            Assert.NotNull(model.StaffPlanGrid);
            Assert.NotNull(model.CompareStaff2Grid);
            Assert.Equal("staffBookedGrid", model.StaffPlanGrid.GridId);
            Assert.Equal("compareStaff2Grid", model.CompareStaff2Grid.GridId);
        }

        [Fact]
        public async Task Index_CompareStaff2Grid_HasAllOperationsDisabled()
        {
            // Arrange
            SetupProjectList();
            SetupProjectById("AH0033");
            SetupTotalStaffCost("AH0033");

            // Act
            var result = await _controller.Index("AH0033");

            // Assert
            var model = Assert.IsType<ProjectStaffPlanActualViewModel>(
                Assert.IsType<ViewResult>(result).Model);
            Assert.False(model.CompareStaff2Grid.AllowAdd);
            Assert.False(model.CompareStaff2Grid.AllowEdit);
            Assert.True(model.CompareStaff2Grid.AllowDelete);
        }

        [Fact]
        public async Task Index_StaffPlanGrid_HasOperationsEnabled()
        {
            // Arrange
            SetupProjectList();
            SetupProjectById("AH0033");
            SetupTotalStaffCost("AH0033");

            // Act
            var result = await _controller.Index("AH0033");

            // Assert
            var model = Assert.IsType<ProjectStaffPlanActualViewModel>(
                Assert.IsType<ViewResult>(result).Model);
            Assert.True(model.StaffPlanGrid.AllowAdd);
            Assert.True(model.StaffPlanGrid.AllowEdit);
            Assert.True(model.StaffPlanGrid.AllowDelete);
        }

        #endregion

        #region LoadCompareStaff2Grid — Happy path

        [Fact]
        public async Task LoadCompareStaff2Grid_WithValidRequest_ReturnsPartialViewWithDataGrid()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10 };
            var projectCode = "AH0033";
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var items = new List<TimeCostCalcsViewDto>
            {
                new() { Project = projectCode, StaffId = "S01", Name = "Alice", WorkGroup = "WG1" }
            };
            var paginationDto = new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 1 };
            var serviceResponse = ApiResponseDto<List<TimeCostCalcsViewDto>>.SuccessResponse(items, paginationDto);
            var gridItems = new List<CompareStaff2Item>
            {
                new() { Name = "Alice", WorkGroup = "WG1" }
            };
            var paginationModel = new PaginationModel { PageNumber = 1, PageSize = 10, TotalRecords = 1 };

            _mapper.Map<QueryParameters<string>>(request).Returns(query);
            _projPlanVsActualsStaffService.GetTimeCostCalcsByProjectAsync(query, projectCode).Returns(serviceResponse);
            _mapper.Map<List<CompareStaff2Item>>(items).Returns(gridItems);
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>()).Returns(paginationModel);

            // Act
            var result = await _controller.LoadCompareStaff2Grid(request, projectCode);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partial.ViewName);
            var gridConfig = Assert.IsType<DataGridConfig<CompareStaff2Item>>(partial.Model);
            Assert.Equal("compareStaff2Grid", gridConfig.GridId);
            Assert.Equal("Actual Time (PACT)", gridConfig.Title);
            Assert.Single(gridConfig.Data);
            Assert.False(gridConfig.AllowAdd);
            Assert.False(gridConfig.AllowEdit);
            Assert.True(gridConfig.AllowDelete);
        }

        [Fact]
        public async Task LoadCompareStaff2Grid_WithNullProjectCode_UsesEmptyString()
        {
            // Arrange
            var request = new PaginationFilter<string> { Page = 1, PageSize = 10 };
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var serviceResponse = ApiResponseDto<List<TimeCostCalcsViewDto>>.SuccessResponse(
                new List<TimeCostCalcsViewDto>(), new PaginationDto());

            _mapper.Map<QueryParameters<string>>(request).Returns(query);
            _projPlanVsActualsStaffService.GetTimeCostCalcsByProjectAsync(query, string.Empty).Returns(serviceResponse);
            _mapper.Map<List<CompareStaff2Item>>(Arg.Any<List<TimeCostCalcsViewDto>>()).Returns(new List<CompareStaff2Item>());
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>()).Returns(new PaginationModel());

            // Act
            var result = await _controller.LoadCompareStaff2Grid(request, null);

            // Assert
            Assert.IsType<PartialViewResult>(result);
            await _projPlanVsActualsStaffService.Received(1)
                .GetTimeCostCalcsByProjectAsync(query, string.Empty);
        }

        [Fact]
        public async Task LoadCompareStaff2Grid_WhenModelStateIsInvalid_ReturnsFailureJson()
        {
            // Arrange
            _controller.ModelState.AddModelError("Page", "Page is required");
            var request = new PaginationFilter<string>();

            // Act
            var result = await _controller.LoadCompareStaff2Grid(request, "AH0033");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
            Assert.Equal("Invalid request data", value.message);
            await _projPlanVsActualsStaffService.DidNotReceive()
                .GetTimeCostCalcsByProjectAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<string>());
        }

        #endregion

        #region GetProjectInfo

        [Fact]
        public async Task GetProjectInfo_WithValidProjectCode_ReturnsSuccessJson()
        {
            // Arrange
            var projectCode = "AH0033";
            var projectDto = new ProjectDto { ParentProject = projectCode, ProjectTitle = "Test Project", Program = "P001", Contract = "C001" };
            _projectService.GetProjectByIdAsync(projectCode)
                .Returns(ApiResponseDto<ProjectDto>.SuccessResponse(projectDto));

            // Act
            var result = await _controller.GetProjectInfo(projectCode);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<GetProjectInfoResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.True(value.success);
            Assert.Equal("Test Project", value.projectTitle);
            Assert.Equal("P001", value.program);
            Assert.Equal("C001", value.contract);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task GetProjectInfo_WithNullOrWhitespaceProjectCode_ReturnsFailureJson(string? projectCode)
        {
            // Act
            var result = await _controller.GetProjectInfo(projectCode!);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.False(value.success);
            await _projectService.DidNotReceive().GetProjectByIdAsync(Arg.Any<string>());
        }

        [Fact]
        public async Task GetProjectInfo_WhenProjectNotFound_ReturnsFailureJson()
        {
            // Arrange
            var projectCode = "UNKNOWN";
            var errors = new List<ApiErrorDto> { new() { Code = "NOT_FOUND", Message = "Not found" } };
            _projectService.GetProjectByIdAsync(projectCode)
                .Returns(ApiResponseDto<ProjectDto>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _controller.GetProjectInfo(projectCode);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.False(value!.success);
        }

        #endregion

        #region GetTotalPlannedCost

        [Fact]
        public async Task GetTotalPlannedCost_WithValidProjectCode_ReturnsSuccessJson()
        {
            // Arrange
            var projectCode = "AH0033";
            var expectedCost = 1250.50m;
            _staffJobService.GetTotalStaffCostAsync(projectCode)
                .Returns(ApiResponseDto<decimal>.SuccessResponse(expectedCost));

            // Act
            var result = await _controller.GetTotalPlannedCost(projectCode);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<TotalPlannedCostResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.True(value.success);
            Assert.Equal((double)expectedCost, value.totalPlannedCost);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task GetTotalPlannedCost_WithNullOrWhitespaceProjectCode_ReturnsFailureJson(string? projectCode)
        {
            // Act
            var result = await _controller.GetTotalPlannedCost(projectCode!);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.False(value!.success);
            await _staffJobService.DidNotReceive().GetTotalStaffCostAsync(Arg.Any<string>());
        }

        [Fact]
        public async Task GetTotalPlannedCost_WhenServiceFails_ReturnsFailureJson()
        {
            // Arrange
            var projectCode = "AH0033";
            var errors = new List<ApiErrorDto> { new() { Code = "ERROR", Message = "Failed" } };
            _staffJobService.GetTotalStaffCostAsync(projectCode)
                .Returns(ApiResponseDto<decimal>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _controller.GetTotalPlannedCost(projectCode);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.False(value!.success);
        }

        #endregion

        // Helper deserialization classes
        private class GetProjectInfoResponse
        {
            public bool success { get; set; }
            public string? projectTitle { get; set; }
            public string? program { get; set; }
            public string? contract { get; set; }
        }

        private class TotalPlannedCostResponse
        {
            public bool success { get; set; }
            public double totalPlannedCost { get; set; }
        }

        private class TotalActualCostResponse
        {
            public bool success { get; set; }
            public double totalHours { get; set; }
            public double totalCost { get; set; }
        }

        private class SuccessResponse
        {
            public bool success { get; set; }
            public string? message { get; set; }
        }

        #region GetTotalActualCost

        [Fact]
        public async Task GetTotalActualCost_WithValidProjectCode_ReturnsSuccessJson()
        {
            // Arrange
            var projectCode = "AH0033";
            var totals = new TimeCostCalcsTotalsDto { TotalHours = 40.5, TotalCost = 5000.0 };
            _projPlanVsActualsStaffService.GetTotalActualByProjectAsync(projectCode)
                .Returns(ApiResponseDto<TimeCostCalcsTotalsDto>.SuccessResponse(totals));

            // Act
            var result = await _controller.GetTotalActualCost(projectCode);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<TotalActualCostResponse>(jsonResult);
            Assert.NotNull(value);
            Assert.True(value.success);
            Assert.Equal(40.5, value.totalHours);
            Assert.Equal(5000.0, value.totalCost);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task GetTotalActualCost_WithNullOrWhitespaceProjectCode_ReturnsFailureJson(string? projectCode)
        {
            // Act
            var result = await _controller.GetTotalActualCost(projectCode!);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.False(value!.success);
            await _projPlanVsActualsStaffService.DidNotReceive()
                .GetTotalActualByProjectAsync(Arg.Any<string>());
        }

        [Fact]
        public async Task GetTotalActualCost_WhenServiceFails_ReturnsFailureJson()
        {
            // Arrange
            var projectCode = "AH0033";
            var errors = new List<ApiErrorDto> { new() { Code = "ERROR", Message = "Failed" } };
            _projPlanVsActualsStaffService.GetTotalActualByProjectAsync(projectCode)
                .Returns(ApiResponseDto<TimeCostCalcsTotalsDto>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _controller.GetTotalActualCost(projectCode);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.False(value!.success);
        }

        [Fact]
        public async Task GetTotalActualCost_WhenServiceSucceedsWithNullData_ReturnsFailureJson()
        {
            // Arrange
            var projectCode = "AH0033";
            _projPlanVsActualsStaffService.GetTotalActualByProjectAsync(projectCode)
                .Returns(ApiResponseDto<TimeCostCalcsTotalsDto>.SuccessResponse(null!));

            // Act
            var result = await _controller.GetTotalActualCost(projectCode);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.False(value!.success);
        }

        #endregion

        #region DeleteTimeCostCalcs

        [Fact]
        public async Task DeleteTimeCostCalcs_WithValidRowKey_ReturnsSuccessJson()
        {
            // Arrange
            var rowKey = "WG1|JOB1|AH0033|1|S01";
            _projPlanVsActualsStaffService
                .DeleteTimeCostCalcsAsync("WG1", "JOB1", "AH0033", 1, "S01")
                .Returns(ApiResponseDto<bool>.SuccessResponse(true));

            // Act
            var result = await _controller.DeleteTimeCostCalcs(rowKey);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<SuccessResponse>(jsonResult);
            Assert.True(value!.success);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task DeleteTimeCostCalcs_WithNullOrWhitespaceRowKey_ReturnsFailureJson(string? rowKey)
        {
            // Act
            var result = await _controller.DeleteTimeCostCalcs(rowKey!);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.False(value!.success);
            await _projPlanVsActualsStaffService.DidNotReceive()
                .DeleteTimeCostCalcsAsync(Arg.Any<string>(), Arg.Any<string>(),
                    Arg.Any<string>(), Arg.Any<double>(), Arg.Any<string>());
        }

        [Theory]
        [InlineData("WG1|JOB1|AH0033|1")]
        [InlineData("WG1|JOB1|AH0033|1|S01|EXTRA")]
        [InlineData("invalid")]
        public async Task DeleteTimeCostCalcs_WithInvalidRowKeyFormat_ReturnsFailureJson(string rowKey)
        {
            // Act
            var result = await _controller.DeleteTimeCostCalcs(rowKey);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.False(value!.success);
            Assert.Equal("Invalid row key format.", value.message);
        }

        [Fact]
        public async Task DeleteTimeCostCalcs_WhenServiceFails_ReturnsFailureJson()
        {
            // Arrange
            var rowKey = "WG1|JOB1|AH0033|1|S01";
            var errors = new List<ApiErrorDto> { new() { Code = "NOT_FOUND", Message = "Not found" } };
            _projPlanVsActualsStaffService
                .DeleteTimeCostCalcsAsync("WG1", "JOB1", "AH0033", 1, "S01")
                .Returns(ApiResponseDto<bool>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _controller.DeleteTimeCostCalcs(rowKey);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<JsonResponse>(jsonResult);
            Assert.False(value!.success);
        }

        [Fact]
        public async Task DeleteTimeCostCalcs_ParsesMonthCorrectly()
        {
            // Arrange
            var rowKey = "WG1|JOB1|AH0033|3.5|S01";
            _projPlanVsActualsStaffService
                .DeleteTimeCostCalcsAsync("WG1", "JOB1", "AH0033", 3.5, "S01")
                .Returns(ApiResponseDto<bool>.SuccessResponse(true));

            // Act
            var result = await _controller.DeleteTimeCostCalcs(rowKey);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultValue<SuccessResponse>(jsonResult);
            Assert.True(value!.success);
            await _projPlanVsActualsStaffService.Received(1)
                .DeleteTimeCostCalcsAsync("WG1", "JOB1", "AH0033", 3.5, "S01");
        }

        [Fact]
        public async Task DeleteTimeCostCalcs_WithNonNumericMonth_UsesZero()
        {
            // Arrange
            var rowKey = "WG1|JOB1|AH0033|ABC|S01";
            _projPlanVsActualsStaffService
                .DeleteTimeCostCalcsAsync("WG1", "JOB1", "AH0033", 0, "S01")
                .Returns(ApiResponseDto<bool>.SuccessResponse(true));

            // Act
            var result = await _controller.DeleteTimeCostCalcs(rowKey);

            // Assert
            await _projPlanVsActualsStaffService.Received(1)
                .DeleteTimeCostCalcsAsync("WG1", "JOB1", "AH0033", 0, "S01");
        }

        #endregion
    }
}

