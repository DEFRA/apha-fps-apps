using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.PactApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Application.Services.PACT;
using NSubstitute;
using Xunit;

namespace Apha.FPSApps.Application.UnitTests.Services.PACT.TestListServiceTest
{
    public class TestListServiceTests
    {
        private readonly IPactApiClient _pactClient;
        private readonly IPactTestorProductApiClient _pactTestListApiClient;
        private readonly TestorProductService _service;

        public TestListServiceTests()
        {
            _pactClient = Substitute.For<IPactApiClient>();
            _pactTestListApiClient = Substitute.For<IPactTestorProductApiClient>();
            _pactClient.PactTestList.Returns(_pactTestListApiClient);
            _service = new TestorProductService(_pactClient);
        }

        #region GetPagedTestOrProductsAsync

        [Fact]
        public async Task GetPagedTestOrProductsAsync_WithValidQuery_ReturnsPaginatedTestOrProducts()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10, Search = "TEST" };
            var testItems = new List<TestorProductDto>
            {
                new() { ItemCode = "T001", ItemDescription = "Test One" },
                new() { ItemCode = "T002", ItemDescription = "Test Two" }
            };
            var expectedResponse = ApiResponseDto<List<TestorProductDto>>.SuccessResponse(testItems);
            _pactTestListApiClient.GetPagedTestOrProductsAsync(query).Returns(Task.FromResult(expectedResponse));

            // Act
            var result = await _service.GetPagedTestOrProductsAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _pactTestListApiClient.Received(1).GetPagedTestOrProductsAsync(query);
        }

        [Fact]
        public async Task GetPagedTestOrProductsAsync_WithEmptyResult_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var expectedResponse = ApiResponseDto<List<TestorProductDto>>.SuccessResponse(new List<TestorProductDto>());
            _pactTestListApiClient.GetPagedTestOrProductsAsync(query).Returns(Task.FromResult(expectedResponse));

            // Act
            var result = await _service.GetPagedTestOrProductsAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetPagedTestOrProductsAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiErrorDto> { new() { Message = "API Error", Code = "API_ERROR" } };
            var expectedResponse = ApiResponseDto<List<TestorProductDto>>.FailureResponse(errors, new ApiMetaDto());
            _pactTestListApiClient.GetPagedTestOrProductsAsync(query).Returns(Task.FromResult(expectedResponse));

            // Act
            var result = await _service.GetPagedTestOrProductsAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
        }

        #endregion

        #region GetTestOrProductByIdAsync

        [Fact]
        public async Task GetTestOrProductByIdAsync_WithValidItemCode_ReturnsTestOrProduct()
        {
            // Arrange
            var itemCode = "T001";
            var testOrProduct = new TestorProductDto { ItemCode = itemCode, ItemDescription = "Test Product" };
            var expectedResponse = ApiResponseDto<TestorProductDto>.SuccessResponse(testOrProduct);
            _pactTestListApiClient.GetTestOrProductByIdAsync(itemCode).Returns(Task.FromResult(expectedResponse));

            // Act
            var result = await _service.GetTestOrProductByIdAsync(itemCode);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(itemCode, result.Data?.ItemCode);
            await _pactTestListApiClient.Received(1).GetTestOrProductByIdAsync(itemCode);
        }

        [Fact]
        public async Task GetTestOrProductByIdAsync_WithNonExistentItemCode_ReturnsFailure()
        {
            // Arrange
            var itemCode = "NOTFOUND";
            var errors = new List<ApiErrorDto> { new() { Message = "Not Found", Code = "NOT_FOUND" } };
            var expectedResponse = ApiResponseDto<TestorProductDto>.FailureResponse(errors, new ApiMetaDto());
            _pactTestListApiClient.GetTestOrProductByIdAsync(itemCode).Returns(Task.FromResult(expectedResponse));

            // Act
            var result = await _service.GetTestOrProductByIdAsync(itemCode);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        [Fact]
        public async Task GetTestOrProductByIdAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var itemCode = "T001";
            var errors = new List<ApiErrorDto> { new() { Message = "Service error", Code = "SERVICE_ERROR" } };
            var expectedResponse = ApiResponseDto<TestorProductDto>.FailureResponse(errors, new ApiMetaDto());
            _pactTestListApiClient.GetTestOrProductByIdAsync(itemCode).Returns(Task.FromResult(expectedResponse));

            // Act
            var result = await _service.GetTestOrProductByIdAsync(itemCode);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        #endregion

        #region CreateTestOrProductAsync

        [Fact]
        public async Task CreateTestOrProductAsync_WithValidRequest_ReturnsCreatedTestOrProduct()
        {
            // Arrange
            var request = new TestorProductDto { ItemCode = "T001", ItemDescription = "New Test" };
            var createdTestOrProduct = new TestorProductDto { ItemCode = "T001", ItemDescription = "New Test" };
            var expectedResponse = ApiResponseDto<TestorProductDto>.SuccessResponse(createdTestOrProduct);
            _pactTestListApiClient.CreateTestOrProductAsync(request).Returns(Task.FromResult(expectedResponse));

            // Act
            var result = await _service.CreateTestOrProductAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal("T001", result.Data?.ItemCode);
            await _pactTestListApiClient.Received(1).CreateTestOrProductAsync(request);
        }

        [Fact]
        public async Task CreateTestOrProductAsync_WhenValidationFails_ReturnsFailureResponse()
        {
            // Arrange
            var request = new TestorProductDto { ItemCode = "T001" };
            var errors = new List<ApiErrorDto> { new() { Message = "Validation failed", Code = "VALIDATION_ERROR" } };
            var expectedResponse = ApiResponseDto<TestorProductDto>.FailureResponse(errors, new ApiMetaDto());
            _pactTestListApiClient.CreateTestOrProductAsync(request).Returns(Task.FromResult(expectedResponse));

            // Act
            var result = await _service.CreateTestOrProductAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        [Fact]
        public async Task CreateTestOrProductAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var request = new TestorProductDto { ItemCode = "T001" };
            var errors = new List<ApiErrorDto> { new() { Message = "Internal error", Code = "INTERNAL_ERROR" } };
            var expectedResponse = ApiResponseDto<TestorProductDto>.FailureResponse(errors, new ApiMetaDto());
            _pactTestListApiClient.CreateTestOrProductAsync(request).Returns(Task.FromResult(expectedResponse));

            // Act
            var result = await _service.CreateTestOrProductAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        #endregion

        #region UpdateTestOrProductAsync

        [Fact]
        public async Task UpdateTestOrProductAsync_WithValidItemCodeAndRequest_ReturnsUpdatedTestOrProduct()
        {
            // Arrange
            var itemCode = "T001";
            var request = new TestorProductDto { ItemCode = itemCode, ItemDescription = "Updated Test" };
            var updatedTestOrProduct = new TestorProductDto { ItemCode = itemCode, ItemDescription = "Updated Test" };
            var expectedResponse = ApiResponseDto<TestorProductDto>.SuccessResponse(updatedTestOrProduct);
            _pactTestListApiClient.UpdateTestOrProductAsync(itemCode, request).Returns(Task.FromResult(expectedResponse));

            // Act
            var result = await _service.UpdateTestOrProductAsync(itemCode, request);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(itemCode, result.Data?.ItemCode);
            await _pactTestListApiClient.Received(1).UpdateTestOrProductAsync(itemCode, request);
        }

        [Fact]
        public async Task UpdateTestOrProductAsync_WithNonExistentItemCode_ReturnsFailure()
        {
            // Arrange
            var itemCode = "NOTFOUND";
            var request = new TestorProductDto { ItemCode = itemCode };
            var errors = new List<ApiErrorDto> { new() { Message = "Not Found", Code = "NOT_FOUND" } };
            var expectedResponse = ApiResponseDto<TestorProductDto>.FailureResponse(errors, new ApiMetaDto());
            _pactTestListApiClient.UpdateTestOrProductAsync(itemCode, request).Returns(Task.FromResult(expectedResponse));

            // Act
            var result = await _service.UpdateTestOrProductAsync(itemCode, request);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        [Fact]
        public async Task UpdateTestOrProductAsync_WhenValidationFails_ReturnsFailureResponse()
        {
            // Arrange
            var itemCode = "T001";
            var request = new TestorProductDto { ItemCode = itemCode };
            var errors = new List<ApiErrorDto> { new() { Message = "Validation failed", Code = "VALIDATION_ERROR" } };
            var expectedResponse = ApiResponseDto<TestorProductDto>.FailureResponse(errors, new ApiMetaDto());
            _pactTestListApiClient.UpdateTestOrProductAsync(itemCode, request).Returns(Task.FromResult(expectedResponse));

            // Act
            var result = await _service.UpdateTestOrProductAsync(itemCode, request);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        [Fact]
        public async Task UpdateTestOrProductAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var itemCode = "T001";
            var request = new TestorProductDto { ItemCode = itemCode };
            var errors = new List<ApiErrorDto> { new() { Message = "Internal error", Code = "INTERNAL_ERROR" } };
            var expectedResponse = ApiResponseDto<TestorProductDto>.FailureResponse(errors, new ApiMetaDto());
            _pactTestListApiClient.UpdateTestOrProductAsync(itemCode, request).Returns(Task.FromResult(expectedResponse));

            // Act
            var result = await _service.UpdateTestOrProductAsync(itemCode, request);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        #endregion

        #region DeleteTestOrProductAsync

        [Fact]
        public async Task DeleteTestOrProductAsync_WithValidItemCode_ReturnsSuccessTrue()
        {
            // Arrange
            var itemCode = "T001";
            var expectedResponse = ApiResponseDto<bool>.SuccessResponse(true);
            _pactTestListApiClient.DeleteTestOrProductAsync(itemCode).Returns(Task.FromResult(expectedResponse));

            // Act
            var result = await _service.DeleteTestOrProductAsync(itemCode);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
            await _pactTestListApiClient.Received(1).DeleteTestOrProductAsync(itemCode);
        }

        [Fact]
        public async Task DeleteTestOrProductAsync_WithNonExistentItemCode_ReturnsSuccessFalse()
        {
            // Arrange
            var itemCode = "NOTFOUND";
            var expectedResponse = ApiResponseDto<bool>.SuccessResponse(false);
            _pactTestListApiClient.DeleteTestOrProductAsync(itemCode).Returns(Task.FromResult(expectedResponse));

            // Act
            var result = await _service.DeleteTestOrProductAsync(itemCode);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.False(result.Data);
        }

        [Fact]
        public async Task DeleteTestOrProductAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var itemCode = "T001";
            var errors = new List<ApiErrorDto> { new() { Message = "Delete failed", Code = "DELETE_ERROR" } };
            var expectedResponse = ApiResponseDto<bool>.FailureResponse(errors, new ApiMetaDto());
            _pactTestListApiClient.DeleteTestOrProductAsync(itemCode).Returns(Task.FromResult(expectedResponse));

            // Act
            var result = await _service.DeleteTestOrProductAsync(itemCode);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region GetOwnersAsync

        [Fact]
        public async Task GetOwnersAsync_WithValidRequest_ReturnsOwnersList()
        {
            // Arrange
            var owners = new List<string> { "AB", "CD", "EF" };
            var expectedResponse = ApiResponseDto<List<string>>.SuccessResponse(owners);
            _pactTestListApiClient.GetOwnersAsync().Returns(Task.FromResult(expectedResponse));

            // Act
            var result = await _service.GetOwnersAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(3, result.Data?.Count);
            await _pactTestListApiClient.Received(1).GetOwnersAsync();
        }

        [Fact]
        public async Task GetOwnersAsync_WithEmptyResult_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var expectedResponse = ApiResponseDto<List<string>>.SuccessResponse(new List<string>());
            _pactTestListApiClient.GetOwnersAsync().Returns(Task.FromResult(expectedResponse));

            // Act
            var result = await _service.GetOwnersAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetOwnersAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Message = "Service error", Code = "SERVICE_ERROR" } };
            var expectedResponse = ApiResponseDto<List<string>>.FailureResponse(errors, new ApiMetaDto());
            _pactTestListApiClient.GetOwnersAsync().Returns(Task.FromResult(expectedResponse));

            // Act
            var result = await _service.GetOwnersAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion
    }
}
