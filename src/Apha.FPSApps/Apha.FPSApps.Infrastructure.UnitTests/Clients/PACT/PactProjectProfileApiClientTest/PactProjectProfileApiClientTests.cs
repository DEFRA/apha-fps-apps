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

        #region GetProfileDataAsync Tests

        [Fact]
        public async Task GetProfileDataAsync_WhenApiReturnsSuccess_ReturnsMappedDtoList()
        {
            // Arrange
            var project = "PRJ1";
            var expectedUrl = string.Format(PactApiEndpoints.GetProjectProfile, Uri.EscapeDataString(project));
            var resList = new List<ProjectProfileRes>
            {
                new() { MonthNo = 1, Profile = 100m, TotalCost = 200m },
                new() { MonthNo = 2, Profile = 150m, TotalCost = 300m }
            };
            var apiResponse = new ApiResponse<List<ProjectProfileRes>> { Success = true, Data = resList };
            var expectedDto = ApiResponseDto<List<ProjectProfileDto>>.SuccessResponse(new List<ProjectProfileDto>
            {
                new() { MonthNo = 1, Profile = 100m, TotalCost = 200m },
                new() { MonthNo = 2, Profile = 150m, TotalCost = 300m }
            });

            _http.GetAsync<List<ProjectProfileRes>>(expectedUrl).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectProfileDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetProfileDataAsync(project);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            Assert.Equal(1, result.Data![0].MonthNo);
            Assert.Equal(100m, result.Data[0].Profile);
            Assert.Equal(200m, result.Data[0].TotalCost);
            await _http.Received(1).GetAsync<List<ProjectProfileRes>>(expectedUrl);
            _mapper.Received(1).Map<ApiResponseDto<List<ProjectProfileDto>>>(apiResponse);
        }

        [Fact]
        public async Task GetProfileDataAsync_WhenApiReturnsEmptyList_ReturnsMappedEmptyList()
        {
            // Arrange
            var project = "PRJ_NONE";
            var expectedUrl = string.Format(PactApiEndpoints.GetProjectProfile, Uri.EscapeDataString(project));
            var apiResponse = new ApiResponse<List<ProjectProfileRes>> { Success = true, Data = new List<ProjectProfileRes>() };
            var expectedDto = ApiResponseDto<List<ProjectProfileDto>>.SuccessResponse(new List<ProjectProfileDto>());

            _http.GetAsync<List<ProjectProfileRes>>(expectedUrl).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectProfileDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetProfileDataAsync(project);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
            await _http.Received(1).GetAsync<List<ProjectProfileRes>>(expectedUrl);
        }

        [Fact]
        public async Task GetProfileDataAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var project = "PRJ1";
            var expectedUrl = string.Format(PactApiEndpoints.GetProjectProfile, Uri.EscapeDataString(project));
            var errors = new List<ApiError> { new() { Message = "API Error", Code = "API_ERROR" } };
            var apiResponse = new ApiResponse<List<ProjectProfileRes>> { Success = false, Errors = errors };
            var mappedResponse = new ApiResponseDto<List<ProjectProfileDto>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "API Error", Code = "API_ERROR" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<ProjectProfileRes>>(expectedUrl).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectProfileDto>>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetProfileDataAsync(project);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("API_ERROR", result.Errors.First().Code);
            _mapper.Received(1).Map<ApiResponseDto<List<ProjectProfileDto>>>(apiResponse);
        }

        [Fact]
        public async Task GetProfileDataAsync_UrlEncodesProjectParameter()
        {
            // Arrange
            var project = "PRJ/1";
            var expectedUrl = string.Format(PactApiEndpoints.GetProjectProfile, Uri.EscapeDataString(project));
            var apiResponse = new ApiResponse<List<ProjectProfileRes>> { Success = true, Data = new List<ProjectProfileRes>() };
            var expectedDto = ApiResponseDto<List<ProjectProfileDto>>.SuccessResponse(new List<ProjectProfileDto>());

            _http.GetAsync<List<ProjectProfileRes>>(expectedUrl).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectProfileDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetProfileDataAsync(project);

            // Assert
            Assert.NotNull(result);
            await _http.Received(1).GetAsync<List<ProjectProfileRes>>(expectedUrl);
        }

        #endregion

        #region GetCumulativeDataAsync Tests

        [Fact]
        public async Task GetCumulativeDataAsync_WhenApiReturnsSuccess_ReturnsMappedDtoList()
        {
            // Arrange
            var project = "PRJ1";
            var expectedUrl = string.Format(PactApiEndpoints.GetProjectProfileCumulative, Uri.EscapeDataString(project));
            var resList = new List<ProjectProfileCumulativeRes>
            {
                new() { MonthNo = 1, CumulativeProfile = 100m, CumulativeCost = 200m },
                new() { MonthNo = 2, CumulativeProfile = 250m, CumulativeCost = 500m }
            };
            var apiResponse = new ApiResponse<List<ProjectProfileCumulativeRes>> { Success = true, Data = resList };
            var expectedDto = ApiResponseDto<List<ProjectProfileCumulativeDto>>.SuccessResponse(new List<ProjectProfileCumulativeDto>
            {
                new() { MonthNo = 1, CumulativeProfile = 100m, CumulativeCost = 200m },
                new() { MonthNo = 2, CumulativeProfile = 250m, CumulativeCost = 500m }
            });

            _http.GetAsync<List<ProjectProfileCumulativeRes>>(expectedUrl).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectProfileCumulativeDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetCumulativeDataAsync(project);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            Assert.Equal(1, result.Data![0].MonthNo);
            Assert.Equal(100m, result.Data[0].CumulativeProfile);
            Assert.Equal(200m, result.Data[0].CumulativeCost);
            await _http.Received(1).GetAsync<List<ProjectProfileCumulativeRes>>(expectedUrl);
            _mapper.Received(1).Map<ApiResponseDto<List<ProjectProfileCumulativeDto>>>(apiResponse);
        }

        [Fact]
        public async Task GetCumulativeDataAsync_WhenApiReturnsEmptyList_ReturnsMappedEmptyList()
        {
            // Arrange
            var project = "PRJ_NONE";
            var expectedUrl = string.Format(PactApiEndpoints.GetProjectProfileCumulative, Uri.EscapeDataString(project));
            var apiResponse = new ApiResponse<List<ProjectProfileCumulativeRes>> { Success = true, Data = new List<ProjectProfileCumulativeRes>() };
            var expectedDto = ApiResponseDto<List<ProjectProfileCumulativeDto>>.SuccessResponse(new List<ProjectProfileCumulativeDto>());

            _http.GetAsync<List<ProjectProfileCumulativeRes>>(expectedUrl).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectProfileCumulativeDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetCumulativeDataAsync(project);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
            await _http.Received(1).GetAsync<List<ProjectProfileCumulativeRes>>(expectedUrl);
        }

        [Fact]
        public async Task GetCumulativeDataAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var project = "PRJ1";
            var expectedUrl = string.Format(PactApiEndpoints.GetProjectProfileCumulative, Uri.EscapeDataString(project));
            var errors = new List<ApiError> { new() { Message = "API Error", Code = "API_ERROR" } };
            var apiResponse = new ApiResponse<List<ProjectProfileCumulativeRes>> { Success = false, Errors = errors };
            var mappedResponse = new ApiResponseDto<List<ProjectProfileCumulativeDto>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "API Error", Code = "API_ERROR" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<ProjectProfileCumulativeRes>>(expectedUrl).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectProfileCumulativeDto>>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetCumulativeDataAsync(project);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("API_ERROR", result.Errors.First().Code);
            _mapper.Received(1).Map<ApiResponseDto<List<ProjectProfileCumulativeDto>>>(apiResponse);
        }

        [Fact]
        public async Task GetCumulativeDataAsync_UrlEncodesProjectParameter()
        {
            // Arrange
            var project = "PRJ/1";
            var expectedUrl = string.Format(PactApiEndpoints.GetProjectProfileCumulative, Uri.EscapeDataString(project));
            var apiResponse = new ApiResponse<List<ProjectProfileCumulativeRes>> { Success = true, Data = new List<ProjectProfileCumulativeRes>() };
            var expectedDto = ApiResponseDto<List<ProjectProfileCumulativeDto>>.SuccessResponse(new List<ProjectProfileCumulativeDto>());

            _http.GetAsync<List<ProjectProfileCumulativeRes>>(expectedUrl).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectProfileCumulativeDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetCumulativeDataAsync(project);

            // Assert
            Assert.NotNull(result);
            await _http.Received(1).GetAsync<List<ProjectProfileCumulativeRes>>(expectedUrl);
        }

        #endregion
    }
}
