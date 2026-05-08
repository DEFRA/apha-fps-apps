using Apha.Common.Constants;
using Apha.Common.Contracts;
using Apha.Common.Contracts.PACT;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using Apha.FPSApps.Infrastructure.Integrations.PACTApis.Clients;
using AutoMapper;
using NSubstitute;

namespace Apha.FPSApps.Infrastructure.UnitTests.Clients.PACT.PactProjectProfileApiClientTest
{
    public class PactProjectProfileApiClientTests
    {
        private readonly IPactHttpExecutor _http;
        private readonly IMapper _mapper;
        private readonly PactProjectProfileApiClient _client;

        public PactProjectProfileApiClientTests()
        {
            _http = Substitute.For<IPactHttpExecutor>();
            _mapper = Substitute.For<IMapper>();
            _client = new PactProjectProfileApiClient(_http, _mapper);
        }

        #region GetProfileGraphDataAsync Tests

        [Fact]
        public async Task GetProfileGraphDataAsync_WhenApiReturnsSuccess_ReturnsMappedDtoList()
        {
            // Arrange
            var project = "PRJ1";
            var expectedUrl = string.Format(PactApiEndpoints.GetProjectProfileGraph, Uri.EscapeDataString(project));
            var resList = new List<ProjectProfileGraphRes>
            {
                new() { MonthNo = 1, Profile = 100m, TotalCost = 200m },
                new() { MonthNo = 2, Profile = 150m, TotalCost = 300m }
            };
            var apiResponse = new ApiResponse<List<ProjectProfileGraphRes>> { Success = true, Data = resList };
            var expectedDto = ApiResponseDto<List<ProjectProfileGraphDto>>.SuccessResponse(new List<ProjectProfileGraphDto>
            {
                new() { MonthNo = 1, Profile = 100m, TotalCost = 200m },
                new() { MonthNo = 2, Profile = 150m, TotalCost = 300m }
            });

            _http.GetAsync<List<ProjectProfileGraphRes>>(expectedUrl).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectProfileGraphDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetProfileGraphDataAsync(project);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            Assert.Equal(1, result.Data![0].MonthNo);
            Assert.Equal(100m, result.Data[0].Profile);
            Assert.Equal(200m, result.Data[0].TotalCost);
            await _http.Received(1).GetAsync<List<ProjectProfileGraphRes>>(expectedUrl);
            _mapper.Received(1).Map<ApiResponseDto<List<ProjectProfileGraphDto>>>(apiResponse);
        }

        [Fact]
        public async Task GetProfileGraphDataAsync_WhenApiReturnsEmptyList_ReturnsMappedEmptyList()
        {
            // Arrange
            var project = "PRJ_NONE";
            var expectedUrl = string.Format(PactApiEndpoints.GetProjectProfileGraph, Uri.EscapeDataString(project));
            var apiResponse = new ApiResponse<List<ProjectProfileGraphRes>> { Success = true, Data = new List<ProjectProfileGraphRes>() };
            var expectedDto = ApiResponseDto<List<ProjectProfileGraphDto>>.SuccessResponse(new List<ProjectProfileGraphDto>());

            _http.GetAsync<List<ProjectProfileGraphRes>>(expectedUrl).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectProfileGraphDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetProfileGraphDataAsync(project);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
            await _http.Received(1).GetAsync<List<ProjectProfileGraphRes>>(expectedUrl);
        }

        [Fact]
        public async Task GetProfileGraphDataAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var project = "PRJ1";
            var expectedUrl = string.Format(PactApiEndpoints.GetProjectProfileGraph, Uri.EscapeDataString(project));
            var errors = new List<ApiError> { new() { Message = "API Error", Code = "API_ERROR" } };
            var apiResponse = new ApiResponse<List<ProjectProfileGraphRes>> { Success = false, Errors = errors };
            var mappedResponse = new ApiResponseDto<List<ProjectProfileGraphDto>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "API Error", Code = "API_ERROR" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<ProjectProfileGraphRes>>(expectedUrl).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectProfileGraphDto>>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetProfileGraphDataAsync(project);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("API_ERROR", result.Errors.First().Code);
            _mapper.Received(1).Map<ApiResponseDto<List<ProjectProfileGraphDto>>>(apiResponse);
        }

        [Fact]
        public async Task GetProfileGraphDataAsync_UrlEncodesProjectParameter()
        {
            // Arrange
            var project = "PRJ/1";
            var expectedUrl = string.Format(PactApiEndpoints.GetProjectProfileGraph, Uri.EscapeDataString(project));
            var apiResponse = new ApiResponse<List<ProjectProfileGraphRes>> { Success = true, Data = new List<ProjectProfileGraphRes>() };
            var expectedDto = ApiResponseDto<List<ProjectProfileGraphDto>>.SuccessResponse(new List<ProjectProfileGraphDto>());

            _http.GetAsync<List<ProjectProfileGraphRes>>(expectedUrl).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectProfileGraphDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetProfileGraphDataAsync(project);

            // Assert
            Assert.NotNull(result);
            await _http.Received(1).GetAsync<List<ProjectProfileGraphRes>>(expectedUrl);
        }

        #endregion

        #region GetCumulativeGraphDataAsync Tests

        [Fact]
        public async Task GetCumulativeGraphDataAsync_WhenApiReturnsSuccess_ReturnsMappedDtoList()
        {
            // Arrange
            var project = "PRJ1";
            var expectedUrl = string.Format(PactApiEndpoints.GetProjectProfileCumulativeGraph, Uri.EscapeDataString(project));
            var resList = new List<ProjectProfileCumulativeGraphRes>
            {
                new() { MonthNo = 1, CumulativeProfile = 100m, CumulativeCost = 200m },
                new() { MonthNo = 2, CumulativeProfile = 250m, CumulativeCost = 500m }
            };
            var apiResponse = new ApiResponse<List<ProjectProfileCumulativeGraphRes>> { Success = true, Data = resList };
            var expectedDto = ApiResponseDto<List<ProjectProfileCumulativeGraphDto>>.SuccessResponse(new List<ProjectProfileCumulativeGraphDto>
            {
                new() { MonthNo = 1, CumulativeProfile = 100m, CumulativeCost = 200m },
                new() { MonthNo = 2, CumulativeProfile = 250m, CumulativeCost = 500m }
            });

            _http.GetAsync<List<ProjectProfileCumulativeGraphRes>>(expectedUrl).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectProfileCumulativeGraphDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetCumulativeGraphDataAsync(project);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            Assert.Equal(1, result.Data![0].MonthNo);
            Assert.Equal(100m, result.Data[0].CumulativeProfile);
            Assert.Equal(200m, result.Data[0].CumulativeCost);
            await _http.Received(1).GetAsync<List<ProjectProfileCumulativeGraphRes>>(expectedUrl);
            _mapper.Received(1).Map<ApiResponseDto<List<ProjectProfileCumulativeGraphDto>>>(apiResponse);
        }

        [Fact]
        public async Task GetCumulativeGraphDataAsync_WhenApiReturnsEmptyList_ReturnsMappedEmptyList()
        {
            // Arrange
            var project = "PRJ_NONE";
            var expectedUrl = string.Format(PactApiEndpoints.GetProjectProfileCumulativeGraph, Uri.EscapeDataString(project));
            var apiResponse = new ApiResponse<List<ProjectProfileCumulativeGraphRes>> { Success = true, Data = new List<ProjectProfileCumulativeGraphRes>() };
            var expectedDto = ApiResponseDto<List<ProjectProfileCumulativeGraphDto>>.SuccessResponse(new List<ProjectProfileCumulativeGraphDto>());

            _http.GetAsync<List<ProjectProfileCumulativeGraphRes>>(expectedUrl).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectProfileCumulativeGraphDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetCumulativeGraphDataAsync(project);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
            await _http.Received(1).GetAsync<List<ProjectProfileCumulativeGraphRes>>(expectedUrl);
        }

        [Fact]
        public async Task GetCumulativeGraphDataAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var project = "PRJ1";
            var expectedUrl = string.Format(PactApiEndpoints.GetProjectProfileCumulativeGraph, Uri.EscapeDataString(project));
            var errors = new List<ApiError> { new() { Message = "API Error", Code = "API_ERROR" } };
            var apiResponse = new ApiResponse<List<ProjectProfileCumulativeGraphRes>> { Success = false, Errors = errors };
            var mappedResponse = new ApiResponseDto<List<ProjectProfileCumulativeGraphDto>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "API Error", Code = "API_ERROR" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<ProjectProfileCumulativeGraphRes>>(expectedUrl).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectProfileCumulativeGraphDto>>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetCumulativeGraphDataAsync(project);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("API_ERROR", result.Errors.First().Code);
            _mapper.Received(1).Map<ApiResponseDto<List<ProjectProfileCumulativeGraphDto>>>(apiResponse);
        }

        [Fact]
        public async Task GetCumulativeGraphDataAsync_UrlEncodesProjectParameter()
        {
            // Arrange
            var project = "PRJ/1";
            var expectedUrl = string.Format(PactApiEndpoints.GetProjectProfileCumulativeGraph, Uri.EscapeDataString(project));
            var apiResponse = new ApiResponse<List<ProjectProfileCumulativeGraphRes>> { Success = true, Data = new List<ProjectProfileCumulativeGraphRes>() };
            var expectedDto = ApiResponseDto<List<ProjectProfileCumulativeGraphDto>>.SuccessResponse(new List<ProjectProfileCumulativeGraphDto>());

            _http.GetAsync<List<ProjectProfileCumulativeGraphRes>>(expectedUrl).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectProfileCumulativeGraphDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetCumulativeGraphDataAsync(project);

            // Assert
            Assert.NotNull(result);
            await _http.Received(1).GetAsync<List<ProjectProfileCumulativeGraphRes>>(expectedUrl);
        }

        #endregion
    }
}
