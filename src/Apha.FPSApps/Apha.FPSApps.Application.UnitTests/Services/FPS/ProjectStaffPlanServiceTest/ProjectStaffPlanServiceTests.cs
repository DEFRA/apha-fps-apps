using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Application.Services.FPS;
using NSubstitute;
using Xunit;

namespace Apha.FPSApps.Application.UnitTests.Services.FPS.ProjectStaffPlanServiceTest
{
    public class ProjectStaffPlanServiceTests
    {
        private readonly IFpsApiClient _fpsClient;
        private readonly IFpsProjectStaffPlanApiClient _apiClient;
        private readonly ProjectStaffPlanService _service;

        public ProjectStaffPlanServiceTests()
        {
            _fpsClient = Substitute.For<IFpsApiClient>();
            _apiClient = Substitute.For<IFpsProjectStaffPlanApiClient>();
            _fpsClient.FpsProjectStaffPlan.Returns(_apiClient);
            _service   = new ProjectStaffPlanService(_fpsClient);
        }

        private static QueryParameters<string> DefaultQuery(int page = 1, int pageSize = 10)
            => new QueryParameters<string> { Page = page, PageSize = pageSize };

        private static List<ProjectStaffPlanViewDto> BuildDtoList() =>
        [
            new() { ParentProject = "AH0032", ProgramNo = "Wildlife", Name = "E_WILDLIFE, General",
                    StaffId = "1625", PlannedHours = 25344, ChargeRate = 53.34m,
                    Cost = 1351848.96m, PayCost = 1001341.44m, WorkGroup = "Wildlife", GradeCode = "E" },
            new() { ParentProject = "ED1044", ProgramNo = "SIU",      Name = "C_SVCA, General",
                    StaffId = "1357", PlannedHours = 12000, ChargeRate = 69.92m,
                    Cost = 839040.00m, PayCost = 624720.00m, WorkGroup = "SVCA",     GradeCode = "C" }
        ];

        #region GetPagedAsync — Happy path

        [Fact]
        public async Task GetPagedAsync_WithSuccessResponse_ReturnsDtoList()
        {
            // Arrange
            var query    = DefaultQuery();
            var expected = ApiResponseDto<List<ProjectStaffPlanViewDto>>.SuccessResponse(
                BuildDtoList(),
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 2 });

            _apiClient.GetPagedAsync(query).Returns(expected);

            // Act
            var result = await _service.GetPagedAsync(query);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
        }

        [Fact]
        public async Task GetPagedAsync_WithSuccessResponse_DataMatchesExpected()
        {
            // Arrange
            var query    = DefaultQuery();
            var dtoList  = BuildDtoList();
            var expected = ApiResponseDto<List<ProjectStaffPlanViewDto>>.SuccessResponse(dtoList);

            _apiClient.GetPagedAsync(query).Returns(expected);

            // Act
            var result = await _service.GetPagedAsync(query);

            // Assert
            Assert.Equal("AH0032",           result.Data![0].ParentProject);
            Assert.Equal("Wildlife",          result.Data![0].ProgramNo);
            Assert.Equal(1351848.96m,         result.Data![0].Cost);
            Assert.Equal("ED1044",            result.Data![1].ParentProject);
        }

        [Fact]
        public async Task GetPagedAsync_WithEmptyResult_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var query    = DefaultQuery();
            var expected = ApiResponseDto<List<ProjectStaffPlanViewDto>>.SuccessResponse(
                [],
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 0 });

            _apiClient.GetPagedAsync(query).Returns(expected);

            // Act
            var result = await _service.GetPagedAsync(query);

            // Assert
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetPagedAsync_WithSuccessResponse_PaginationIsPreserved()
        {
            // Arrange
            var query    = DefaultQuery(page: 2, pageSize: 25);
            var expected = ApiResponseDto<List<ProjectStaffPlanViewDto>>.SuccessResponse(
                BuildDtoList(),
                new PaginationDto { PageNumber = 2, PageSize = 25, TotalPages = 4, TotalRecords = 100 });

            _apiClient.GetPagedAsync(query).Returns(expected);

            // Act
            var result = await _service.GetPagedAsync(query);

            // Assert
            Assert.Equal(2,   result.Pagination!.PageNumber);
            Assert.Equal(25,  result.Pagination!.PageSize);
            Assert.Equal(100, result.Pagination!.TotalRecords);
        }

        #endregion

        #region GetPagedAsync — Failure path

        [Fact]
        public async Task GetPagedAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var query  = DefaultQuery();
            var errors = new List<ApiErrorDto> { new() { Message = "API error", Code = "API_ERROR" } };
            var failed = ApiResponseDto<List<ProjectStaffPlanViewDto>>.FailureResponse(errors, new ApiMetaDto());

            _apiClient.GetPagedAsync(query).Returns(failed);

            // Act
            var result = await _service.GetPagedAsync(query);

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("API_ERROR", result.Errors[0].Code);
        }

        [Fact]
        public async Task GetPagedAsync_WhenApiFails_DataIsNull()
        {
            // Arrange
            var query  = DefaultQuery();
            var failed = ApiResponseDto<List<ProjectStaffPlanViewDto>>.FailureResponse([], new ApiMetaDto());

            _apiClient.GetPagedAsync(query).Returns(failed);

            // Act
            var result = await _service.GetPagedAsync(query);

            // Assert
            Assert.False(result.Success);
            Assert.Null(result.Data);
        }

        #endregion

        #region GetPagedAsync — Delegation

        [Fact]
        public async Task GetPagedAsync_Always_DelegatesToApiClient()
        {
            // Arrange
            var query    = DefaultQuery();
            var expected = ApiResponseDto<List<ProjectStaffPlanViewDto>>.SuccessResponse([]);

            _apiClient.GetPagedAsync(query).Returns(expected);

            // Act
            await _service.GetPagedAsync(query);

            // Assert
            await _apiClient.Received(1).GetPagedAsync(query);
        }

        [Fact]
        public async Task GetPagedAsync_Always_AccessesCorrectClientProperty()
        {
            // Arrange
            var query    = DefaultQuery();
            var expected = ApiResponseDto<List<ProjectStaffPlanViewDto>>.SuccessResponse([]);

            _apiClient.GetPagedAsync(query).Returns(expected);

            // Act
            await _service.GetPagedAsync(query);

            // Assert
            _ = _fpsClient.Received(1).FpsProjectStaffPlan;
        }

        #endregion
    }
}
