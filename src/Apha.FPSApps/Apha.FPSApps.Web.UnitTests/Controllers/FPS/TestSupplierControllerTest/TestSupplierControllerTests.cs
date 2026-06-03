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
using Microsoft.AspNetCore.Mvc.ModelBinding;
using NSubstitute;
using System.Text.Json;

namespace Apha.FPSApps.Web.UnitTests.Controllers.FPS.TestSupplierControllerTest
{
    public class TestSupplierControllerTests
    {
        private const string DefaultTestCode = "TEST001";
        private const string DefaultBuyer = "BUYER001";

        private readonly IMapper _mapper;
        private readonly ITestSupplierService _testSupplierService;
        private readonly ITestorProductService _testorProductService;
        private readonly TestSupplierController _controller;

        public TestSupplierControllerTests()
        {
            _mapper = Substitute.For<IMapper>();
            _testSupplierService = Substitute.For<ITestSupplierService>();
            _testorProductService = Substitute.For<ITestorProductService>();
            _controller = new TestSupplierController(_mapper, _testSupplierService, _testorProductService);
        }

        private static JsonElement GetJsonResultElement(JsonResult jsonResult)
        {
            var json = JsonSerializer.Serialize(jsonResult.Value);
            return JsonSerializer.Deserialize<JsonElement>(json);
        }

        #region Delete

        [Fact]
        public async Task Delete_WithValidParameters_WhenSucceeds_ReturnsSuccessJson()
        {
            var successResponse = ApiResponseDto<bool>.SuccessResponse(true);
            _testSupplierService.DeleteAsync(DefaultTestCode, DefaultBuyer).Returns(successResponse);

            var result = await _controller.Delete(DefaultTestCode, DefaultBuyer);

            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.True(value.GetProperty("success").GetBoolean());
            await _testSupplierService.Received(1).DeleteAsync(DefaultTestCode, DefaultBuyer);
        }

        [Fact]
        public async Task Delete_WithNullTestCode_ReturnsFailureJson()
        {
            var result = await _controller.Delete(null!, DefaultBuyer);

            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
            await _testSupplierService.DidNotReceive().DeleteAsync(Arg.Any<string>(), Arg.Any<string>());
        }

        [Fact]
        public async Task Delete_WithEmptyBuyer_ReturnsFailureJson()
        {
            var result = await _controller.Delete(DefaultTestCode, string.Empty);

            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
            await _testSupplierService.DidNotReceive().DeleteAsync(Arg.Any<string>(), Arg.Any<string>());
        }

        [Fact]
        public async Task Delete_WithWhitespaceTestCode_ReturnsFailureJson()
        {
            var result = await _controller.Delete("   ", DefaultBuyer);

            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
            Assert.Equal("TestCode and Buyer are required.", value.GetProperty("message").GetString());
            await _testSupplierService.DidNotReceive().DeleteAsync(Arg.Any<string>(), Arg.Any<string>());
        }

        [Fact]
        public async Task Delete_WhenServiceReturnsFailure_ReturnsFailureJson()
        {
            var failureResponse = ApiResponseDto<bool>.FailureResponse(
                new List<ApiErrorDto> { new() { Code = "404", Message = "Record not found" } }, new ApiMetaDto());
            _testSupplierService.DeleteAsync(DefaultTestCode, DefaultBuyer).Returns(failureResponse);

            var result = await _controller.Delete(DefaultTestCode, DefaultBuyer);

            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
            Assert.Equal("Record not found", value.GetProperty("message").GetString());
        }

        [Fact]
        public async Task Delete_WhenServiceReturnsFailureWithNoErrors_ReturnsDefaultMessage()
        {
            var failureResponse = ApiResponseDto<bool>.FailureResponse(new List<ApiErrorDto>(), new ApiMetaDto());
            _testSupplierService.DeleteAsync(DefaultTestCode, DefaultBuyer).Returns(failureResponse);

            var result = await _controller.Delete(DefaultTestCode, DefaultBuyer);

            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
            Assert.Equal("Failed to delete test supplier.", value.GetProperty("message").GetString());
        }

        #endregion

        #region Edit (GET)

        [Fact]
        public async Task Edit_Get_WithEmptyTestCode_ReturnsFailureJson()
        {
            var result = await _controller.Edit(string.Empty, DefaultBuyer);

            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
            await _testSupplierService.DidNotReceive().GetViewByIdAsync(Arg.Any<string>(), Arg.Any<string>());
        }

        [Fact]
        public async Task Edit_Get_WithWhitespaceBuyer_ReturnsFailureJson()
        {
            var result = await _controller.Edit(DefaultTestCode, "   ");

            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
            Assert.Equal("TestCode and Buyer are required.", value.GetProperty("message").GetString());
            await _testSupplierService.DidNotReceive().GetViewByIdAsync(Arg.Any<string>(), Arg.Any<string>());
        }

        [Fact]
        public async Task Edit_Get_WhenViewNotFound_ReturnsFailureJson()
        {
            var failureResponse = ApiResponseDto<TestSupplierViewDto>.FailureResponse(
                new List<ApiErrorDto> { new() { Code = "404", Message = "Not found" } }, new ApiMetaDto());
            _testSupplierService.GetViewByIdAsync(DefaultTestCode, DefaultBuyer).Returns(failureResponse);

            var result = await _controller.Edit(DefaultTestCode, DefaultBuyer);

            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
        }

        [Fact]
        public async Task Edit_Get_WhenRecordFound_ReturnsPartialView()
        {
            var viewDto = new TestSupplierViewDto { TestCode = DefaultTestCode, JobCode = DefaultBuyer };
            var viewResponse = ApiResponseDto<TestSupplierViewDto>.SuccessResponse(viewDto);
            var detailResponse = ApiResponseDto<FpsTestRequirementDto>.FailureResponse(
                new List<ApiErrorDto> { new() { Code = "404", Message = "Not found" } }, new ApiMetaDto());
            var item = new TestSupplierItem { TestCode = DefaultTestCode, Buyer = DefaultBuyer };

            _testSupplierService.GetViewByIdAsync(DefaultTestCode, DefaultBuyer).Returns(viewResponse);
            _testSupplierService.GetByIdAsync(DefaultTestCode, DefaultBuyer).Returns(detailResponse);
            _mapper.Map<TestSupplierItem>(viewDto).Returns(item);

            var result = await _controller.Edit(DefaultTestCode, DefaultBuyer);

            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_AddEditTestSupplier", partialViewResult.ViewName);
        }

        [Fact]
        public async Task Edit_Get_WhenDetailSucceeds_EnrichesItemWithBuyerCodes()
        {
            var viewDto = new TestSupplierViewDto { TestCode = DefaultTestCode, JobCode = DefaultBuyer };
            var viewResponse = ApiResponseDto<TestSupplierViewDto>.SuccessResponse(viewDto);
            var detailDto = new FpsTestRequirementDto { TestCode = DefaultTestCode, Buyer = DefaultBuyer, ProjectBuyerCode = "PB001", TestBuyerCode = "TB001" };
            var detailResponse = ApiResponseDto<FpsTestRequirementDto>.SuccessResponse(detailDto);
            var item = new TestSupplierItem { TestCode = DefaultTestCode, Buyer = DefaultBuyer };

            _testSupplierService.GetViewByIdAsync(DefaultTestCode, DefaultBuyer).Returns(viewResponse);
            _testSupplierService.GetByIdAsync(DefaultTestCode, DefaultBuyer).Returns(detailResponse);
            _mapper.Map<TestSupplierItem>(viewDto).Returns(item);

            var result = await _controller.Edit(DefaultTestCode, DefaultBuyer);

            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<TestSupplierItem>(partialViewResult.Model);
            Assert.Equal("PB001", model.ProjectBuyerCode);
            Assert.Equal("TB001", model.TestBuyerCode);
        }

        #endregion

        #region Edit (POST)

        [Fact]
        public async Task Edit_Post_WhenSucceeds_ReturnsSuccessJson()
        {
            var dto = new FpsTestRequirementDto { TestCode = DefaultTestCode, Buyer = DefaultBuyer };
            var updatedDto = new FpsTestRequirementDto { TestCode = DefaultTestCode, Buyer = DefaultBuyer };
            var successResponse = ApiResponseDto<FpsTestRequirementDto>.SuccessResponse(updatedDto);

            _testSupplierService.UpdateAsync(dto).Returns(successResponse);

            var result = await _controller.Edit(dto);

            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.True(value.GetProperty("success").GetBoolean());
            await _testSupplierService.Received(1).UpdateAsync(dto);
        }

        [Fact]
        public async Task Edit_Post_WhenModelStateInvalid_ReturnsValidationErrorJson()
        {
            _controller.ModelState.AddModelError("TestCode", "TestCode is required.");
            var dto = new FpsTestRequirementDto();

            var result = await _controller.Edit(dto);

            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
            Assert.Equal("Please correct the errors below.", value.GetProperty("message").GetString());
            await _testSupplierService.DidNotReceive().UpdateAsync(Arg.Any<FpsTestRequirementDto>());
        }

        [Fact]
        public async Task Edit_Post_WhenServiceReturnsFailure_ReturnsFailureJson()
        {
            var dto = new FpsTestRequirementDto { TestCode = DefaultTestCode, Buyer = DefaultBuyer };
            var failureResponse = ApiResponseDto<FpsTestRequirementDto>.FailureResponse(
                new List<ApiErrorDto> { new() { Code = "400", Message = "Validation failed" } }, new ApiMetaDto());

            _testSupplierService.UpdateAsync(dto).Returns(failureResponse);

            var result = await _controller.Edit(dto);

            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
            Assert.Equal("Validation failed", value.GetProperty("message").GetString());
        }

        [Fact]
        public async Task Edit_Post_WhenServiceReturnsFailureWithNoErrors_ReturnsDefaultMessage()
        {
            var dto = new FpsTestRequirementDto { TestCode = DefaultTestCode, Buyer = DefaultBuyer };
            var failureResponse = ApiResponseDto<FpsTestRequirementDto>.FailureResponse(new List<ApiErrorDto>(), new ApiMetaDto());

            _testSupplierService.UpdateAsync(dto).Returns(failureResponse);

            var result = await _controller.Edit(dto);

            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.Equal("Failed to update test supplier.", value.GetProperty("message").GetString());
        }

        #endregion

        #region Create (GET)

        [Fact]
        public void Create_Get_ReturnsPartialView()
        {
            var result = _controller.Create();

            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            Assert.Equal("_AddEditTestSupplier", partialViewResult.ViewName);
            var model = Assert.IsType<TestSupplierItem>(partialViewResult.Model);
            Assert.NotNull(model.ProjectStatusOptions);
        }

        [Fact]
        public void Create_Get_ProjectStatusOptions_ContainsApprovedAndRejected()
        {
            var result = _controller.Create();

            var partialViewResult = Assert.IsType<PartialViewResult>(result);
            var model = Assert.IsType<TestSupplierItem>(partialViewResult.Model);
            Assert.Contains(model.ProjectStatusOptions, o => o.Value == "Approved");
            Assert.Contains(model.ProjectStatusOptions, o => o.Value == "Rejected");
        }

        #endregion

        #region Create (POST)

        [Fact]
        public async Task Create_Post_WhenSucceeds_ReturnsSuccessJson()
        {
            var dto = new FpsTestRequirementDto { TestCode = DefaultTestCode, Buyer = DefaultBuyer };
            var createdDto = new FpsTestRequirementDto { TestCode = DefaultTestCode, Buyer = DefaultBuyer };
            var successResponse = ApiResponseDto<FpsTestRequirementDto>.SuccessResponse(createdDto);

            _testSupplierService.CreateAsync(dto).Returns(successResponse);

            var result = await _controller.Create(dto);

            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.True(value.GetProperty("success").GetBoolean());
            await _testSupplierService.Received(1).CreateAsync(dto);
        }

        [Fact]
        public async Task Create_Post_WhenModelStateInvalid_ReturnsValidationErrorJson()
        {
            _controller.ModelState.AddModelError("Buyer", "Buyer is required.");
            var dto = new FpsTestRequirementDto();

            var result = await _controller.Create(dto);

            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
            Assert.Equal("Please correct the errors below.", value.GetProperty("message").GetString());
            await _testSupplierService.DidNotReceive().CreateAsync(Arg.Any<FpsTestRequirementDto>());
        }

        [Fact]
        public async Task Create_Post_WhenServiceReturnsFailure_ReturnsFailureJson()
        {
            var dto = new FpsTestRequirementDto { TestCode = DefaultTestCode, Buyer = DefaultBuyer };
            var failureResponse = ApiResponseDto<FpsTestRequirementDto>.FailureResponse(
                new List<ApiErrorDto> { new() { Code = "400", Message = "Duplicate record" } }, new ApiMetaDto());

            _testSupplierService.CreateAsync(dto).Returns(failureResponse);

            var result = await _controller.Create(dto);

            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
            Assert.Equal("Duplicate record", value.GetProperty("message").GetString());
        }

        [Fact]
        public async Task Create_Post_WhenServiceReturnsFailureWithNoErrors_ReturnsDefaultMessage()
        {
            var dto = new FpsTestRequirementDto { TestCode = DefaultTestCode, Buyer = DefaultBuyer };
            var failureResponse = ApiResponseDto<FpsTestRequirementDto>.FailureResponse(new List<ApiErrorDto>(), new ApiMetaDto());

            _testSupplierService.CreateAsync(dto).Returns(failureResponse);

            var result = await _controller.Create(dto);

            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.Equal("Failed to create test supplier.", value.GetProperty("message").GetString());
        }

        #endregion

        #region Index

        [Fact]
        public async Task Index_ReturnsViewWithViewModel()
        {
            var testorProducts = new List<TestorProductDto>
            {
                new() { ItemCode = "T001", ItemDescription = "Product 1" }
            };
            var productsResponse = ApiResponseDto<List<TestorProductDto>>.SuccessResponse(testorProducts);

            _testorProductService.GetAllTestorProductsAsync().Returns(productsResponse);
            _mapper.Map<QueryParameters<string>>(Arg.Any<object>()).Returns(new QueryParameters<string>());

            var response = ApiResponseDto<List<TestSupplierViewDto>>.SuccessResponse(new List<TestSupplierViewDto>());
            _testSupplierService.GetPagedAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<string>(), Arg.Any<bool>())
                .Returns(response);
            _mapper.Map<List<TestSupplierItem>>(Arg.Any<object>()).Returns(new List<TestSupplierItem>());

            var result = await _controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<TestSupplierViewModel>(viewResult.Model);
            Assert.NotNull(model.TestSupplierGrid);
        }

        [Fact]
        public async Task Index_WhenTestorProductServiceFails_ReturnsViewWithEmptyDropdown()
        {
            var failureResponse = ApiResponseDto<List<TestorProductDto>>.FailureResponse(
                new List<ApiErrorDto> { new() { Code = "500", Message = "Error" } }, new ApiMetaDto());
            _testorProductService.GetAllTestorProductsAsync().Returns(failureResponse);

            var result = await _controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<TestSupplierViewModel>(viewResult.Model);
            Assert.Empty(model.TestCodeOptions);
        }

        [Fact]
        public async Task Index_GridConfig_HasCorrectGridId()
        {
            var productsResponse = ApiResponseDto<List<TestorProductDto>>.SuccessResponse(new List<TestorProductDto>());
            _testorProductService.GetAllTestorProductsAsync().Returns(productsResponse);

            var result = await _controller.Index();

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<TestSupplierViewModel>(viewResult.Model);
            Assert.Equal("testSupplierGrid", model.TestSupplierGrid.GridId);
        }

        #endregion

        #region LoadTestSupplierGrid

        [Fact]
        public async Task LoadTestSupplierGrid_WithValidRequest_ReturnsPartialView()
        {
            var request = new PaginationFilter<string> { Filter = "{}", Page = 1, PageSize = 10 };
            var pagedItems = new List<TestSupplierViewDto> { new() { TestCode = DefaultTestCode, JobCode = DefaultBuyer } };
            var response = ApiResponseDto<List<TestSupplierViewDto>>.SuccessResponse(pagedItems);

            _mapper.Map<QueryParameters<string>>(request).Returns(new QueryParameters<string> { Page = 1, PageSize = 10 });
            _testSupplierService.GetPagedAsync(Arg.Any<QueryParameters<string>>(), DefaultTestCode, false).Returns(response);
            _mapper.Map<List<TestSupplierItem>>(pagedItems).Returns(new List<TestSupplierItem> { new() { TestCode = DefaultTestCode } });

            var result = await _controller.LoadTestSupplierGrid(request, DefaultTestCode, false);

            Assert.IsType<PartialViewResult>(result);
        }

        [Fact]
        public async Task LoadTestSupplierGrid_WithInvalidModelState_ReturnsFailureJson()
        {
            _controller.ModelState.AddModelError("PageSize", "Invalid page size.");
            var request = new PaginationFilter<string> { Filter = "{}" };

            var result = await _controller.LoadTestSupplierGrid(request);

            var jsonResult = Assert.IsType<JsonResult>(result);
            var value = GetJsonResultElement(jsonResult);
            Assert.False(value.GetProperty("success").GetBoolean());
            Assert.Equal("Invalid request data", value.GetProperty("message").GetString());
        }

        [Fact]
        public async Task LoadTestSupplierGrid_WhenServiceFails_ReturnsEmptyGridPartialView()
        {
            var request = new PaginationFilter<string> { Filter = "{}", Page = 1, PageSize = 10 };
            var failureResponse = ApiResponseDto<List<TestSupplierViewDto>>.FailureResponse(
                new List<ApiErrorDto> { new() { Code = "500", Message = "Error" } }, new ApiMetaDto());

            _mapper.Map<QueryParameters<string>>(request).Returns(new QueryParameters<string>());
            _testSupplierService.GetPagedAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<string>(), Arg.Any<bool>())
                .Returns(failureResponse);

            var result = await _controller.LoadTestSupplierGrid(request, DefaultTestCode, false);

            Assert.IsType<PartialViewResult>(result);
        }

        [Fact]
        public async Task LoadTestSupplierGrid_WithNullTestCode_UsesEmptyString()
        {
            var request = new PaginationFilter<string> { Filter = "{}", Page = 1, PageSize = 10 };
            var response = ApiResponseDto<List<TestSupplierViewDto>>.SuccessResponse(new List<TestSupplierViewDto>());

            _mapper.Map<QueryParameters<string>>(request).Returns(new QueryParameters<string>());
            _testSupplierService.GetPagedAsync(Arg.Any<QueryParameters<string>>(), string.Empty, false).Returns(response);
            _mapper.Map<List<TestSupplierItem>>(Arg.Any<object>()).Returns(new List<TestSupplierItem>());

            var result = await _controller.LoadTestSupplierGrid(request, null, false);

            Assert.IsType<PartialViewResult>(result);
            await _testSupplierService.Received(1).GetPagedAsync(Arg.Any<QueryParameters<string>>(), string.Empty, false);
        }

        #endregion
    }
}
