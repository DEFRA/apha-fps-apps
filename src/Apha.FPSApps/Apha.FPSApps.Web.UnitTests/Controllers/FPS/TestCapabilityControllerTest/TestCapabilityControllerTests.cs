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

namespace Apha.FPSApps.Web.UnitTests.Controllers.FPS.TestCapabilityControllerTest
{
    public class TestCapabilityControllerTests
    {
        private const string DefaultTestCode = "PT0001";
        private const string DefaultWorkGroup = "BM1";
        private const string DefaultPortfolio = "ABOG0508";

        private readonly IMapper _mapper;
        private readonly ITestCapabilityService _testCapabilityService;
        private readonly IProjectService _projectService;
        private readonly ITestorProductService _testorProductService;
        private readonly TestCapabilityController _controller;

        public TestCapabilityControllerTests()
        {
            _mapper = Substitute.For<IMapper>();
            _testCapabilityService = Substitute.For<ITestCapabilityService>();
            _projectService = Substitute.For<IProjectService>();
            _testorProductService = Substitute.For<ITestorProductService>();
            _controller = new TestCapabilityController(
                _mapper,
                _testCapabilityService,
                _projectService,
                _testorProductService);
        }

        private static JsonElement GetJsonResultElement(JsonResult jsonResult)
        {
            var json = JsonSerializer.Serialize(jsonResult.Value);
            return JsonSerializer.Deserialize<JsonElement>(json);
        }

        private void SetupPortfolioOptions() =>
            _projectService.GetAllProjectsAsync()
                .Returns(ApiResponseDto<List<ProjectDto>>.SuccessResponse(
                [
                    new ProjectDto { ParentProject = DefaultPortfolio, ProjectTitle = "Antibody Testing", ProjectGroup = "Prod_Port" }
                ]));

        private void SetupWorkGroupOptions() =>
            _testCapabilityService.GetAllWorkGroupsAsync()
                .Returns(ApiResponseDto<List<WorkGroupDto>>.SuccessResponse(
                [
                    new WorkGroupDto { WorkGroupName = DefaultWorkGroup }
                ]));

        private void SetupTestorProductOptions() =>
            _testorProductService.GetAllTestorProductsAsync()
                .Returns(ApiResponseDto<List<TestorProductDto>>.SuccessResponse(
                [
                    new TestorProductDto { ItemCode = DefaultTestCode, ItemDescription = "Some Test" }
                ]));

        private void SetupGridMapper()
        {
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _mapper.Map<List<TestCapabilityItem>>(Arg.Any<List<TestCapabilityDto>>())
                .Returns([]);
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>())
                .Returns(new PaginationModel());
        }

        // ── INDEX ──────────────────────────────────────────────────────────────

        #region Index

        [Fact]
        public async Task Index_Always_ReturnsViewResultWithViewModel()
        {
            SetupPortfolioOptions();
            SetupWorkGroupOptions();

            var result = await _controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<TestCapabilityViewModel>(viewResult.Model);
            Assert.Equal("testCapabilityGrid", model.TestCapabilityGrid.GridId);
        }

        [Fact]
        public async Task Index_ProjectsLoaded_PopulatesPortfolioOptions()
        {
            SetupPortfolioOptions();
            SetupWorkGroupOptions();

            var result = await _controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<TestCapabilityViewModel>(viewResult.Model);
            Assert.Single(model.PortfolioOptions);
            Assert.Equal(DefaultPortfolio, model.PortfolioOptions.First().Value);
        }

        [Fact]
        public async Task Index_WorkGroupsLoaded_PopulatesWorkGroupOptions()
        {
            SetupPortfolioOptions();
            SetupWorkGroupOptions();

            var result = await _controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<TestCapabilityViewModel>(viewResult.Model);
            Assert.Single(model.WorkGroupOptions);
            Assert.Equal(DefaultWorkGroup, model.WorkGroupOptions.First().Value);
        }

        [Fact]
        public async Task Index_ProjectServiceFails_ReturnsViewWithEmptyPortfolioOptions()
        {
            var errors = new List<ApiErrorDto> { new() { Code = "ERR" } };
            _projectService.GetAllProjectsAsync()
                .Returns(ApiResponseDto<List<ProjectDto>>.FailureResponse(errors, new ApiMetaDto()));
            SetupWorkGroupOptions();

            var result = await _controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<TestCapabilityViewModel>(viewResult.Model);
            Assert.Empty(model.PortfolioOptions);
        }

        [Fact]
        public async Task Index_WorkGroupServiceFails_ReturnsViewWithEmptyWorkGroupOptions()
        {
            SetupPortfolioOptions();
            var errors = new List<ApiErrorDto> { new() { Code = "ERR" } };
            _testCapabilityService.GetAllWorkGroupsAsync()
                .Returns(ApiResponseDto<List<WorkGroupDto>>.FailureResponse(errors, new ApiMetaDto()));

            var result = await _controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<TestCapabilityViewModel>(viewResult.Model);
            Assert.Empty(model.WorkGroupOptions);
        }

        [Fact]
        public async Task Index_PortfolioTitlePresent_BuildsOptionWithDashSeparator()
        {
            _projectService.GetAllProjectsAsync()
                .Returns(ApiResponseDto<List<ProjectDto>>.SuccessResponse(
                [
                    new ProjectDto { ParentProject = DefaultPortfolio, ProjectTitle = "Antibody Testing", ProjectGroup = "Prod_Port" }
                ]));
            SetupWorkGroupOptions();

            var result = await _controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<TestCapabilityViewModel>(viewResult.Model);
            Assert.Contains("\u2013", model.PortfolioOptions.First().Text);
        }

        [Fact]
        public async Task Index_OnlyIncludesProjectsWhoseProjectGroupEndsWithPort()
        {
            _projectService.GetAllProjectsAsync()
                .Returns(ApiResponseDto<List<ProjectDto>>.SuccessResponse(
                [
                    new ProjectDto { ParentProject = "FESPROD", ProjectTitle = "Food Safety", ProjectGroup = "Prod_Port" },
                    new ProjectDto { ParentProject = "BPUPORT", ProjectTitle = "BPU products", ProjectGroup = "Port" },
                    new ProjectDto { ParentProject = "NOTPORT1", ProjectTitle = "Non portfolio", ProjectGroup = "Bact" },
                    new ProjectDto { ParentProject = "NOGROUP1", ProjectTitle = "Unassigned", ProjectGroup = null },
                    new ProjectDto { ParentProject = "BLANKGRP", ProjectTitle = "Blank group", ProjectGroup = "  " }
                ]));
            SetupWorkGroupOptions();

            var result = await _controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<TestCapabilityViewModel>(viewResult.Model);
            Assert.Equal(2, model.PortfolioOptions.Count);
            Assert.Equal(new[] { "BPUPORT", "FESPROD" }, model.PortfolioOptions.Select(o => o.Value).ToArray());
        }

        [Fact]
        public async Task Index_PortfolioProjectGroupMatchIsCaseInsensitive()
        {
            _projectService.GetAllProjectsAsync()
                .Returns(ApiResponseDto<List<ProjectDto>>.SuccessResponse(
                [
                    new ProjectDto { ParentProject = "UPPER", ProjectTitle = "Upper", ProjectGroup = "PROD_PORT" },
                    new ProjectDto { ParentProject = "LOWER", ProjectTitle = "Lower", ProjectGroup = "prod_port" }
                ]));
            SetupWorkGroupOptions();

            var result = await _controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<TestCapabilityViewModel>(viewResult.Model);
            Assert.Equal(2, model.PortfolioOptions.Count);
        }

        [Fact]
        public async Task Index_NoProjectIsAPortfolio_ReturnsEmptyPortfolioOptions()
        {
            _projectService.GetAllProjectsAsync()
                .Returns(ApiResponseDto<List<ProjectDto>>.SuccessResponse(
                [
                    new ProjectDto { ParentProject = "PP001", ProjectTitle = "One", ProjectGroup = "Bact" },
                    new ProjectDto { ParentProject = "PP002", ProjectTitle = "Two", ProjectGroup = null }
                ]));
            SetupWorkGroupOptions();

            var result = await _controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<TestCapabilityViewModel>(viewResult.Model);
            Assert.Empty(model.PortfolioOptions);
        }

        #endregion

        // ── GRID ───────────────────────────────────────────────────────────────

        #region LoadTestCapabilityGrid

        [Fact]
        public async Task LoadTestCapabilityGrid_ValidRequest_ReturnsPartialViewWithGrid()
        {
            var request = new PaginationFilter<string> { Filter = "{}" };
            _testCapabilityService.GetPagedTestCapabilityByPortfolioAsync(
                    Arg.Any<QueryParameters<string>>(), DefaultPortfolio)
                .Returns(ApiResponseDto<List<TestCapabilityDto>>.SuccessResponse([]));
            SetupGridMapper();

            var result = await _controller.LoadTestCapabilityGrid(request, DefaultPortfolio);

            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partial.ViewName);
            Assert.IsType<DataGridConfig<TestCapabilityItem>>(partial.Model);
        }

        [Fact]
        public async Task LoadTestCapabilityGrid_NullPortfolio_ReturnsPartialView()
        {
            var request = new PaginationFilter<string> { Filter = "{}" };
            _testCapabilityService.GetPagedTestCapabilityByPortfolioAsync(
                    Arg.Any<QueryParameters<string>>(), null)
                .Returns(ApiResponseDto<List<TestCapabilityDto>>.SuccessResponse([]));
            SetupGridMapper();

            var result = await _controller.LoadTestCapabilityGrid(request, null);

            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partial.ViewName);
        }

        [Fact]
        public async Task LoadTestCapabilityGrid_InvalidModelState_ReturnsJsonFailure()
        {
            _controller.ModelState.AddModelError("Test", "Test error");

            var result = await _controller.LoadTestCapabilityGrid(new PaginationFilter<string> { Filter = "{}" }, null);

            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task LoadTestCapabilityGrid_ServiceFails_ReturnsPartialViewWithEmptyData()
        {
            var request = new PaginationFilter<string> { Filter = "{}" };
            var errors = new List<ApiErrorDto> { new() { Code = "ERR" } };
            _testCapabilityService.GetPagedTestCapabilityByPortfolioAsync(
                    Arg.Any<QueryParameters<string>>(), null)
                .Returns(ApiResponseDto<List<TestCapabilityDto>>.FailureResponse(errors, new ApiMetaDto()));
            SetupGridMapper();

            var result = await _controller.LoadTestCapabilityGrid(request, null);

            var partial = Assert.IsType<PartialViewResult>(result);
            var grid = Assert.IsType<DataGridConfig<TestCapabilityItem>>(partial.Model);
            Assert.Empty(grid.Data);
        }

        #endregion

        // ── CREATE ─────────────────────────────────────────────────────────────

        #region CreateTestCapability (GET)

        [Fact]
        public async Task CreateTestCapability_Get_WithPortfolio_ReturnsPartialViewWithPortfolioPreFilled()
        {
            SetupWorkGroupOptions();
            SetupTestorProductOptions();

            var result = await _controller.CreateTestCapability(DefaultPortfolio);

            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_AddEditTestCapability", partial.ViewName);
            var model = Assert.IsType<TestCapabilityItem>(partial.Model);
            Assert.Equal(DefaultPortfolio, model.PlanPortfolio);
        }

        [Fact]
        public async Task CreateTestCapability_Get_NullPortfolio_ReturnsPartialViewWithEmptyPortfolio()
        {
            SetupWorkGroupOptions();
            SetupTestorProductOptions();

            var result = await _controller.CreateTestCapability(portfolio: null);

            var partial = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<TestCapabilityItem>(partial.Model);
            Assert.Equal(string.Empty, model.PlanPortfolio);
        }

        #endregion

        #region CreateTestCapability (POST)

        [Fact]
        public async Task CreateTestCapability_Post_ValidModel_ReturnsJsonSuccess()
        {
            var model = new TestCapabilityItem { TestCode = DefaultTestCode, WorkGroup = DefaultWorkGroup, PlanPortfolio = DefaultPortfolio };
            var dto = new TestCapabilityDto { TestCode = DefaultTestCode };
            _mapper.Map<TestCapabilityDto>(model).Returns(dto);
            _testCapabilityService.CreateTestCapabilityAsync(dto)
                .Returns(ApiResponseDto<TestCapabilityDto>.SuccessResponse(dto));

            var result = await _controller.CreateTestCapability(model);

            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.True(value.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task CreateTestCapability_Post_ServiceFails_ReturnsJsonFailure()
        {
            var model = new TestCapabilityItem { TestCode = DefaultTestCode, WorkGroup = DefaultWorkGroup, PlanPortfolio = DefaultPortfolio };
            var dto = new TestCapabilityDto { TestCode = DefaultTestCode };
            var errors = new List<ApiErrorDto> { new() { Code = "BUSINESS_RULE_VIOLATION", Message = "Already exists" } };
            _mapper.Map<TestCapabilityDto>(model).Returns(dto);
            _testCapabilityService.CreateTestCapabilityAsync(dto)
                .Returns(ApiResponseDto<TestCapabilityDto>.FailureResponse(errors, new ApiMetaDto()));

            var result = await _controller.CreateTestCapability(model);

            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
            Assert.Contains("Already exists", value.GetProperty("message").GetString());
        }

        [Fact]
        public async Task CreateTestCapability_Post_InvalidModelState_ReturnsJsonFailure()
        {
            _controller.ModelState.AddModelError("TestCode", "Test Code is required.");

            var result = await _controller.CreateTestCapability(new TestCapabilityItem());

            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
        }

        #endregion

        // ── EDIT ───────────────────────────────────────────────────────────────

        #region EditTestCapability (GET)

        [Fact]
        public async Task EditTestCapability_Get_RecordFound_ReturnsPartialViewWithModel()
        {
            var dto = new TestCapabilityDto { TestCode = DefaultTestCode, WorkGroup = DefaultWorkGroup };
            var item = new TestCapabilityItem { TestCode = DefaultTestCode, WorkGroup = DefaultWorkGroup, PlanPortfolio = DefaultPortfolio };
            _testCapabilityService.GetTestCapabilityByIdAsync(DefaultTestCode, DefaultWorkGroup)
                .Returns(ApiResponseDto<TestCapabilityDto>.SuccessResponse(dto));
            _mapper.Map<TestCapabilityItem>(dto).Returns(item);
            SetupWorkGroupOptions();
            SetupTestorProductOptions();

            var result = await _controller.EditTestCapability(DefaultTestCode, DefaultWorkGroup);

            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_AddEditTestCapability", partial.ViewName);
            var model = Assert.IsType<TestCapabilityItem>(partial.Model);
            Assert.Equal(DefaultTestCode, model.TestCode);
        }

        [Fact]
        public async Task EditTestCapability_Get_RecordNotFound_ReturnsNotFound()
        {
            var errors = new List<ApiErrorDto> { new() { Code = "NOT_FOUND", Message = "Not found" } };
            _testCapabilityService.GetTestCapabilityByIdAsync("MISSING", DefaultWorkGroup)
                .Returns(ApiResponseDto<TestCapabilityDto>.FailureResponse(errors, new ApiMetaDto()));

            var result = await _controller.EditTestCapability("MISSING", DefaultWorkGroup);

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task EditTestCapability_Get_TestorProductMatchFound_SetsItemDescription()
        {
            var dto = new TestCapabilityDto { TestCode = DefaultTestCode, WorkGroup = DefaultWorkGroup };
            var item = new TestCapabilityItem { TestCode = DefaultTestCode, WorkGroup = DefaultWorkGroup, PlanPortfolio = DefaultPortfolio };
            _testCapabilityService.GetTestCapabilityByIdAsync(DefaultTestCode, DefaultWorkGroup)
                .Returns(ApiResponseDto<TestCapabilityDto>.SuccessResponse(dto));
            _mapper.Map<TestCapabilityItem>(dto).Returns(item);
            SetupWorkGroupOptions();
            _testorProductService.GetAllTestorProductsAsync()
                .Returns(ApiResponseDto<List<TestorProductDto>>.SuccessResponse(
                [
                    new TestorProductDto { ItemCode = DefaultTestCode, ItemDescription = "Antibody Test" }
                ]));

            var result = await _controller.EditTestCapability(DefaultTestCode, DefaultWorkGroup);

            var partial = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<TestCapabilityItem>(partial.Model);
            Assert.Equal("Antibody Test", model.ItemDescription);
        }

        #endregion

        #region EditTestCapability (POST)

        [Fact]
        public async Task EditTestCapability_Post_ValidModel_ReturnsJsonSuccess()
        {
            var model = new TestCapabilityItem { TestCode = DefaultTestCode, WorkGroup = DefaultWorkGroup, PlanPortfolio = DefaultPortfolio };
            var dto = new TestCapabilityDto { TestCode = DefaultTestCode };
            _mapper.Map<TestCapabilityDto>(model).Returns(dto);
            _testCapabilityService.UpdateTestCapabilityAsync(dto)
                .Returns(ApiResponseDto<TestCapabilityDto>.SuccessResponse(dto));

            var result = await _controller.EditTestCapability(model);

            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.True(value.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task EditTestCapability_Post_ServiceFails_ReturnsJsonFailure()
        {
            var model = new TestCapabilityItem { TestCode = DefaultTestCode, WorkGroup = DefaultWorkGroup, PlanPortfolio = DefaultPortfolio };
            var dto = new TestCapabilityDto { TestCode = DefaultTestCode };
            var errors = new List<ApiErrorDto> { new() { Code = "ERR", Message = "Update failed" } };
            _mapper.Map<TestCapabilityDto>(model).Returns(dto);
            _testCapabilityService.UpdateTestCapabilityAsync(dto)
                .Returns(ApiResponseDto<TestCapabilityDto>.FailureResponse(errors, new ApiMetaDto()));

            var result = await _controller.EditTestCapability(model);

            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task EditTestCapability_Post_InvalidModelState_ReturnsJsonFailure()
        {
            _controller.ModelState.AddModelError("WorkGroup", "Work Group is required.");

            var result = await _controller.EditTestCapability(new TestCapabilityItem());

            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
        }

        #endregion

        // ── DELETE ─────────────────────────────────────────────────────────────

        #region DeleteTestCapability

        [Fact]
        public async Task DeleteTestCapability_Success_ReturnsJsonSuccess()
        {
            _testCapabilityService.DeleteTestCapabilityAsync(DefaultTestCode, DefaultWorkGroup)
                .Returns(ApiResponseDto<bool>.SuccessResponse(true));

            var result = await _controller.DeleteTestCapability(DefaultTestCode, DefaultWorkGroup);

            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.True(value.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task DeleteTestCapability_ServiceFails_ReturnsJsonFailure()
        {
            var errors = new List<ApiErrorDto> { new() { Code = "NOT_FOUND", Message = "Not found" } };
            _testCapabilityService.DeleteTestCapabilityAsync("MISSING", DefaultWorkGroup)
                .Returns(ApiResponseDto<bool>.FailureResponse(errors, new ApiMetaDto()));

            var result = await _controller.DeleteTestCapability("MISSING", DefaultWorkGroup);

            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task DeleteTestCapability_ServiceFailsWithNullErrors_ReturnsJsonFailureWithDefaultMessage()
        {
            var response = ApiResponseDto<bool>.SuccessResponse(false);
            response.Success = false;
            response.Errors = null;
            _testCapabilityService.DeleteTestCapabilityAsync(DefaultTestCode, DefaultWorkGroup)
                .Returns(response);

            var result = await _controller.DeleteTestCapability(DefaultTestCode, DefaultWorkGroup);

            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
        }

        #endregion
    }
}
