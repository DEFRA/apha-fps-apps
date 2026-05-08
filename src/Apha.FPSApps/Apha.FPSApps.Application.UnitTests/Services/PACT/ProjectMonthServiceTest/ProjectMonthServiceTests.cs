using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.PactApiClients;
using Apha.FPSApps.Application.Services.PACT;
using NSubstitute;
using Xunit;

namespace Apha.FPSApps.Application.UnitTests.Services.PACT.ProjectMonthServiceTest
{
    public class ProjectMonthServiceTests
    {
        private readonly IPactApiClient _pactClient;
        private readonly IPactProjectMonthApiClient _pactProjectMonthApiClient;
        private readonly ProjectMonthService _service;
        private static readonly string[] value = new[] { "Project is required" };
        private static readonly string[] valueArray = new[] { "MonthNo must be greater than zero" };

        public ProjectMonthServiceTests()
        {
            _pactClient = Substitute.For<IPactApiClient>();
            _pactProjectMonthApiClient = Substitute.For<IPactProjectMonthApiClient>();
            _pactClient.PactProjectMonth.Returns(_pactProjectMonthApiClient);
            _service = new ProjectMonthService(_pactClient);
        }

        #region GetMonthsAsync Tests

        [Fact]
        public async Task GetMonthsAsync_WithData_ReturnsSuccessResponse()
        {
            // Arrange
            var months = new List<MonthDto>
            {
                new() { Monthnumber = 1, Monthname = "January" },
                new() { Monthnumber = 2, Monthname = "February" }
            };
            var expectedResponse = ApiResponseDto<List<MonthDto>>.SuccessResponse(months);
            _pactProjectMonthApiClient.GetMonthsAsync().Returns(expectedResponse);

            // Act
            var result = await _service.GetMonthsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _pactProjectMonthApiClient.Received(1).GetMonthsAsync();
        }

        [Fact]
        public async Task GetMonthsAsync_WithEmptyList_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var expectedResponse = ApiResponseDto<List<MonthDto>>.SuccessResponse(new List<MonthDto>());
            _pactProjectMonthApiClient.GetMonthsAsync().Returns(expectedResponse);

            // Act
            var result = await _service.GetMonthsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetMonthsAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "API Error", Code = "API_ERROR" } };
            var expectedResponse = ApiResponseDto<List<MonthDto>>.FailureResponse(errors, new ApiMetaDto());
            _pactProjectMonthApiClient.GetMonthsAsync().Returns(expectedResponse);

            // Act
            var result = await _service.GetMonthsAsync();

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
        }

        #endregion

        #region GetProjectMonthByProjectAsync Tests

        [Fact]
        public async Task GetProjectMonthByProjectAsync_WithValidProject_ReturnsSuccessResponse()
        {
            // Arrange
            var project = "PRJ1";
            var projectMonths = new List<ProjectMonthDto>
            {
                new ProjectMonthDto { Project = project, MonthNo = 1, CostProfile = 100m },
                new ProjectMonthDto { Project = project, MonthNo = 2, CostProfile = 200m }
            };
            var expectedResponse = ApiResponseDto<List<ProjectMonthDto>>.SuccessResponse(projectMonths);
            _pactProjectMonthApiClient.GetProjectMonthByProjectAsync(project).Returns(expectedResponse);

            // Act
            var result = await _service.GetProjectMonthByProjectAsync(project);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _pactProjectMonthApiClient.Received(1).GetProjectMonthByProjectAsync(project);
        }

        [Fact]
        public async Task GetProjectMonthByProjectAsync_WithNoMatchingRecords_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var project = "PRJ_NONE";
            var expectedResponse = ApiResponseDto<List<ProjectMonthDto>>.SuccessResponse(new List<ProjectMonthDto>());
            _pactProjectMonthApiClient.GetProjectMonthByProjectAsync(project).Returns(expectedResponse);

            // Act
            var result = await _service.GetProjectMonthByProjectAsync(project);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetProjectMonthByProjectAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var project = "PRJ1";
            var errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "API Error", Code = "API_ERROR" } };
            var expectedResponse = ApiResponseDto<List<ProjectMonthDto>>.FailureResponse(errors, new ApiMetaDto());
            _pactProjectMonthApiClient.GetProjectMonthByProjectAsync(project).Returns(expectedResponse);

            // Act
            var result = await _service.GetProjectMonthByProjectAsync(project);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
        }

        #endregion

        #region GetProjectMonthAsync Tests

        [Fact]
        public async Task GetProjectMonthAsync_WithValidProjectAndMonthNo_ReturnsSuccessResponse()
        {
            // Arrange
            var project = "PRJ1";
            var monthNo = 3;
            var projectMonth = new ProjectMonthDto { Project = project, MonthNo = monthNo, CostProfile = 250m };
            var expectedResponse = ApiResponseDto<ProjectMonthDto>.SuccessResponse(projectMonth);
            _pactProjectMonthApiClient.GetProjectMonthAsync(project, monthNo).Returns(expectedResponse);

            // Act
            var result = await _service.GetProjectMonthAsync(project, monthNo);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(project, result.Data?.Project);
            Assert.Equal(monthNo, result.Data?.MonthNo);
            await _pactProjectMonthApiClient.Received(1).GetProjectMonthAsync(project, monthNo);
        }

        [Fact]
        public async Task GetProjectMonthAsync_WhenNotFound_ReturnsFailureResponse()
        {
            // Arrange
            var project = "PRJ_NONE";
            var monthNo = 99;
            var errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Not Found", Code = "NOT_FOUND" } };
            var expectedResponse = ApiResponseDto<ProjectMonthDto>.FailureResponse(errors, new ApiMetaDto());
            _pactProjectMonthApiClient.GetProjectMonthAsync(project, monthNo).Returns(expectedResponse);

            // Act
            var result = await _service.GetProjectMonthAsync(project, monthNo);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
        }

        [Fact]
        public async Task GetProjectMonthAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var project = "PRJ1";
            var monthNo = 1;
            var errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "API Error", Code = "API_ERROR" } };
            var expectedResponse = ApiResponseDto<ProjectMonthDto>.FailureResponse(errors, new ApiMetaDto());
            _pactProjectMonthApiClient.GetProjectMonthAsync(project, monthNo).Returns(expectedResponse);

            // Act
            var result = await _service.GetProjectMonthAsync(project, monthNo);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotEmpty(result.Errors!);
        }

        #endregion

        #region CreateProjectMonthAsync Tests

        [Fact]
        public async Task CreateProjectMonthAsync_WithValidDto_ReturnsSuccessResponse()
        {
            // Arrange
            var dto = new ProjectMonthDto { Project = "PRJ1", MonthNo = 1, CostProfile = 100m };
            var createdDto = new ProjectMonthDto { Project = "PRJ1", MonthNo = 1, CostProfile = 100m };
            var expectedResponse = ApiResponseDto<ProjectMonthDto>.SuccessResponse(createdDto);
            _pactProjectMonthApiClient.CreateProjectMonthAsync(dto).Returns(expectedResponse);

            // Act
            var result = await _service.CreateProjectMonthAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal("PRJ1", result.Data?.Project);
            Assert.Equal(1, result.Data?.MonthNo);
            await _pactProjectMonthApiClient.Received(1).CreateProjectMonthAsync(dto);
        }

        [Fact]
        public async Task CreateProjectMonthAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var dto = new ProjectMonthDto { Project = "PRJ1", MonthNo = 1 };
            var errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Create failed", Code = "CREATE_ERROR" } };
            var expectedResponse = ApiResponseDto<ProjectMonthDto>.FailureResponse(errors, new ApiMetaDto());
            _pactProjectMonthApiClient.CreateProjectMonthAsync(dto).Returns(expectedResponse);

            // Act
            var result = await _service.CreateProjectMonthAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
        }

        [Fact]
        public async Task CreateProjectMonthAsync_WhenValidationFails_ReturnsValidationFailureResponse()
        {
            // Arrange
            var dto = new ProjectMonthDto { Project = "", MonthNo = 0 };
            var expectedResponse = ApiResponseDto<ProjectMonthDto>.ValidationFailure(
                "Validation failed",
                new Dictionary<string, string[]> { { "Project", value } });
            _pactProjectMonthApiClient.CreateProjectMonthAsync(dto).Returns(expectedResponse);

            // Act
            var result = await _service.CreateProjectMonthAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Equal("VALIDATION_ERROR", result.Errors.First().Code);
        }

        #endregion

        #region UpdateProjectMonthAsync Tests

        [Fact]
        public async Task UpdateProjectMonthAsync_WithValidDto_ReturnsSuccessResponse()
        {
            // Arrange
            var dto = new ProjectMonthDto { Project = "PRJ1", MonthNo = 2, CostProfile = 500m };
            var updatedDto = new ProjectMonthDto { Project = "PRJ1", MonthNo = 2, CostProfile = 500m };
            var expectedResponse = ApiResponseDto<ProjectMonthDto>.SuccessResponse(updatedDto);
            _pactProjectMonthApiClient.UpdateProjectMonthAsync(dto).Returns(expectedResponse);

            // Act
            var result = await _service.UpdateProjectMonthAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal("PRJ1", result.Data?.Project);
            Assert.Equal(2, result.Data?.MonthNo);
            await _pactProjectMonthApiClient.Received(1).UpdateProjectMonthAsync(dto);
        }

        [Fact]
        public async Task UpdateProjectMonthAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var dto = new ProjectMonthDto { Project = "PRJ1", MonthNo = 2 };
            var errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Update failed", Code = "UPDATE_ERROR" } };
            var expectedResponse = ApiResponseDto<ProjectMonthDto>.FailureResponse(errors, new ApiMetaDto());
            _pactProjectMonthApiClient.UpdateProjectMonthAsync(dto).Returns(expectedResponse);

            // Act
            var result = await _service.UpdateProjectMonthAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
        }

        [Fact]
        public async Task UpdateProjectMonthAsync_WhenValidationFails_ReturnsValidationFailureResponse()
        {
            // Arrange
            var dto = new ProjectMonthDto { Project = "", MonthNo = 0 };
            var expectedResponse = ApiResponseDto<ProjectMonthDto>.ValidationFailure(
                "Validation failed",
                new Dictionary<string, string[]> { { "MonthNo", valueArray } });
            _pactProjectMonthApiClient.UpdateProjectMonthAsync(dto).Returns(expectedResponse);

            // Act
            var result = await _service.UpdateProjectMonthAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Equal("VALIDATION_ERROR", result.Errors.First().Code);
        }

        #endregion

        #region DeleteProjectMonthAsync Tests

        [Fact]
        public async Task DeleteProjectMonthAsync_WithValidProjectAndMonthNo_ReturnsSuccessTrue()
        {
            // Arrange
            var project = "PRJ1";
            var monthNo = 1;
            var expectedResponse = ApiResponseDto<bool>.SuccessResponse(true);
            _pactProjectMonthApiClient.DeleteProjectMonthAsync(project, monthNo).Returns(expectedResponse);

            // Act
            var result = await _service.DeleteProjectMonthAsync(project, monthNo);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
            await _pactProjectMonthApiClient.Received(1).DeleteProjectMonthAsync(project, monthNo);
        }

        [Fact]
        public async Task DeleteProjectMonthAsync_WhenNotFound_ReturnsSuccessFalse()
        {
            // Arrange
            var project = "PRJ_NONE";
            var monthNo = 99;
            var expectedResponse = ApiResponseDto<bool>.SuccessResponse(false);
            _pactProjectMonthApiClient.DeleteProjectMonthAsync(project, monthNo).Returns(expectedResponse);

            // Act
            var result = await _service.DeleteProjectMonthAsync(project, monthNo);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.False(result.Data);
            await _pactProjectMonthApiClient.Received(1).DeleteProjectMonthAsync(project, monthNo);
        }

        [Fact]
        public async Task DeleteProjectMonthAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var project = "PRJ1";
            var monthNo = 1;
            var errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Delete failed", Code = "DELETE_ERROR" } };
            var expectedResponse = ApiResponseDto<bool>.FailureResponse(errors, new ApiMetaDto());
            _pactProjectMonthApiClient.DeleteProjectMonthAsync(project, monthNo).Returns(expectedResponse);

            // Act
            var result = await _service.DeleteProjectMonthAsync(project, monthNo);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
        }

        #endregion
    }
}
