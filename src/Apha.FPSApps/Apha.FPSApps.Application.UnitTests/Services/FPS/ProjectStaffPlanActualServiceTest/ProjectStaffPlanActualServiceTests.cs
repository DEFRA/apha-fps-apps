using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Application.Services.FPS;
using NSubstitute;
using Xunit;

namespace Apha.FPSApps.Application.UnitTests.Services.FPS.ProjectStaffPlanActualServiceTest
{
    public class ProjectStaffPlanActualServiceTests
    {
        private readonly IFpsApiClient _fpsClient;
        private readonly IFpsProjectStaffPlanActualApiClient _apiClient;
        private readonly ProjectStaffPlanActualService _service;

        public ProjectStaffPlanActualServiceTests()
        {
            _fpsClient = Substitute.For<IFpsApiClient>();
            _apiClient = Substitute.For<IFpsProjectStaffPlanActualApiClient>();
            _fpsClient.FpsProjectStaffPlanActual.Returns(_apiClient);
            _service   = new ProjectStaffPlanActualService(_fpsClient);
        }

        private static QueryParameters<string> DefaultQuery(int page = 1, int pageSize = 10)
            => new QueryParameters<string> { Page = page, PageSize = pageSize };

        #region GetTimeCostCalcsByProjectAsync — Happy path

        [Fact]
        public async Task GetTimeCostCalcsByProjectAsync_WithSuccessResponse_ReturnsDtoList()
        {
            // Arrange
            var query       = DefaultQuery();
            var projectCode = "AH0033";
            var items = new List<TimeCostCalcsViewDto>
            {
                new() { Project = projectCode, StaffId = "S01", Name = "Alice", WorkGroup = "WG1", GradeCode = "G1", JobCode = "JB1", Month = 1, Time = 8, Cost = 100 },
                new() { Project = projectCode, StaffId = "S02", Name = "Bob",   WorkGroup = "WG2", GradeCode = "G2", JobCode = "JB2", Month = 2, Time = 6, Cost = 80  }
            };
            var expectedResponse = ApiResponseDto<List<TimeCostCalcsViewDto>>.SuccessResponse(
                items,
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 2 });

            _apiClient.GetTimeCostCalcsByProjectAsync(query, projectCode).Returns(expectedResponse);

            // Act
            var result = await _service.GetTimeCostCalcsByProjectAsync(query, projectCode);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _apiClient.Received(1).GetTimeCostCalcsByProjectAsync(query, projectCode);
        }

        [Fact]
        public async Task GetTimeCostCalcsByProjectAsync_WithEmptyResult_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var query           = DefaultQuery();
            var projectCode     = "AH0033";
            var expectedResponse = ApiResponseDto<List<TimeCostCalcsViewDto>>.SuccessResponse(
                new List<TimeCostCalcsViewDto>(),
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 0 });

            _apiClient.GetTimeCostCalcsByProjectAsync(query, projectCode).Returns(expectedResponse);

            // Act
            var result = await _service.GetTimeCostCalcsByProjectAsync(query, projectCode);

            // Assert
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetTimeCostCalcsByProjectAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var query       = DefaultQuery();
            var projectCode = "AH0033";
            var errors      = new List<ApiErrorDto> { new() { Message = "API error", Code = "API_ERROR" } };
            var failResponse = ApiResponseDto<List<TimeCostCalcsViewDto>>.FailureResponse(errors, new ApiMetaDto());

            _apiClient.GetTimeCostCalcsByProjectAsync(query, projectCode).Returns(failResponse);

            // Act
            var result = await _service.GetTimeCostCalcsByProjectAsync(query, projectCode);

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
        }

        #endregion

        #region GetTimeCostCalcsByProjectAsync — Delegation

        [Fact]
        public async Task GetTimeCostCalcsByProjectAsync_DelegatesToFpsProjPlanVsActualsStaffApiClient()
        {
            // Arrange
            var query       = DefaultQuery();
            var projectCode = "AH0033";
            var response    = ApiResponseDto<List<TimeCostCalcsViewDto>>.SuccessResponse(new List<TimeCostCalcsViewDto>());

            _apiClient.GetTimeCostCalcsByProjectAsync(query, projectCode).Returns(response);

            // Act
            await _service.GetTimeCostCalcsByProjectAsync(query, projectCode);

            // Assert
            await _apiClient.Received(1).GetTimeCostCalcsByProjectAsync(query, projectCode);
            _ = _fpsClient.Received(1).FpsProjectStaffPlanActual;
        }

        [Theory]
        [InlineData("AH0033")]
        [InlineData("PROJ001")]
        [InlineData("BCP-OPS")]
        public async Task GetTimeCostCalcsByProjectAsync_PassesCorrectProjectCodeToApiClient(string projectCode)
        {
            // Arrange
            var query    = DefaultQuery();
            var response = ApiResponseDto<List<TimeCostCalcsViewDto>>.SuccessResponse(new List<TimeCostCalcsViewDto>());

            _apiClient.GetTimeCostCalcsByProjectAsync(query, projectCode).Returns(response);

            // Act
            await _service.GetTimeCostCalcsByProjectAsync(query, projectCode);

            // Assert
            await _apiClient.Received(1).GetTimeCostCalcsByProjectAsync(query, projectCode);
        }

        #endregion

        #region GetTotalActualByProjectAsync

        [Fact]
        public async Task GetTotalActualByProjectAsync_WithSuccessResponse_ReturnsDto()
        {
            // Arrange
            var projectCode = "AH0033";
            var totals      = new TimeCostCalcsTotalsDto { TotalHours = 40.5, TotalCost = 5000.0 };
            _apiClient.GetTotalActualByProjectAsync(projectCode)
                .Returns(ApiResponseDto<TimeCostCalcsTotalsDto>.SuccessResponse(totals));

            // Act
            var result = await _service.GetTotalActualByProjectAsync(projectCode);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(40.5,   result.Data?.TotalHours);
            Assert.Equal(5000.0, result.Data?.TotalCost);
            await _apiClient.Received(1).GetTotalActualByProjectAsync(projectCode);
        }

        [Fact]
        public async Task GetTotalActualByProjectAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var projectCode = "AH0033";
            var errors      = new List<ApiErrorDto> { new() { Code = "ERROR", Message = "API error" } };
            _apiClient.GetTotalActualByProjectAsync(projectCode)
                .Returns(ApiResponseDto<TimeCostCalcsTotalsDto>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _service.GetTotalActualByProjectAsync(projectCode);

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        [Fact]
        public async Task GetTotalActualByProjectAsync_DelegatesToApiClient()
        {
            // Arrange
            var projectCode = "AH0033";
            _apiClient.GetTotalActualByProjectAsync(projectCode)
                .Returns(ApiResponseDto<TimeCostCalcsTotalsDto>.SuccessResponse(new TimeCostCalcsTotalsDto()));

            // Act
            await _service.GetTotalActualByProjectAsync(projectCode);

            // Assert
            await _apiClient.Received(1).GetTotalActualByProjectAsync(projectCode);
            _ = _fpsClient.Received(1).FpsProjectStaffPlanActual;
        }

        #endregion

        #region DeleteTimeCostCalcsAsync

        [Fact]
        public async Task DeleteTimeCostCalcsAsync_WithSuccessResponse_ReturnsSuccess()
        {
            // Arrange
            _apiClient.DeleteTimeCostCalcsAsync("WG1", "JOB1", "AH0033", 1, "S01")
                .Returns(ApiResponseDto<bool>.SuccessResponse(true));

            // Act
            var result = await _service.DeleteTimeCostCalcsAsync("WG1", "JOB1", "AH0033", 1, "S01");

            // Assert
            Assert.True(result.Success);
            Assert.True(result.Data);
        }

        [Fact]
        public async Task DeleteTimeCostCalcsAsync_WhenApiFails_ReturnsFailure()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Code = "NOT_FOUND", Message = "Not found" } };
            _apiClient.DeleteTimeCostCalcsAsync("WG1", "JOB1", "AH0033", 1, "S01")
                .Returns(ApiResponseDto<bool>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _service.DeleteTimeCostCalcsAsync("WG1", "JOB1", "AH0033", 1, "S01");

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task DeleteTimeCostCalcsAsync_DelegatesToApiClient()
        {
            // Arrange
            _apiClient.DeleteTimeCostCalcsAsync("WG1", "JOB1", "AH0033", 3.5, "S01")
                .Returns(ApiResponseDto<bool>.SuccessResponse(true));

            // Act
            await _service.DeleteTimeCostCalcsAsync("WG1", "JOB1", "AH0033", 3.5, "S01");

            // Assert
            await _apiClient.Received(1).DeleteTimeCostCalcsAsync("WG1", "JOB1", "AH0033", 3.5, "S01");
            _ = _fpsClient.Received(1).FpsProjectStaffPlanActual;
        }

        #endregion
    }
}
