using Apha.Common.Contracts;
using Apha.Common.Contracts.FPS;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Infrastructure.Integrations.FPSApis.Clients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;
using NSubstitute;
using Xunit;

namespace Apha.FPSApps.Infrastructure.UnitTests.Clients.FPS.FpsWorkGroupApiClientTest
{
    public class FpsWorkGroupApiClientTests
    {
        private readonly IFpsHttpExecutor _http;
        private readonly IMapper _mapper;
        private readonly FpsWorkGroupApiClient _client;

        public FpsWorkGroupApiClientTests()
        {
            _http   = Substitute.For<IFpsHttpExecutor>();
            _mapper = Substitute.For<IMapper>();
            _client = new FpsWorkGroupApiClient(_http, _mapper);
        }

        [Fact]
        public void Constructor_WithNullHttp_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new FpsWorkGroupApiClient(null!, _mapper));
        }

        [Fact]
        public void Constructor_WithNullMapper_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new FpsWorkGroupApiClient(_http, null!));
        }

        [Fact]
        public async Task GetAllWorkGroupNamesAsync_WithSuccessResponse_ReturnsWorkGroupNames()
        {
            // Arrange
            var names       = new List<string> { "WG01", "WG02" };
            var apiResponse = new ApiResponse<List<string>> { Success = true, Data = names };
            var expectedDto = ApiResponseDto<List<string>>.SuccessResponse(names);

            _http.GetAsync<List<string>>(Arg.Is<string>(url => url.Contains("workgroups/names")))
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<string>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetAllWorkGroupNamesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _http.Received(1).GetAsync<List<string>>(
                Arg.Is<string>(url => url.Contains("workgroups/names")));
        }

        [Fact]
        public async Task GetAllWorkGroupNamesAsync_WithEmptyResult_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var apiResponse = new ApiResponse<List<string>> { Success = true, Data = new List<string>() };
            var expectedDto = ApiResponseDto<List<string>>.SuccessResponse(new List<string>());

            _http.GetAsync<List<string>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<string>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetAllWorkGroupNamesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetAllWorkGroupNamesAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var apiResponse = new ApiResponse<List<string>>
            {
                Success = false,
                Errors  = new List<ApiError> { new() { Message = "Error", Code = "ERR" } }
            };
            var mappedResponse = new ApiResponseDto<List<string>>
            {
                Success = false,
                Errors  = new List<ApiErrorDto> { new() { Message = "Error", Code = "ERR" } },
                Meta    = new ApiMetaDto()
            };

            _http.GetAsync<List<string>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<string>>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetAllWorkGroupNamesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Single(result.Errors!);
        }

        [Fact]
        public async Task GetWorkGroupsAsync_WithSuccessResponse_ReturnsWorkGroups()
        {
            // Arrange
            var res = new List<WorkGroupRes> { new() { WorkGroupName = "WG01", ProfitCentre = "PC01" } };
            var apiResponse = new ApiResponse<List<WorkGroupRes>> { Success = true, Data = res };
            var expectedDto = ApiResponseDto<List<WorkGroupViewDto>>.SuccessResponse(new List<WorkGroupViewDto>
            {
                new() { WorkGroupName = "WG01", ProfitCentre = "PC01" }
            });

            _http.GetAsync<List<WorkGroupRes>>(Arg.Is<string>(url => url.Contains("PC01")))
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<WorkGroupViewDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetWorkGroupsAsync("PC01");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Single(result.Data!);
            Assert.Equal("WG01", result.Data![0].WorkGroupName);
        }

        [Fact]
        public async Task GetWorkGroupsAsync_WithEmptyResult_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var apiResponse = new ApiResponse<List<WorkGroupRes>> { Success = true, Data = new List<WorkGroupRes>() };
            var expectedDto = ApiResponseDto<List<WorkGroupViewDto>>.SuccessResponse(new List<WorkGroupViewDto>());

            _http.GetAsync<List<WorkGroupRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<WorkGroupViewDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetWorkGroupsAsync("PC01");

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetWorkGroupsAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var apiResponse = new ApiResponse<List<WorkGroupRes>>
            {
                Success = false,
                Errors  = new List<ApiError> { new() { Message = "Error", Code = "ERR" } }
            };
            var mappedResponse = new ApiResponseDto<List<WorkGroupViewDto>>
            {
                Success = false,
                Errors  = new List<ApiErrorDto> { new() { Message = "Error", Code = "ERR" } },
                Meta    = new ApiMetaDto()
            };

            _http.GetAsync<List<WorkGroupRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<WorkGroupViewDto>>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetWorkGroupsAsync("PC01");

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Single(result.Errors!);
        }
    }
}
