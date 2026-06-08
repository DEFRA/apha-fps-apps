using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Services.FPS;
using NSubstitute;
using Xunit;

namespace Apha.FPSApps.Application.UnitTests.Services.FPS.BudgetBidsServiceTest
{
    public class BudgetBidsServiceTests
    {
        private readonly IFpsApiClient _fpsClient;
        private readonly IFpsBudgetBidsApiClient _fpsBudgetBidsApiClient;
        private readonly BudgetBidsService _sut;

        public BudgetBidsServiceTests()
        {
            _fpsClient             = Substitute.For<IFpsApiClient>();
            _fpsBudgetBidsApiClient = Substitute.For<IFpsBudgetBidsApiClient>();
            _fpsClient.FpsBudgetBids.Returns(_fpsBudgetBidsApiClient);
            _sut = new BudgetBidsService(_fpsClient);
        }

        #region GetBidViewAsync Tests

        [Fact]
        public async Task GetBidViewAsync_WithSuccessResponse_ReturnsBidViews()
        {
            // Arrange
            var bidList = new List<BidViewDto> { new() { WorkGroupName = "WG01", Account = "ACC1", GenBid = 100m } };
            var expectedResponse = ApiResponseDto<List<BidViewDto>>.SuccessResponse(bidList);
            _fpsBudgetBidsApiClient.GetBidViewAsync("WG01").Returns(expectedResponse);

            // Act
            var result = await _sut.GetBidViewAsync("WG01");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Single(result.Data!);
            await _fpsBudgetBidsApiClient.Received(1).GetBidViewAsync("WG01");
        }

        [Fact]
        public async Task GetBidViewAsync_WithEmptyResult_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var expectedResponse = ApiResponseDto<List<BidViewDto>>.SuccessResponse(new List<BidViewDto>());
            _fpsBudgetBidsApiClient.GetBidViewAsync("WG01").Returns(expectedResponse);

            // Act
            var result = await _sut.GetBidViewAsync("WG01");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetBidViewAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Message = "API Error", Code = "API_ERROR" } };
            var expectedResponse = ApiResponseDto<List<BidViewDto>>.FailureResponse(errors, new ApiMetaDto());
            _fpsBudgetBidsApiClient.GetBidViewAsync("WG01").Returns(expectedResponse);

            // Act
            var result = await _sut.GetBidViewAsync("WG01");

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Single(result.Errors!);
        }

        #endregion

        #region GetBidByIdAsync Tests

        [Fact]
        public async Task GetBidByIdAsync_WithSuccessResponse_ReturnsBid()
        {
            // Arrange
            var dto = new BidDto { WorkGroupName = "WG01", Account = "ACC1", GenBid = 100m };
            var expectedResponse = ApiResponseDto<BidDto>.SuccessResponse(dto);
            _fpsBudgetBidsApiClient.GetBidByIdAsync("WG01", "ACC1").Returns(expectedResponse);

            // Act
            var result = await _sut.GetBidByIdAsync("WG01", "ACC1");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal("ACC1", result.Data?.Account);
        }

        [Fact]
        public async Task GetBidByIdAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Message = "Not found", Code = "NOT_FOUND" } };
            var expectedResponse = ApiResponseDto<BidDto>.FailureResponse(errors, new ApiMetaDto());
            _fpsBudgetBidsApiClient.GetBidByIdAsync("WG01", "NOTEXIST").Returns(expectedResponse);

            // Act
            var result = await _sut.GetBidByIdAsync("WG01", "NOTEXIST");

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        #endregion

        #region CreateBidAsync Tests

        [Fact]
        public async Task CreateBidAsync_WithSuccessResponse_ReturnsBid()
        {
            // Arrange
            var bidDto = new BidDto { WorkGroupName = "WG01", Account = "ACC1", GenBid = 100m };
            var expectedResponse = ApiResponseDto<BidDto>.SuccessResponse(bidDto);
            _fpsBudgetBidsApiClient.CreateBidAsync(bidDto).Returns(expectedResponse);

            // Act
            var result = await _sut.CreateBidAsync(bidDto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            await _fpsBudgetBidsApiClient.Received(1).CreateBidAsync(bidDto);
        }

        [Fact]
        public async Task CreateBidAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var bidDto = new BidDto { WorkGroupName = "WG01", Account = "ACC1", GenBid = 100m };
            var errors = new List<ApiErrorDto> { new() { Message = "Already exists", Code = "CONFLICT" } };
            var expectedResponse = ApiResponseDto<BidDto>.FailureResponse(errors, new ApiMetaDto());
            _fpsBudgetBidsApiClient.CreateBidAsync(bidDto).Returns(expectedResponse);

            // Act
            var result = await _sut.CreateBidAsync(bidDto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        #endregion

        #region UpdateBidAsync Tests

        [Fact]
        public async Task UpdateBidAsync_WithSuccessResponse_ReturnsBid()
        {
            // Arrange
            var bidDto = new BidDto { WorkGroupName = "WG01", Account = "ACC1", GenBid = 200m };
            var expectedResponse = ApiResponseDto<BidDto>.SuccessResponse(bidDto);
            _fpsBudgetBidsApiClient.UpdateBidAsync(bidDto).Returns(expectedResponse);

            // Act
            var result = await _sut.UpdateBidAsync(bidDto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            await _fpsBudgetBidsApiClient.Received(1).UpdateBidAsync(bidDto);
        }

        [Fact]
        public async Task UpdateBidAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var bidDto = new BidDto { WorkGroupName = "WG01", Account = "ACC1", GenBid = 200m };
            var errors = new List<ApiErrorDto> { new() { Message = "Not found", Code = "NOT_FOUND" } };
            var expectedResponse = ApiResponseDto<BidDto>.FailureResponse(errors, new ApiMetaDto());
            _fpsBudgetBidsApiClient.UpdateBidAsync(bidDto).Returns(expectedResponse);

            // Act
            var result = await _sut.UpdateBidAsync(bidDto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        #endregion

        #region DeleteBidAsync Tests

        [Fact]
        public async Task DeleteBidAsync_WithSuccessResponse_ReturnsTrue()
        {
            // Arrange
            var bidDto = new BidDto { WorkGroupName = "WG01", Account = "ACC1" };
            var expectedResponse = ApiResponseDto<bool>.SuccessResponse(true);
            _fpsBudgetBidsApiClient.DeleteBidAsync(bidDto).Returns(expectedResponse);

            // Act
            var result = await _sut.DeleteBidAsync(bidDto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            await _fpsBudgetBidsApiClient.Received(1).DeleteBidAsync(bidDto);
        }

        [Fact]
        public async Task DeleteBidAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var bidDto = new BidDto { WorkGroupName = "WG01", Account = "ACC1" };
            var errors = new List<ApiErrorDto> { new() { Message = "Not found", Code = "NOT_FOUND" } };
            var expectedResponse = ApiResponseDto<bool>.FailureResponse(errors, new ApiMetaDto());
            _fpsBudgetBidsApiClient.DeleteBidAsync(bidDto).Returns(expectedResponse);

            // Act
            var result = await _sut.DeleteBidAsync(bidDto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        #endregion

        #region Constructor Tests

        [Fact]
        public void Constructor_WithNullFpsClient_ThrowsArgumentNullException()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => new BudgetBidsService(null!));
            Assert.Equal("fpsClient", ex.ParamName);
        }

        #endregion

        #region GetAccountCategoriesAsync Tests

        [Fact]
        public async Task GetAccountCategoriesAsync_WithSuccessResponse_ReturnsCategories()
        {
            // Arrange
            var categories = new List<AccountCategoryDto> { new() { AccShortName = "ACC1" } };
            var expectedResponse = ApiResponseDto<List<AccountCategoryDto>>.SuccessResponse(categories);
            _fpsBudgetBidsApiClient.GetAccountCategoriesAsync().Returns(expectedResponse);

            // Act
            var result = await _sut.GetAccountCategoriesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Single(result.Data!);
            await _fpsBudgetBidsApiClient.Received(1).GetAccountCategoriesAsync();
        }

        [Fact]
        public async Task GetAccountCategoriesAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Message = "API Error", Code = "API_ERROR" } };
            var expectedResponse = ApiResponseDto<List<AccountCategoryDto>>.FailureResponse(errors, new ApiMetaDto());
            _fpsBudgetBidsApiClient.GetAccountCategoriesAsync().Returns(expectedResponse);

            // Act
            var result = await _sut.GetAccountCategoriesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        #endregion
    }
}
