using Apha.Common.Contracts;
using Apha.Common.Contracts.PACT;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using Apha.FPSApps.Infrastructure.Integrations.PACTApis.Clients;
using AutoMapper;
using NSubstitute;
using Xunit;

namespace Apha.FPSApps.Infrastructure.UnitTests.Clients.PACT.PactWorkGroupApiClientTest
{
    public class PactWorkGroupApiClientTests
    {
        private readonly IPactHttpExecutor _http;
        private readonly IMapper _mapper;
        private readonly PactWorkGroupApiClient _client;

        public PactWorkGroupApiClientTests()
        {
            _http = Substitute.For<IPactHttpExecutor>();
            _mapper = Substitute.For<IMapper>();
            _client = new PactWorkGroupApiClient(_http, _mapper);
        }

        #region GetAllWorkGroupsAsync Tests

        [Fact]
        public async Task GetAllWorkGroupsAsync_WithSuccessResponse_ReturnsMappedWorkGroupList()
        {
            // Arrange
            var workGroupList = new List<WorkGroupRes>
            {
                new() { WorkGroupName = "WG001", ProfitCentre = "PC001" },
                new() { WorkGroupName = "WG002", ProfitCentre = "PC002" }
            };
            var apiResponse = new ApiResponse<List<WorkGroupRes>> { Success = true, Data = workGroupList };
            var expectedDto = ApiResponseDto<List<WorkGroupDto>>.SuccessResponse(
                new List<WorkGroupDto>
                {
                    new() { WorkGroupName = "WG001", ProfitCentre = "PC001" },
                    new() { WorkGroupName = "WG002", ProfitCentre = "PC002" }
                }
            );

            _http.GetAsync<List<WorkGroupRes>>("api/v1/workgroup").Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<WorkGroupDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetAllWorkGroupsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _http.Received(1).GetAsync<List<WorkGroupRes>>("api/v1/workgroup");
        }

        [Fact]
        public async Task GetAllWorkGroupsAsync_WithEmptyResult_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var apiResponse = new ApiResponse<List<WorkGroupRes>> { Success = true, Data = new List<WorkGroupRes>() };
            var expectedDto = ApiResponseDto<List<WorkGroupDto>>.SuccessResponse(new List<WorkGroupDto>());

            _http.GetAsync<List<WorkGroupRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<WorkGroupDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetAllWorkGroupsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetAllWorkGroupsAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiError> { new() { Message = "API Error", Code = "API_ERROR" } };
            var apiResponse = new ApiResponse<List<WorkGroupRes>> { Success = false, Errors = errors };
            var mappedResponse = new ApiResponseDto<List<WorkGroupDto>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "API Error", Code = "API_ERROR" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<WorkGroupRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<WorkGroupDto>>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetAllWorkGroupsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion
    }
}
