using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.PactApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Application.Services.PACT;
using NSubstitute;
using Xunit;

namespace Apha.FPSApps.Application.UnitTests.Services.PACT.WorkGroupTestCapabilityServiceTest
{
    public class WorkGroupTestCapabilityServiceTests
    {
        private readonly IPactApiClient _pactClient;
        private readonly IPactWorkGroupTestCapabilityApiClient _apiClient;
        private readonly IPactWorkGroupApiClient _workGroupApiClient;
        private readonly WorkGroupTestCapabilityService _service;

        public WorkGroupTestCapabilityServiceTests()
        {
            _pactClient = Substitute.For<IPactApiClient>();
            _apiClient = Substitute.For<IPactWorkGroupTestCapabilityApiClient>();
            _workGroupApiClient = Substitute.For<IPactWorkGroupApiClient>();
            _pactClient.PactWorkGroupTestCapability.Returns(_apiClient);
            _pactClient.PactWorkGroup.Returns(_workGroupApiClient);
            _service = new WorkGroupTestCapabilityService(_pactClient);
        }

        #region GetPagedByWorkGroupAsync

        [Fact]
        public async Task GetPagedByWorkGroupAsync_DelegatesToApiClient_ReturnsResult()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var expected = ApiResponseDto<List<TestCapabilityDto>>.SuccessResponse([new TestCapabilityDto { TestCode = "TC1" }]);
            _apiClient.GetPagedByWorkGroupAsync(query, "WG1").Returns(expected);

            var result = await _service.GetPagedByWorkGroupAsync(query, "WG1");

            Assert.Equal(expected, result);
            await _apiClient.Received(1).GetPagedByWorkGroupAsync(query, "WG1");
        }

        [Fact]
        public async Task GetPagedByWorkGroupAsync_WithNullWorkGroup_PassesNullToClient()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 5 };
            var expected = ApiResponseDto<List<TestCapabilityDto>>.SuccessResponse([]);
            _apiClient.GetPagedByWorkGroupAsync(query, null).Returns(expected);

            var result = await _service.GetPagedByWorkGroupAsync(query, null);

            Assert.Equal(expected, result);
            await _apiClient.Received(1).GetPagedByWorkGroupAsync(query, null);
        }

        #endregion

        #region GetPagedByTestCodeAsync

        [Fact]
        public async Task GetPagedByTestCodeAsync_DelegatesToApiClient_ReturnsResult()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var expected = ApiResponseDto<List<TestCapabilityDto>>.SuccessResponse([new TestCapabilityDto { TestCode = "TC1" }]);
            _apiClient.GetPagedByTestCodeAsync(query, "TC1").Returns(expected);

            var result = await _service.GetPagedByTestCodeAsync(query, "TC1");

            Assert.Equal(expected, result);
            await _apiClient.Received(1).GetPagedByTestCodeAsync(query, "TC1");
        }

        [Fact]
        public async Task GetPagedByTestCodeAsync_WithNullTestCode_PassesNullToClient()
        {
            var query = new QueryParameters<string>();
            var expected = ApiResponseDto<List<TestCapabilityDto>>.SuccessResponse([]);
            _apiClient.GetPagedByTestCodeAsync(query, null).Returns(expected);

            var result = await _service.GetPagedByTestCodeAsync(query, null);

            Assert.Equal(expected, result);
        }

        #endregion

        #region GetTestCapabilityByIdAsync

        [Fact]
        public async Task GetTestCapabilityByIdAsync_DelegatesToApiClient_ReturnsResult()
        {
            var expected = ApiResponseDto<TestCapabilityDto>.SuccessResponse(new TestCapabilityDto { TestCode = "TC1", WorkGroup = "WG1" });
            _apiClient.GetTestCapabilityByIdAsync("TC1", "WG1").Returns(expected);

            var result = await _service.GetTestCapabilityByIdAsync("TC1", "WG1");

            Assert.Equal(expected, result);
            await _apiClient.Received(1).GetTestCapabilityByIdAsync("TC1", "WG1");
        }

        [Fact]
        public async Task GetTestCapabilityByIdAsync_WhenNotFound_ReturnsFailureResponse()
        {
            var errors = new List<ApiErrorDto> { new() { Code = "NOT_FOUND", Message = "Not found" } };
            var expected = ApiResponseDto<TestCapabilityDto>.FailureResponse(errors, new ApiMetaDto());
            _apiClient.GetTestCapabilityByIdAsync("MISSING", "WG1").Returns(expected);

            var result = await _service.GetTestCapabilityByIdAsync("MISSING", "WG1");

            Assert.False(result.Success);
            Assert.Equal(expected, result);
        }

        #endregion

        #region CreateTestCapabilityAsync

        [Fact]
        public async Task CreateTestCapabilityAsync_DelegatesToApiClient_ReturnsCreatedDto()
        {
            var dto = new TestCapabilityDto { TestCode = "TC1", WorkGroup = "WG1" };
            var expected = ApiResponseDto<TestCapabilityDto>.SuccessResponse(dto);
            _apiClient.CreateTestCapabilityAsync(dto).Returns(expected);

            var result = await _service.CreateTestCapabilityAsync(dto);

            Assert.Equal(expected, result);
            await _apiClient.Received(1).CreateTestCapabilityAsync(dto);
        }

        [Fact]
        public async Task CreateTestCapabilityAsync_WhenApiFails_ReturnsFailureResponse()
        {
            var dto = new TestCapabilityDto { TestCode = "TC1", WorkGroup = "WG1" };
            var errors = new List<ApiErrorDto> { new() { Code = "CONFLICT", Message = "Already exists" } };
            var expected = ApiResponseDto<TestCapabilityDto>.FailureResponse(errors, new ApiMetaDto());
            _apiClient.CreateTestCapabilityAsync(dto).Returns(expected);

            var result = await _service.CreateTestCapabilityAsync(dto);

            Assert.False(result.Success);
        }

        #endregion

        #region UpdateTestCapabilityAsync

        [Fact]
        public async Task UpdateTestCapabilityAsync_DelegatesToApiClient_ReturnsUpdatedDto()
        {
            var dto = new TestCapabilityDto { TestCode = "TC1", WorkGroup = "WG1" };
            var expected = ApiResponseDto<TestCapabilityDto>.SuccessResponse(dto);
            _apiClient.UpdateTestCapabilityAsync(dto).Returns(expected);

            var result = await _service.UpdateTestCapabilityAsync(dto);

            Assert.Equal(expected, result);
            await _apiClient.Received(1).UpdateTestCapabilityAsync(dto);
        }

        #endregion

        #region DeleteTestCapabilityAsync

        [Fact]
        public async Task DeleteTestCapabilityAsync_DelegatesToApiClient_ReturnsTrue()
        {
            var expected = ApiResponseDto<bool>.SuccessResponse(true);
            _apiClient.DeleteTestCapabilityAsync("TC1", "WG1").Returns(expected);

            var result = await _service.DeleteTestCapabilityAsync("TC1", "WG1");

            Assert.Equal(expected, result);
            await _apiClient.Received(1).DeleteTestCapabilityAsync("TC1", "WG1");
        }

        [Fact]
        public async Task DeleteTestCapabilityAsync_WhenApiFails_ReturnsFailureResponse()
        {
            var errors = new List<ApiErrorDto> { new() { Code = "NOT_FOUND", Message = "Not found" } };
            var expected = ApiResponseDto<bool>.FailureResponse(errors, new ApiMetaDto());
            _apiClient.DeleteTestCapabilityAsync("MISSING", "WG1").Returns(expected);

            var result = await _service.DeleteTestCapabilityAsync("MISSING", "WG1");

            Assert.False(result.Success);
        }

        #endregion

        #region GetPagedTestReqmtAsync

        [Fact]
        public async Task GetPagedTestReqmtAsync_DelegatesToApiClient_ReturnsResult()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var expected = ApiResponseDto<List<TestReqmtDto>>.SuccessResponse([new TestReqmtDto { TestCode = "BLOOD", Buyer = "PRJ1" }]);
            _apiClient.GetPagedTestReqmtAsync(query, "BLOOD").Returns(expected);

            var result = await _service.GetPagedTestReqmtAsync(query, "BLOOD");

            Assert.Equal(expected, result);
            await _apiClient.Received(1).GetPagedTestReqmtAsync(query, "BLOOD");
        }

        #endregion

        #region GetAllTestReqmtForExportAsync

        [Fact]
        public async Task GetAllTestReqmtForExportAsync_DelegatesToApiClient_ReturnsResult()
        {
            var expected = ApiResponseDto<List<TestReqmtDto>>.SuccessResponse([new TestReqmtDto { TestCode = "BLOOD" }]);
            _apiClient.GetAllTestReqmtForExportAsync("BLOOD", "{}").Returns(expected);

            var result = await _service.GetAllTestReqmtForExportAsync("BLOOD", "{}");

            Assert.Equal(expected, result);
            await _apiClient.Received(1).GetAllTestReqmtForExportAsync("BLOOD", "{}");
        }

        [Fact]
        public async Task GetAllTestReqmtForExportAsync_WithNullFilter_PassesNullToClient()
        {
            var expected = ApiResponseDto<List<TestReqmtDto>>.SuccessResponse([]);
            _apiClient.GetAllTestReqmtForExportAsync("BLOOD", null).Returns(expected);

            var result = await _service.GetAllTestReqmtForExportAsync("BLOOD", null);

            Assert.Equal(expected, result);
            await _apiClient.Received(1).GetAllTestReqmtForExportAsync("BLOOD", null);
        }

        #endregion

        #region GetTestReqmtByIdAsync

        [Fact]
        public async Task GetTestReqmtByIdAsync_DelegatesToApiClient_ReturnsResult()
        {
            var expected = ApiResponseDto<TestReqmtDto>.SuccessResponse(new TestReqmtDto { TestCode = "BLOOD", Buyer = "PRJ1" });
            _apiClient.GetTestReqmtByIdAsync("BLOOD", "PRJ1").Returns(expected);

            var result = await _service.GetTestReqmtByIdAsync("BLOOD", "PRJ1");

            Assert.Equal(expected, result);
            await _apiClient.Received(1).GetTestReqmtByIdAsync("BLOOD", "PRJ1");
        }

        [Fact]
        public async Task GetTestReqmtByIdAsync_WhenNotFound_ReturnsFailureResponse()
        {
            var errors = new List<ApiErrorDto> { new() { Code = "NOT_FOUND" } };
            var expected = ApiResponseDto<TestReqmtDto>.FailureResponse(errors, new ApiMetaDto());
            _apiClient.GetTestReqmtByIdAsync("MISSING", "PRJ1").Returns(expected);

            var result = await _service.GetTestReqmtByIdAsync("MISSING", "PRJ1");

            Assert.False(result.Success);
        }

        #endregion

        #region CreateTestReqmtAsync

        [Fact]
        public async Task CreateTestReqmtAsync_DelegatesToApiClient_ReturnsCreatedDto()
        {
            var dto = new TestReqmtDto { TestCode = "BLOOD", Buyer = "PRJ1" };
            var expected = ApiResponseDto<TestReqmtDto>.SuccessResponse(dto);
            _apiClient.CreateTestReqmtAsync(dto).Returns(expected);

            var result = await _service.CreateTestReqmtAsync(dto);

            Assert.Equal(expected, result);
            await _apiClient.Received(1).CreateTestReqmtAsync(dto);
        }

        [Fact]
        public async Task CreateTestReqmtAsync_WhenApiFails_ReturnsFailureResponse()
        {
            var dto = new TestReqmtDto { TestCode = "BLOOD", Buyer = "PRJ1" };
            var errors = new List<ApiErrorDto> { new() { Code = "CONFLICT" } };
            var expected = ApiResponseDto<TestReqmtDto>.FailureResponse(errors, new ApiMetaDto());
            _apiClient.CreateTestReqmtAsync(dto).Returns(expected);

            var result = await _service.CreateTestReqmtAsync(dto);

            Assert.False(result.Success);
        }

        #endregion

        #region UpdateTestReqmtAsync

        [Fact]
        public async Task UpdateTestReqmtAsync_DelegatesToApiClient_ReturnsUpdatedDto()
        {
            var dto = new TestReqmtDto { TestCode = "BLOOD", Buyer = "PRJ1" };
            var expected = ApiResponseDto<TestReqmtDto>.SuccessResponse(dto);
            _apiClient.UpdateTestReqmtAsync(dto).Returns(expected);

            var result = await _service.UpdateTestReqmtAsync(dto);

            Assert.Equal(expected, result);
            await _apiClient.Received(1).UpdateTestReqmtAsync(dto);
        }

        #endregion

        #region DeleteTestReqmtAsync

        [Fact]
        public async Task DeleteTestReqmtAsync_DelegatesToApiClient_ReturnsTrue()
        {
            var expected = ApiResponseDto<bool>.SuccessResponse(true);
            _apiClient.DeleteTestReqmtAsync("BLOOD", "PRJ1").Returns(expected);

            var result = await _service.DeleteTestReqmtAsync("BLOOD", "PRJ1");

            Assert.Equal(expected, result);
            await _apiClient.Received(1).DeleteTestReqmtAsync("BLOOD", "PRJ1");
        }

        [Fact]
        public async Task DeleteTestReqmtAsync_WhenApiFails_ReturnsFailureResponse()
        {
            var errors = new List<ApiErrorDto> { new() { Code = "NOT_FOUND" } };
            var expected = ApiResponseDto<bool>.FailureResponse(errors, new ApiMetaDto());
            _apiClient.DeleteTestReqmtAsync("MISSING", "PRJ1").Returns(expected);

            var result = await _service.DeleteTestReqmtAsync("MISSING", "PRJ1");

            Assert.False(result.Success);
        }

        #endregion

        #region GetAllTestorProductsAsync

        [Fact]
        public async Task GetAllTestorProductsAsync_DelegatesToApiClient_ReturnsResult()
        {
            var expected = ApiResponseDto<List<TestorProductDto>>.SuccessResponse(
            [
                new TestorProductDto { ItemCode = "BLOOD" },
                new TestorProductDto { ItemCode = "URINE" }
            ]);
            _apiClient.GetAllTestorProductsAsync().Returns(expected);

            var result = await _service.GetAllTestorProductsAsync();

            Assert.Equal(expected, result);
            await _apiClient.Received(1).GetAllTestorProductsAsync();
        }

        [Fact]
        public async Task GetAllTestorProductsAsync_WhenEmpty_ReturnsEmptyList()
        {
            var expected = ApiResponseDto<List<TestorProductDto>>.SuccessResponse([]);
            _apiClient.GetAllTestorProductsAsync().Returns(expected);

            var result = await _service.GetAllTestorProductsAsync();

            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        #endregion

        #region GetAllWorkGroupsAsync

        [Fact]
        public async Task GetAllWorkGroupsAsync_DelegatesToPactWorkGroupClient_ReturnsResult()
        {
            var expected = ApiResponseDto<List<WorkGroupDto>>.SuccessResponse(
            [
                new WorkGroupDto { WorkGroupName = "Work Group 1" },
                new WorkGroupDto { WorkGroupName = "Work Group 2" }
            ]);
            _workGroupApiClient.GetAllWorkGroupsAsync().Returns(expected);

            var result = await _service.GetAllWorkGroupsAsync();

            Assert.Equal(expected, result);
            await _workGroupApiClient.Received(1).GetAllWorkGroupsAsync();
            await _apiClient.DidNotReceive().GetAllTestorProductsAsync();
        }

        [Fact]
        public async Task GetAllWorkGroupsAsync_WhenApiFails_ReturnsFailureResponse()
        {
            var errors = new List<ApiErrorDto> { new() { Code = "INTERNAL_ERROR" } };
            var expected = ApiResponseDto<List<WorkGroupDto>>.FailureResponse(errors, new ApiMetaDto());
            _workGroupApiClient.GetAllWorkGroupsAsync().Returns(expected);

            var result = await _service.GetAllWorkGroupsAsync();

            Assert.False(result.Success);
        }

        #endregion

        #region GetTestReqmtPricingAsync

        [Fact]
        public async Task GetTestReqmtPricingAsync_WithTestCodeOnly_DelegatesToApiClient()
        {
            var expected = ApiResponseDto<TestReqmtDto>.SuccessResponse(new TestReqmtDto { TestCode = "BLOOD", RecUnitPrice = 10.5m });
            _apiClient.GetTestReqmtPricingAsync("BLOOD", null).Returns(expected);

            var result = await _service.GetTestReqmtPricingAsync("BLOOD");

            Assert.Equal(expected, result);
            await _apiClient.Received(1).GetTestReqmtPricingAsync("BLOOD", null);
        }

        [Fact]
        public async Task GetTestReqmtPricingAsync_WithProjectCode_PassesProjectCodeToClient()
        {
            var expected = ApiResponseDto<TestReqmtDto>.SuccessResponse(new TestReqmtDto { TestCode = "BLOOD", RecUnitPrice = 5.0m });
            _apiClient.GetTestReqmtPricingAsync("BLOOD", "PRJ1").Returns(expected);

            var result = await _service.GetTestReqmtPricingAsync("BLOOD", "PRJ1");

            Assert.Equal(expected, result);
            await _apiClient.Received(1).GetTestReqmtPricingAsync("BLOOD", "PRJ1");
        }

        [Fact]
        public async Task GetTestReqmtPricingAsync_WhenNotFound_ReturnsFailureResponse()
        {
            var errors = new List<ApiErrorDto> { new() { Code = "NOT_FOUND" } };
            var expected = ApiResponseDto<TestReqmtDto>.FailureResponse(errors, new ApiMetaDto());
            _apiClient.GetTestReqmtPricingAsync("MISSING", null).Returns(expected);

            var result = await _service.GetTestReqmtPricingAsync("MISSING");

            Assert.False(result.Success);
        }

        #endregion
    }
}
