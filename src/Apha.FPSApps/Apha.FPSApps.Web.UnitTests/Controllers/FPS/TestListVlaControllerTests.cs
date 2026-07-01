/*
 * TRANSFORMENGINE MIGRATION — TestListVlaControllerTests.cs (frontend MVC)
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 13 — Unit Tests - Backend + Frontend xUnit Coverage
 * Migrated : 2026-07-01
 *
 * CHANGED:
 *   - New xUnit test class for the frontend MVC TestListVlaController (FPS area)
 *   - Covers Index, LoadTestListVlaGrid, CreateTestListVla (GET/POST), EditTestListVla (GET/POST),
 *     DeleteTestListVla, tab grid loaders, and sub-resource CRUD actions
 *   - NSubstitute for all dependencies: IMapper, ITestListVlaService, ITestRequirementService,
 *     ITestCapabilityService, IFpsApiClient, IFpsYearContext
 *
 * PRESERVED:
 *   - Naming convention: [MethodName]_[StateUnderTest]_[ExpectedResult]
 *   - Frontend controller uses ITestListVlaService for main CRUD and IFpsApiClient for sub-resources
 *
 * DEFERRED: none — fully automated.
 */

using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Interfaces.PACT;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.FPS.Controllers;
using Apha.FPSApps.Web.Areas.FPS.Models;
using Apha.FPSApps.Web.Handler;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using System.Text.Json;
using Xunit;

namespace Apha.FPSApps.Web.UnitTests.Controllers.FPS.TestListVlaControllerTest
{
    public class TestListVlaControllerTests
    {
        private const string DefaultItemCode = "TEST001";
        private const string DefaultTestCode = "TEST001";
        private const string DefaultBuyer = "BUYER01";
        private const string DefaultProfitCentre = "PC001";
        private const int DefaultFpsYear = 2025;

        private readonly IMapper _mapper;
        private readonly ITestListVlaService _testListVlaService;
        private readonly ITestRequirementService _testRequirementService;
        private readonly ITestCapabilityService _testCapabilityService;
        private readonly IFpsApiClient _fpsApiClient;
        private readonly IFpsTestRCCostApiClient _fpsTestRCCostApiClient;
        private readonly IFpsTestRequirementRCCostApiClient _fpsTestRequirementRCCostApiClient;
        private readonly IFpsYearContext _fpsYearContext;
        private readonly TestListVlaController _controller;

        public TestListVlaControllerTests()
        {
            _mapper = Substitute.For<IMapper>();
            _testListVlaService = Substitute.For<ITestListVlaService>();
            _testRequirementService = Substitute.For<ITestRequirementService>();
            _testCapabilityService = Substitute.For<ITestCapabilityService>();
            _fpsApiClient = Substitute.For<IFpsApiClient>();
            _fpsTestRCCostApiClient = Substitute.For<IFpsTestRCCostApiClient>();
            _fpsTestRequirementRCCostApiClient = Substitute.For<IFpsTestRequirementRCCostApiClient>();
            _fpsYearContext = Substitute.For<IFpsYearContext>();
            _fpsYearContext.Year.Returns(DefaultFpsYear);
            _fpsApiClient.FpsTestRCCost.Returns(_fpsTestRCCostApiClient);
            _fpsApiClient.FpsTestRequirementRCCost.Returns(_fpsTestRequirementRCCostApiClient);

            _controller = new TestListVlaController(
                _mapper,
                _testListVlaService,
                _testRequirementService,
                _testCapabilityService,
                _fpsApiClient,
                _fpsYearContext);
        }

        private static JsonElement GetJsonResultElement(JsonResult jsonResult)
        {
            var json = JsonSerializer.Serialize(jsonResult.Value);
            return JsonSerializer.Deserialize<JsonElement>(json);
        }

        private void SetupGridMapper()
        {
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>())
                .Returns(new QueryParameters<string>());
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>())
                .Returns(new PaginationModel());
        }

        #region Index

        [Fact]
        public void Index_Always_ReturnsViewResultWithViewModel()
        {
            // Act
            var result = _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<TestListVlaViewModel>(viewResult.Model);
            Assert.Equal(DefaultFpsYear, model.FpsYear);
        }

        [Fact]
        public void Index_Always_PopulatesAllFiveGridConfigs()
        {
            // Act
            var result = _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<TestListVlaViewModel>(viewResult.Model);
            Assert.NotNull(model.TestListGrid);
            Assert.NotNull(model.TestRequirementsGrid);
            Assert.NotNull(model.ComponentChargesGeneralGrid);
            Assert.NotNull(model.ComponentChargesProjectGrid);
            Assert.NotNull(model.SuppliersGrid);
        }

        [Fact]
        public void Index_Always_MainGridAllowsCRUD()
        {
            // Act
            var result = _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<TestListVlaViewModel>(viewResult.Model);
            Assert.True(model.TestListGrid.AllowAdd);
            Assert.True(model.TestListGrid.AllowEdit);
            Assert.True(model.TestListGrid.AllowDelete);
        }

        [Fact]
        public void Index_Always_SuppliersGridIsReadOnly()
        {
            // Act
            var result = _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<TestListVlaViewModel>(viewResult.Model);
            Assert.False(model.SuppliersGrid.AllowAdd);
            Assert.False(model.SuppliersGrid.AllowEdit);
            Assert.False(model.SuppliersGrid.AllowDelete);
        }

        #endregion

        #region LoadTestListVlaGrid

        [Fact]
        public async Task LoadTestListVlaGrid_InvalidModelState_ReturnsJsonWithSuccessFalse()
        {
            // Arrange
            _controller.ModelState.AddModelError("test", "Error");

            // Act
            var result = await _controller.LoadTestListVlaGrid(new PaginationFilter<string>());

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);
            Assert.False(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task LoadTestListVlaGrid_ServiceReturnsData_ReturnsPartialViewWithGridConfig()
        {
            // Arrange
            SetupGridMapper();
            var query = new QueryParameters<string>();
            var response = ApiResponseDto<List<TestListVlaDto>>.SuccessResponse(
                new List<TestListVlaDto> { new() { ItemCode = DefaultItemCode, FpsYear = DefaultFpsYear } },
                new PaginationDto { TotalRecords = 1 });

            _testListVlaService.GetAllAsync(Arg.Any<QueryParameters<string>>(), DefaultFpsYear)
                .Returns(response);
            _mapper.Map<List<TestListVlaItem>>(Arg.Any<List<TestListVlaDto>>())
                .Returns(new List<TestListVlaItem> { new() { ItemCode = DefaultItemCode } });

            // Act
            var result = await _controller.LoadTestListVlaGrid(new PaginationFilter<string>());

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partialView.ViewName);
        }

        [Fact]
        public async Task LoadTestListVlaGrid_ServiceReturnsEmpty_ReturnsPartialViewWithEmptyData()
        {
            // Arrange
            SetupGridMapper();
            var response = ApiResponseDto<List<TestListVlaDto>>.SuccessResponse(
                new List<TestListVlaDto>(),
                new PaginationDto { TotalRecords = 0 });

            _testListVlaService.GetAllAsync(Arg.Any<QueryParameters<string>>(), DefaultFpsYear)
                .Returns(response);
            _mapper.Map<List<TestListVlaItem>>(Arg.Any<List<TestListVlaDto>>())
                .Returns(new List<TestListVlaItem>());

            // Act
            var result = await _controller.LoadTestListVlaGrid(new PaginationFilter<string>());

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            var config = Assert.IsType<DataGridConfig<TestListVlaItem>>(partialView.Model);
            Assert.Empty(config.Data);
        }

        #endregion

        #region CreateTestListVla

        [Fact]
        public void CreateTestListVla_Get_ReturnsPartialViewWithEmptyItem()
        {
            // Act
            var result = _controller.CreateTestListVla();

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_AddEditTestListVla", partialView.ViewName);
            var item = Assert.IsType<TestListVlaItem>(partialView.Model);
            Assert.Equal(DefaultFpsYear, item.FpsYear);
        }

        [Fact]
        public async Task CreateTestListVla_Post_ValidModel_ServiceReturnsSuccess_ReturnsJsonTrue()
        {
            // Arrange
            var model = new TestListVlaItem { ItemCode = DefaultItemCode, FpsYear = DefaultFpsYear };
            var dto = new TestListVlaDto { ItemCode = DefaultItemCode, FpsYear = DefaultFpsYear };
            var response = ApiResponseDto<TestListVlaDto>.SuccessResponse(dto);

            _mapper.Map<TestListVlaDto>(model).Returns(dto);
            _testListVlaService.CreateAsync(Arg.Any<TestListVlaDto>()).Returns(response);

            // Act
            var result = await _controller.CreateTestListVla(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);
            Assert.True(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task CreateTestListVla_Post_InvalidModelState_ReturnsJsonFalse()
        {
            // Arrange
            _controller.ModelState.AddModelError("ItemCode", "Required");

            // Act
            var result = await _controller.CreateTestListVla(new TestListVlaItem());

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);
            Assert.False(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task CreateTestListVla_Post_ServiceReturnsFailure_ReturnsJsonFalse()
        {
            // Arrange
            var model = new TestListVlaItem { ItemCode = DefaultItemCode, FpsYear = DefaultFpsYear };
            var dto = new TestListVlaDto { ItemCode = DefaultItemCode, FpsYear = DefaultFpsYear };
            var response = ApiResponseDto<TestListVlaDto>.FailureResponse(
                new List<ApiErrorDto> { new() { Message = "Duplicate key" } }, new ApiMetaDto());

            _mapper.Map<TestListVlaDto>(model).Returns(dto);
            _testListVlaService.CreateAsync(Arg.Any<TestListVlaDto>()).Returns(response);

            // Act
            var result = await _controller.CreateTestListVla(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);
            Assert.False(element.GetProperty("success").GetBoolean());
        }

        #endregion

        #region EditTestListVla

        [Fact]
        public async Task EditTestListVla_Get_ServiceReturnsSuccess_ReturnsPartialView()
        {
            // Arrange
            var dto = new TestListVlaDto { ItemCode = DefaultItemCode, FpsYear = DefaultFpsYear };
            var response = ApiResponseDto<TestListVlaDto>.SuccessResponse(dto);
            var item = new TestListVlaItem { ItemCode = DefaultItemCode };

            _testListVlaService.GetByIdAsync(DefaultItemCode, DefaultFpsYear).Returns(response);
            _mapper.Map<TestListVlaItem>(dto).Returns(item);

            // Act
            var result = await _controller.EditTestListVla(DefaultItemCode);

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_AddEditTestListVla", partialView.ViewName);
        }

        [Fact]
        public async Task EditTestListVla_Get_ServiceReturnsFailure_ReturnsNotFound()
        {
            // Arrange
            var response = ApiResponseDto<TestListVlaDto>.FailureResponse(
                new List<ApiErrorDto> { new() { Message = "Not found" } }, new ApiMetaDto());
            _testListVlaService.GetByIdAsync("NOTEXIST", DefaultFpsYear).Returns(response);

            // Act
            var result = await _controller.EditTestListVla("NOTEXIST");

            // Assert
            // Phase 14 security fix: controller now returns generic NotFound() (NotFoundResult)
            // rather than NotFound("message") (NotFoundObjectResult) to prevent information disclosure.
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task EditTestListVla_Post_ValidModel_ServiceReturnsSuccess_ReturnsJsonTrue()
        {
            // Arrange
            var model = new TestListVlaItem { ItemCode = DefaultItemCode, FpsYear = DefaultFpsYear };
            var dto = new TestListVlaDto { ItemCode = DefaultItemCode, FpsYear = DefaultFpsYear };
            var response = ApiResponseDto<TestListVlaDto>.SuccessResponse(dto);

            _mapper.Map<TestListVlaDto>(model).Returns(dto);
            _testListVlaService.UpdateAsync(DefaultItemCode, DefaultFpsYear, Arg.Any<TestListVlaDto>())
                .Returns(response);

            // Act
            var result = await _controller.EditTestListVla(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);
            Assert.True(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task EditTestListVla_Post_InvalidModelState_ReturnsJsonFalse()
        {
            // Arrange
            _controller.ModelState.AddModelError("ItemCode", "Required");

            // Act
            var result = await _controller.EditTestListVla(new TestListVlaItem());

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);
            Assert.False(element.GetProperty("success").GetBoolean());
        }

        #endregion

        #region DeleteTestListVla

        [Fact]
        public async Task DeleteTestListVla_ServiceReturnsSuccess_ReturnsJsonTrue()
        {
            // Arrange
            var response = ApiResponseDto<bool>.SuccessResponse(true);
            _testListVlaService.DeleteAsync(DefaultItemCode, DefaultFpsYear).Returns(response);

            // Act
            var result = await _controller.DeleteTestListVla(DefaultItemCode);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);
            Assert.True(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task DeleteTestListVla_ServiceReturnsFailure_ReturnsJsonFalse()
        {
            // Arrange
            var response = ApiResponseDto<bool>.FailureResponse(
                new List<ApiErrorDto> { new() { Message = "Not found" } }, new ApiMetaDto());
            _testListVlaService.DeleteAsync("NOTEXIST", DefaultFpsYear).Returns(response);

            // Act
            var result = await _controller.DeleteTestListVla("NOTEXIST");

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);
            Assert.False(element.GetProperty("success").GetBoolean());
        }

        #endregion

        #region LoadComponentChargesGeneralGrid

        [Fact]
        public async Task LoadComponentChargesGeneralGrid_InvalidModelState_ReturnsJsonFalse()
        {
            // Arrange
            _controller.ModelState.AddModelError("test", "Error");

            // Act
            var result = await _controller.LoadComponentChargesGeneralGrid(new PaginationFilter<string>(), DefaultTestCode);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var element = GetJsonResultElement(jsonResult);
            Assert.False(element.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task LoadComponentChargesGeneralGrid_NullTestCode_ReturnsPartialViewWithEmptyData()
        {
            // Act — no testCode provided
            var result = await _controller.LoadComponentChargesGeneralGrid(new PaginationFilter<string>(), null);

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partialView.ViewName);
        }

        [Fact]
        public async Task LoadComponentChargesGeneralGrid_WithTestCode_CallsRCCostApiClient()
        {
            // Arrange
            var response = ApiResponseDto<List<TestRCCostDto>>.SuccessResponse(new List<TestRCCostDto>());
            _fpsTestRCCostApiClient.GetByTestCodeAsync(DefaultTestCode, DefaultFpsYear).Returns(response);
            _mapper.Map<List<TestRCCostItem>>(Arg.Any<List<TestRCCostDto>>())
                .Returns(new List<TestRCCostItem>());

            // Act
            await _controller.LoadComponentChargesGeneralGrid(new PaginationFilter<string>(), DefaultTestCode);

            // Assert
            await _fpsTestRCCostApiClient.Received(1).GetByTestCodeAsync(DefaultTestCode, DefaultFpsYear);
        }

        #endregion

        #region LoadComponentChargesProjectGrid

        [Fact]
        public async Task LoadComponentChargesProjectGrid_WithTestCode_CallsRequirementRCCostApiClient()
        {
            // Arrange
            var response = ApiResponseDto<List<TestRequirementRCCostDto>>.SuccessResponse(new List<TestRequirementRCCostDto>());
            _fpsTestRequirementRCCostApiClient.GetByTestCodeAsync(DefaultTestCode, DefaultFpsYear).Returns(response);
            _mapper.Map<List<TestRequirementRCCostItem>>(Arg.Any<List<TestRequirementRCCostDto>>())
                .Returns(new List<TestRequirementRCCostItem>());

            // Act
            await _controller.LoadComponentChargesProjectGrid(new PaginationFilter<string>(), DefaultTestCode);

            // Assert
            await _fpsTestRequirementRCCostApiClient.Received(1).GetByTestCodeAsync(DefaultTestCode, DefaultFpsYear);
        }

        [Fact]
        public async Task LoadComponentChargesProjectGrid_NullTestCode_ReturnsPartialViewWithEmptyData()
        {
            // Act
            var result = await _controller.LoadComponentChargesProjectGrid(new PaginationFilter<string>(), null);

            // Assert
            var partialView = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_DataGrid", partialView.ViewName);
        }

        #endregion
    }
}
