using Apha.Common.Constants;
using Apha.Common.Contracts;
using Apha.Common.Contracts.PACT;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using Apha.FPSApps.Infrastructure.Integrations.PACTApis.Clients;
using AutoMapper;
using NSubstitute;
using Xunit;

namespace Apha.FPSApps.Infrastructure.UnitTests.Clients.PACT.PactMonthHourApiClientTest
{
    public class PactMonthHourApiClientTests
    {
        private readonly IPactHttpExecutor _http;
        private readonly IMapper _mapper;
        private readonly PactMonthHourApiClient _client;

        public PactMonthHourApiClientTests()
        {
            _http = Substitute.For<IPactHttpExecutor>();
            _mapper = Substitute.For<IMapper>();
            _client = new PactMonthHourApiClient(_http, _mapper);
        }

        #region GetAllAsync Tests

        [Fact]
        public async Task GetAllAsync_WithSuccessResponse_ReturnsMappedMonthHourList()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = new ApiResponse<List<MonthHourRes>>
            {
                Success = true,
                Data =
                [
                    new MonthHourRes { Year = 2025, Month = 1, CvlHours = 160 },
                    new MonthHourRes { Year = 2025, Month = 2, CvlHours = 152 }
                ]
            };
            var expectedDto = ApiResponseDto<List<MonthHourDto>>.SuccessResponse(
            [
                new MonthHourDto { Year = 2025, Month = 1, CvlHours = 160 },
                new MonthHourDto { Year = 2025, Month = 2, CvlHours = 152 }
            ]);

            _http.GetAsync<List<MonthHourRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<MonthHourDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetAllAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _http.Received(1).GetAsync<List<MonthHourRes>>(Arg.Any<string>());
        }

        [Fact]
        public async Task GetAllAsync_WithEmptyResult_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = new ApiResponse<List<MonthHourRes>> { Success = true, Data = [] };
            var expectedDto = ApiResponseDto<List<MonthHourDto>>.SuccessResponse([]);

            _http.GetAsync<List<MonthHourRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<MonthHourDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetAllAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetAllAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = new ApiResponse<List<MonthHourRes>>
            {
                Success = false,
                Errors = [new ApiError { Message = "API Error", Code = "API_ERROR" }]
            };
            var mappedResponse = new ApiResponseDto<List<MonthHourDto>>
            {
                Success = false,
                Errors = [new ApiErrorDto { Message = "API Error", Code = "API_ERROR" }],
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<MonthHourRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<MonthHourDto>>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetAllAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region GetByYearAsync Tests

        [Fact]
        public async Task GetByYearAsync_WithSuccessResponse_ReturnsMappedMonthHourList()
        {
            // Arrange
            const short year = 2025;
            var apiResponse = new ApiResponse<List<MonthHourRes>>
            {
                Success = true,
                Data = [new MonthHourRes { Year = year, Month = 1, CvlHours = 160 }]
            };
            var expectedDto = ApiResponseDto<List<MonthHourDto>>.SuccessResponse(
                [new MonthHourDto { Year = year, Month = 1, CvlHours = 160 }]);

            _http.GetAsync<List<MonthHourRes>>(string.Format(PactApiEndpoints.GetMonthHoursByYear, year)).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<MonthHourDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetByYearAsync(year);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Single(result.Data!);
            await _http.Received(1).GetAsync<List<MonthHourRes>>(string.Format(PactApiEndpoints.GetMonthHoursByYear, year));
        }

        [Fact]
        public async Task GetByYearAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            const short year = 2025;
            var apiResponse = new ApiResponse<List<MonthHourRes>>
            {
                Success = false,
                Errors = [new ApiError { Message = "Not found", Code = "NOT_FOUND" }]
            };
            var mappedResponse = new ApiResponseDto<List<MonthHourDto>>
            {
                Success = false,
                Errors = [new ApiErrorDto { Message = "Not found", Code = "NOT_FOUND" }],
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<MonthHourRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<MonthHourDto>>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetByYearAsync(year);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region GetDistinctYearsAsync Tests

        [Fact]
        public async Task GetDistinctYearsAsync_WithSuccessResponse_ReturnsSuccessWithYears()
        {
            // Arrange
            var apiResponse = new ApiResponse<List<short>>
            {
                Success = true,
                Data = [2023, 2024, 2025]
            };

            _http.GetAsync<List<short>>(PactApiEndpoints.GetDistinctMonthHourYears).Returns(apiResponse);

            // Act
            var result = await _client.GetDistinctYearsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(3, result.Data?.Count);
            Assert.Equal((short)2025, result.Data![2]);
            await _http.Received(1).GetAsync<List<short>>(PactApiEndpoints.GetDistinctMonthHourYears);
        }

        [Fact]
        public async Task GetDistinctYearsAsync_WithEmptyResult_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var apiResponse = new ApiResponse<List<short>> { Success = true, Data = [] };
            _http.GetAsync<List<short>>(PactApiEndpoints.GetDistinctMonthHourYears).Returns(apiResponse);

            // Act
            var result = await _client.GetDistinctYearsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetDistinctYearsAsync_WhenApiReturnsFailure_ReturnsFailureResponseWithCorrelationId()
        {
            // Arrange
            var apiResponse = new ApiResponse<List<short>>
            {
                Success = false,
                Meta = new ApiMeta { CorrelationId = "corr-123" }
            };
            _http.GetAsync<List<short>>(PactApiEndpoints.GetDistinctMonthHourYears).Returns(apiResponse);

            // Act
            var result = await _client.GetDistinctYearsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Meta);
            Assert.Equal("corr-123", result.Meta.CorrelationId);
        }

        #endregion
    }
}
