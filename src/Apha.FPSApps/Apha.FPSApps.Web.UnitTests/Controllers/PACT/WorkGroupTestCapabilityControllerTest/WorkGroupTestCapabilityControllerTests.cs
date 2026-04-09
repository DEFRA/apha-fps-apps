using Apha.Common.Utilities.ExcelExport;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.PACT;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.PACT.Controllers;
using Apha.FPSApps.Web.Areas.PACT.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using System.Text.Json;

namespace Apha.FPSApps.Web.UnitTests.Controllers.PACT.WorkGroupTestCapabilityControllerTest
{
    public class WorkGroupTestCapabilityControllerTests
    {
        private readonly IMapper _mapper;
        private readonly IWorkGroupTestCapabilityService _service;
        private readonly IProjectService _projectService;
        private readonly IExcelExportService _excelExportService;
        private readonly WorkGroupTestCapabilityController _controller;

        public WorkGroupTestCapabilityControllerTests()
        {
            _mapper = Substitute.For<IMapper>();
            _service = Substitute.For<IWorkGroupTestCapabilityService>();
            _projectService = Substitute.For<IProjectService>();
            _excelExportService = Substitute.For<IExcelExportService>();
            _controller = new WorkGroupTestCapabilityController(
                _mapper,
                _service,
                _projectService,
                _excelExportService);
        }

        private static JsonElement GetJsonResultElement(JsonResult jsonResult)
        {
            var json = JsonSerializer.Serialize(jsonResult.Value);
            return JsonSerializer.Deserialize<JsonElement>(json);
        }

        private void SetupTestCapabilityGridMapper()
        {
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _mapper.Map<List<WorkGroupTestCapabilityItem>>(Arg.Any<List<TestCapabilityDto>>())
                .Returns([]);
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>())
                .Returns(new PaginationModel());
        }

        private void SetupTestReqmtGridMapper()
        {
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _mapper.Map<List<TestRequirementItem>>(Arg.Any<List<TestRequirementDto>>())
                .Returns([]);
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>())
                .Returns(new PaginationModel());
        }

        private void SetupDropdowns()
        {
            _service.GetAllWorkGroupsAsync()
                .Returns(ApiResponseDto<List<WorkGroupDto>>.SuccessResponse([]));
            _service.GetAllTestorProductsAsync()
                .Returns(ApiResponseDto<List<TestorProductDto>>.SuccessResponse([]));
            _projectService.GetAllProjectsAsync()
                .Returns(ApiResponseDto<List<ProjectDto>>.SuccessResponse([]));
        }

        #region Index

        [Fact]
        public async Task Index_Always_ReturnsViewResultWithViewModel()
        {
            // Arrange
            _service.GetPagedByWorkGroupAsync(Arg.Any<QueryParameters<string>>(), null)
                .Returns(ApiResponseDto<List<TestCapabilityDto>>.SuccessResponse([]));
            _service.GetAllWorkGroupsAsync()
                .Returns(ApiResponseDto<List<WorkGroupDto>>.SuccessResponse([new WorkGroupDto { WorkGroupName = "WG1" }]));
            _service.GetAllTestorProductsAsync()
                .Returns(ApiResponseDto<List<TestorProductDto>>.SuccessResponse([new TestorProductDto { ItemCode = "BLOOD" }]));
            SetupTestCapabilityGridMapper();

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<WorkGroupTestCapabilityViewModel>(viewResult.Model);
            Assert.Equal("testCapabilityGrid", model.TestCapabilityGrid.GridId);
            Assert.Equal("testReqmtGrid", model.TestReqmtGrid.GridId);
        }

        [Fact]
        public async Task Index_WorkGroupsAndTestorProductsLoaded_PopulatesDropdownOptions()
        {
            // Arrange
            _service.GetPagedByWorkGroupAsync(Arg.Any<QueryParameters<string>>(), null)
                .Returns(ApiResponseDto<List<TestCapabilityDto>>.SuccessResponse([]));
            _service.GetAllWorkGroupsAsync()
                .Returns(ApiResponseDto<List<WorkGroupDto>>.SuccessResponse(
                [
                    new WorkGroupDto { WorkGroupName = "WG1" },
                    new WorkGroupDto { WorkGroupName = "WG2" }
                ]));
            _service.GetAllTestorProductsAsync()
                .Returns(ApiResponseDto<List<TestorProductDto>>.SuccessResponse(
                [
                    new TestorProductDto { ItemCode = "BLOOD" },
                    new TestorProductDto { ItemCode = "URINE" }
                ]));
            SetupTestCapabilityGridMapper();

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<WorkGroupTestCapabilityViewModel>(viewResult.Model);
            Assert.Equal(2, model.WorkGroupOptions.Count);
            Assert.Equal(2, model.TestorProductOptions.Count);
        }

        [Fact]
        public async Task Index_ServicesFail_ReturnsViewWithEmptyDropdowns()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Code = "ERR" } };
            _service.GetPagedByWorkGroupAsync(Arg.Any<QueryParameters<string>>(), null)
                .Returns(ApiResponseDto<List<TestCapabilityDto>>.SuccessResponse([]));
            _service.GetAllWorkGroupsAsync()
                .Returns(ApiResponseDto<List<WorkGroupDto>>.FailureResponse(errors, new ApiMetaDto()));
            _service.GetAllTestorProductsAsync()
                .Returns(ApiResponseDto<List<TestorProductDto>>.FailureResponse(errors, new ApiMetaDto()));
            SetupTestCapabilityGridMapper();

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<WorkGroupTestCapabilityViewModel>(viewResult.Model);
            Assert.Empty(model.WorkGroupOptions);
            Assert.Empty(model.TestorProductOptions);
        }

        #endregion

        #region LoadTestCapabilityGrid

        [Fact]
        public async Task LoadTestCapabilityGrid_ViewBy1_ReturnsPartialViewWithWorkGroupGrid()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}" };
            _service.GetPagedByWorkGroupAsync(Arg.Any<QueryParameters<string>>(), "WG1")
                .Returns(ApiResponseDto<List<TestCapabilityDto>>.SuccessResponse([]));
            SetupTestCapabilityGridMapper();

            // Act
            var result = await _controller.LoadTestCapabilityGrid(request, viewBy: 1, filterValue: "WG1");

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partial.ViewName);
            Assert.IsType<DataGridConfig<WorkGroupTestCapabilityItem>>(partial.Model);
        }

        [Fact]
        public async Task LoadTestCapabilityGrid_ViewBy2_ReturnsPartialViewWithTestCodeGrid()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}" };
            _service.GetPagedByTestCodeAsync(Arg.Any<QueryParameters<string>>(), "TC1")
                .Returns(ApiResponseDto<List<TestCapabilityDto>>.SuccessResponse([]));
            SetupTestCapabilityGridMapper();

            // Act
            var result = await _controller.LoadTestCapabilityGrid(request, viewBy: 2, filterValue: "TC1");

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partial.ViewName);
            Assert.IsType<DataGridConfig<WorkGroupTestCapabilityItem>>(partial.Model);
        }

        [Fact]
        public async Task LoadTestCapabilityGrid_InvalidModelState_ReturnsJsonFailure()
        {
            // Arrange
            _controller.ModelState.AddModelError("Test", "Test error");

            // Act
            var result = await _controller.LoadTestCapabilityGrid(new PaginationFilter<string> { Filter = "{}" }, viewBy: 1, filterValue: null);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task LoadTestCapabilityGrid_ServiceFails_ReturnsPartialViewWithEmptyData()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}" };
            var errors = new List<ApiErrorDto> { new() { Code = "ERR" } };
            _service.GetPagedByWorkGroupAsync(Arg.Any<QueryParameters<string>>(), null)
                .Returns(ApiResponseDto<List<TestCapabilityDto>>.FailureResponse(errors, new ApiMetaDto()));
            SetupTestCapabilityGridMapper();

            // Act
            var result = await _controller.LoadTestCapabilityGrid(request, viewBy: 1, filterValue: null);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var grid = Assert.IsType<DataGridConfig<WorkGroupTestCapabilityItem>>(partial.Model);
            Assert.Empty(grid.Data);
        }

        #endregion

        #region LoadTestReqmtGrid

        [Fact]
        public async Task LoadTestReqmtGrid_ValidRequest_ReturnsPartialViewWithTestReqmtGrid()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}" };
            _service.GetPagedTestReqmtAsync(Arg.Any<QueryParameters<string>>(), "BLOOD")
                .Returns(ApiResponseDto<List<TestRequirementDto>>.SuccessResponse([]));
            SetupTestReqmtGridMapper();

            // Act
            var result = await _controller.LoadTestReqmtGrid(request, "BLOOD");

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partial.ViewName);
            Assert.IsType<DataGridConfig<TestRequirementItem>>(partial.Model);
        }

        [Fact]
        public async Task LoadTestReqmtGrid_InvalidModelState_ReturnsJsonFailure()
        {
            // Arrange
            _controller.ModelState.AddModelError("Test", "Test error");

            // Act
            var result = await _controller.LoadTestReqmtGrid(new PaginationFilter<string> { Filter = "{}" }, "BLOOD");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task LoadTestReqmtGrid_ServiceFails_ReturnsPartialViewWithEmptyData()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}" };
            var errors = new List<ApiErrorDto> { new() { Code = "ERR" } };
            _service.GetPagedTestReqmtAsync(Arg.Any<QueryParameters<string>>(), "BLOOD")
                .Returns(ApiResponseDto<List<TestRequirementDto>>.FailureResponse(errors, new ApiMetaDto()));
            SetupTestReqmtGridMapper();

            // Act
            var result = await _controller.LoadTestReqmtGrid(request, "BLOOD");

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var grid = Assert.IsType<DataGridConfig<TestRequirementItem>>(partial.Model);
            Assert.Empty(grid.Data);
        }

        #endregion

        #region CreateTestCapability (GET)

        [Fact]
        public async Task CreateTestCapability_Get_ReturnsPartialViewWithEmptyModel()
        {
            // Arrange
            SetupDropdowns();

            // Act
            var result = await _controller.CreateTestCapability();

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_AddEditTestCapability", partial.ViewName);
            Assert.IsType<WorkGroupTestCapabilityItem>(partial.Model);
        }

        #endregion

        #region CreateTestCapability (POST)

        [Fact]
        public async Task CreateTestCapability_Post_ValidModel_ReturnsJsonSuccess()
        {
            // Arrange
            var model = new WorkGroupTestCapabilityItem { TestCode = "TC1", WorkGroup = "WG1", PlanPortfolio = "PP1" };
            var dto = new TestCapabilityDto { TestCode = "TC1" };
            _mapper.Map<TestCapabilityDto>(model).Returns(dto);
            _service.CreateTestCapabilityAsync(dto).Returns(ApiResponseDto<TestCapabilityDto>.SuccessResponse(dto));

            // Act
            var result = await _controller.CreateTestCapability(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.True(value.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task CreateTestCapability_Post_ServiceFails_ReturnsJsonFailure()
        {
            // Arrange
            var model = new WorkGroupTestCapabilityItem { TestCode = "TC1", WorkGroup = "WG1", PlanPortfolio = "PP1" };
            var dto = new TestCapabilityDto { TestCode = "TC1" };
            var errors = new List<ApiErrorDto> { new() { Code = "CONFLICT", Message = "Already exists" } };
            _mapper.Map<TestCapabilityDto>(model).Returns(dto);
            _service.CreateTestCapabilityAsync(dto).Returns(ApiResponseDto<TestCapabilityDto>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _controller.CreateTestCapability(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task CreateTestCapability_Post_InvalidModelState_ReturnsJsonFailure()
        {
            // Arrange
            _controller.ModelState.AddModelError("TestCode", "Test Code is required.");

            // Act
            var result = await _controller.CreateTestCapability(new WorkGroupTestCapabilityItem());

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
        }

        #endregion

        #region EditTestCapability (GET)

        [Fact]
        public async Task EditTestCapability_Get_CapabilityExists_ReturnsPartialViewWithModel()
        {
            // Arrange
            var dto = new TestCapabilityDto { TestCode = "TC1", WorkGroup = "WG1" };
            var item = new WorkGroupTestCapabilityItem { TestCode = "TC1", WorkGroup = "WG1", PlanPortfolio = "PP1" };
            _service.GetTestCapabilityByIdAsync("TC1", "WG1")
                .Returns(ApiResponseDto<TestCapabilityDto>.SuccessResponse(dto));
            _mapper.Map<WorkGroupTestCapabilityItem>(dto).Returns(item);
            SetupDropdowns();

            // Act
            var result = await _controller.EditTestCapability("TC1", "WG1");

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_AddEditTestCapability", partial.ViewName);
            var model = Assert.IsType<WorkGroupTestCapabilityItem>(partial.Model);
            Assert.Equal("TC1", model.TestCode);
        }

        [Fact]
        public async Task EditTestCapability_Get_CapabilityNotFound_ReturnsNotFoundResult()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Code = "NOT_FOUND", Message = "Not found" } };
            _service.GetTestCapabilityByIdAsync("MISSING", "WG1")
                .Returns(ApiResponseDto<TestCapabilityDto>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _controller.EditTestCapability("MISSING", "WG1");

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        #endregion

        #region EditTestCapability (POST)

        [Fact]
        public async Task EditTestCapability_Post_ValidModel_ReturnsJsonSuccess()
        {
            // Arrange
            var model = new WorkGroupTestCapabilityItem { TestCode = "TC1", WorkGroup = "WG1", PlanPortfolio = "PP1" };
            var dto = new TestCapabilityDto { TestCode = "TC1" };
            _mapper.Map<TestCapabilityDto>(model).Returns(dto);
            _service.UpdateTestCapabilityAsync(dto).Returns(ApiResponseDto<TestCapabilityDto>.SuccessResponse(dto));

            // Act
            var result = await _controller.EditTestCapability(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.True(value.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task EditTestCapability_Post_ServiceFails_ReturnsJsonFailure()
        {
            // Arrange
            var model = new WorkGroupTestCapabilityItem { TestCode = "TC1", WorkGroup = "WG1", PlanPortfolio = "PP1" };
            var dto = new TestCapabilityDto { TestCode = "TC1" };
            var errors = new List<ApiErrorDto> { new() { Code = "ERR", Message = "Update failed" } };
            _mapper.Map<TestCapabilityDto>(model).Returns(dto);
            _service.UpdateTestCapabilityAsync(dto).Returns(ApiResponseDto<TestCapabilityDto>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _controller.EditTestCapability(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task EditTestCapability_Post_InvalidModelState_ReturnsJsonFailure()
        {
            // Arrange
            _controller.ModelState.AddModelError("WorkGroup", "Work Group is required.");

            // Act
            var result = await _controller.EditTestCapability(new WorkGroupTestCapabilityItem());

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
        }

        #endregion

        #region DeleteTestCapability

        [Fact]
        public async Task DeleteTestCapability_Success_ReturnsJsonSuccess()
        {
            // Arrange
            _service.DeleteTestCapabilityAsync("TC1", "WG1")
                .Returns(ApiResponseDto<bool>.SuccessResponse(true));

            // Act
            var result = await _controller.DeleteTestCapability("TC1", "WG1");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.True(value.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task DeleteTestCapability_ServiceFails_ReturnsJsonFailure()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Code = "NOT_FOUND", Message = "Not found" } };
            _service.DeleteTestCapabilityAsync("MISSING", "WG1")
                .Returns(ApiResponseDto<bool>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _controller.DeleteTestCapability("MISSING", "WG1");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
        }

        #endregion

        #region CreateTestReqmt (GET)

        [Fact]
        public async Task CreateTestReqmt_Get_WithTestCode_PreFillsPricingAndReturnsPartialView()
        {
            // Arrange
            var pricing = new TestRequirementDto { TestCode = "BLOOD", RecUnitPrice = 15.75m };
            _service.GetTestReqmtPricingAsync("BLOOD", null)
                .Returns(ApiResponseDto<TestRequirementDto>.SuccessResponse(pricing));
            SetupDropdowns();

            // Act
            var result = await _controller.CreateTestReqmt("BLOOD");

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_AddEditTestReqmt", partial.ViewName);
            var model = Assert.IsType<TestRequirementItem>(partial.Model);
            Assert.Equal("BLOOD", model.TestCode);
            Assert.Equal(15.75m, model.RecUnitPrice);
            Assert.Equal(15.75m, model.UnitPrice);
        }

        [Fact]
        public async Task CreateTestReqmt_Get_WithEmptyTestCode_ReturnsPartialViewWithoutPricing()
        {
            // Arrange
            SetupDropdowns();

            // Act
            var result = await _controller.CreateTestReqmt(string.Empty);

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_AddEditTestReqmt", partial.ViewName);
            await _service.DidNotReceive().GetTestReqmtPricingAsync(Arg.Any<string>(), Arg.Any<string?>());
        }

        [Fact]
        public async Task CreateTestReqmt_Get_PricingFails_ReturnsPartialViewWithoutPricing()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Code = "NOT_FOUND" } };
            _service.GetTestReqmtPricingAsync("BLOOD", null)
                .Returns(ApiResponseDto<TestRequirementDto>.FailureResponse(errors, new ApiMetaDto()));
            SetupDropdowns();

            // Act
            var result = await _controller.CreateTestReqmt("BLOOD");

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<TestRequirementItem>(partial.Model);
            Assert.Null(model.RecUnitPrice);
        }

        #endregion

        #region CreateTestReqmt (POST)

        [Fact]
        public async Task CreateTestReqmt_Post_ValidModel_ReturnsJsonSuccess()
        {
            // Arrange
            var model = new TestRequirementItem { TestCode = "BLOOD", Buyer = "PRJ1" };
            var dto = new TestRequirementDto { TestCode = "BLOOD", Buyer = "PRJ1" };
            _mapper.Map<TestRequirementDto>(model).Returns(dto);
            _service.CreateTestReqmtAsync(dto).Returns(ApiResponseDto<TestRequirementDto>.SuccessResponse(dto));

            // Act
            var result = await _controller.CreateTestReqmt(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.True(value.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task CreateTestReqmt_Post_ServiceFails_ReturnsJsonFailure()
        {
            // Arrange
            var model = new TestRequirementItem { TestCode = "BLOOD", Buyer = "PRJ1" };
            var dto = new TestRequirementDto { TestCode = "BLOOD", Buyer = "PRJ1" };
            var errors = new List<ApiErrorDto> { new() { Code = "CONFLICT", Message = "Already exists" } };
            _mapper.Map<TestRequirementDto>(model).Returns(dto);
            _service.CreateTestReqmtAsync(dto).Returns(ApiResponseDto<TestRequirementDto>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _controller.CreateTestReqmt(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task CreateTestReqmt_Post_InvalidModelState_ReturnsJsonFailure()
        {
            // Arrange
            _controller.ModelState.AddModelError("Buyer", "Buyer is required.");

            // Act
            var result = await _controller.CreateTestReqmt(new TestRequirementItem());

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
        }

        #endregion

        #region EditTestReqmt (GET)

        [Fact]
        public async Task EditTestReqmt_Get_ReqmtExists_ReturnsPartialViewWithModel()
        {
            // Arrange
            var dto = new TestRequirementDto { TestCode = "BLOOD", Buyer = "PRJ1" };
            var item = new TestRequirementItem { TestCode = "BLOOD", Buyer = "PRJ1" };
            _service.GetTestReqmtByIdAsync("BLOOD", "PRJ1")
                .Returns(ApiResponseDto<TestRequirementDto>.SuccessResponse(dto));
            _mapper.Map<TestRequirementItem>(dto).Returns(item);
            _service.GetAllTestorProductsAsync().Returns(ApiResponseDto<List<TestorProductDto>>.SuccessResponse([]));
            _projectService.GetAllProjectsAsync().Returns(ApiResponseDto<List<ProjectDto>>.SuccessResponse([]));

            // Act
            var result = await _controller.EditTestReqmt("BLOOD", "PRJ1");

            // Assert
            var partial = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_AddEditTestReqmt", partial.ViewName);
            var model = Assert.IsType<TestRequirementItem>(partial.Model);
            Assert.Equal("BLOOD", model.TestCode);
        }

        [Fact]
        public async Task EditTestReqmt_Get_ReqmtNotFound_ReturnsNotFoundResult()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Code = "NOT_FOUND", Message = "Not found" } };
            _service.GetTestReqmtByIdAsync("MISSING", "PRJ1")
                .Returns(ApiResponseDto<TestRequirementDto>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _controller.EditTestReqmt("MISSING", "PRJ1");

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        #endregion

        #region EditTestReqmt (POST)

        [Fact]
        public async Task EditTestReqmt_Post_ValidModel_ReturnsJsonSuccess()
        {
            // Arrange
            var model = new TestRequirementItem { TestCode = "BLOOD", Buyer = "PRJ1" };
            var dto = new TestRequirementDto { TestCode = "BLOOD", Buyer = "PRJ1" };
            _mapper.Map<TestRequirementDto>(model).Returns(dto);
            _service.UpdateTestReqmtAsync(dto).Returns(ApiResponseDto<TestRequirementDto>.SuccessResponse(dto));

            // Act
            var result = await _controller.EditTestReqmt(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.True(value.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task EditTestReqmt_Post_ServiceFails_ReturnsJsonFailure()
        {
            // Arrange
            var model = new TestRequirementItem { TestCode = "BLOOD", Buyer = "PRJ1" };
            var dto = new TestRequirementDto { TestCode = "BLOOD", Buyer = "PRJ1" };
            var errors = new List<ApiErrorDto> { new() { Code = "ERR", Message = "Update failed" } };
            _mapper.Map<TestRequirementDto>(model).Returns(dto);
            _service.UpdateTestReqmtAsync(dto).Returns(ApiResponseDto<TestRequirementDto>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _controller.EditTestReqmt(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task EditTestReqmt_Post_InvalidModelState_ReturnsJsonFailure()
        {
            // Arrange
            _controller.ModelState.AddModelError("TestCode", "Test Code is required.");

            // Act
            var result = await _controller.EditTestReqmt(new TestRequirementItem());

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
        }

        #endregion

        #region DeleteTestReqmt

        [Fact]
        public async Task DeleteTestReqmt_Success_ReturnsJsonSuccess()
        {
            // Arrange
            _service.DeleteTestReqmtAsync("BLOOD", "PRJ1")
                .Returns(ApiResponseDto<bool>.SuccessResponse(true));

            // Act
            var result = await _controller.DeleteTestReqmt("BLOOD", "PRJ1");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.True(value.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task DeleteTestReqmt_ServiceFails_ReturnsJsonFailure()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Code = "NOT_FOUND", Message = "Not found" } };
            _service.DeleteTestReqmtAsync("MISSING", "PRJ1")
                .Returns(ApiResponseDto<bool>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _controller.DeleteTestReqmt("MISSING", "PRJ1");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
        }

        #endregion

        #region ExportTestReqmt

        [Fact]
        public async Task ExportTestReqmt_WithData_ReturnsFileContentResult()
        {
            // Arrange
            var dtos = new List<TestRequirementDto> { new() { TestCode = "BLOOD", Buyer = "PRJ1" } };
            var items = new List<TestRequirementItem> { new() { TestCode = "BLOOD", Buyer = "PRJ1" } };
            var fileBytes = new byte[] { 1, 2, 3 };

            _service.GetAllTestReqmtForExportAsync("BLOOD", null)
                .Returns(ApiResponseDto<List<TestRequirementDto>>.SuccessResponse(dtos));
            _mapper.Map<List<TestRequirementItem>>(dtos).Returns(items);
            _excelExportService.ExportToExcel(items, "Test Requirements").Returns(fileBytes);

            // Act
            var result = await _controller.ExportTestReqmt("BLOOD", null);

            // Assert
            var fileResult = Assert.IsType<FileContentResult>(result);
            Assert.Equal("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileResult.ContentType);
            Assert.StartsWith("TestRequirements_BLOOD_", fileResult.FileDownloadName);
            Assert.Equal(fileBytes, fileResult.FileContents);
        }

        [Fact]
        public async Task ExportTestReqmt_WithFilter_PassesFilterToService()
        {
            // Arrange
            var filter = "{\"Buyer\":\"PRJ1\"}";
            _service.GetAllTestReqmtForExportAsync("BLOOD", filter)
                .Returns(ApiResponseDto<List<TestRequirementDto>>.SuccessResponse([]));
            _mapper.Map<List<TestRequirementItem>>(Arg.Any<List<TestRequirementDto>>()).Returns([]);
            _excelExportService.ExportToExcel(Arg.Any<List<TestRequirementItem>>(), "Test Requirements").Returns([]);

            // Act
            var result = await _controller.ExportTestReqmt("BLOOD", filter);

            // Assert
            Assert.IsType<FileContentResult>(result);
            await _service.Received(1).GetAllTestReqmtForExportAsync("BLOOD", filter);
        }

        [Fact]
        public async Task ExportTestReqmt_ServiceFails_ReturnsFileWithEmptyData()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Code = "ERR" } };
            _service.GetAllTestReqmtForExportAsync("BLOOD", null)
                .Returns(ApiResponseDto<List<TestRequirementDto>>.FailureResponse(errors, new ApiMetaDto()));
            _excelExportService.ExportToExcel(Arg.Any<List<TestRequirementItem>>(), "Test Requirements").Returns([]);

            // Act
            var result = await _controller.ExportTestReqmt("BLOOD", null);

            // Assert
            Assert.IsType<FileContentResult>(result);
            _excelExportService.Received(1).ExportToExcel(
                Arg.Is<List<TestRequirementItem>>(l => l.Count == 0), "Test Requirements");
        }

        #endregion

        #region GetTestReqmtPricing

        [Fact]
        public async Task GetTestReqmtPricing_EmptyTestCode_ReturnsJsonFailure()
        {
            // Act
            var result = await _controller.GetTestReqmtPricing(string.Empty);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
            await _service.DidNotReceive().GetTestReqmtPricingAsync(Arg.Any<string>(), Arg.Any<string?>());
        }

        [Fact]
        public async Task GetTestReqmtPricing_ServiceFails_ReturnsJsonFailure()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Code = "NOT_FOUND" } };
            _service.GetTestReqmtPricingAsync("BLOOD", null)
                .Returns(ApiResponseDto<TestRequirementDto>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _controller.GetTestReqmtPricing("BLOOD");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task GetTestReqmtPricing_WithTestCodeOnly_ReturnsRecUnitPriceAndNullIsDefraProject()
        {
            // Arrange
            var pricing = new TestRequirementDto { TestCode = "BLOOD", RecUnitPrice = 12.50m, IsDefraProject = 1 };
            _service.GetTestReqmtPricingAsync("BLOOD", null)
                .Returns(ApiResponseDto<TestRequirementDto>.SuccessResponse(pricing));

            // Act
            var result = await _controller.GetTestReqmtPricing("BLOOD");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.True(value.GetProperty("success").GetBoolean());
            Assert.Equal(12.50m, value.GetProperty("recUnitPrice").GetDecimal());
            Assert.Equal(JsonValueKind.Null, value.GetProperty("isDefraProject").ValueKind);
        }

        [Fact]
        public async Task GetTestReqmtPricing_WithProjectCode_ReturnsIsDefraProjectValue()
        {
            // Arrange
            var pricing = new TestRequirementDto { TestCode = "BLOOD", RecUnitPrice = 10.0m, IsDefraProject = 1 };
            _service.GetTestReqmtPricingAsync("BLOOD", "PRJ1")
                .Returns(ApiResponseDto<TestRequirementDto>.SuccessResponse(pricing));

            // Act
            var result = await _controller.GetTestReqmtPricing("BLOOD", "PRJ1");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.True(value.GetProperty("success").GetBoolean());
            Assert.Equal(10.0m, value.GetProperty("recUnitPrice").GetDecimal());
            Assert.NotEqual(JsonValueKind.Null, value.GetProperty("isDefraProject").ValueKind);
        }

        #endregion
    }
}
