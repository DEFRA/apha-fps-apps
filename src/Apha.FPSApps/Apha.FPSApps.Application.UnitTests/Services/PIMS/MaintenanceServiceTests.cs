/*
 * TRANSFORMENGINE MIGRATION — MaintenanceServiceTests.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 13 — Unit Tests - Backend + Frontend xUnit Coverage
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - Expanded xUnit test class for Apha.FPSApps.Application.Services.PIMS.MaintenanceService
 *   - Added full coverage for all 14 sub-client surfaces exposed by IMaintenanceService:
 *       Report, ReportGroup, ReportGroupLink, ProjectManager, ProgramManagerLink,
 *       ProfitCentreManagerLink, Setting, AccessUser, AccessLevel, AccessUserLevel,
 *       AccessSystem (read-only), Frequency, ReviewItem, RadTrackProg
 *   - Added missing sub-client mocks: IPimsReportGroupApiClient, IPimsReportGroupLinkApiClient,
 *       IPimsProgramManagerLinkApiClient, IPimsProfitCentreManagerLinkApiClient,
 *       IPimsAccessLevelApiClient, IPimsAccessUserLevelApiClient, IPimsAccessSystemApiClient
 *   - Added missing test scenarios for Frequency (GetById, Create, Update),
 *       Setting (GetAll, GetById), AccessUser (GetById), ReviewItem, RadTrackProg
 *   - Uses NSubstitute for IPimsApiClient and all sub-client interfaces
 *
 * PRESERVED:
 *   - Single delegation pattern: each method is a single return await call
 *   - Sub-client property access pattern (_client.PimsXxx)
 *   - All previously passing test method bodies
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - none — fully automated.
 */
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Application.Interfaces.PimsApiClients;
using Apha.FPSApps.Application.Services.PIMS;
using NSubstitute;
using Xunit;

namespace Apha.FPSApps.Application.UnitTests.Services.PIMS
{
    public class MaintenanceServiceTests
    {
        // TRANSFORMENGINE: aggregate client mock + one field per sub-client interface
        private readonly IPimsApiClient _pimsClient;
        private readonly IPimsReportApiClient _pimsReportApiClient;
        private readonly IPimsReportGroupApiClient _pimsReportGroupApiClient;
        private readonly IPimsReportGroupLinkApiClient _pimsReportGroupLinkApiClient;
        private readonly IPimsProjectManagerApiClient _pimsProjectManagerApiClient;
        private readonly IPimsProgramManagerLinkApiClient _pimsProgramManagerLinkApiClient;
        private readonly IPimsProfitCentreManagerLinkApiClient _pimsProfitCentreManagerLinkApiClient;
        private readonly IPimsSettingApiClient _pimsSettingApiClient;
        private readonly IPimsAccessUserApiClient _pimsAccessUserApiClient;
        private readonly IPimsAccessLevelApiClient _pimsAccessLevelApiClient;
        private readonly IPimsAccessUserLevelApiClient _pimsAccessUserLevelApiClient;
        private readonly IPimsAccessSystemApiClient _pimsAccessSystemApiClient;
        private readonly IPimsFrequencyApiClient _pimsFrequencyApiClient;
        private readonly IPimsReviewItemApiClient _pimsReviewItemApiClient;
        private readonly IPimsRadTrackProgApiClient _pimsRadTrackProgApiClient;
        private readonly MaintenanceService _service;

        public MaintenanceServiceTests()
        {
            _pimsClient                        = Substitute.For<IPimsApiClient>();
            _pimsReportApiClient               = Substitute.For<IPimsReportApiClient>();
            _pimsReportGroupApiClient          = Substitute.For<IPimsReportGroupApiClient>();
            _pimsReportGroupLinkApiClient      = Substitute.For<IPimsReportGroupLinkApiClient>();
            _pimsProjectManagerApiClient       = Substitute.For<IPimsProjectManagerApiClient>();
            _pimsProgramManagerLinkApiClient   = Substitute.For<IPimsProgramManagerLinkApiClient>();
            _pimsProfitCentreManagerLinkApiClient = Substitute.For<IPimsProfitCentreManagerLinkApiClient>();
            _pimsSettingApiClient              = Substitute.For<IPimsSettingApiClient>();
            _pimsAccessUserApiClient           = Substitute.For<IPimsAccessUserApiClient>();
            _pimsAccessLevelApiClient          = Substitute.For<IPimsAccessLevelApiClient>();
            _pimsAccessUserLevelApiClient      = Substitute.For<IPimsAccessUserLevelApiClient>();
            _pimsAccessSystemApiClient         = Substitute.For<IPimsAccessSystemApiClient>();
            _pimsFrequencyApiClient            = Substitute.For<IPimsFrequencyApiClient>();
            _pimsReviewItemApiClient           = Substitute.For<IPimsReviewItemApiClient>();
            _pimsRadTrackProgApiClient         = Substitute.For<IPimsRadTrackProgApiClient>();

            // TRANSFORMENGINE: wire each property on the aggregate client mock to the corresponding sub-client mock
            _pimsClient.PimsReport.Returns(_pimsReportApiClient);
            _pimsClient.PimsReportGroup.Returns(_pimsReportGroupApiClient);
            _pimsClient.PimsReportGroupLink.Returns(_pimsReportGroupLinkApiClient);
            _pimsClient.PimsProjectManager.Returns(_pimsProjectManagerApiClient);
            _pimsClient.PimsProgramManagerLink.Returns(_pimsProgramManagerLinkApiClient);
            _pimsClient.PimsProfitCentreManagerLink.Returns(_pimsProfitCentreManagerLinkApiClient);
            _pimsClient.PimsSetting.Returns(_pimsSettingApiClient);
            _pimsClient.PimsAccessUser.Returns(_pimsAccessUserApiClient);
            _pimsClient.PimsAccessLevel.Returns(_pimsAccessLevelApiClient);
            _pimsClient.PimsAccessUserLevel.Returns(_pimsAccessUserLevelApiClient);
            _pimsClient.PimsAccessSystem.Returns(_pimsAccessSystemApiClient);
            _pimsClient.PimsFrequency.Returns(_pimsFrequencyApiClient);
            _pimsClient.PimsReviewItem.Returns(_pimsReviewItemApiClient);
            _pimsClient.PimsRadTrackProg.Returns(_pimsRadTrackProgApiClient);

            _service = new MaintenanceService(_pimsClient);
        }

        // ── helpers ───────────────────────────────────────────────────────────────

        private static List<ApiErrorDto> OneError(string code = "ERR", string message = "Error") =>
            new List<ApiErrorDto> { new ApiErrorDto { Code = code, Message = message } };

        private static ApiResponseDto<T> SuccessDto<T>(T data) =>
            ApiResponseDto<T>.SuccessResponse(data);

        private static ApiResponseDto<T> FailureDto<T>() =>
            ApiResponseDto<T>.FailureResponse(OneError(), new ApiMetaDto());

        // ── Report surface ────────────────────────────────────────────────────────

        #region Report

        [Fact]
        public async Task GetAllReportsAsync_DelegatesToPimsReportClient_ReturnsResult()
        {
            // Arrange
            var expected = SuccessDto(new List<ReportDto> { new() { Reportname = "R1" } });
            _pimsReportApiClient.GetAllAsync().Returns(expected);

            // Act
            var result = await _service.GetAllReportsAsync();

            // Assert
            Assert.True(result.Success);
            Assert.Single(result.Data!);
            await _pimsReportApiClient.Received(1).GetAllAsync();
        }

        [Fact]
        public async Task GetAllReportsAsync_ClientReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            _pimsReportApiClient.GetAllAsync().Returns(FailureDto<List<ReportDto>>());

            // Act
            var result = await _service.GetAllReportsAsync();

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task GetReportByIdAsync_DelegatesToPimsReportClient_ReturnsResult()
        {
            // Arrange
            var dto      = new ReportDto { Id = 5, Reportname = "R5" };
            var expected = SuccessDto(dto);
            _pimsReportApiClient.GetByIdAsync(5).Returns(expected);

            // Act
            var result = await _service.GetReportByIdAsync(5);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(5, result.Data!.Id);
            await _pimsReportApiClient.Received(1).GetByIdAsync(5);
        }

        [Fact]
        public async Task GetReportByIdAsync_ClientReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            _pimsReportApiClient.GetByIdAsync(Arg.Any<int>()).Returns(FailureDto<ReportDto>());

            // Act
            var result = await _service.GetReportByIdAsync(99);

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task CreateReportAsync_DelegatesToPimsReportClient_ReturnsResult()
        {
            // Arrange
            var dto      = new ReportDto { Reportname = "New", Type = "R" };
            var expected = SuccessDto(dto);
            _pimsReportApiClient.CreateAsync(dto).Returns(expected);

            // Act
            var result = await _service.CreateReportAsync(dto);

            // Assert
            Assert.True(result.Success);
            await _pimsReportApiClient.Received(1).CreateAsync(dto);
        }

        [Fact]
        public async Task CreateReportAsync_ClientReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var dto = new ReportDto { Reportname = "Bad" };
            _pimsReportApiClient.CreateAsync(dto).Returns(FailureDto<ReportDto>());

            // Act
            var result = await _service.CreateReportAsync(dto);

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task UpdateReportAsync_DelegatesToPimsReportClient_ReturnsResult()
        {
            // Arrange
            var dto      = new ReportDto { Id = 3, Reportname = "Updated", Type = "R" };
            var expected = SuccessDto(dto);
            _pimsReportApiClient.UpdateAsync(3, dto).Returns(expected);

            // Act
            var result = await _service.UpdateReportAsync(3, dto);

            // Assert
            Assert.True(result.Success);
            await _pimsReportApiClient.Received(1).UpdateAsync(3, dto);
        }

        [Fact]
        public async Task DeleteReportAsync_DelegatesToPimsReportClient_ReturnsResult()
        {
            // Arrange
            var expected = SuccessDto(true);
            _pimsReportApiClient.DeleteAsync(7).Returns(expected);

            // Act
            var result = await _service.DeleteReportAsync(7);

            // Assert
            Assert.True(result.Success);
            await _pimsReportApiClient.Received(1).DeleteAsync(7);
        }

        [Fact]
        public async Task DeleteReportAsync_ClientReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            _pimsReportApiClient.DeleteAsync(Arg.Any<int>()).Returns(FailureDto<bool>());

            // Act
            var result = await _service.DeleteReportAsync(99);

            // Assert
            Assert.False(result.Success);
        }

        #endregion

        // ── ReportGroup surface ───────────────────────────────────────────────────

        #region ReportGroup

        [Fact]
        public async Task GetAllReportGroupsAsync_DelegatesToPimsReportGroupClient_ReturnsResult()
        {
            // Arrange
            var dtos     = new List<ReportGroupDto> { new() { Groupid = 1, Description = "Group A" } };
            var expected = SuccessDto(dtos);
            _pimsReportGroupApiClient.GetAllAsync().Returns(expected);

            // Act
            var result = await _service.GetAllReportGroupsAsync();

            // Assert
            Assert.True(result.Success);
            Assert.Single(result.Data!);
            await _pimsReportGroupApiClient.Received(1).GetAllAsync();
        }

        [Fact]
        public async Task GetAllReportGroupsAsync_ClientReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            _pimsReportGroupApiClient.GetAllAsync().Returns(FailureDto<List<ReportGroupDto>>());

            // Act
            var result = await _service.GetAllReportGroupsAsync();

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task GetReportGroupByIdAsync_DelegatesToPimsReportGroupClient_ReturnsResult()
        {
            // Arrange
            var dto      = new ReportGroupDto { Groupid = 2, Description = "Group B" };
            var expected = SuccessDto(dto);
            _pimsReportGroupApiClient.GetByIdAsync(2).Returns(expected);

            // Act
            var result = await _service.GetReportGroupByIdAsync(2);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(2, result.Data!.Groupid);
            await _pimsReportGroupApiClient.Received(1).GetByIdAsync(2);
        }

        [Fact]
        public async Task GetReportGroupByIdAsync_ClientReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            _pimsReportGroupApiClient.GetByIdAsync(Arg.Any<int>()).Returns(FailureDto<ReportGroupDto>());

            // Act
            var result = await _service.GetReportGroupByIdAsync(99);

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task CreateReportGroupAsync_DelegatesToPimsReportGroupClient_ReturnsResult()
        {
            // Arrange
            var dto      = new ReportGroupDto { Description = "New Group" };
            var expected = SuccessDto(dto);
            _pimsReportGroupApiClient.CreateAsync(dto).Returns(expected);

            // Act
            var result = await _service.CreateReportGroupAsync(dto);

            // Assert
            Assert.True(result.Success);
            await _pimsReportGroupApiClient.Received(1).CreateAsync(dto);
        }

        [Fact]
        public async Task UpdateReportGroupAsync_DelegatesToPimsReportGroupClient_ReturnsResult()
        {
            // Arrange
            var dto      = new ReportGroupDto { Groupid = 3, Description = "Updated Group" };
            var expected = SuccessDto(dto);
            _pimsReportGroupApiClient.UpdateAsync(3, dto).Returns(expected);

            // Act
            var result = await _service.UpdateReportGroupAsync(3, dto);

            // Assert
            Assert.True(result.Success);
            await _pimsReportGroupApiClient.Received(1).UpdateAsync(3, dto);
        }

        [Fact]
        public async Task DeleteReportGroupAsync_DelegatesToPimsReportGroupClient_ReturnsResult()
        {
            // Arrange
            var expected = SuccessDto(true);
            _pimsReportGroupApiClient.DeleteAsync(4).Returns(expected);

            // Act
            var result = await _service.DeleteReportGroupAsync(4);

            // Assert
            Assert.True(result.Success);
            await _pimsReportGroupApiClient.Received(1).DeleteAsync(4);
        }

        [Fact]
        public async Task DeleteReportGroupAsync_ClientReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            _pimsReportGroupApiClient.DeleteAsync(Arg.Any<int>()).Returns(FailureDto<bool>());

            // Act
            var result = await _service.DeleteReportGroupAsync(99);

            // Assert
            Assert.False(result.Success);
        }

        #endregion

        // ── ReportGroupLink surface ───────────────────────────────────────────────

        #region ReportGroupLink

        [Fact]
        public async Task GetAllReportGroupLinksAsync_DelegatesToPimsReportGroupLinkClient_ReturnsResult()
        {
            // Arrange
            var dtos     = new List<ReportGroupLinkDto> { new() { Reportid = 1, Groupid = 2 } };
            var expected = SuccessDto(dtos);
            _pimsReportGroupLinkApiClient.GetAllAsync().Returns(expected);

            // Act
            var result = await _service.GetAllReportGroupLinksAsync();

            // Assert
            Assert.True(result.Success);
            Assert.Single(result.Data!);
            await _pimsReportGroupLinkApiClient.Received(1).GetAllAsync();
        }

        [Fact]
        public async Task GetReportGroupLinksByReportIdAsync_DelegatesToPimsReportGroupLinkClient_ReturnsResult()
        {
            // Arrange
            var dtos     = new List<ReportGroupLinkDto> { new() { Reportid = 5, Groupid = 1 } };
            var expected = SuccessDto(dtos);
            _pimsReportGroupLinkApiClient.GetByReportIdAsync(5).Returns(expected);

            // Act
            var result = await _service.GetReportGroupLinksByReportIdAsync(5);

            // Assert
            Assert.True(result.Success);
            Assert.Single(result.Data!);
            await _pimsReportGroupLinkApiClient.Received(1).GetByReportIdAsync(5);
        }

        [Fact]
        public async Task GetReportGroupLinksByReportIdAsync_ClientReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            _pimsReportGroupLinkApiClient.GetByReportIdAsync(Arg.Any<int>()).Returns(FailureDto<List<ReportGroupLinkDto>>());

            // Act
            var result = await _service.GetReportGroupLinksByReportIdAsync(99);

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task GetReportGroupLinkByIdAsync_DelegatesToPimsReportGroupLinkClient_ReturnsResult()
        {
            // Arrange
            var dto      = new ReportGroupLinkDto { Reportid = 3, Groupid = 7 };
            var expected = SuccessDto(dto);
            _pimsReportGroupLinkApiClient.GetByIdAsync(3, 7).Returns(expected);

            // Act
            var result = await _service.GetReportGroupLinkByIdAsync(3, 7);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(3, result.Data!.Reportid);
            Assert.Equal(7, result.Data.Groupid);
            await _pimsReportGroupLinkApiClient.Received(1).GetByIdAsync(3, 7);
        }

        [Fact]
        public async Task CreateReportGroupLinkAsync_DelegatesToPimsReportGroupLinkClient_ReturnsResult()
        {
            // Arrange
            var dto      = new ReportGroupLinkDto { Reportid = 1, Groupid = 2 };
            var expected = SuccessDto(dto);
            _pimsReportGroupLinkApiClient.CreateAsync(dto).Returns(expected);

            // Act
            var result = await _service.CreateReportGroupLinkAsync(dto);

            // Assert
            Assert.True(result.Success);
            await _pimsReportGroupLinkApiClient.Received(1).CreateAsync(dto);
        }

        [Fact]
        public async Task DeleteReportGroupLinkAsync_DelegatesToPimsReportGroupLinkClient_ReturnsResult()
        {
            // Arrange
            var expected = SuccessDto(true);
            _pimsReportGroupLinkApiClient.DeleteAsync(2, 5).Returns(expected);

            // Act
            var result = await _service.DeleteReportGroupLinkAsync(2, 5);

            // Assert
            Assert.True(result.Success);
            await _pimsReportGroupLinkApiClient.Received(1).DeleteAsync(2, 5);
        }

        [Fact]
        public async Task DeleteReportGroupLinkAsync_ClientReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            _pimsReportGroupLinkApiClient.DeleteAsync(Arg.Any<int>(), Arg.Any<int>()).Returns(FailureDto<bool>());

            // Act
            var result = await _service.DeleteReportGroupLinkAsync(99, 99);

            // Assert
            Assert.False(result.Success);
        }

        #endregion

        // ── ProjectManager surface ────────────────────────────────────────────────

        #region ProjectManager

        [Fact]
        public async Task GetAllProjectManagersAsync_DelegatesToPimsProjectManagerClient_ReturnsResult()
        {
            // Arrange
            var dtos     = new List<ProjectManagerDto> { new() { Projectmanager = "Smith" } };
            var expected = SuccessDto(dtos);
            _pimsProjectManagerApiClient.GetAllAsync().Returns(expected);

            // Act
            var result = await _service.GetAllProjectManagersAsync();

            // Assert
            Assert.True(result.Success);
            Assert.Single(result.Data!);
            await _pimsProjectManagerApiClient.Received(1).GetAllAsync();
        }

        [Fact]
        public async Task GetProjectManagerByIdAsync_DelegatesToPimsProjectManagerClient_ReturnsResult()
        {
            // Arrange
            var dto      = new ProjectManagerDto { Projectmanager = "Smith, J." };
            var expected = SuccessDto(dto);
            _pimsProjectManagerApiClient.GetByIdAsync("Smith, J.").Returns(expected);

            // Act
            var result = await _service.GetProjectManagerByIdAsync("Smith, J.");

            // Assert
            Assert.True(result.Success);
            await _pimsProjectManagerApiClient.Received(1).GetByIdAsync("Smith, J.");
        }

        [Fact]
        public async Task GetProjectManagerByIdAsync_ClientReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            _pimsProjectManagerApiClient.GetByIdAsync(Arg.Any<string>()).Returns(FailureDto<ProjectManagerDto>());

            // Act
            var result = await _service.GetProjectManagerByIdAsync("Unknown");

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task CreateProjectManagerAsync_DelegatesToPimsProjectManagerClient_ReturnsResult()
        {
            // Arrange
            var dto      = new ProjectManagerDto { Projectmanager = "New Manager" };
            var expected = SuccessDto(dto);
            _pimsProjectManagerApiClient.CreateAsync(dto).Returns(expected);

            // Act
            var result = await _service.CreateProjectManagerAsync(dto);

            // Assert
            Assert.True(result.Success);
            await _pimsProjectManagerApiClient.Received(1).CreateAsync(dto);
        }

        [Fact]
        public async Task UpdateProjectManagerAsync_DelegatesToPimsProjectManagerClient_ReturnsResult()
        {
            // Arrange
            var dto      = new ProjectManagerDto { Projectmanager = "Smith, J." };
            var expected = SuccessDto(dto);
            _pimsProjectManagerApiClient.UpdateAsync("Smith, J.", dto).Returns(expected);

            // Act
            var result = await _service.UpdateProjectManagerAsync("Smith, J.", dto);

            // Assert
            Assert.True(result.Success);
            await _pimsProjectManagerApiClient.Received(1).UpdateAsync("Smith, J.", dto);
        }

        [Fact]
        public async Task DeleteProjectManagerAsync_DelegatesToPimsProjectManagerClient_ReturnsResult()
        {
            // Arrange
            var expected = SuccessDto(true);
            _pimsProjectManagerApiClient.DeleteAsync("Smith, J.").Returns(expected);

            // Act
            var result = await _service.DeleteProjectManagerAsync("Smith, J.");

            // Assert
            Assert.True(result.Success);
            await _pimsProjectManagerApiClient.Received(1).DeleteAsync("Smith, J.");
        }

        [Fact]
        public async Task DeleteProjectManagerAsync_ClientReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            _pimsProjectManagerApiClient.DeleteAsync(Arg.Any<string>()).Returns(FailureDto<bool>());

            // Act
            var result = await _service.DeleteProjectManagerAsync("Unknown");

            // Assert
            Assert.False(result.Success);
        }

        #endregion

        // ── ProgramManagerLink surface ────────────────────────────────────────────

        #region ProgramManagerLink

        [Fact]
        public async Task GetAllProgramManagerLinksAsync_DelegatesToPimsProgramManagerLinkClient_ReturnsResult()
        {
            // Arrange
            var dtos     = new List<ProgramManagerLinkDto> { new() { Program = "RAD", Manager = "Jones" } };
            var expected = SuccessDto(dtos);
            _pimsProgramManagerLinkApiClient.GetAllAsync().Returns(expected);

            // Act
            var result = await _service.GetAllProgramManagerLinksAsync();

            // Assert
            Assert.True(result.Success);
            Assert.Single(result.Data!);
            await _pimsProgramManagerLinkApiClient.Received(1).GetAllAsync();
        }

        [Fact]
        public async Task GetProgramManagerLinksByProgramAsync_DelegatesToPimsProgramManagerLinkClient_ReturnsResult()
        {
            // Arrange
            var dtos     = new List<ProgramManagerLinkDto> { new() { Program = "RAD", Manager = "Jones" } };
            var expected = SuccessDto(dtos);
            _pimsProgramManagerLinkApiClient.GetByProgramAsync("RAD").Returns(expected);

            // Act
            var result = await _service.GetProgramManagerLinksByProgramAsync("RAD");

            // Assert
            Assert.True(result.Success);
            await _pimsProgramManagerLinkApiClient.Received(1).GetByProgramAsync("RAD");
        }

        [Fact]
        public async Task GetProgramManagerLinksByProgramAsync_ClientReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            _pimsProgramManagerLinkApiClient.GetByProgramAsync(Arg.Any<string>()).Returns(FailureDto<List<ProgramManagerLinkDto>>());

            // Act
            var result = await _service.GetProgramManagerLinksByProgramAsync("UNKNOWN");

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task GetProgramManagerLinkByIdAsync_DelegatesToPimsProgramManagerLinkClient_ReturnsResult()
        {
            // Arrange
            var dto      = new ProgramManagerLinkDto { Program = "RAD", Manager = "Jones" };
            var expected = SuccessDto(dto);
            _pimsProgramManagerLinkApiClient.GetByIdAsync("RAD", "Jones").Returns(expected);

            // Act
            var result = await _service.GetProgramManagerLinkByIdAsync("RAD", "Jones");

            // Assert
            Assert.True(result.Success);
            Assert.Equal("RAD",   result.Data!.Program);
            Assert.Equal("Jones", result.Data.Manager);
            await _pimsProgramManagerLinkApiClient.Received(1).GetByIdAsync("RAD", "Jones");
        }

        [Fact]
        public async Task CreateProgramManagerLinkAsync_DelegatesToPimsProgramManagerLinkClient_ReturnsResult()
        {
            // Arrange
            var dto      = new ProgramManagerLinkDto { Program = "RAD", Manager = "Smith" };
            var expected = SuccessDto(dto);
            _pimsProgramManagerLinkApiClient.CreateAsync(dto).Returns(expected);

            // Act
            var result = await _service.CreateProgramManagerLinkAsync(dto);

            // Assert
            Assert.True(result.Success);
            await _pimsProgramManagerLinkApiClient.Received(1).CreateAsync(dto);
        }

        [Fact]
        public async Task DeleteProgramManagerLinkAsync_DelegatesToPimsProgramManagerLinkClient_ReturnsResult()
        {
            // Arrange
            var expected = SuccessDto(true);
            _pimsProgramManagerLinkApiClient.DeleteAsync("RAD", "Jones").Returns(expected);

            // Act
            var result = await _service.DeleteProgramManagerLinkAsync("RAD", "Jones");

            // Assert
            Assert.True(result.Success);
            await _pimsProgramManagerLinkApiClient.Received(1).DeleteAsync("RAD", "Jones");
        }

        [Fact]
        public async Task DeleteProgramManagerLinkAsync_ClientReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            _pimsProgramManagerLinkApiClient.DeleteAsync(Arg.Any<string>(), Arg.Any<string>()).Returns(FailureDto<bool>());

            // Act
            var result = await _service.DeleteProgramManagerLinkAsync("X", "Y");

            // Assert
            Assert.False(result.Success);
        }

        #endregion

        // ── ProfitCentreManagerLink surface ───────────────────────────────────────

        #region ProfitCentreManagerLink

        [Fact]
        public async Task GetAllProfitCentreManagerLinksAsync_DelegatesToPimsProfitCentreManagerLinkClient_ReturnsResult()
        {
            // Arrange
            var dtos     = new List<ProfitCentreManagerLinkDto> { new() { Profitcentre = "PC01", Manager = "Jones" } };
            var expected = SuccessDto(dtos);
            _pimsProfitCentreManagerLinkApiClient.GetAllAsync().Returns(expected);

            // Act
            var result = await _service.GetAllProfitCentreManagerLinksAsync();

            // Assert
            Assert.True(result.Success);
            Assert.Single(result.Data!);
            await _pimsProfitCentreManagerLinkApiClient.Received(1).GetAllAsync();
        }

        [Fact]
        public async Task GetProfitCentreManagerLinksByProfitCentreAsync_DelegatesToPimsProfitCentreManagerLinkClient_ReturnsResult()
        {
            // Arrange
            var dtos     = new List<ProfitCentreManagerLinkDto> { new() { Profitcentre = "PC01", Manager = "Jones" } };
            var expected = SuccessDto(dtos);
            _pimsProfitCentreManagerLinkApiClient.GetByProfitCentreAsync("PC01").Returns(expected);

            // Act
            var result = await _service.GetProfitCentreManagerLinksByProfitCentreAsync("PC01");

            // Assert
            Assert.True(result.Success);
            await _pimsProfitCentreManagerLinkApiClient.Received(1).GetByProfitCentreAsync("PC01");
        }

        [Fact]
        public async Task GetProfitCentreManagerLinksByProfitCentreAsync_ClientReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            _pimsProfitCentreManagerLinkApiClient.GetByProfitCentreAsync(Arg.Any<string>()).Returns(FailureDto<List<ProfitCentreManagerLinkDto>>());

            // Act
            var result = await _service.GetProfitCentreManagerLinksByProfitCentreAsync("UNKNOWN");

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task GetProfitCentreManagerLinkByIdAsync_DelegatesToPimsProfitCentreManagerLinkClient_ReturnsResult()
        {
            // Arrange
            var dto      = new ProfitCentreManagerLinkDto { Profitcentre = "PC01", Manager = "Jones" };
            var expected = SuccessDto(dto);
            _pimsProfitCentreManagerLinkApiClient.GetByIdAsync("PC01", "Jones").Returns(expected);

            // Act
            var result = await _service.GetProfitCentreManagerLinkByIdAsync("PC01", "Jones");

            // Assert
            Assert.True(result.Success);
            Assert.Equal("PC01",  result.Data!.Profitcentre);
            Assert.Equal("Jones", result.Data.Manager);
            await _pimsProfitCentreManagerLinkApiClient.Received(1).GetByIdAsync("PC01", "Jones");
        }

        [Fact]
        public async Task CreateProfitCentreManagerLinkAsync_DelegatesToPimsProfitCentreManagerLinkClient_ReturnsResult()
        {
            // Arrange
            var dto      = new ProfitCentreManagerLinkDto { Profitcentre = "PC02", Manager = "Smith" };
            var expected = SuccessDto(dto);
            _pimsProfitCentreManagerLinkApiClient.CreateAsync(dto).Returns(expected);

            // Act
            var result = await _service.CreateProfitCentreManagerLinkAsync(dto);

            // Assert
            Assert.True(result.Success);
            await _pimsProfitCentreManagerLinkApiClient.Received(1).CreateAsync(dto);
        }

        [Fact]
        public async Task DeleteProfitCentreManagerLinkAsync_DelegatesToPimsProfitCentreManagerLinkClient_ReturnsResult()
        {
            // Arrange
            var expected = SuccessDto(true);
            _pimsProfitCentreManagerLinkApiClient.DeleteAsync("PC01", "Jones").Returns(expected);

            // Act
            var result = await _service.DeleteProfitCentreManagerLinkAsync("PC01", "Jones");

            // Assert
            Assert.True(result.Success);
            await _pimsProfitCentreManagerLinkApiClient.Received(1).DeleteAsync("PC01", "Jones");
        }

        [Fact]
        public async Task DeleteProfitCentreManagerLinkAsync_ClientReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            _pimsProfitCentreManagerLinkApiClient.DeleteAsync(Arg.Any<string>(), Arg.Any<string>()).Returns(FailureDto<bool>());

            // Act
            var result = await _service.DeleteProfitCentreManagerLinkAsync("X", "Y");

            // Assert
            Assert.False(result.Success);
        }

        #endregion

        // ── Setting surface ───────────────────────────────────────────────────────

        #region Setting

        [Fact]
        public async Task GetAllSettingsAsync_DelegatesToPimsSettingClient_ReturnsResult()
        {
            // Arrange
            var dtos     = new List<SettingDto> { new() { Id = "WorkingHours" }, new() { Id = "TestSetting" } };
            var expected = SuccessDto(dtos);
            _pimsSettingApiClient.GetAllAsync().Returns(expected);

            // Act
            var result = await _service.GetAllSettingsAsync();

            // Assert
            Assert.True(result.Success);
            Assert.Equal(2, result.Data!.Count);
            await _pimsSettingApiClient.Received(1).GetAllAsync();
        }

        [Fact]
        public async Task GetAllSettingsAsync_ClientReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            _pimsSettingApiClient.GetAllAsync().Returns(FailureDto<List<SettingDto>>());

            // Act
            var result = await _service.GetAllSettingsAsync();

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task GetAllUserUpdateableSettingsAsync_DelegatesToPimsSettingClient_ReturnsResult()
        {
            // Arrange
            var dtos     = new List<SettingDto> { new() { Id = "WorkingHours" } };
            var expected = SuccessDto(dtos);
            _pimsSettingApiClient.GetAllUserUpdateableAsync().Returns(expected);

            // Act
            var result = await _service.GetAllUserUpdateableSettingsAsync();

            // Assert
            Assert.True(result.Success);
            await _pimsSettingApiClient.Received(1).GetAllUserUpdateableAsync();
        }

        [Fact]
        public async Task GetAllUserUpdateableSettingsAsync_ClientReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            _pimsSettingApiClient.GetAllUserUpdateableAsync().Returns(FailureDto<List<SettingDto>>());

            // Act
            var result = await _service.GetAllUserUpdateableSettingsAsync();

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task GetSettingByIdAsync_DelegatesToPimsSettingClient_ReturnsResult()
        {
            // Arrange
            var dto      = new SettingDto { Id = "WorkingHours", SettingValue = "7.4" };
            var expected = SuccessDto(dto);
            _pimsSettingApiClient.GetByIdAsync("WorkingHours").Returns(expected);

            // Act
            var result = await _service.GetSettingByIdAsync("WorkingHours");

            // Assert
            Assert.True(result.Success);
            Assert.Equal("WorkingHours", result.Data!.Id);
            await _pimsSettingApiClient.Received(1).GetByIdAsync("WorkingHours");
        }

        [Fact]
        public async Task GetSettingByIdAsync_ClientReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            _pimsSettingApiClient.GetByIdAsync(Arg.Any<string>()).Returns(FailureDto<SettingDto>());

            // Act
            var result = await _service.GetSettingByIdAsync("Unknown");

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task UpdateSettingAsync_DelegatesToPimsSettingClient_ReturnsResult()
        {
            // Arrange
            var dto      = new SettingDto { Id = "WorkingHours" };
            var expected = SuccessDto(dto);
            _pimsSettingApiClient.UpdateAsync("WorkingHours", dto).Returns(expected);

            // Act
            var result = await _service.UpdateSettingAsync("WorkingHours", dto);

            // Assert
            Assert.True(result.Success);
            await _pimsSettingApiClient.Received(1).UpdateAsync("WorkingHours", dto);
        }

        [Fact]
        public async Task UpdateSettingAsync_ClientReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var dto = new SettingDto { Id = "WorkingHours" };
            _pimsSettingApiClient.UpdateAsync(Arg.Any<string>(), Arg.Any<SettingDto>()).Returns(FailureDto<SettingDto>());

            // Act
            var result = await _service.UpdateSettingAsync("WorkingHours", dto);

            // Assert
            Assert.False(result.Success);
        }

        #endregion

        // ── AccessUser surface ────────────────────────────────────────────────────

        #region AccessUser

        [Fact]
        public async Task GetAllAccessUsersAsync_DelegatesToPimsAccessUserClient_ReturnsResult()
        {
            // Arrange
            var dtos     = new List<AccessUserDto> { new() { Systemid = 1, Ntlogin = "dom\\u1" } };
            var expected = SuccessDto(dtos);
            _pimsAccessUserApiClient.GetAllAsync().Returns(expected);

            // Act
            var result = await _service.GetAllAccessUsersAsync();

            // Assert
            Assert.True(result.Success);
            await _pimsAccessUserApiClient.Received(1).GetAllAsync();
        }

        [Fact]
        public async Task GetAccessUsersBySystemIdAsync_DelegatesToPimsAccessUserClient_ReturnsResult()
        {
            // Arrange
            var dtos     = new List<AccessUserDto> { new() { Systemid = 2, Ntlogin = "dom\\u1" } };
            var expected = SuccessDto(dtos);
            _pimsAccessUserApiClient.GetBySystemIdAsync(2).Returns(expected);

            // Act
            var result = await _service.GetAccessUsersBySystemIdAsync(2);

            // Assert
            Assert.True(result.Success);
            await _pimsAccessUserApiClient.Received(1).GetBySystemIdAsync(2);
        }

        [Fact]
        public async Task GetAccessUsersBySystemIdAsync_ClientReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            _pimsAccessUserApiClient.GetBySystemIdAsync(Arg.Any<int>()).Returns(FailureDto<List<AccessUserDto>>());

            // Act
            var result = await _service.GetAccessUsersBySystemIdAsync(99);

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task GetAccessUserByIdAsync_DelegatesToPimsAccessUserClient_ReturnsResult()
        {
            // Arrange
            var dto      = new AccessUserDto { Systemid = 1, Ntlogin = "dom\\user" };
            var expected = SuccessDto(dto);
            _pimsAccessUserApiClient.GetByIdAsync(1, "dom\\user").Returns(expected);

            // Act
            var result = await _service.GetAccessUserByIdAsync(1, "dom\\user");

            // Assert
            Assert.True(result.Success);
            Assert.Equal(1,            result.Data!.Systemid);
            Assert.Equal("dom\\user",  result.Data.Ntlogin);
            await _pimsAccessUserApiClient.Received(1).GetByIdAsync(1, "dom\\user");
        }

        [Fact]
        public async Task GetAccessUserByIdAsync_ClientReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            _pimsAccessUserApiClient.GetByIdAsync(Arg.Any<int>(), Arg.Any<string>()).Returns(FailureDto<AccessUserDto>());

            // Act
            var result = await _service.GetAccessUserByIdAsync(99, "unknown");

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task CreateAccessUserAsync_DelegatesToPimsAccessUserClient_ReturnsResult()
        {
            // Arrange
            var dto      = new AccessUserDto { Systemid = 1, Ntlogin = "dom\\newuser" };
            var expected = SuccessDto(dto);
            _pimsAccessUserApiClient.CreateAsync(dto).Returns(expected);

            // Act
            var result = await _service.CreateAccessUserAsync(dto);

            // Assert
            Assert.True(result.Success);
            await _pimsAccessUserApiClient.Received(1).CreateAsync(dto);
        }

        [Fact]
        public async Task UpdateAccessUserAsync_DelegatesToPimsAccessUserClient_ReturnsResult()
        {
            // Arrange
            var dto      = new AccessUserDto { Systemid = 1, Ntlogin = "dom\\user" };
            var expected = SuccessDto(dto);
            _pimsAccessUserApiClient.UpdateAsync(1, "dom\\user", dto).Returns(expected);

            // Act
            var result = await _service.UpdateAccessUserAsync(1, "dom\\user", dto);

            // Assert
            Assert.True(result.Success);
            await _pimsAccessUserApiClient.Received(1).UpdateAsync(1, "dom\\user", dto);
        }

        [Fact]
        public async Task DeleteAccessUserAsync_DelegatesToPimsAccessUserClient_ReturnsResult()
        {
            // Arrange
            var expected = SuccessDto(true);
            _pimsAccessUserApiClient.DeleteAsync(1, "dom\\user").Returns(expected);

            // Act
            var result = await _service.DeleteAccessUserAsync(1, "dom\\user");

            // Assert
            Assert.True(result.Success);
            await _pimsAccessUserApiClient.Received(1).DeleteAsync(1, "dom\\user");
        }

        [Fact]
        public async Task DeleteAccessUserAsync_ClientReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            _pimsAccessUserApiClient.DeleteAsync(Arg.Any<int>(), Arg.Any<string>()).Returns(FailureDto<bool>());

            // Act
            var result = await _service.DeleteAccessUserAsync(99, "unknown");

            // Assert
            Assert.False(result.Success);
        }

        #endregion

        // ── AccessLevel surface ───────────────────────────────────────────────────

        #region AccessLevel

        [Fact]
        public async Task GetAllAccessLevelsAsync_DelegatesToPimsAccessLevelClient_ReturnsResult()
        {
            // Arrange
            var dtos     = new List<AccessLevelDto> { new() { Systemid = 1, Accesslevelid = 10, Accesslevel = "Admin" } };
            var expected = SuccessDto(dtos);
            _pimsAccessLevelApiClient.GetAllAsync().Returns(expected);

            // Act
            var result = await _service.GetAllAccessLevelsAsync();

            // Assert
            Assert.True(result.Success);
            Assert.Single(result.Data!);
            await _pimsAccessLevelApiClient.Received(1).GetAllAsync();
        }

        [Fact]
        public async Task GetAllAccessLevelsAsync_ClientReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            _pimsAccessLevelApiClient.GetAllAsync().Returns(FailureDto<List<AccessLevelDto>>());

            // Act
            var result = await _service.GetAllAccessLevelsAsync();

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task GetAccessLevelsBySystemIdAsync_DelegatesToPimsAccessLevelClient_ReturnsResult()
        {
            // Arrange
            var dtos     = new List<AccessLevelDto> { new() { Systemid = 1, Accesslevelid = 10 } };
            var expected = SuccessDto(dtos);
            _pimsAccessLevelApiClient.GetBySystemIdAsync(1).Returns(expected);

            // Act
            var result = await _service.GetAccessLevelsBySystemIdAsync(1);

            // Assert
            Assert.True(result.Success);
            await _pimsAccessLevelApiClient.Received(1).GetBySystemIdAsync(1);
        }

        [Fact]
        public async Task GetAccessLevelByIdAsync_DelegatesToPimsAccessLevelClient_ReturnsResult()
        {
            // Arrange
            var dto      = new AccessLevelDto { Systemid = 1, Accesslevelid = 10, Accesslevel = "Admin" };
            var expected = SuccessDto(dto);
            _pimsAccessLevelApiClient.GetByIdAsync(1, 10).Returns(expected);

            // Act
            var result = await _service.GetAccessLevelByIdAsync(1, 10);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(10, result.Data!.Accesslevelid);
            await _pimsAccessLevelApiClient.Received(1).GetByIdAsync(1, 10);
        }

        [Fact]
        public async Task GetAccessLevelByIdAsync_ClientReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            _pimsAccessLevelApiClient.GetByIdAsync(Arg.Any<int>(), Arg.Any<int>()).Returns(FailureDto<AccessLevelDto>());

            // Act
            var result = await _service.GetAccessLevelByIdAsync(99, 99);

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task CreateAccessLevelAsync_DelegatesToPimsAccessLevelClient_ReturnsResult()
        {
            // Arrange
            var dto      = new AccessLevelDto { Systemid = 1, Accesslevelid = 20, Accesslevel = "ReadOnly" };
            var expected = SuccessDto(dto);
            _pimsAccessLevelApiClient.CreateAsync(dto).Returns(expected);

            // Act
            var result = await _service.CreateAccessLevelAsync(dto);

            // Assert
            Assert.True(result.Success);
            await _pimsAccessLevelApiClient.Received(1).CreateAsync(dto);
        }

        [Fact]
        public async Task UpdateAccessLevelAsync_DelegatesToPimsAccessLevelClient_ReturnsResult()
        {
            // Arrange
            var dto      = new AccessLevelDto { Systemid = 1, Accesslevelid = 10, Accesslevel = "SuperAdmin" };
            var expected = SuccessDto(dto);
            _pimsAccessLevelApiClient.UpdateAsync(1, 10, dto).Returns(expected);

            // Act
            var result = await _service.UpdateAccessLevelAsync(1, 10, dto);

            // Assert
            Assert.True(result.Success);
            await _pimsAccessLevelApiClient.Received(1).UpdateAsync(1, 10, dto);
        }

        [Fact]
        public async Task DeleteAccessLevelAsync_DelegatesToPimsAccessLevelClient_ReturnsResult()
        {
            // Arrange
            var expected = SuccessDto(true);
            _pimsAccessLevelApiClient.DeleteAsync(1, 10).Returns(expected);

            // Act
            var result = await _service.DeleteAccessLevelAsync(1, 10);

            // Assert
            Assert.True(result.Success);
            await _pimsAccessLevelApiClient.Received(1).DeleteAsync(1, 10);
        }

        [Fact]
        public async Task DeleteAccessLevelAsync_ClientReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            _pimsAccessLevelApiClient.DeleteAsync(Arg.Any<int>(), Arg.Any<int>()).Returns(FailureDto<bool>());

            // Act
            var result = await _service.DeleteAccessLevelAsync(99, 99);

            // Assert
            Assert.False(result.Success);
        }

        #endregion

        // ── AccessUserLevel surface ───────────────────────────────────────────────

        #region AccessUserLevel

        [Fact]
        public async Task GetAllAccessUserLevelsAsync_DelegatesToPimsAccessUserLevelClient_ReturnsResult()
        {
            // Arrange
            var dtos     = new List<AccessUserLevelDto> { new() { Systemid = 1, Ntlogin = "dom\\u1", Accesslevelid = 10 } };
            var expected = SuccessDto(dtos);
            _pimsAccessUserLevelApiClient.GetAllAsync().Returns(expected);

            // Act
            var result = await _service.GetAllAccessUserLevelsAsync();

            // Assert
            Assert.True(result.Success);
            Assert.Single(result.Data!);
            await _pimsAccessUserLevelApiClient.Received(1).GetAllAsync();
        }

        [Fact]
        public async Task GetAccessUserLevelsBySystemIdAsync_DelegatesToPimsAccessUserLevelClient_ReturnsResult()
        {
            // Arrange
            var dtos     = new List<AccessUserLevelDto> { new() { Systemid = 1, Ntlogin = "dom\\u1", Accesslevelid = 10 } };
            var expected = SuccessDto(dtos);
            _pimsAccessUserLevelApiClient.GetBySystemIdAsync(1).Returns(expected);

            // Act
            var result = await _service.GetAccessUserLevelsBySystemIdAsync(1);

            // Assert
            Assert.True(result.Success);
            await _pimsAccessUserLevelApiClient.Received(1).GetBySystemIdAsync(1);
        }

        [Fact]
        public async Task GetAccessUserLevelsByUserAsync_DelegatesToPimsAccessUserLevelClient_ReturnsResult()
        {
            // Arrange
            var dtos     = new List<AccessUserLevelDto> { new() { Systemid = 1, Ntlogin = "dom\\u1", Accesslevelid = 10 } };
            var expected = SuccessDto(dtos);
            _pimsAccessUserLevelApiClient.GetByUserAsync(1, "dom\\u1").Returns(expected);

            // Act
            var result = await _service.GetAccessUserLevelsByUserAsync(1, "dom\\u1");

            // Assert
            Assert.True(result.Success);
            await _pimsAccessUserLevelApiClient.Received(1).GetByUserAsync(1, "dom\\u1");
        }

        [Fact]
        public async Task GetAccessUserLevelsByUserAsync_ClientReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            _pimsAccessUserLevelApiClient.GetByUserAsync(Arg.Any<int>(), Arg.Any<string>()).Returns(FailureDto<List<AccessUserLevelDto>>());

            // Act
            var result = await _service.GetAccessUserLevelsByUserAsync(99, "unknown");

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task GetAccessUserLevelByIdAsync_DelegatesToPimsAccessUserLevelClient_ReturnsResult()
        {
            // Arrange
            var dto      = new AccessUserLevelDto { Systemid = 1, Ntlogin = "dom\\u1", Accesslevelid = 10 };
            var expected = SuccessDto(dto);
            _pimsAccessUserLevelApiClient.GetByIdAsync(1, "dom\\u1", 10).Returns(expected);

            // Act
            var result = await _service.GetAccessUserLevelByIdAsync(1, "dom\\u1", 10);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(10, result.Data!.Accesslevelid);
            await _pimsAccessUserLevelApiClient.Received(1).GetByIdAsync(1, "dom\\u1", 10);
        }

        [Fact]
        public async Task CreateAccessUserLevelAsync_DelegatesToPimsAccessUserLevelClient_ReturnsResult()
        {
            // Arrange
            var dto      = new AccessUserLevelDto { Systemid = 1, Ntlogin = "dom\\newuser", Accesslevelid = 20 };
            var expected = SuccessDto(dto);
            _pimsAccessUserLevelApiClient.CreateAsync(dto).Returns(expected);

            // Act
            var result = await _service.CreateAccessUserLevelAsync(dto);

            // Assert
            Assert.True(result.Success);
            await _pimsAccessUserLevelApiClient.Received(1).CreateAsync(dto);
        }

        [Fact]
        public async Task DeleteAccessUserLevelAsync_DelegatesToPimsAccessUserLevelClient_ReturnsResult()
        {
            // Arrange
            var expected = SuccessDto(true);
            _pimsAccessUserLevelApiClient.DeleteAsync(1, "dom\\u1", 10).Returns(expected);

            // Act
            var result = await _service.DeleteAccessUserLevelAsync(1, "dom\\u1", 10);

            // Assert
            Assert.True(result.Success);
            await _pimsAccessUserLevelApiClient.Received(1).DeleteAsync(1, "dom\\u1", 10);
        }

        [Fact]
        public async Task DeleteAccessUserLevelAsync_ClientReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            _pimsAccessUserLevelApiClient.DeleteAsync(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<int>()).Returns(FailureDto<bool>());

            // Act
            var result = await _service.DeleteAccessUserLevelAsync(99, "unknown", 99);

            // Assert
            Assert.False(result.Success);
        }

        #endregion

        // ── AccessSystem surface (read-only) ──────────────────────────────────────

        #region AccessSystem

        [Fact]
        public async Task GetAllAccessSystemsAsync_DelegatesToPimsAccessSystemClient_ReturnsResult()
        {
            // Arrange
            var dtos     = new List<AccessSystemDto> { new() { Systemid = 1, Systemname = "PIMS" }, new() { Systemid = 2, Systemname = "FPS" } };
            var expected = SuccessDto(dtos);
            _pimsAccessSystemApiClient.GetAllAsync().Returns(expected);

            // Act
            var result = await _service.GetAllAccessSystemsAsync();

            // Assert
            Assert.True(result.Success);
            Assert.Equal(2, result.Data!.Count);
            await _pimsAccessSystemApiClient.Received(1).GetAllAsync();
        }

        [Fact]
        public async Task GetAllAccessSystemsAsync_ClientReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            _pimsAccessSystemApiClient.GetAllAsync().Returns(FailureDto<List<AccessSystemDto>>());

            // Act
            var result = await _service.GetAllAccessSystemsAsync();

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task GetAccessSystemByIdAsync_DelegatesToPimsAccessSystemClient_ReturnsResult()
        {
            // Arrange
            var dto      = new AccessSystemDto { Systemid = 1, Systemname = "PIMS" };
            var expected = SuccessDto(dto);
            _pimsAccessSystemApiClient.GetByIdAsync(1).Returns(expected);

            // Act
            var result = await _service.GetAccessSystemByIdAsync(1);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("PIMS", result.Data!.Systemname);
            await _pimsAccessSystemApiClient.Received(1).GetByIdAsync(1);
        }

        [Fact]
        public async Task GetAccessSystemByIdAsync_ClientReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            _pimsAccessSystemApiClient.GetByIdAsync(Arg.Any<int>()).Returns(FailureDto<AccessSystemDto>());

            // Act
            var result = await _service.GetAccessSystemByIdAsync(99);

            // Assert
            Assert.False(result.Success);
        }

        #endregion

        // ── Frequency surface ─────────────────────────────────────────────────────

        #region Frequency

        [Fact]
        public async Task GetAllFrequenciesAsync_DelegatesToPimsFrequencyClient_ReturnsResult()
        {
            // Arrange
            var dtos     = new List<FrequencyDto> { new() { Frequencyid = 1 } };
            var expected = SuccessDto(dtos);
            _pimsFrequencyApiClient.GetAllAsync().Returns(expected);

            // Act
            var result = await _service.GetAllFrequenciesAsync();

            // Assert
            Assert.True(result.Success);
            await _pimsFrequencyApiClient.Received(1).GetAllAsync();
        }

        [Fact]
        public async Task GetAllFrequenciesAsync_ClientReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            _pimsFrequencyApiClient.GetAllAsync().Returns(FailureDto<List<FrequencyDto>>());

            // Act
            var result = await _service.GetAllFrequenciesAsync();

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task GetFrequencyByIdAsync_DelegatesToPimsFrequencyClient_ReturnsResult()
        {
            // Arrange
            var dto      = new FrequencyDto { Frequencyid = 5, FrequencyValue = "Monthly" };
            var expected = SuccessDto(dto);
            _pimsFrequencyApiClient.GetByIdAsync(5).Returns(expected);

            // Act
            var result = await _service.GetFrequencyByIdAsync(5);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(5, result.Data!.Frequencyid);
            await _pimsFrequencyApiClient.Received(1).GetByIdAsync(5);
        }

        [Fact]
        public async Task GetFrequencyByIdAsync_ClientReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            _pimsFrequencyApiClient.GetByIdAsync(Arg.Any<int>()).Returns(FailureDto<FrequencyDto>());

            // Act
            var result = await _service.GetFrequencyByIdAsync(99);

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task CreateFrequencyAsync_DelegatesToPimsFrequencyClient_ReturnsResult()
        {
            // Arrange
            var dto      = new FrequencyDto { FrequencyValue = "Weekly" };
            var expected = SuccessDto(dto);
            _pimsFrequencyApiClient.CreateAsync(dto).Returns(expected);

            // Act
            var result = await _service.CreateFrequencyAsync(dto);

            // Assert
            Assert.True(result.Success);
            await _pimsFrequencyApiClient.Received(1).CreateAsync(dto);
        }

        [Fact]
        public async Task CreateFrequencyAsync_ClientReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var dto = new FrequencyDto { FrequencyValue = "Bad" };
            _pimsFrequencyApiClient.CreateAsync(dto).Returns(FailureDto<FrequencyDto>());

            // Act
            var result = await _service.CreateFrequencyAsync(dto);

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task UpdateFrequencyAsync_DelegatesToPimsFrequencyClient_ReturnsResult()
        {
            // Arrange
            var dto      = new FrequencyDto { Frequencyid = 3, FrequencyValue = "Quarterly" };
            var expected = SuccessDto(dto);
            _pimsFrequencyApiClient.UpdateAsync(3, dto).Returns(expected);

            // Act
            var result = await _service.UpdateFrequencyAsync(3, dto);

            // Assert
            Assert.True(result.Success);
            await _pimsFrequencyApiClient.Received(1).UpdateAsync(3, dto);
        }

        [Fact]
        public async Task DeleteFrequencyAsync_DelegatesToPimsFrequencyClient_ReturnsResult()
        {
            // Arrange
            var expected = SuccessDto(true);
            _pimsFrequencyApiClient.DeleteAsync(3).Returns(expected);

            // Act
            var result = await _service.DeleteFrequencyAsync(3);

            // Assert
            Assert.True(result.Success);
            await _pimsFrequencyApiClient.Received(1).DeleteAsync(3);
        }

        [Fact]
        public async Task DeleteFrequencyAsync_ClientReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            _pimsFrequencyApiClient.DeleteAsync(Arg.Any<int>()).Returns(FailureDto<bool>());

            // Act
            var result = await _service.DeleteFrequencyAsync(99);

            // Assert
            Assert.False(result.Success);
        }

        #endregion

        // ── ReviewItem surface ────────────────────────────────────────────────────

        #region ReviewItem

        [Fact]
        public async Task GetAllReviewItemsAsync_DelegatesToPimsReviewItemClient_ReturnsResult()
        {
            // Arrange
            var dtos     = new List<ReviewItemDto> { new() { Itemid = 1, Item = "Item A" } };
            var expected = SuccessDto(dtos);
            _pimsReviewItemApiClient.GetAllAsync().Returns(expected);

            // Act
            var result = await _service.GetAllReviewItemsAsync();

            // Assert
            Assert.True(result.Success);
            Assert.Single(result.Data!);
            await _pimsReviewItemApiClient.Received(1).GetAllAsync();
        }

        [Fact]
        public async Task GetAllReviewItemsAsync_ClientReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            _pimsReviewItemApiClient.GetAllAsync().Returns(FailureDto<List<ReviewItemDto>>());

            // Act
            var result = await _service.GetAllReviewItemsAsync();

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task GetReviewItemByIdAsync_DelegatesToPimsReviewItemClient_ReturnsResult()
        {
            // Arrange
            var dto      = new ReviewItemDto { Itemid = 5, Item = "Item E" };
            var expected = SuccessDto(dto);
            _pimsReviewItemApiClient.GetByIdAsync(5).Returns(expected);

            // Act
            var result = await _service.GetReviewItemByIdAsync(5);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(5, result.Data!.Itemid);
            await _pimsReviewItemApiClient.Received(1).GetByIdAsync(5);
        }

        [Fact]
        public async Task GetReviewItemByIdAsync_ClientReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            _pimsReviewItemApiClient.GetByIdAsync(Arg.Any<int>()).Returns(FailureDto<ReviewItemDto>());

            // Act
            var result = await _service.GetReviewItemByIdAsync(99);

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task CreateReviewItemAsync_DelegatesToPimsReviewItemClient_ReturnsResult()
        {
            // Arrange
            var dto      = new ReviewItemDto { Item = "New Item" };
            var expected = SuccessDto(dto);
            _pimsReviewItemApiClient.CreateAsync(dto).Returns(expected);

            // Act
            var result = await _service.CreateReviewItemAsync(dto);

            // Assert
            Assert.True(result.Success);
            await _pimsReviewItemApiClient.Received(1).CreateAsync(dto);
        }

        [Fact]
        public async Task CreateReviewItemAsync_ClientReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var dto = new ReviewItemDto { Item = "Bad" };
            _pimsReviewItemApiClient.CreateAsync(dto).Returns(FailureDto<ReviewItemDto>());

            // Act
            var result = await _service.CreateReviewItemAsync(dto);

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task UpdateReviewItemAsync_DelegatesToPimsReviewItemClient_ReturnsResult()
        {
            // Arrange
            var dto      = new ReviewItemDto { Itemid = 3, Item = "Updated Item" };
            var expected = SuccessDto(dto);
            _pimsReviewItemApiClient.UpdateAsync(3, dto).Returns(expected);

            // Act
            var result = await _service.UpdateReviewItemAsync(3, dto);

            // Assert
            Assert.True(result.Success);
            await _pimsReviewItemApiClient.Received(1).UpdateAsync(3, dto);
        }

        [Fact]
        public async Task DeleteReviewItemAsync_DelegatesToPimsReviewItemClient_ReturnsResult()
        {
            // Arrange
            var expected = SuccessDto(true);
            _pimsReviewItemApiClient.DeleteAsync(4).Returns(expected);

            // Act
            var result = await _service.DeleteReviewItemAsync(4);

            // Assert
            Assert.True(result.Success);
            await _pimsReviewItemApiClient.Received(1).DeleteAsync(4);
        }

        [Fact]
        public async Task DeleteReviewItemAsync_ClientReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            _pimsReviewItemApiClient.DeleteAsync(Arg.Any<int>()).Returns(FailureDto<bool>());

            // Act
            var result = await _service.DeleteReviewItemAsync(99);

            // Assert
            Assert.False(result.Success);
        }

        #endregion

        // ── RadTrackProg surface ──────────────────────────────────────────────────

        #region RadTrackProg

        [Fact]
        public async Task GetAllRadTrackProgsAsync_DelegatesToPimsRadTrackProgClient_ReturnsResult()
        {
            // Arrange
            var dtos     = new List<RadTrackProgDto> { new() { Program = "RAD1", Radtrackprog = true } };
            var expected = SuccessDto(dtos);
            _pimsRadTrackProgApiClient.GetAllAsync().Returns(expected);

            // Act
            var result = await _service.GetAllRadTrackProgsAsync();

            // Assert
            Assert.True(result.Success);
            Assert.Single(result.Data!);
            await _pimsRadTrackProgApiClient.Received(1).GetAllAsync();
        }

        [Fact]
        public async Task GetAllRadTrackProgsAsync_ClientReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            _pimsRadTrackProgApiClient.GetAllAsync().Returns(FailureDto<List<RadTrackProgDto>>());

            // Act
            var result = await _service.GetAllRadTrackProgsAsync();

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task GetRadTrackProgByIdAsync_DelegatesToPimsRadTrackProgClient_ReturnsResult()
        {
            // Arrange
            var dto      = new RadTrackProgDto { Program = "RAD1", Radtrackprog = true, Publicationprefix = "RT" };
            var expected = SuccessDto(dto);
            _pimsRadTrackProgApiClient.GetByIdAsync("RAD1").Returns(expected);

            // Act
            var result = await _service.GetRadTrackProgByIdAsync("RAD1");

            // Assert
            Assert.True(result.Success);
            Assert.Equal("RAD1", result.Data!.Program);
            await _pimsRadTrackProgApiClient.Received(1).GetByIdAsync("RAD1");
        }

        [Fact]
        public async Task GetRadTrackProgByIdAsync_ClientReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            _pimsRadTrackProgApiClient.GetByIdAsync(Arg.Any<string>()).Returns(FailureDto<RadTrackProgDto>());

            // Act
            var result = await _service.GetRadTrackProgByIdAsync("UNKNOWN");

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task CreateRadTrackProgAsync_DelegatesToPimsRadTrackProgClient_ReturnsResult()
        {
            // Arrange
            var dto      = new RadTrackProgDto { Program = "RAD2", Radtrackprog = false };
            var expected = SuccessDto(dto);
            _pimsRadTrackProgApiClient.CreateAsync(dto).Returns(expected);

            // Act
            var result = await _service.CreateRadTrackProgAsync(dto);

            // Assert
            Assert.True(result.Success);
            await _pimsRadTrackProgApiClient.Received(1).CreateAsync(dto);
        }

        [Fact]
        public async Task CreateRadTrackProgAsync_ClientReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            var dto = new RadTrackProgDto { Program = "BAD" };
            _pimsRadTrackProgApiClient.CreateAsync(dto).Returns(FailureDto<RadTrackProgDto>());

            // Act
            var result = await _service.CreateRadTrackProgAsync(dto);

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task UpdateRadTrackProgAsync_DelegatesToPimsRadTrackProgClient_ReturnsResult()
        {
            // Arrange
            var dto      = new RadTrackProgDto { Program = "RAD1", Radtrackprog = true, Publicationprefix = "RT2" };
            var expected = SuccessDto(dto);
            _pimsRadTrackProgApiClient.UpdateAsync("RAD1", dto).Returns(expected);

            // Act
            var result = await _service.UpdateRadTrackProgAsync("RAD1", dto);

            // Assert
            Assert.True(result.Success);
            await _pimsRadTrackProgApiClient.Received(1).UpdateAsync("RAD1", dto);
        }

        [Fact]
        public async Task DeleteRadTrackProgAsync_DelegatesToPimsRadTrackProgClient_ReturnsResult()
        {
            // Arrange
            var expected = SuccessDto(true);
            _pimsRadTrackProgApiClient.DeleteAsync("RAD1").Returns(expected);

            // Act
            var result = await _service.DeleteRadTrackProgAsync("RAD1");

            // Assert
            Assert.True(result.Success);
            await _pimsRadTrackProgApiClient.Received(1).DeleteAsync("RAD1");
        }

        [Fact]
        public async Task DeleteRadTrackProgAsync_ClientReturnsFailure_ReturnsFailureResponse()
        {
            // Arrange
            _pimsRadTrackProgApiClient.DeleteAsync(Arg.Any<string>()).Returns(FailureDto<bool>());

            // Act
            var result = await _service.DeleteRadTrackProgAsync("UNKNOWN");

            // Assert
            Assert.False(result.Success);
        }

        #endregion
    }
}
