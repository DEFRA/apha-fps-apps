using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Application.Services.FPS;
using NSubstitute;
using Xunit;

namespace Apha.FPSApps.Application.UnitTests.Services.FPS.WorkGroupEmployeeServiceTest
{
    public class WorkGroupEmployeeServiceTests
    {
        private const string DefaultWgGrade = "WG01";
        private const string DefaultPactId  = "PACT001";

        private readonly IFpsApiClient _fpsClient;
        private readonly IFpsWorkGroupEmployeeApiClient _fpsWgEmployeeApiClient;
        private readonly WorkGroupEmployeeService _sut;

        public WorkGroupEmployeeServiceTests()
        {
            _fpsClient              = Substitute.For<IFpsApiClient>();
            _fpsWgEmployeeApiClient = Substitute.For<IFpsWorkGroupEmployeeApiClient>();
            _fpsClient.FpsWorkGroupEmployee.Returns(_fpsWgEmployeeApiClient);
            _sut = new WorkGroupEmployeeService(_fpsClient);
        }

        #region GetWorkGroupEmployeeAsync Tests

        [Fact]
        public async Task GetWorkGroupEmployeeAsync_WithSuccessResponse_ReturnsEmployeeList()
        {
            // Arrange
            var employees = new List<WorkGroupEmployeeDto>
            {
                new() { PactId = DefaultPactId, SpNumber = "SP001", WorkGroupGrade = DefaultWgGrade }
            };
            var expectedResponse = ApiResponseDto<List<WorkGroupEmployeeDto>>.SuccessResponse(employees);
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };

            _fpsWgEmployeeApiClient.GetWorkGroupEmployeeAsync(query, DefaultWgGrade).Returns(expectedResponse);

            // Act
            var result = await _sut.GetWorkGroupEmployeeAsync(query, DefaultWgGrade);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Single(result.Data!);
            await _fpsWgEmployeeApiClient.Received(1).GetWorkGroupEmployeeAsync(query, DefaultWgGrade);
        }

        [Fact]
        public async Task GetWorkGroupEmployeeAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Message = "API Error", Code = "API_ERROR" } };
            var expectedResponse = ApiResponseDto<List<WorkGroupEmployeeDto>>.FailureResponse(errors, new ApiMetaDto());
            var query = new QueryParameters<string>();

            _fpsWgEmployeeApiClient.GetWorkGroupEmployeeAsync(query, DefaultWgGrade).Returns(expectedResponse);

            // Act
            var result = await _sut.GetWorkGroupEmployeeAsync(query, DefaultWgGrade);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        #endregion

        #region GetWorkGroupEmployeeByIdAsync Tests

        [Fact]
        public async Task GetWorkGroupEmployeeByIdAsync_WithSuccessResponse_ReturnsEmployee()
        {
            // Arrange
            var dto = new WorkGroupEmployeeDto { PactId = DefaultPactId, SpNumber = "SP001" };
            var expectedResponse = ApiResponseDto<WorkGroupEmployeeDto>.SuccessResponse(dto);

            _fpsWgEmployeeApiClient.GetWorkGroupEmployeeByIdAsync(DefaultPactId).Returns(expectedResponse);

            // Act
            var result = await _sut.GetWorkGroupEmployeeByIdAsync(DefaultPactId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(DefaultPactId, result.Data?.PactId);
            await _fpsWgEmployeeApiClient.Received(1).GetWorkGroupEmployeeByIdAsync(DefaultPactId);
        }

        [Fact]
        public async Task GetWorkGroupEmployeeByIdAsync_WhenNotFound_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Message = "Not found", Code = "NOT_FOUND" } };
            var expectedResponse = ApiResponseDto<WorkGroupEmployeeDto>.FailureResponse(errors, new ApiMetaDto());

            _fpsWgEmployeeApiClient.GetWorkGroupEmployeeByIdAsync(DefaultPactId).Returns(expectedResponse);

            // Act
            var result = await _sut.GetWorkGroupEmployeeByIdAsync(DefaultPactId);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        #endregion

        #region UpdateWorkGroupEmployeeAsync Tests

        [Fact]
        public async Task UpdateWorkGroupEmployeeAsync_WithValidDto_ReturnsSuccessResponse()
        {
            // Arrange
            var dto     = new WorkGroupEmployeeDto { PactId = DefaultPactId, HrsPaid = 40.0 };
            var updated = new WorkGroupEmployeeDto { PactId = DefaultPactId, HrsPaid = 40.0 };
            var expectedResponse = ApiResponseDto<WorkGroupEmployeeDto>.SuccessResponse(updated);

            _fpsWgEmployeeApiClient.UpdateWorkGroupEmployeeAsync(dto).Returns(expectedResponse);

            // Act
            var result = await _sut.UpdateWorkGroupEmployeeAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(DefaultPactId, result.Data?.PactId);
            await _fpsWgEmployeeApiClient.Received(1).UpdateWorkGroupEmployeeAsync(dto);
        }

        [Fact]
        public async Task UpdateWorkGroupEmployeeAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var dto    = new WorkGroupEmployeeDto { PactId = DefaultPactId };
            var errors = new List<ApiErrorDto> { new() { Message = "Update failed", Code = "ERR" } };
            var expectedResponse = ApiResponseDto<WorkGroupEmployeeDto>.FailureResponse(errors, new ApiMetaDto());

            _fpsWgEmployeeApiClient.UpdateWorkGroupEmployeeAsync(dto).Returns(expectedResponse);

            // Act
            var result = await _sut.UpdateWorkGroupEmployeeAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        #endregion

        #region DeleteWorkGroupEmployeeAsync Tests

        [Fact]
        public async Task DeleteWorkGroupEmployeeAsync_WithSuccessResponse_ReturnsSuccess()
        {
            // Arrange
            var expectedResponse = ApiResponseDto<bool>.SuccessResponse(true);

            _fpsWgEmployeeApiClient.DeleteWorkGroupEmployeeAsync(DefaultPactId).Returns(expectedResponse);

            // Act
            var result = await _sut.DeleteWorkGroupEmployeeAsync(DefaultPactId);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            await _fpsWgEmployeeApiClient.Received(1).DeleteWorkGroupEmployeeAsync(DefaultPactId);
        }

        [Fact]
        public async Task DeleteWorkGroupEmployeeAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Message = "Delete failed", Code = "ERR" } };
            var expectedResponse = ApiResponseDto<bool>.FailureResponse(errors, new ApiMetaDto());

            _fpsWgEmployeeApiClient.DeleteWorkGroupEmployeeAsync(DefaultPactId).Returns(expectedResponse);

            // Act
            var result = await _sut.DeleteWorkGroupEmployeeAsync(DefaultPactId);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
        }

        #endregion
    }
}
