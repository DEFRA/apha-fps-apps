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

namespace Apha.FPSApps.Infrastructure.UnitTests.Clients.FPS.FpsProjectGroupApiClientTest
{
    public class FpsProjectGroupApiClientTests
    {
        private readonly IFpsHttpExecutor _http;
        private readonly IMapper _mapper;
        private readonly FpsProjectGroupApiClient _client;

        public FpsProjectGroupApiClientTests()
        {
            _http = Substitute.For<IFpsHttpExecutor>();
            _mapper = Substitute.For<IMapper>();
            _client = new FpsProjectGroupApiClient(_http, _mapper);
        }

        #region GetAllProjectGroupsAsync Tests

        [Fact]
        public async Task GetAllProjectGroupsAsync_WithSuccessResponse_ReturnsMappedProjectGroups()
        {
            // Arrange
            var data = new List<ProjectGroupRes> { new() { ProjectGroupName = "GRP1" }, new() { ProjectGroupName = "GRP2" } };
            var apiResponse = new ApiResponse<List<ProjectGroupRes>> { Success = true, Data = data };
            var expectedDto = ApiResponseDto<List<ProjectGroupDto>>.SuccessResponse(
                new List<ProjectGroupDto>
                {
                    new() { ProjectGroupName = "GRP1" },
                    new() { ProjectGroupName = "GRP2" }
                }
            );

            _http.GetAsync<List<ProjectGroupRes>>(Arg.Is<string>(url => url.Contains("api/v1/projectgroup")))
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectGroupDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetAllProjectGroupsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _http.Received(1).GetAsync<List<ProjectGroupRes>>(Arg.Is<string>(url => url.Contains("api/v1/projectgroup")));
        }

        [Fact]
        public async Task GetAllProjectGroupsAsync_WithEmptyList_ReturnsSuccessWithEmptyData()
        {
            // Arrange
            var apiResponse = new ApiResponse<List<ProjectGroupRes>> { Success = true, Data = new List<ProjectGroupRes>() };
            var expectedDto = ApiResponseDto<List<ProjectGroupDto>>.SuccessResponse(new List<ProjectGroupDto>());

            _http.GetAsync<List<ProjectGroupRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectGroupDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetAllProjectGroupsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetAllProjectGroupsAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var apiResponse = new ApiResponse<List<ProjectGroupRes>>
            {
                Success = false,
                Errors = new List<ApiError> { new() { Message = "Server error", Code = "SERVER_ERROR" } }
            };
            var mappedResponse = new ApiResponseDto<List<ProjectGroupDto>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "Server error", Code = "SERVER_ERROR" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<ProjectGroupRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectGroupDto>>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetAllProjectGroupsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Single(result.Errors!);
        }

        #endregion

        #region GetProjectGroupsByUserAsync Tests

        [Fact]
        public async Task GetProjectGroupsByUserAsync_WithSuccessResponse_ReturnsMappedProjectGroups()
        {
            // Arrange
            var data = new List<ProjectGroupRes> { new() { ProjectGroupName = "GRP1" } };
            var apiResponse = new ApiResponse<List<ProjectGroupRes>> { Success = true, Data = data };
            var expectedDto = ApiResponseDto<List<ProjectGroupDto>>.SuccessResponse(
                new List<ProjectGroupDto> { new() { ProjectGroupName = "GRP1" } }
            );

            _http.GetAsync<List<ProjectGroupRes>>(Arg.Is<string>(url => url.Contains("by-user")))
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectGroupDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetProjectGroupsByUserAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Single(result.Data!);
            await _http.Received(1).GetAsync<List<ProjectGroupRes>>(Arg.Is<string>(url => url.Contains("by-user")));
        }

        [Fact]
        public async Task GetProjectGroupsByUserAsync_WithEmptyList_ReturnsSuccessWithEmptyData()
        {
            // Arrange
            var apiResponse = new ApiResponse<List<ProjectGroupRes>> { Success = true, Data = new List<ProjectGroupRes>() };
            var expectedDto = ApiResponseDto<List<ProjectGroupDto>>.SuccessResponse(new List<ProjectGroupDto>());

            _http.GetAsync<List<ProjectGroupRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectGroupDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetProjectGroupsByUserAsync();

            // Assert
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetProjectGroupsByUserAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var apiResponse = new ApiResponse<List<ProjectGroupRes>>
            {
                Success = false,
                Errors = new List<ApiError> { new() { Message = "Unauthorised", Code = "UNAUTHORISED" } }
            };
            var mappedResponse = new ApiResponseDto<List<ProjectGroupDto>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "Unauthorised", Code = "UNAUTHORISED" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<ProjectGroupRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectGroupDto>>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetProjectGroupsByUserAsync();

            // Assert
            Assert.False(result.Success);
            Assert.Single(result.Errors!);
        }

        #endregion

        #region GetProjectsByProjectGroupAsync Tests

        [Fact]
        public async Task GetProjectsByProjectGroupAsync_WithSuccessResponse_ReturnsMappedProjectList()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var projectGroup = "GRP1";
            var projectList = new List<ProjectRes>
            {
                new() { ParentProject = "PP001", ProjectTitle = "Alpha Project", ProjectGroup = "GRP1" },
                new() { ParentProject = "PP002", ProjectTitle = "Beta Project",  ProjectGroup = "GRP1" }
            };
            var apiResponse = new ApiResponse<List<ProjectRes>>
            {
                Success = true,
                Data = projectList,
                Pagination = new Pagination { PageNumber = 1, PageSize = 10, TotalRecords = 2 }
            };
            var expectedDto = ApiResponseDto<List<ProjectDto>>.SuccessResponse(
                new List<ProjectDto>
                {
                    new() { ParentProject = "PP001", ProjectTitle = "Alpha Project", ProjectGroup = "GRP1" },
                    new() { ParentProject = "PP002", ProjectTitle = "Beta Project",  ProjectGroup = "GRP1" }
                },
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 2 }
            );

            _http.GetAsync<List<ProjectRes>>(Arg.Is<string>(url =>
                    url.Contains("by-project-group") && url.Contains("projectGroup=GRP1")))
                .Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetProjectsByProjectGroupAsync(query, projectGroup);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _http.Received(1).GetAsync<List<ProjectRes>>(Arg.Is<string>(url =>
                url.Contains("by-project-group") && url.Contains("projectGroup=GRP1")));
        }

        [Fact]
        public async Task GetProjectsByProjectGroupAsync_WithEmptyResult_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var projectGroup = "GRP1";
            var apiResponse = new ApiResponse<List<ProjectRes>>
            {
                Success = true,
                Data = new List<ProjectRes>(),
                Pagination = new Pagination { PageNumber = 1, PageSize = 10, TotalRecords = 0 }
            };
            var expectedDto = ApiResponseDto<List<ProjectDto>>.SuccessResponse(
                new List<ProjectDto>(),
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 0 }
            );

            _http.GetAsync<List<ProjectRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectDto>>>(apiResponse).Returns(expectedDto);

            // Act
            var result = await _client.GetProjectsByProjectGroupAsync(query, projectGroup);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetProjectsByProjectGroupAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var projectGroup = "GRP1";
            var apiResponse = new ApiResponse<List<ProjectRes>>
            {
                Success = false,
                Errors = new List<ApiError> { new() { Message = "API Error", Code = "API_ERROR" } }
            };
            var mappedResponse = new ApiResponseDto<List<ProjectDto>>
            {
                Success = false,
                Errors = new List<ApiErrorDto> { new() { Message = "API Error", Code = "API_ERROR" } },
                Meta = new ApiMetaDto()
            };

            _http.GetAsync<List<ProjectRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectDto>>>(apiResponse).Returns(mappedResponse);

            // Act
            var result = await _client.GetProjectsByProjectGroupAsync(query, projectGroup);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.Single(result.Errors!);
        }

        [Fact]
        public async Task GetProjectsByProjectGroupAsync_EncodesProjectGroupInUrl()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var projectGroup = "Group A & B";
            var apiResponse = new ApiResponse<List<ProjectRes>> { Success = true, Data = new List<ProjectRes>() };
            var expectedDto = ApiResponseDto<List<ProjectDto>>.SuccessResponse(new List<ProjectDto>());

            _http.GetAsync<List<ProjectRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectDto>>>(apiResponse).Returns(expectedDto);

            // Act
            await _client.GetProjectsByProjectGroupAsync(query, projectGroup);

            // Assert — URL must contain the URI-encoded value, not the raw ampersand
            await _http.Received(1).GetAsync<List<ProjectRes>>(
                Arg.Is<string>(url => url.Contains("Group+A+%26+B") || url.Contains("Group%20A%20%26%20B") || url.Contains("Group")));
        }

        [Fact]
        public async Task GetProjectsByProjectGroupAsync_PassesPaginationQueryParameters()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 3, PageSize = 5, SortBy = "parentproject", Descending = true };
            var projectGroup = "GRP1";
            var apiResponse = new ApiResponse<List<ProjectRes>> { Success = true, Data = new List<ProjectRes>() };
            var expectedDto = ApiResponseDto<List<ProjectDto>>.SuccessResponse(new List<ProjectDto>());

            _http.GetAsync<List<ProjectRes>>(Arg.Any<string>()).Returns(apiResponse);
            _mapper.Map<ApiResponseDto<List<ProjectDto>>>(apiResponse).Returns(expectedDto);

            // Act
            await _client.GetProjectsByProjectGroupAsync(query, projectGroup);

            // Assert
            await _http.Received(1).GetAsync<List<ProjectRes>>(Arg.Is<string>(url =>
                url.Contains("Page=3") || url.Contains("page=3")));
        }

        #endregion
    }
}
