using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Services.FPS;
using NSubstitute;
using Xunit;

namespace Apha.FPSApps.Application.UnitTests.Services.FPS.MaintTotalBusinessOverheadsServiceTest
{
    public class TotalBusinessOverheadsServiceTests
    {
        private readonly IFpsApiClient _mockFpsClient;
        private readonly IFpsTotalBusinessOverheadsApiClient _mockTotalBusinessOverheadsApiClient;
        private readonly TotalBusinessOverheadsService _sut;

        public TotalBusinessOverheadsServiceTests()
        {
            _mockFpsClient = Substitute.For<IFpsApiClient>();
            _mockTotalBusinessOverheadsApiClient = Substitute.For<IFpsTotalBusinessOverheadsApiClient>();
            _mockFpsClient.FpsTotalBusinessOverheads.Returns(_mockTotalBusinessOverheadsApiClient);
            _sut = new TotalBusinessOverheadsService(_mockFpsClient);
        }

        private static TotalBusinessOverheadsDto BuildDto(decimal? overheads = 1000000m, int fpsYear = 2025) =>
            new() { TotalBusinessOverheads = overheads, FpsYear = fpsYear };

        #region GetAsync Tests

        [Fact]
        public async Task GetAsync_ReturnsApiResponse()
        {
            // Arrange
            var dto = BuildDto();
            var apiResponse = ApiResponseDto<TotalBusinessOverheadsDto>.SuccessResponse(dto);

            _mockTotalBusinessOverheadsApiClient.GetAsync().Returns(apiResponse);

            // Act
            var result = await _sut.GetAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(1000000m, result.Data.TotalBusinessOverheads);
            Assert.Equal(2025, result.Data.FpsYear);
            await _mockTotalBusinessOverheadsApiClient.Received(1).GetAsync();
        }

        [Fact]
        public async Task GetAsync_PropagatesApiErrors()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Message = "Error", Code = "ERROR" } };
            var apiResponse = ApiResponseDto<TotalBusinessOverheadsDto>.FailureResponse(errors, new ApiMetaDto());

            _mockTotalBusinessOverheadsApiClient.GetAsync().Returns(apiResponse);

            // Act
            var result = await _sut.GetAsync();

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("Error", result.Errors.First().Message);
        }

        [Fact]
        public async Task GetAsync_WhenDataIsNull_ReturnsNullData()
        {
            // Arrange
            var apiResponse = new ApiResponseDto<TotalBusinessOverheadsDto>
            {
                Success = true,
                Data = null
            };

            _mockTotalBusinessOverheadsApiClient.GetAsync().Returns(apiResponse);

            // Act
            var result = await _sut.GetAsync();

            // Assert
            Assert.True(result.Success);
            Assert.Null(result.Data);
        }

        [Fact]
        public async Task GetAsync_WhenNotFound_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Message = "Not found", Code = "NOT_FOUND" } };
            var apiResponse = ApiResponseDto<TotalBusinessOverheadsDto>.FailureResponse(errors, new ApiMetaDto());

            _mockTotalBusinessOverheadsApiClient.GetAsync().Returns(apiResponse);

            // Act
            var result = await _sut.GetAsync();

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region UpdateAsync Tests

        [Fact]
        public async Task UpdateAsync_ReturnsApiResponse()
        {
            // Arrange
            var dto = BuildDto(1500000m);
            var apiResponse = ApiResponseDto<TotalBusinessOverheadsDto>.SuccessResponse(dto);

            _mockTotalBusinessOverheadsApiClient.UpdateAsync(dto).Returns(apiResponse);

            // Act
            var result = await _sut.UpdateAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(1500000m, result.Data.TotalBusinessOverheads);
            await _mockTotalBusinessOverheadsApiClient.Received(1).UpdateAsync(dto);
        }

        [Fact]
        public async Task UpdateAsync_PropagatesApiErrors()
        {
            // Arrange
            var dto = BuildDto();
            var errors = new List<ApiErrorDto> { new() { Message = "Update failed", Code = "UPDATE_ERROR" } };
            var apiResponse = ApiResponseDto<TotalBusinessOverheadsDto>.FailureResponse(errors, new ApiMetaDto());

            _mockTotalBusinessOverheadsApiClient.UpdateAsync(dto).Returns(apiResponse);

            // Act
            var result = await _sut.UpdateAsync(dto);

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("Update failed", result.Errors.First().Message);
        }

        [Fact]
        public async Task UpdateAsync_WithNullOverheads_CallsApiClient()
        {
            // Arrange
            var dto = BuildDto(null);
            var apiResponse = ApiResponseDto<TotalBusinessOverheadsDto>.SuccessResponse(dto);

            _mockTotalBusinessOverheadsApiClient.UpdateAsync(dto).Returns(apiResponse);

            // Act
            var result = await _sut.UpdateAsync(dto);

            // Assert
            Assert.True(result.Success);
            await _mockTotalBusinessOverheadsApiClient.Received(1).UpdateAsync(dto);
        }

        [Fact]
        public async Task UpdateAsync_WithZeroOverheads_CallsApiClient()
        {
            // Arrange
            var dto = BuildDto(0m);
            var apiResponse = ApiResponseDto<TotalBusinessOverheadsDto>.SuccessResponse(dto);

            _mockTotalBusinessOverheadsApiClient.UpdateAsync(dto).Returns(apiResponse);

            // Act
            var result = await _sut.UpdateAsync(dto);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(0m, result.Data!.TotalBusinessOverheads);
        }

        [Fact]
        public async Task UpdateAsync_PassesDtoToApiClient()
        {
            // Arrange
            var dto = BuildDto();
            var apiResponse = ApiResponseDto<TotalBusinessOverheadsDto>.SuccessResponse(dto);

            _mockTotalBusinessOverheadsApiClient.UpdateAsync(Arg.Any<TotalBusinessOverheadsDto>()).Returns(apiResponse);

            // Act
            await _sut.UpdateAsync(dto);

            // Assert
            await _mockTotalBusinessOverheadsApiClient.Received(1).UpdateAsync(
                Arg.Is<TotalBusinessOverheadsDto>(d =>
                    d.TotalBusinessOverheads == dto.TotalBusinessOverheads &&
                    d.FpsYear == dto.FpsYear));
        }

        #endregion
    }
}
