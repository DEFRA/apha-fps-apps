using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.PactApiClients;
using Apha.FPSApps.Application.Services.PACT;
using NSubstitute;
using Xunit;

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

        #region GetAllCalenderMonthsAsync Tests

        [Fact]
        public async Task GetAllCalenderMonthsAsync_WithSuccessResponse_ReturnsMappedCalenderMonthList()
        {
            // Arrange
            var months = new List<CalenderMonthDto>
            {
                new() { Monthnumber = 1, Monthname = "January",  AccntsPeriod = 1, Fquarter = 1 },
                new() { Monthnumber = 2, Monthname = "February", AccntsPeriod = 2, Fquarter = 1 }
            };
            var expectedResponse = ApiResponseDto<List<CalenderMonthDto>>.SuccessResponse(months);
            _pactCalenderMonthApiClient.GetAllCalenderMonthsAsync().Returns(expectedResponse);

            // Act
            var result = await _service.GetAllCalenderMonthsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _pactCalenderMonthApiClient.Received(1).GetAllCalenderMonthsAsync();
        }

        [Fact]
        public async Task GetAllCalenderMonthsAsync_WithEmptyResult_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var expectedResponse = ApiResponseDto<List<CalenderMonthDto>>.SuccessResponse(new List<CalenderMonthDto>());
            _pactCalenderMonthApiClient.GetAllCalenderMonthsAsync().Returns(expectedResponse);

            // Act
            var result = await _service.GetAllCalenderMonthsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetAllCalenderMonthsAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Message = "API Error", Code = "API_ERROR" } };
            var expectedResponse = ApiResponseDto<List<CalenderMonthDto>>.FailureResponse(errors, new ApiMetaDto());
            _pactCalenderMonthApiClient.GetAllCalenderMonthsAsync().Returns(expectedResponse);

            // Act
            var result = await _service.GetAllCalenderMonthsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
        }

        #endregion
    }
}
