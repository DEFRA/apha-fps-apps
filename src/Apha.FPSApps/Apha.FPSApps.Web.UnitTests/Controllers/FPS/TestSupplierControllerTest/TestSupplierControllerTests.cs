using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.PACT;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Web.Areas.FPS.Controllers;
using Apha.FPSApps.Web.Areas.FPS.Models;
using Apha.FPSApps.Web.Models.Components.DataGrid;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using System.Text.Json;

namespace Apha.FPSApps.Web.UnitTests.Controllers.FPS.TestSupplierControllerTest
{
    public class TestSupplierControllerTests
    {
        private const string DefaultTestCode = "TST001";
        private const string DefaultBuyer = "B001";

        private readonly IMapper _mapper;
        private readonly ITestRequirementService _testReqmtService;
        private readonly ITestorProductService _testorProductService;
        private readonly TestSupplierController _controller;

        public TestSupplierControllerTests()
        {
            _mapper = Substitute.For<IMapper>();
            _testReqmtService = Substitute.For<ITestRequirementService>();
            _testorProductService = Substitute.For<ITestorProductService>();
            _controller = new TestSupplierController(
                _mapper, _testReqmtService, _testorProductService);
        }

        private static T? GetJsonValue<T>(JsonResult jsonResult)
        {
            var json = JsonSerializer.Serialize(jsonResult.Value);
            return JsonSerializer.Deserialize<T>(json);
        }

        private static ApiResponseDto<List<TestorProductDto>> BuildTestorProductResponse(int count = 2) =>
            ApiResponseDto<List<TestorProductDto>>.SuccessResponse(
                Enumerable.Range(1, count).Select(i => new TestorProductDto
                {
                    ItemCode = $"TC{i:D3}",
                    ItemDescription = $"Test {i}"
                }).ToList());

        private static ApiResponseDto<List<TestSupplierViewDto>> BuildViewResponse(int count = 2) =>
            ApiResponseDto<List<TestSupplierViewDto>>.SuccessResponse(
                Enumerable.Range(1, count).Select(i => new TestSupplierViewDto
                {
                    TestCode = DefaultTestCode,
                    Buyer = $"B{i:D3}"
                }).ToList(),
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = count });

        private void SetupDefaultGridDependencies()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var viewResponse = BuildViewResponse();

            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>()).Returns(query);
            _testReqmtService.GetPagedBySupplierTestCodeAsync(query, Arg.Any<string>(), Arg.Any<bool>())
                .Returns(viewResponse);
            _mapper.Map<List<TestSupplierItem>>(Arg.Any<List<TestSupplierViewDto>>())
                .Returns(new List<TestSupplierItem>());
            _mapper.Map<PaginationModel>(Arg.Any<PaginationDto>())
                .Returns(new PaginationModel { PageNumber = 1, PageSize = 10 });
        }

        #region Index Tests

        [Fact]
        public async Task Index_WithTestorProducts_ReturnsViewWithViewModel()
        {
            // Arrange
            var productsResponse = BuildTestorProductResponse();
            _testorProductService.GetAllTestorProductsAsync().Returns(productsResponse);
            SetupDefaultGridDependencies();

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<TestSupplierViewModel>(viewResult.Model);
            Assert.NotNull(model);
            Assert.Equal(2, model.TestCodeList.Count);
        }

        [Fact]
        public async Task Index_TestorProductServiceFails_ReturnsViewWithEmptyTestCodeList()
        {
            // Arrange
            var failedResponse = new ApiResponseDto<List<TestorProductDto>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "Service error" } }
            };
            _testorProductService.GetAllTestorProductsAsync().Returns(failedResponse);
            SetupDefaultGridDependencies();

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<TestSupplierViewModel>(viewResult.Model);
            Assert.Empty(model.TestCodeList);
        }

        [Fact]
        public async Task Index_CallsGetAllTestorProductsAsync()
        {
            // Arrange
            _testorProductService.GetAllTestorProductsAsync()
                .Returns(BuildTestorProductResponse());
            SetupDefaultGridDependencies();

            // Act
            await _controller.Index();

            // Assert
            await _testorProductService.Received(1).GetAllTestorProductsAsync();
        }

        #endregion

        #region LoadTestSupplierGrid Tests

        [Fact]
        public async Task LoadTestSupplierGrid_ValidRequest_ReturnsPartialView()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}", Page = 1, PageSize = 10 };
            SetupDefaultGridDependencies();

            // Act
            var result = await _controller.LoadTestSupplierGrid(request, DefaultTestCode, showRejected: false);

            // Assert
            Assert.IsType<PartialViewResult>(result);
        }

        [Fact]
        public async Task LoadTestSupplierGrid_CallsTestSupplierService()
        {
            // Arrange
            var request = new PaginationFilter<string> { Filter = "{}", Page = 1, PageSize = 10 };
            SetupDefaultGridDependencies();

            // Act
            await _controller.LoadTestSupplierGrid(request, DefaultTestCode, showRejected: false);

            // Assert
            await _testReqmtService.Received(1)
                .GetPagedBySupplierTestCodeAsync(Arg.Any<QueryParameters<string>>(), DefaultTestCode, false);
        }

        #endregion

        #region EditTestReqmt GET Tests

        [Fact]
        public async Task EditTestReqmtGet_WithValidTestCodeAndBuyer_ReturnsPartialView()
        {
            // Arrange
            var dto = new TestRequirementDto
            {
                TestCode = DefaultTestCode, Buyer = DefaultBuyer, UnitPrice = 50m, NoRequired = 3
            };
            var serviceResult = ApiResponseDto<TestRequirementDto>.SuccessResponse(dto);
            var viewResponse = BuildViewResponse();

            _testorProductService.GetAllTestorProductsAsync()
                .Returns(BuildTestorProductResponse());
            _testReqmtService.GetTestReqmtByIdAsync(DefaultTestCode, DefaultBuyer).Returns(serviceResult);
            _mapper.Map<TestSupplierItem>(dto).Returns(new TestSupplierItem
            {
                TestCode = DefaultTestCode, Buyer = DefaultBuyer
            });
            var query = new QueryParameters<string> { Page = 1, PageSize = 100 };
            _mapper.Map<QueryParameters<string>>(Arg.Any<PaginationFilter<string>>()).Returns(query);
            _testReqmtService.GetPagedBySupplierTestCodeAsync(Arg.Any<QueryParameters<string>>(), DefaultTestCode, Arg.Any<bool>())
                .Returns(viewResponse);

            // Act
            var result = await _controller.EditTestReqmt(DefaultTestCode, DefaultBuyer);

            // Assert
            Assert.IsType<PartialViewResult>(result);
        }

        [Fact]
        public async Task EditTestReqmtGet_ServiceFails_ReturnsNotFound()
        {
            // Arrange
            var failureResult = new ApiResponseDto<TestRequirementDto>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "Not found" } }
            };
            _testorProductService.GetAllTestorProductsAsync()
                .Returns(BuildTestorProductResponse());
            _testReqmtService.GetTestReqmtByIdAsync(DefaultTestCode, DefaultBuyer).Returns(failureResult);

            // Act
            var result = await _controller.EditTestReqmt(DefaultTestCode, DefaultBuyer);

            // Assert — controller returns NotFound when the service call fails
            Assert.IsType<NotFoundObjectResult>(result);
        }

        #endregion

        #region EditTestReqmt POST Tests

        [Fact]
        public async Task EditTestReqmtPost_ValidModel_ReturnsJsonSuccess()
        {
            // Arrange
            var model = new TestSupplierItem
            {
                TestCode = DefaultTestCode, Buyer = DefaultBuyer, UnitPrice = 50m
            };
            var dto = new TestRequirementDto { TestCode = DefaultTestCode, Buyer = DefaultBuyer };
            var updateResult = ApiResponseDto<TestRequirementDto>.SuccessResponse(dto);

            _mapper.Map<TestRequirementDto>(model).Returns(dto);
            _testReqmtService.UpdateTestReqmtAsync(dto).Returns(updateResult);

            // Act
            var result = await _controller.EditTestReqmt(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var json = JsonSerializer.Serialize(jsonResult.Value);
            Assert.Contains("true", json);
        }

        [Fact]
        public async Task EditTestReqmtPost_ServiceFails_ReturnsJsonFailure()
        {
            // Arrange
            var model = new TestSupplierItem { TestCode = DefaultTestCode, Buyer = DefaultBuyer };
            var dto = new TestRequirementDto { TestCode = DefaultTestCode, Buyer = DefaultBuyer };
            var failureResult = new ApiResponseDto<TestRequirementDto>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Code = "ERR", Message = "Update failed" } }
            };

            _mapper.Map<TestRequirementDto>(model).Returns(dto);
            _testReqmtService.UpdateTestReqmtAsync(dto).Returns(failureResult);

            // Act
            var result = await _controller.EditTestReqmt(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var json = JsonSerializer.Serialize(jsonResult.Value);
            Assert.Contains("false", json);
        }

        #endregion

        #region DeleteTestReqmt Tests

        [Fact]
        public async Task DeleteTestReqmt_ValidRequest_ReturnsJsonSuccess()
        {
            // Arrange
            var deleteResult = ApiResponseDto<bool>.SuccessResponse(true);
            _testReqmtService.DeleteTestReqmtAsync(DefaultTestCode, DefaultBuyer).Returns(deleteResult);

            // Act
            var result = await _controller.DeleteTestReqmt(DefaultTestCode, DefaultBuyer);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var json = JsonSerializer.Serialize(jsonResult.Value);
            Assert.Contains("true", json);
            await _testReqmtService.Received(1).DeleteTestReqmtAsync(DefaultTestCode, DefaultBuyer);
        }

        [Fact]
        public async Task DeleteTestReqmt_ServiceFails_ReturnsJsonFailure()
        {
            // Arrange
            var failureResult = new ApiResponseDto<bool>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Code = "ERR", Message = "Delete failed" } }
            };
            _testReqmtService.DeleteTestReqmtAsync(DefaultTestCode, DefaultBuyer).Returns(failureResult);

            // Act
            var result = await _controller.DeleteTestReqmt(DefaultTestCode, DefaultBuyer);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var json = JsonSerializer.Serialize(jsonResult.Value);
            Assert.Contains("false", json);
        }

        #endregion

        #region GetTestReqmtPricing Tests

        [Fact]
        public async Task GetTestReqmtPricing_EmptyTestCode_ReturnsJsonFailure()
        {
            // Act
            var result = await _controller.GetTestReqmtPricing(string.Empty);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var json = JsonSerializer.Serialize(jsonResult.Value);
            Assert.Contains("false", json);
        }

        [Fact]
        public async Task GetTestReqmtPricing_ValidTestCode_ReturnsJsonWithRecUnitPrice()
        {
            // Arrange
            var pricingDto = new TestRequirementDto
            {
                TestCode = DefaultTestCode, RecUnitPrice = 25m
            };
            var pricingResult = ApiResponseDto<TestRequirementDto>.SuccessResponse(pricingDto);
            _testReqmtService.GetTestReqmtPricingAsync(DefaultTestCode, null).Returns(pricingResult);

            // Act
            var result = await _controller.GetTestReqmtPricing(DefaultTestCode);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var json = JsonSerializer.Serialize(jsonResult.Value);
            Assert.Contains("true", json);
        }

        [Fact]
        public async Task GetTestReqmtPricing_ServiceFails_ReturnsJsonFailure()
        {
            // Arrange
            var failureResult = new ApiResponseDto<TestRequirementDto>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "Not found" } }
            };
            _testReqmtService.GetTestReqmtPricingAsync(DefaultTestCode, null).Returns(failureResult);

            // Act
            var result = await _controller.GetTestReqmtPricing(DefaultTestCode);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var json = JsonSerializer.Serialize(jsonResult.Value);
            Assert.Contains("false", json);
        }

        #endregion
    }
}
