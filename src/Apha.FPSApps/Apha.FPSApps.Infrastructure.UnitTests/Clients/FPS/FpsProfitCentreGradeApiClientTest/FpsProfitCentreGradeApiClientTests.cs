using Apha.Common.Contracts;
using Apha.Common.Contracts.FPS;
using Apha.Common.Utilities.Query;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Infrastructure.Integrations.FPSApis.Clients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;
using NSubstitute;
using Xunit;

namespace Apha.FPSApps.Infrastructure.UnitTests.Clients.FPS.FpsProfitCentreGradeApiClientTest
{
    public class FpsProfitCentreGradeApiClientTests
    {
        private const string DefaultProfitCentre = "PC01";

        private readonly IFpsHttpExecutor _http;
        private readonly IMapper _mapper;
        private readonly FpsProfitCentreGradeApiClient _client;

        public FpsProfitCentreGradeApiClientTests()
        {
            _http   = Substitute.For<IFpsHttpExecutor>();
            _mapper = Substitute.For<IMapper>();
            _client = new FpsProfitCentreGradeApiClient(_http, _mapper);
        }

        #region GetProfitCentreGradesAsync Tests

        [Fact]
        public async Task GetProfitCentreGradesAsync_WithSuccessResponse_ReturnsMappedGradeList()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var resList = new List<ProfitCentreGradeRes>
            {
                new() { PcGrade = "G001", ProfitCentre = DefaultProfitCentre, ChargeRate = 100m },
                new() { PcGrade = "G002", ProfitCentre = DefaultProfitCentre, ChargeRate = 200m }
            };
            var gradeList = new List<ProfitCentreGradeDto>
            {
                new() { PcGrade = "G001", ProfitCentre = DefaultProfitCentre, ChargeRate = 100m },
                new() { PcGrade = "G002", ProfitCentre = DefaultProfitCentre, ChargeRate = 200m }
            };
            var apiResponse = new ApiResponse<List<ProfitCentreGradeRes>>
            {
                Success = true,
                Data    = resList
            };
            var expectedDto = ApiResponseDto<List<ProfitCentreGradeDto>>.SuccessResponse(gradeList);

            _http.GetAsync<List<ProfitCentreGradeRes>>(
                    Arg.Is<string>(url => url.Contains("rcgrades") && url.Contains(DefaultProfitCentre)))
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProfitCentreGradeDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetProfitCentreGradesAsync(query, DefaultProfitCentre);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _http.Received(1).GetAsync<List<ProfitCentreGradeRes>>(
                Arg.Is<string>(url => url.Contains("rcgrades") && url.Contains(DefaultProfitCentre)));
        }

        [Fact]
        public async Task GetProfitCentreGradesAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = new ApiResponse<List<ProfitCentreGradeRes>>
            {
                Success = false,
                Errors  = new List<ApiError> { new() { Message = "API Error", Code = "ERROR" } }
            };
            var mappedResponse = new ApiResponseDto<List<ProfitCentreGradeDto>>
            {
                Success = false,
                Errors  = new List<ApiErrorDto> { new() { Message = "API Error", Code = "ERROR" } },
                Meta    = new ApiMetaDto()
            };

            _http.GetAsync<List<ProfitCentreGradeRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProfitCentreGradeDto>>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetProfitCentreGradesAsync(query, DefaultProfitCentre);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
        }

        [Fact]
        public async Task GetProfitCentreGradesAsync_WithEmptyResult_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = new ApiResponse<List<ProfitCentreGradeRes>>
            {
                Success = true,
                Data    = new List<ProfitCentreGradeRes>()
            };
            var expectedDto = ApiResponseDto<List<ProfitCentreGradeDto>>.SuccessResponse(new List<ProfitCentreGradeDto>());

            _http.GetAsync<List<ProfitCentreGradeRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProfitCentreGradeDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetProfitCentreGradesAsync(query, DefaultProfitCentre);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetProfitCentreGradesAsync_BuildsUrlWithProfitCentreEncoded()
        {
            // Arrange
            const string profitCentre = "PC 01"; // contains space — should be URL-encoded
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = new ApiResponse<List<ProfitCentreGradeRes>> { Success = true, Data = new List<ProfitCentreGradeRes>() };
            var expectedDto = ApiResponseDto<List<ProfitCentreGradeDto>>.SuccessResponse(new List<ProfitCentreGradeDto>());

            _http.GetAsync<List<ProfitCentreGradeRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProfitCentreGradeDto>>>(apiResponse).Returns(expectedDto);

            // Act
            await _client.GetProfitCentreGradesAsync(query, profitCentre);

            // Assert — URL must contain URL-encoded profit centre
            await _http.Received(1).GetAsync<List<ProfitCentreGradeRes>>(
                Arg.Is<string>(url => url.Contains("PC%2001") || url.Contains("PC+01") || url.Contains("PC%20")));
        }

        #endregion
    }
}
