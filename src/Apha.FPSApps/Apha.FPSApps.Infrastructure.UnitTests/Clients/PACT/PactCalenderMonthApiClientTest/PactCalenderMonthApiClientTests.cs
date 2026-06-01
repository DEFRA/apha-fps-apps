using Apha.Common.Contracts;
using Apha.Common.Contracts.PACT;
using Apha.Common.Constants;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using Apha.FPSApps.Infrastructure.Integrations.PACTApis.Clients;
using AutoMapper;
using NSubstitute;
using Xunit;

namespace Apha.FPSApps.Infrastructure.UnitTests.Clients.PACT.PactCalenderMonthApiClientTest
{
    public class PactCalenderMonthApiClientTests
    {
        private readonly IPactHttpExecutor _http;
        private readonly IMapper _mapper;
        private readonly PactCalenderMonthApiClient _client;

        public PactCalenderMonthApiClientTests()
        {
            _http = Substitute.For<IPactHttpExecutor>();
            _mapper = Substitute.For<IMapper>();
            _client = new PactCalenderMonthApiClient(_http, _mapper);
        }

        #region GetAllCalenderMonthsAsync Tests

        [Fact]
        public async Task GetAllCalenderMonthsAsync_WithSuccessResponse_ReturnsMappedCalenderMonthList()
        {
            // Arrange
            var resList = new List<CalenderMonthRes>
            {
                new() { MonthNumber = 1, MonthName = "January", AccntsPeriod = 1, Fquarter = 1 },
                new() { MonthNumber = 2, MonthName = "February", AccntsPeriod = 2, Fquarter = 1 }
            };
            var apiResponse = new ApiResponse<List<CalenderMonthRes>> { Success = true, Data = resList };
            var expectedDto = ApiResponseDto<List<CalenderMonthDto>>.SuccessResponse(
                new List<CalenderMonthDto>
                {
                    new() { MonthNumber = 1, MonthName = "January", AccntsPeriod = 1, Fquarter = 1 },
                    new() { MonthNumber = 2, MonthName = "February", AccntsPeriod = 2, Fquarter = 1 }
                });

            _http.GetAsync<List<CalenderMonthRes>>(PactApiEndpoints.GetCalenderMonths).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<CalenderMonthDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetCalenderMonthsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _http.Received(1).GetAsync<List<CalenderMonthRes>>(PactApiEndpoints.GetCalenderMonths);
        }

        [Fact]
        public async Task GetAllCalenderMonthsAsync_WithEmptyResult_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var apiResponse = new ApiResponse<List<CalenderMonthRes>> { Success = true, Data = new List<CalenderMonthRes>() };
            var expectedDto = ApiResponseDto<List<CalenderMonthDto>>.SuccessResponse(new List<CalenderMonthDto>());

            _http.GetAsync<List<CalenderMonthRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<CalenderMonthDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetCalenderMonthsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetAllCalenderMonthsAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiError> { new() { Message = "API Error", Code = "API_ERROR" } };
            var apiResponse = new ApiResponse<List<CalenderMonthRes>> { Success = false, Errors = errors };
            var mappedResponse = new ApiResponseDto<List<CalenderMonthDto>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "API Error", Code = "API_ERROR" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<CalenderMonthRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<CalenderMonthDto>>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetCalenderMonthsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion
    }
}
