using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Application.Services.FPS;
using NSubstitute;
using Xunit;

namespace Apha.FPSApps.Application.UnitTests.Services.FPS.ProjectGroupStaffPlanServiceTest
{
    public class ProjectGroupStaffPlanServiceTests
    {
        private readonly IFpsApiClient _fpsClient;
        private readonly IFpsProjectGroupStaffPlanApiClient _apiClient;
        private readonly ProjectGroupStaffPlanService _service;

        public ProjectGroupStaffPlanServiceTests()
        {
            _fpsClient = Substitute.For<IFpsApiClient>();
            _apiClient = Substitute.For<IFpsProjectGroupStaffPlanApiClient>();
            _fpsClient.FpsProjectGroupStaffPlan.Returns(_apiClient);
            _service = new ProjectGroupStaffPlanService(_fpsClient);
        }

        #region GetPagedAsync Tests

        [Fact]
        public async Task GetPagedAsync_WithSuccessResponse_ReturnsData()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var rows = new List<ProjectGroupStaffPlanViewDto>
            {
                new() { ProjectGroup = "GROUP_A", Manager = "Manager_A", ResourceCentre = "RC1", WorkGroup = "WG1", GradeCode = "G1", Name = "Alice Smith",  JobCode = "JC1", ProjectStatus = "Active",    Hrs = 100.0, ChargeRate = 500m, Fee = 250m },
                new() { ProjectGroup = "GROUP_A", Manager = "Manager_B", ResourceCentre = "RC2", WorkGroup = "WG2", GradeCode = "G2", Name = "Bob Jones",    JobCode = "JC2", ProjectStatus = "Completed", Hrs = 80.0,  ChargeRate = 400m, Fee = 200m }
            };
            var expectedResponse = ApiResponseDto<List<ProjectGroupStaffPlanViewDto>>.SuccessResponse(rows);

            _apiClient.GetPagedAsync(query).Returns(expectedResponse);

            // Act
            var result = await _service.GetPagedAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _apiClient.Received(1).GetPagedAsync(query);
        }

        [Fact]
        public async Task GetPagedAsync_WithEmptyResult_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var expectedResponse = ApiResponseDto<List<ProjectGroupStaffPlanViewDto>>.SuccessResponse(new List<ProjectGroupStaffPlanViewDto>());

            _apiClient.GetPagedAsync(query).Returns(expectedResponse);

            // Act
            var result = await _service.GetPagedAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetPagedAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiErrorDto>
            {
                new() { Message = "API Error", Code = "API_ERROR" }
            };
            var expectedResponse = ApiResponseDto<List<ProjectGroupStaffPlanViewDto>>.FailureResponse(errors, new ApiMetaDto());

            _apiClient.GetPagedAsync(query).Returns(expectedResponse);

            // Act
            var result = await _service.GetPagedAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("API_ERROR", result.Errors[0].Code);
        }

        [Fact]
        public async Task GetPagedAsync_CallsApiClientOnce()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var expectedResponse = ApiResponseDto<List<ProjectGroupStaffPlanViewDto>>.SuccessResponse(new List<ProjectGroupStaffPlanViewDto>());

            _apiClient.GetPagedAsync(query).Returns(expectedResponse);

            // Act
            await _service.GetPagedAsync(query);

            // Assert
            await _apiClient.Received(1).GetPagedAsync(query);
        }

        [Fact]
        public async Task GetPagedAsync_PassesCorrectQueryParameters()
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
            var expectedResponse = ApiResponseDto<List<ProjectGroupStaffPlanViewDto>>.SuccessResponse(
                new List<ProjectGroupStaffPlanViewDto>(),
                new PaginationDto { PageNumber = 2, PageSize = 20, TotalRecords = 0 });

            _apiClient.GetPagedAsync(query).Returns(expectedResponse);

            // Act
            await _service.GetPagedAsync(query);

            // Assert
            await _apiClient.Received(1).GetPagedAsync(Arg.Is<QueryParameters<string>>(q =>
                q.Page       == query.Page       &&
                q.PageSize   == query.PageSize   &&
                q.Search     == query.Search     &&
                q.SortBy     == query.SortBy     &&
                q.Descending == query.Descending &&
                q.Filter     == query.Filter));
        }

        [Fact]
        public async Task GetPagedAsync_WithPagination_ReturnsPaginationMetadata()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 5 };
            var pagination = new PaginationDto { PageNumber = 1, PageSize = 5, TotalRecords = 12, TotalPages = 3 };
            var expectedResponse = ApiResponseDto<List<ProjectGroupStaffPlanViewDto>>.SuccessResponse(
                new List<ProjectGroupStaffPlanViewDto> { new() { ProjectGroup = "GROUP_A" } },
                pagination);

            _apiClient.GetPagedAsync(query).Returns(expectedResponse);

            // Act
            var result = await _service.GetPagedAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.NotNull(result.Pagination);
            Assert.Equal(1,  result.Pagination.PageNumber);
            Assert.Equal(5,  result.Pagination.PageSize);
            Assert.Equal(12, result.Pagination.TotalRecords);
            Assert.Equal(3,  result.Pagination.TotalPages);
        }

        [Fact]
        public async Task GetPagedAsync_WithMultipleErrors_ReturnsAllErrors()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiErrorDto>
            {
                new() { Message = "Error one",   Code = "ERR_1" },
                new() { Message = "Error two",   Code = "ERR_2" },
                new() { Message = "Error three", Code = "ERR_3" }
            };
            var expectedResponse = ApiResponseDto<List<ProjectGroupStaffPlanViewDto>>.FailureResponse(errors, new ApiMetaDto());

            _apiClient.GetPagedAsync(query).Returns(expectedResponse);

            // Act
            var result = await _service.GetPagedAsync(query);

            // Assert
            Assert.False(result.Success);
            Assert.Equal(3, result.Errors?.Count);
        }

        [Theory]
        [InlineData(1,  10)]
        [InlineData(2,  5)]
        [InlineData(1, 100)]
        public async Task GetPagedAsync_WithVariousPageParameters_CallsApiClient(int page, int pageSize)
        {
            // Arrange
            var query = new QueryParameters<string> { Page = page, PageSize = pageSize };
            var expectedResponse = ApiResponseDto<List<ProjectGroupStaffPlanViewDto>>.SuccessResponse(
                new List<ProjectGroupStaffPlanViewDto>(),
                new PaginationDto { PageNumber = page, PageSize = pageSize });

            _apiClient.GetPagedAsync(query).Returns(expectedResponse);

            // Act
            await _service.GetPagedAsync(query);

            // Assert
            await _apiClient.Received(1).GetPagedAsync(query);
        }

        [Fact]
        public async Task GetPagedAsync_DelegatesToFpsProjectGroupStaffPlanApiClient()
        {
            // Arrange — verify the service routes through the correct API client property
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var expectedResponse = ApiResponseDto<List<ProjectGroupStaffPlanViewDto>>.SuccessResponse(new List<ProjectGroupStaffPlanViewDto>());

            _apiClient.GetPagedAsync(query).Returns(expectedResponse);

            // Act
            await _service.GetPagedAsync(query);

            // Assert — FpsProjectGroupStaffPlan must have been accessed, not any other sub-client
            _ = _fpsClient.Received(1).FpsProjectGroupStaffPlan;
            await _apiClient.Received(1).GetPagedAsync(query);
        }

        [Fact]
        public async Task GetPagedAsync_ResponseDataMatchesApiClientData()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var rows = new List<ProjectGroupStaffPlanViewDto>
            {
                new() { ProjectGroup = "GROUP_A", Name = "Alice Smith",  Manager = "Manager_A", Hrs = 100.0, ChargeRate = 500m, Fee = 250m },
                new() { ProjectGroup = "GROUP_B", Name = "Bob Jones",    Manager = "Manager_B", Hrs = 80.0,  ChargeRate = 400m, Fee = 200m },
                new() { ProjectGroup = "GROUP_C", Name = "Carol White",  Manager = "Manager_C", Hrs = 60.0,  ChargeRate = 300m, Fee = 150m }
            };
            var expectedResponse = ApiResponseDto<List<ProjectGroupStaffPlanViewDto>>.SuccessResponse(rows);

            _apiClient.GetPagedAsync(query).Returns(expectedResponse);

            // Act
            var result = await _service.GetPagedAsync(query);

            // Assert
            Assert.Equal(3, result.Data?.Count);
            Assert.Equal("GROUP_A", result.Data![0].ProjectGroup);
            Assert.Equal("Alice Smith",  result.Data[0].Name);
            Assert.Equal(100.0, result.Data[0].Hrs);
            Assert.Equal(500m,  result.Data[0].ChargeRate);
            Assert.Equal(250m,  result.Data[0].Fee);
        }

        #endregion
    }
}
