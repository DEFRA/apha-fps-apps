using Apha.Common.Constants;
using Apha.Common.Contracts;
using Apha.Common.Contracts.FPS;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Infrastructure.Integrations.FPSApis.Clients;
using Apha.FPSApps.Infrastructure.Integrations.HttpExecutor;
using AutoMapper;
using NSubstitute;
using Xunit;

namespace Apha.FPSApps.Infrastructure.UnitTests.Clients.FPS.FpsProjectGroupStaffPlanApiClientTest
{
    public class FpsProjectGroupStaffPlanApiClientTests
    {
        private readonly IFpsHttpExecutor _http;
        private readonly IMapper _mapper;
        private readonly FpsProjectGroupStaffPlanApiClient _client;

        public FpsProjectGroupStaffPlanApiClientTests()
        {
            _http = Substitute.For<IFpsHttpExecutor>();
            _mapper = Substitute.For<IMapper>();
            _client = new FpsProjectGroupStaffPlanApiClient(_http, _mapper);
        }

        #region GetPagedAsync Tests

        [Fact]
        public async Task GetPagedAsync_WithSuccessResponse_ReturnsMappedList()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var responseRows = new List<ProjectGroupStaffPlanViewRes>
            {
                new() { ProjectGroup = "GROUP_A", Manager = "Manager_A", ResourceCentre = "RC1", WorkGroup = "WG1", GradeCode = "G1", Name = "Alice Smith",  JobCode = "JC1", ProjectStatus = "Active",    Hrs = 100.0, ChargeRate = 500m, Fee = 250m },
                new() { ProjectGroup = "GROUP_A", Manager = "Manager_B", ResourceCentre = "RC2", WorkGroup = "WG2", GradeCode = "G2", Name = "Bob Jones",    JobCode = "JC2", ProjectStatus = "Completed", Hrs = 80.0,  ChargeRate = 400m, Fee = 200m }
            };
            var apiResponse = new ApiResponse<List<ProjectGroupStaffPlanViewRes>>
            {
                Success = true,
                Data    = responseRows
            };
            var expectedDto = ApiResponseDto<List<ProjectGroupStaffPlanViewDto>>.SuccessResponse(
                new List<ProjectGroupStaffPlanViewDto>
                {
                    new() { ProjectGroup = "GROUP_A", Manager = "Manager_A" },
                    new() { ProjectGroup = "GROUP_A", Manager = "Manager_B" }
                });

            _http.GetAsync<List<ProjectGroupStaffPlanViewRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectGroupStaffPlanViewDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetPagedAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _http.Received(1).GetAsync<List<ProjectGroupStaffPlanViewRes>>(Arg.Any<string>());
            _mapper.Received(1).Map<ApiResponseDto<List<ProjectGroupStaffPlanViewDto>>>(apiResponse);
        }

        [Fact]
        public async Task GetPagedAsync_WithEmptyResult_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = new ApiResponse<List<ProjectGroupStaffPlanViewRes>>
            {
                Success = true,
                Data    = new List<ProjectGroupStaffPlanViewRes>()
            };
            var expectedDto = ApiResponseDto<List<ProjectGroupStaffPlanViewDto>>.SuccessResponse(
                new List<ProjectGroupStaffPlanViewDto>());

            _http.GetAsync<List<ProjectGroupStaffPlanViewRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectGroupStaffPlanViewDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetPagedAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetPagedAsync_WhenApiReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiError> { new() { Message = "API Error", Code = "API_ERROR" } };
            var apiResponse = new ApiResponse<List<ProjectGroupStaffPlanViewRes>>
            {
                Success = false,
                Errors  = errors
            };
            var mappedDto = new ApiResponseDto<List<ProjectGroupStaffPlanViewDto>>
            {
                Success = false,
                Errors  = new List<ApiErrorDto> { new() { Message = "API Error", Code = "API_ERROR" } },
                Meta    = new ApiMetaDto()
            };

            _http.GetAsync<List<ProjectGroupStaffPlanViewRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectGroupStaffPlanViewDto>>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetPagedAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("API_ERROR", result.Errors[0].Code);
        }

        [Fact]
        public async Task GetPagedAsync_WhenApiReturnsFailure_CallsMapperOnce()
        {
            // Arrange — failure path must also call the mapper to extract errors/meta
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = new ApiResponse<List<ProjectGroupStaffPlanViewRes>>
            {
                Success = false,
                Errors  = new List<ApiError> { new() { Message = "Error", Code = "ERR" } }
            };
            var mappedDto = new ApiResponseDto<List<ProjectGroupStaffPlanViewDto>>
            {
                Success = false,
                Errors  = new List<ApiErrorDto> { new() { Message = "Error", Code = "ERR" } },
                Meta    = new ApiMetaDto()
            };

            _http.GetAsync<List<ProjectGroupStaffPlanViewRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectGroupStaffPlanViewDto>>>(apiResponse).Returns(mappedDto);

            // Act
            await _client.GetPagedAsync(query);

            // Assert
            _mapper.Received(1).Map<ApiResponseDto<List<ProjectGroupStaffPlanViewDto>>>(apiResponse);
        }

        [Fact]
        public async Task GetPagedAsync_CallsCorrectEndpoint()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = new ApiResponse<List<ProjectGroupStaffPlanViewRes>> { Success = true, Data = new() };
            var expectedDto = ApiResponseDto<List<ProjectGroupStaffPlanViewDto>>.SuccessResponse(new());

            _http.GetAsync<List<ProjectGroupStaffPlanViewRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectGroupStaffPlanViewDto>>>(apiResponse).Returns(expectedDto);

            // Act
            await _client.GetPagedAsync(query);

            // Assert — URL must be rooted at the correct endpoint
            await _http.Received(1).GetAsync<List<ProjectGroupStaffPlanViewRes>>(
                Arg.Is<string>(url => url.Contains(FpsApiEndpoints.GetPagedProjectGroupStaffPlan)));
        }

        [Fact]
        public async Task GetPagedAsync_ConstructsUrlWithQueryParameters()
        {
            // Arrange
            var query = new QueryParameters<string>
            {
                Page       = 2,
                PageSize   = 20,
                Search     = "GROUP_A",
                SortBy     = "ProjectGroup",
                Descending = true,
                Filter     = "{\"ProjectGroup\":\"GROUP_A\"}"
            };
            var apiResponse = new ApiResponse<List<ProjectGroupStaffPlanViewRes>> { Success = true, Data = new() };
            var expectedDto = ApiResponseDto<List<ProjectGroupStaffPlanViewDto>>.SuccessResponse(new());

            _http.GetAsync<List<ProjectGroupStaffPlanViewRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectGroupStaffPlanViewDto>>>(apiResponse).Returns(expectedDto);

            // Act
            await _client.GetPagedAsync(query);

            // Assert — URL must contain the base endpoint and some query string parameters
            await _http.Received(1).GetAsync<List<ProjectGroupStaffPlanViewRes>>(
                Arg.Is<string>(url =>
                    url.Contains(FpsApiEndpoints.GetPagedProjectGroupStaffPlan) &&
                    url.Contains("Page=2") &&
                    url.Contains("PageSize=20")));
        }

        [Fact]
        public async Task GetPagedAsync_WithMultipleErrors_ReturnsAllErrors()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = new ApiResponse<List<ProjectGroupStaffPlanViewRes>>
            {
                Success = false,
                Errors  = new List<ApiError>
                {
                    new() { Message = "Error one",   Code = "ERR_1" },
                    new() { Message = "Error two",   Code = "ERR_2" }
                }
            };
            var mappedDto = new ApiResponseDto<List<ProjectGroupStaffPlanViewDto>>
            {
                Success = false,
                Errors  = new List<ApiErrorDto>
                {
                    new() { Message = "Error one",   Code = "ERR_1" },
                    new() { Message = "Error two",   Code = "ERR_2" }
                },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<ProjectGroupStaffPlanViewRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectGroupStaffPlanViewDto>>>(apiResponse).Returns(mappedDto);

            // Act
            var result = await _client.GetPagedAsync(query);

            // Assert
            Assert.False(result.Success);
            Assert.Equal(2, result.Errors?.Count);
        }

        [Theory]
        [InlineData(1,  10)]
        [InlineData(2,  5)]
        [InlineData(1, 100)]
        public async Task GetPagedAsync_WithVariousPageParameters_CallsHttpExecutorOnce(int page, int pageSize)
        {
            // Arrange
            var query = new QueryParameters<string> { Page = page, PageSize = pageSize };
            var apiResponse = new ApiResponse<List<ProjectGroupStaffPlanViewRes>> { Success = true, Data = new() };
            var expectedDto = ApiResponseDto<List<ProjectGroupStaffPlanViewDto>>.SuccessResponse(new());

            _http.GetAsync<List<ProjectGroupStaffPlanViewRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectGroupStaffPlanViewDto>>>(apiResponse).Returns(expectedDto);

            // Act
            await _client.GetPagedAsync(query);

            // Assert
            await _http.Received(1).GetAsync<List<ProjectGroupStaffPlanViewRes>>(Arg.Any<string>());
        }

        [Fact]
        public async Task GetPagedAsync_WithSuccessResponse_ReturnsCorrectData()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = new ApiResponse<List<ProjectGroupStaffPlanViewRes>>
            {
                Success = true,
                Data    = new List<ProjectGroupStaffPlanViewRes>
                {
                    new() { ProjectGroup = "GROUP_A", Name = "Alice Smith", Manager = "Manager_A", Hrs = 100.0, ChargeRate = 500m, Fee = 250m }
                }
            };
            var expectedDto = ApiResponseDto<List<ProjectGroupStaffPlanViewDto>>.SuccessResponse(
                new List<ProjectGroupStaffPlanViewDto>
                {
                    new() { ProjectGroup = "GROUP_A", Name = "Alice Smith", Manager = "Manager_A", Hrs = 100.0, ChargeRate = 500m, Fee = 250m }
                });

            _http.GetAsync<List<ProjectGroupStaffPlanViewRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectGroupStaffPlanViewDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetPagedAsync(query);

            // Assert
            Assert.Single(result.Data!);
            Assert.Equal("GROUP_A",    result.Data![0].ProjectGroup);
            Assert.Equal("Alice Smith", result.Data![0].Name);
            Assert.Equal("Manager_A",  result.Data![0].Manager);
            Assert.Equal(100.0,        result.Data![0].Hrs);
            Assert.Equal(500m,         result.Data![0].ChargeRate);
            Assert.Equal(250m,         result.Data![0].Fee);
        }

        [Fact]
        public async Task GetPagedAsync_UsesListOfProjectGroupStaffPlanViewRes_AsHttpResponseType()
        {
            // Arrange — verify the correct response contract type is passed to GetAsync
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var apiResponse = new ApiResponse<List<ProjectGroupStaffPlanViewRes>> { Success = true, Data = new() };
            var expectedDto = ApiResponseDto<List<ProjectGroupStaffPlanViewDto>>.SuccessResponse(new());

            _http.GetAsync<List<ProjectGroupStaffPlanViewRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectGroupStaffPlanViewDto>>>(apiResponse).Returns(expectedDto);

            // Act
            await _client.GetPagedAsync(query);

            // Assert — must not have called GetAsync with any other type
            await _http.DidNotReceive().GetAsync<List<ProjectGroupStaffPlanViewDto>>(Arg.Any<string>());
            await _http.Received(1).GetAsync<List<ProjectGroupStaffPlanViewRes>>(Arg.Any<string>());
        }

        #endregion
    }
}
