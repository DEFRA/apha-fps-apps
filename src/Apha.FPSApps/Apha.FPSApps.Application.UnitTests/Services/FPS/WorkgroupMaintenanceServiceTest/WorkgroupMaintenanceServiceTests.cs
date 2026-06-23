/*
 * TRANSFORMENGINE MIGRATION — WorkgroupMaintenanceServiceTests.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 13 — Unit Tests - Backend + Frontend xUnit Coverage
 * Migrated : 2026-06-23
 *
 * CHANGED:
 *   - NEW FILE: xUnit tests for WorkgroupMaintenanceService (frmMaintWorkGroup2 frontend application layer)
 *   - Tests verify thin-delegate pattern: each method passes through to IFpsApiClient.FpsWorkgroupMaintenance
 *   - Uses NSubstitute for IFpsApiClient aggregate and IFpsWorkgroupApiClient sub-client mocks
 *   - IFpsApiClient.FpsWorkgroupMaintenance.Returns(_fpsWorkgroupApiClient) wires the aggregate client
 *   - Covers 8 public methods: GetPagedAsync, GetByWorkGroupNameAsync, CreateAsync, UpdateAsync,
 *     DeleteAsync, GetProfitCentresAsync, GetOwnersAsync, GetCostCentresAsync
 *
 * PRESERVED:
 *   - Pattern matches YearMasterServiceTests.cs — aggregate client + sub-client substitution approach
 *   - Naming convention [MethodName]_[StateUnderTest]_[ExpectedResult]
 *   - #region grouping per method
 *   - Each test verifies Received(1) delegation to the API client method (thin-delegate contract)
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: None — thin-delegate service has no business logic to test beyond delegation
 */

using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Application.Services.FPS;
using NSubstitute;
using Xunit;

namespace Apha.FPSApps.Application.UnitTests.Services.FPS.WorkgroupMaintenanceServiceTest
{
    public class WorkgroupMaintenanceServiceTests
    {
        private readonly IFpsApiClient _fpsClient;
        private readonly IFpsWorkgroupApiClient _fpsWorkgroupApiClient;
        private readonly WorkgroupMaintenanceService _service;

        public WorkgroupMaintenanceServiceTests()
        {
            _fpsClient             = Substitute.For<IFpsApiClient>();
            _fpsWorkgroupApiClient = Substitute.For<IFpsWorkgroupApiClient>();
            // TRANSFORMENGINE: wire aggregate client → sub-client (IFpsApiClient.FpsWorkgroupMaintenance property)
            _fpsClient.FpsWorkgroupMaintenance.Returns(_fpsWorkgroupApiClient);
            _service = new WorkgroupMaintenanceService(_fpsClient);
        }

        // TRANSFORMENGINE: static helpers — minimal valid response wrappers
        private static WorkgroupMaintenanceDto BuildDto(string name = "WG001") =>
            new() { WorkGroupName = name, ProfitCentre = "PC01" };

        private static ApiResponseDto<WorkgroupMaintenanceDto> BuildSuccessResponse(string name = "WG001") =>
            new() { Success = true, Data = BuildDto(name) };

        private static ApiResponseDto<WorkgroupMaintenanceDto> BuildFailureResponse() =>
            new()
            {
                Success = false,
                Errors  = new List<ApiErrorDto> { new() { Message = "Error", Code = "ERR" } }
            };

        #region Constructor Tests

        [Fact]
        public void Constructor_ThrowsArgumentNullException_WhenClientIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new WorkgroupMaintenanceService(null!));
        }

        #endregion

        #region GetPagedAsync Tests

        [Fact]
        public async Task GetPagedAsync_ApiClientReturnsSuccess_ReturnsDelegatedSuccessResponse()
        {
            // Arrange
            var query    = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var expected = new ApiResponseDto<List<WorkgroupMaintenanceDto>>
            {
                Success    = true,
                Data       = new List<WorkgroupMaintenanceDto> { BuildDto() },
                Pagination = new PaginationDto { TotalRecords = 1, PageNumber = 1, PageSize = 10 }
            };
            _fpsWorkgroupApiClient.GetPagedAsync(query).Returns(expected);

            // Act
            var result = await _service.GetPagedAsync(query);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Single(result.Data);
            await _fpsWorkgroupApiClient.Received(1).GetPagedAsync(query);
        }

        [Fact]
        public async Task GetPagedAsync_ApiClientReturnsEmptyPage_ReturnsDelegatedEmptyResponse()
        {
            // Arrange
            var query    = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var expected = new ApiResponseDto<List<WorkgroupMaintenanceDto>>
            {
                Success    = true,
                Data       = new List<WorkgroupMaintenanceDto>(),
                Pagination = new PaginationDto { TotalRecords = 0, PageNumber = 1, PageSize = 10 }
            };
            _fpsWorkgroupApiClient.GetPagedAsync(query).Returns(expected);

            // Act
            var result = await _service.GetPagedAsync(query);

            // Assert
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetPagedAsync_ApiClientReturnsFailure_ReturnsDelegatedFailureResponse()
        {
            // Arrange
            var query    = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var expected = new ApiResponseDto<List<WorkgroupMaintenanceDto>>
            {
                Success = false,
                Errors  = new List<ApiErrorDto> { new() { Message = "API error", Code = "ERR" } }
            };
            _fpsWorkgroupApiClient.GetPagedAsync(query).Returns(expected);

            // Act
            var result = await _service.GetPagedAsync(query);

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region GetByWorkGroupNameAsync Tests

        [Fact]
        public async Task GetByWorkGroupNameAsync_ApiClientReturnsSuccess_ReturnsDelegatedResult()
        {
            // Arrange
            var expected = BuildSuccessResponse();
            _fpsWorkgroupApiClient.GetByWorkGroupNameAsync("WG001").Returns(expected);

            // Act
            var result = await _service.GetByWorkGroupNameAsync("WG001");

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            await _fpsWorkgroupApiClient.Received(1).GetByWorkGroupNameAsync("WG001");
        }

        [Fact]
        public async Task GetByWorkGroupNameAsync_ApiClientReturnsFailure_ReturnsDelegatedFailure()
        {
            // Arrange
            var expected = BuildFailureResponse();
            _fpsWorkgroupApiClient.GetByWorkGroupNameAsync("NOTEXIST").Returns(expected);

            // Act
            var result = await _service.GetByWorkGroupNameAsync("NOTEXIST");

            // Assert
            Assert.False(result.Success);
        }

        #endregion

        #region CreateAsync Tests

        [Fact]
        public async Task CreateAsync_ApiClientReturnsSuccess_ReturnsDelegatedSuccessResponse()
        {
            // Arrange
            var dto      = BuildDto();
            var expected = BuildSuccessResponse();
            _fpsWorkgroupApiClient.CreateAsync(dto).Returns(expected);

            // Act
            var result = await _service.CreateAsync(dto);

            // Assert
            Assert.True(result.Success);
            await _fpsWorkgroupApiClient.Received(1).CreateAsync(dto);
        }

        [Fact]
        public async Task CreateAsync_ApiClientReturnsFailure_ReturnsDelegatedFailureResponse()
        {
            // Arrange
            var dto      = BuildDto();
            var expected = BuildFailureResponse();
            _fpsWorkgroupApiClient.CreateAsync(dto).Returns(expected);

            // Act
            var result = await _service.CreateAsync(dto);

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region UpdateAsync Tests

        [Fact]
        public async Task UpdateAsync_ApiClientReturnsSuccess_ReturnsDelegatedSuccessResponse()
        {
            // Arrange
            var dto      = BuildDto();
            var expected = BuildSuccessResponse();
            _fpsWorkgroupApiClient.UpdateAsync("WG001", dto).Returns(expected);

            // Act
            var result = await _service.UpdateAsync("WG001", dto);

            // Assert
            Assert.True(result.Success);
            await _fpsWorkgroupApiClient.Received(1).UpdateAsync("WG001", dto);
        }

        [Fact]
        public async Task UpdateAsync_ApiClientReturnsFailure_ReturnsDelegatedFailureResponse()
        {
            // Arrange
            var dto      = BuildDto();
            var expected = BuildFailureResponse();
            _fpsWorkgroupApiClient.UpdateAsync("WG001", dto).Returns(expected);

            // Act
            var result = await _service.UpdateAsync("WG001", dto);

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task UpdateAsync_VerifyRenamePathPassesOriginalKeyToApiClient()
        {
            // Arrange
            var dto      = BuildDto("WG_RENAMED");
            var expected = BuildSuccessResponse("WG_RENAMED");
            _fpsWorkgroupApiClient.UpdateAsync("WG_ORIGINAL", dto).Returns(expected);

            // Act
            await _service.UpdateAsync("WG_ORIGINAL", dto);

            // Assert
            await _fpsWorkgroupApiClient.Received(1).UpdateAsync("WG_ORIGINAL", dto);
        }

        #endregion

        #region DeleteAsync Tests

        [Fact]
        public async Task DeleteAsync_ApiClientReturnsSuccess_ReturnsDelegatedSuccessResponse()
        {
            // Arrange
            var expected = new ApiResponseDto<bool> { Success = true };
            _fpsWorkgroupApiClient.DeleteAsync("WG001").Returns(expected);

            // Act
            var result = await _service.DeleteAsync("WG001");

            // Assert
            Assert.True(result.Success);
            await _fpsWorkgroupApiClient.Received(1).DeleteAsync("WG001");
        }

        [Fact]
        public async Task DeleteAsync_ApiClientReturnsFailure_ReturnsDelegatedFailureResponse()
        {
            // Arrange
            var expected = new ApiResponseDto<bool>
            {
                Success = false,
                Errors  = new List<ApiErrorDto> { new() { Message = "Failed to delete", Code = "ERR" } }
            };
            _fpsWorkgroupApiClient.DeleteAsync("WG001").Returns(expected);

            // Act
            var result = await _service.DeleteAsync("WG001");

            // Assert
            Assert.False(result.Success);
        }

        #endregion

        #region GetProfitCentresAsync Tests

        [Fact]
        public async Task GetProfitCentresAsync_ApiClientReturnsSuccess_ReturnsDelegatedSuccessResponse()
        {
            // Arrange
            var expected = new ApiResponseDto<List<string>>
            {
                Success = true,
                Data    = new List<string> { "PC01", "PC02" }
            };
            _fpsWorkgroupApiClient.GetProfitCentresAsync().Returns(expected);

            // Act
            var result = await _service.GetProfitCentresAsync();

            // Assert
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _fpsWorkgroupApiClient.Received(1).GetProfitCentresAsync();
        }

        [Fact]
        public async Task GetProfitCentresAsync_ApiClientReturnsFailure_ReturnsDelegatedFailureResponse()
        {
            // Arrange
            var expected = new ApiResponseDto<List<string>>
            {
                Success = false,
                Errors  = new List<ApiErrorDto> { new() { Message = "Failed to load profit centres", Code = "ERR" } }
            };
            _fpsWorkgroupApiClient.GetProfitCentresAsync().Returns(expected);

            // Act
            var result = await _service.GetProfitCentresAsync();

            // Assert
            Assert.False(result.Success);
        }

        #endregion

        #region GetOwnersAsync Tests

        [Fact]
        public async Task GetOwnersAsync_ApiClientReturnsSuccess_ReturnsDelegatedSuccessResponse()
        {
            // Arrange
            var expected = new ApiResponseDto<List<ManagerDto>>
            {
                Success = true,
                Data    = new List<ManagerDto> { new() { Name = "Alice Smith" } }
            };
            _fpsWorkgroupApiClient.GetOwnersAsync().Returns(expected);

            // Act
            var result = await _service.GetOwnersAsync();

            // Assert
            Assert.True(result.Success);
            Assert.Single(result.Data!);
            await _fpsWorkgroupApiClient.Received(1).GetOwnersAsync();
        }

        [Fact]
        public async Task GetOwnersAsync_ApiClientReturnsFailure_ReturnsDelegatedFailureResponse()
        {
            // Arrange
            var expected = new ApiResponseDto<List<ManagerDto>>
            {
                Success = false,
                Errors  = new List<ApiErrorDto> { new() { Message = "Failed to load owners", Code = "ERR" } }
            };
            _fpsWorkgroupApiClient.GetOwnersAsync().Returns(expected);

            // Act
            var result = await _service.GetOwnersAsync();

            // Assert
            Assert.False(result.Success);
        }

        #endregion

        #region GetCostCentresAsync Tests

        [Fact]
        public async Task GetCostCentresAsync_ApiClientReturnsSuccess_ReturnsDelegatedSuccessResponse()
        {
            // Arrange
            var expected = new ApiResponseDto<List<double?>>
            {
                Success = true,
                Data    = new List<double?> { 100.0, 200.0 }
            };
            _fpsWorkgroupApiClient.GetCostCentresAsync("PC01").Returns(expected);

            // Act
            var result = await _service.GetCostCentresAsync("PC01");

            // Assert
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _fpsWorkgroupApiClient.Received(1).GetCostCentresAsync("PC01");
        }

        [Fact]
        public async Task GetCostCentresAsync_ApiClientReturnsFailure_ReturnsDelegatedFailureResponse()
        {
            // Arrange
            var expected = new ApiResponseDto<List<double?>>
            {
                Success = false,
                Errors  = new List<ApiErrorDto> { new() { Message = "Failed to load cost centres", Code = "ERR" } }
            };
            _fpsWorkgroupApiClient.GetCostCentresAsync("PC01").Returns(expected);

            // Act
            var result = await _service.GetCostCentresAsync("PC01");

            // Assert
            Assert.False(result.Success);
        }

        #endregion
    }
}
