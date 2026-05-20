using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.PactApiClients;
using Apha.FPSApps.Application.Services.PACT;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace Apha.FPSApps.Application.UnitTests.Services.PACT.CalenderMonthServiceTest
{
    public class CalenderMonthServiceTests
    {
        private readonly IPactApiClient _pactClient;
        private readonly IPactCalenderMonthApiClient _pactCalenderMonthApiClient;
        private readonly CalenderMonthService _service;

        public CalenderMonthServiceTests()
        {
            _pactClient = Substitute.For<IPactApiClient>();
            _pactCalenderMonthApiClient = Substitute.For<IPactCalenderMonthApiClient>();
            _pactClient.PactCalenderMonth.Returns(_pactCalenderMonthApiClient);
            _service = new CalenderMonthService(_pactClient);
        }

        #region GetCalenderMonthsAsync

        [Fact]
        public async Task GetCalenderMonthsAsync_WithData_ReturnsSuccessResponse()
        {
            // Arrange
            var months = new List<CalenderMonthDto>
            {
                new() { MonthNumber = 1, MonthName = "January", AccntsPeriod = 1 },
                new() { MonthNumber = 2, MonthName = "February", AccntsPeriod = 2 }
            };
            var expectedResponse = ApiResponseDto<List<CalenderMonthDto>>.SuccessResponse(months);
            _pactCalenderMonthApiClient.GetCalenderMonthsAsync().Returns(expectedResponse);

            // Act
            var result = await _service.GetCalenderMonthsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data!.Count);
            await _pactCalenderMonthApiClient.Received(1).GetCalenderMonthsAsync();
        }

        [Fact]
        public async Task GetCalenderMonthsAsync_EmptyList_ReturnsSuccessResponseWithEmptyData()
        {
            // Arrange
            var expectedResponse = ApiResponseDto<List<CalenderMonthDto>>.SuccessResponse([]);
            _pactCalenderMonthApiClient.GetCalenderMonthsAsync().Returns(expectedResponse);

            // Act
            var result = await _service.GetCalenderMonthsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
            await _pactCalenderMonthApiClient.Received(1).GetCalenderMonthsAsync();
        }

        [Fact]
        public async Task GetCalenderMonthsAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Message = "API Error", Code = "API_ERROR" } };
            var expectedResponse = ApiResponseDto<List<CalenderMonthDto>>.FailureResponse(errors, new ApiMetaDto());
            _pactCalenderMonthApiClient.GetCalenderMonthsAsync().Returns(expectedResponse);

            // Act
            var result = await _service.GetCalenderMonthsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            await _pactCalenderMonthApiClient.Received(1).GetCalenderMonthsAsync();
        }

        [Fact]
        public async Task GetCalenderMonthsAsync_ApiClientThrows_PropagatesException()
        {
            // Arrange
            _pactCalenderMonthApiClient.GetCalenderMonthsAsync()
                .ThrowsAsync(new Exception("Connection error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _service.GetCalenderMonthsAsync());
        }

        #endregion
    }
}