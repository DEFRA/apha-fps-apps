using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Services.FPS;
using NSubstitute;
using Xunit;

namespace Apha.FPSApps.Application.UnitTests.Services.FPS.PurchasesServiceTest
{
    public class PurchasesServiceTests
    {
        private readonly IFpsApiClient _fpsClient;
        private readonly IFpsPurchasesApiClient _fpsPurchasesApiClient;
        private readonly PurchasesService _sut;

        public PurchasesServiceTests()
        {
            _fpsClient            = Substitute.For<IFpsApiClient>();
            _fpsPurchasesApiClient = Substitute.For<IFpsPurchasesApiClient>();
            _fpsClient.FpsPurchases.Returns(_fpsPurchasesApiClient);
            _sut = new PurchasesService(_fpsClient);
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_WithNullFpsClient_ThrowsArgumentNullException()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => new PurchasesService(null!));
            Assert.Equal("fpsClient", ex.ParamName);
        }

        #endregion

        #region GetPurchasesAsync Tests

        [Fact]
        public async Task GetPurchasesAsync_WithSuccessResponse_ReturnsPurchases()
        {
            // Arrange
            var list = new List<PurchaseDto> { new() { WorkGroupName = "WG01", Account = "ACC1", ItemDescription = "Item A", Amount = 100m } };
            var expectedResponse = ApiResponseDto<List<PurchaseDto>>.SuccessResponse(list);
            _fpsPurchasesApiClient.GetPurchasesAsync("WG01", "ACC1").Returns(expectedResponse);

            // Act
            var result = await _sut.GetPurchasesAsync("WG01", "ACC1");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Single(result.Data!);
            await _fpsPurchasesApiClient.Received(1).GetPurchasesAsync("WG01", "ACC1");
        }

        [Fact]
        public async Task GetPurchasesAsync_WithEmptyResult_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var expectedResponse = ApiResponseDto<List<PurchaseDto>>.SuccessResponse(new List<PurchaseDto>());
            _fpsPurchasesApiClient.GetPurchasesAsync("WG01", "ACC1").Returns(expectedResponse);

            // Act
            var result = await _sut.GetPurchasesAsync("WG01", "ACC1");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetPurchasesAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Message = "API Error", Code = "API_ERROR" } };
            var expectedResponse = ApiResponseDto<List<PurchaseDto>>.FailureResponse(errors, new ApiMetaDto());
            _fpsPurchasesApiClient.GetPurchasesAsync("WG01", "ACC1").Returns(expectedResponse);

            // Act
            var result = await _sut.GetPurchasesAsync("WG01", "ACC1");

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Single(result.Errors!);
        }

        #endregion

        #region GetPurchaseByIdAsync Tests

        [Fact]
        public async Task GetPurchaseByIdAsync_WithSuccessResponse_ReturnsPurchase()
        {
            // Arrange
            var dto = new PurchaseDto { WorkGroupName = "WG01", Account = "ACC1", ItemDescription = "Item A", Amount = 100m };
            var expectedResponse = ApiResponseDto<PurchaseDto>.SuccessResponse(dto);
            _fpsPurchasesApiClient.GetPurchaseByIdAsync("WG01", "ACC1", "Item A").Returns(expectedResponse);

            // Act
            var result = await _sut.GetPurchaseByIdAsync("WG01", "ACC1", "Item A");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal("Item A", result.Data?.ItemDescription);
        }

        [Fact]
        public async Task GetPurchaseByIdAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Message = "Not found", Code = "NOT_FOUND" } };
            var expectedResponse = ApiResponseDto<PurchaseDto>.FailureResponse(errors, new ApiMetaDto());
            _fpsPurchasesApiClient.GetPurchaseByIdAsync("WG01", "ACC1", "NOTEXIST").Returns(expectedResponse);

            // Act
            var result = await _sut.GetPurchaseByIdAsync("WG01", "ACC1", "NOTEXIST");

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        #endregion

        #region CreatePurchaseAsync Tests

        [Fact]
        public async Task CreatePurchaseAsync_WithSuccessResponse_ReturnsPurchase()
        {
            // Arrange
            var dto = new PurchaseDto { WorkGroupName = "WG01", Account = "ACC1", ItemDescription = "Item A", Amount = 100m };
            var expectedResponse = ApiResponseDto<PurchaseDto>.SuccessResponse(dto);
            _fpsPurchasesApiClient.CreatePurchaseAsync(dto).Returns(expectedResponse);

            // Act
            var result = await _sut.CreatePurchaseAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            await _fpsPurchasesApiClient.Received(1).CreatePurchaseAsync(dto);
        }

        [Fact]
        public async Task CreatePurchaseAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var dto = new PurchaseDto { WorkGroupName = "WG01", Account = "ACC1", ItemDescription = "Item A", Amount = 100m };
            var errors = new List<ApiErrorDto> { new() { Message = "Already exists", Code = "CONFLICT" } };
            var expectedResponse = ApiResponseDto<PurchaseDto>.FailureResponse(errors, new ApiMetaDto());
            _fpsPurchasesApiClient.CreatePurchaseAsync(dto).Returns(expectedResponse);

            // Act
            var result = await _sut.CreatePurchaseAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        #endregion

        #region UpdatePurchaseAsync Tests

        [Fact]
        public async Task UpdatePurchaseAsync_WithSuccessResponse_ReturnsPurchase()
        {
            // Arrange
            var dto = new PurchaseDto { WorkGroupName = "WG01", Account = "ACC1", ItemDescription = "Item B", Amount = 200m, OldItemDescription = "Item A" };
            var expectedResponse = ApiResponseDto<PurchaseDto>.SuccessResponse(dto);
            _fpsPurchasesApiClient.UpdatePurchaseAsync(dto).Returns(expectedResponse);

            // Act
            var result = await _sut.UpdatePurchaseAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            await _fpsPurchasesApiClient.Received(1).UpdatePurchaseAsync(dto);
        }

        [Fact]
        public async Task UpdatePurchaseAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var dto = new PurchaseDto { WorkGroupName = "WG01", Account = "ACC1", ItemDescription = "Item B", Amount = 200m };
            var errors = new List<ApiErrorDto> { new() { Message = "Not found", Code = "NOT_FOUND" } };
            var expectedResponse = ApiResponseDto<PurchaseDto>.FailureResponse(errors, new ApiMetaDto());
            _fpsPurchasesApiClient.UpdatePurchaseAsync(dto).Returns(expectedResponse);

            // Act
            var result = await _sut.UpdatePurchaseAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        #endregion

        #region DeletePurchaseAsync Tests

        [Fact]
        public async Task DeletePurchaseAsync_WithSuccessResponse_ReturnsTrue()
        {
            // Arrange
            var dto = new PurchaseDto { WorkGroupName = "WG01", Account = "ACC1", ItemDescription = "Item A" };
            var expectedResponse = ApiResponseDto<bool>.SuccessResponse(true);
            _fpsPurchasesApiClient.DeletePurchaseAsync(dto).Returns(expectedResponse);

            // Act
            var result = await _sut.DeletePurchaseAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            await _fpsPurchasesApiClient.Received(1).DeletePurchaseAsync(dto);
        }

        [Fact]
        public async Task DeletePurchaseAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var dto = new PurchaseDto { WorkGroupName = "WG01", Account = "ACC1", ItemDescription = "Item A" };
            var errors = new List<ApiErrorDto> { new() { Message = "Not found", Code = "NOT_FOUND" } };
            var expectedResponse = ApiResponseDto<bool>.FailureResponse(errors, new ApiMetaDto());
            _fpsPurchasesApiClient.DeletePurchaseAsync(dto).Returns(expectedResponse);

            // Act
            var result = await _sut.DeletePurchaseAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        #endregion
    }
}
