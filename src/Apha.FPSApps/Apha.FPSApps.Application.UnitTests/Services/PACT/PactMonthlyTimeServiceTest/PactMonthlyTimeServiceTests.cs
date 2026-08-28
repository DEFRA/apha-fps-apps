using Apha.Common.Utilities.ExcelImport;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.PACT;
using Apha.FPSApps.Application.Interfaces.PactApiClients;
using Apha.Common.Utilities.Storage;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Application.Services.PACT;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Apha.FPSApps.Application.UnitTests.Services.PACT.PactMonthlyTimeServiceTest
{
    public class PactMonthlyTimeServiceTests
    {
        private readonly IPactApiClient _pactClient;
        private readonly IPactMonthlyTimeApiClient _pactMonthlyTimeApiClient;
        private readonly IExcelImportService _excelImportService;
        private readonly IWorkGroupService _workGroupService;
        private readonly IPactTimeCodeValidService _timeCodeValidService;
        private readonly IMonthService _monthService;
        private readonly IS3StorageService _s3StorageService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;
        private readonly IMonthlyImportServiceDependencies _monthlyImportServiceDependencies;
        private readonly ILogger<PactMonthlyTimeService> _logger;
        private readonly PactMonthlyTimeService _service;

        public PactMonthlyTimeServiceTests()
        {
            _pactClient = Substitute.For<IPactApiClient>();
            _pactMonthlyTimeApiClient = Substitute.For<IPactMonthlyTimeApiClient>();
            _excelImportService = Substitute.For<IExcelImportService>();
            _workGroupService = Substitute.For<IWorkGroupService>();
            _timeCodeValidService = Substitute.For<IPactTimeCodeValidService>();
            _monthService = Substitute.For<IMonthService>();
            _s3StorageService = Substitute.For<IS3StorageService>();
            _httpContextAccessor = Substitute.For<IHttpContextAccessor>();
            _configuration = Substitute.For<IConfiguration>();
            _monthlyImportServiceDependencies = Substitute.For<IMonthlyImportServiceDependencies>();
            _logger = Substitute.For<ILogger<PactMonthlyTimeService>>();

            _monthlyImportServiceDependencies.ExcelImportService.Returns(_excelImportService);
            _monthlyImportServiceDependencies.WorkGroupService.Returns(_workGroupService);
            _monthlyImportServiceDependencies.TimeCodeValidService.Returns(_timeCodeValidService);
            _monthlyImportServiceDependencies.MonthService.Returns(_monthService);
            _monthlyImportServiceDependencies.S3StorageService.Returns(_s3StorageService);
            _monthlyImportServiceDependencies.HttpContextAccessor.Returns(_httpContextAccessor);
            _monthlyImportServiceDependencies.Configuration.Returns(_configuration);

            _pactClient.PactMonthlyTime.Returns(_pactMonthlyTimeApiClient);
            _service = new PactMonthlyTimeService(
                _pactClient,
                _monthlyImportServiceDependencies,
                _logger);
        }

        #region SearchAsync Tests

        [Fact]
        public async Task SearchAsync_WithValidQueryAndFilter_ReturnsSuccessResponse()
        {
            // Arrange
            var query = new QueryParameters<string>();
            var filter = new MonthlyTimeLogFilterDto { WorkGroup = "WG1", TimeCode = "TC1" };
            var logs = new List<MonthlyTimeLogDto>
            {
                new() { SequenceNo = 1, TimeCode = "TC1", PactStaffId = "S001", WorkGroup = "WG1" },
                new() { SequenceNo = 2, TimeCode = "TC1", PactStaffId = "S002", WorkGroup = "WG1" }
            };
            var expectedResponse = ApiResponseDto<List<MonthlyTimeLogDto>>.SuccessResponse(logs);
            _pactMonthlyTimeApiClient.SearchAsync(query, filter).Returns(expectedResponse);

            // Act
            var result = await _service.SearchAsync(query, filter);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _pactMonthlyTimeApiClient.Received(1).SearchAsync(query, filter);
        }

        [Fact]
        public async Task SearchAsync_WithNoMatchingRecords_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var query = new QueryParameters<string>();
            var filter = new MonthlyTimeLogFilterDto { WorkGroup = "WG_NONE" };
            var expectedResponse = ApiResponseDto<List<MonthlyTimeLogDto>>.SuccessResponse(new List<MonthlyTimeLogDto>());
            _pactMonthlyTimeApiClient.SearchAsync(query, filter).Returns(expectedResponse);

            // Act
            var result = await _service.SearchAsync(query, filter);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task SearchAsync_WithAllFilters_PassesFilterToApiClient()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var filter = new MonthlyTimeLogFilterDto
            {
                WorkGroup = "WG1",
                TimeCode = "TC1",
                PactStaffId = "S001",
                ParentProject = "PP1",
                DateImported = new DateTime(2024, 6, 1),
                Month = 6.0,
                UserId = "USER1",
                InsertDelete = "I"
            };
            var logs = new List<MonthlyTimeLogDto>
            {
                new() { SequenceNo = 1, TimeCode = "TC1", PactStaffId = "S001", WorkGroup = "WG1", Month = 6.0, UserId = "USER1", InsertDelete = "I" }
            };
            var expectedResponse = ApiResponseDto<List<MonthlyTimeLogDto>>.SuccessResponse(logs);
            _pactMonthlyTimeApiClient.SearchAsync(query, filter).Returns(expectedResponse);

            // Act
            var result = await _service.SearchAsync(query, filter);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Single(result.Data!);
            await _pactMonthlyTimeApiClient.Received(1).SearchAsync(query, filter);
        }

        [Fact]
        public async Task SearchAsync_WithEmptyFilter_DelegatesToApiClient()
        {
            // Arrange
            var query = new QueryParameters<string>();
            var filter = new MonthlyTimeLogFilterDto();
            var expectedResponse = ApiResponseDto<List<MonthlyTimeLogDto>>.SuccessResponse(new List<MonthlyTimeLogDto>());
            _pactMonthlyTimeApiClient.SearchAsync(query, filter).Returns(expectedResponse);

            // Act
            var result = await _service.SearchAsync(query, filter);

            // Assert
            Assert.NotNull(result);
            await _pactMonthlyTimeApiClient.Received(1).SearchAsync(query, filter);
        }

        [Fact]
        public async Task SearchAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string>();
            var filter = new MonthlyTimeLogFilterDto { WorkGroup = "WG1" };
            var errors = new List<ApiErrorDto> { new() { Message = "API Error", Code = "API_ERROR" } };
            var expectedResponse = ApiResponseDto<List<MonthlyTimeLogDto>>.FailureResponse(errors, new ApiMetaDto());
            _pactMonthlyTimeApiClient.SearchAsync(query, filter).Returns(expectedResponse);

            // Act
            var result = await _service.SearchAsync(query, filter);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
        }

        [Fact]
        public async Task SearchAsync_ApiClientThrows_PropagatesException()
        {
            // Arrange
            var query = new QueryParameters<string>();
            var filter = new MonthlyTimeLogFilterDto();
            _pactMonthlyTimeApiClient
                .SearchAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<MonthlyTimeLogFilterDto>())
                .ThrowsAsync(new Exception("API client error"));

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _service.SearchAsync(query, filter));
        }

        #endregion

        #region Live Methods Tests

        [Fact]
        public async Task GetLiveAsync_WithValidFilters_DelegatesToApiClient()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var expected = ApiResponseDto<List<MonthlyTimeDto>>.SuccessResponse([]);
            _pactMonthlyTimeApiClient.GetLiveAsync(query, "WG1", "TC1", "S001", "PP1", 6).Returns(expected);

            var result = await _service.GetLiveAsync(query, "WG1", "TC1", "S001", "PP1", 6);

            Assert.Same(expected, result);
            await _pactMonthlyTimeApiClient.Received(1).GetLiveAsync(query, "WG1", "TC1", "S001", "PP1", 6);
        }

        [Fact]
        public async Task GetLiveByKeyAsync_WithValidKey_DelegatesToApiClient()
        {
            var dto = new MonthlyTimeDto { PactStaffId = "S001", TimeCode = "TC1", Month = 6, ParentProject = "PP1" };
            var expected = ApiResponseDto<MonthlyTimeDto>.SuccessResponse(dto);
            _pactMonthlyTimeApiClient.GetLiveByKeyAsync("S001", "TC1", 6, "PP1").Returns(expected);

            var result = await _service.GetLiveByKeyAsync("S001", "TC1", 6, "PP1");

            Assert.Same(expected, result);
            await _pactMonthlyTimeApiClient.Received(1).GetLiveByKeyAsync("S001", "TC1", 6, "PP1");
        }

        [Fact]
        public async Task UpdateLiveAsync_WithDto_DelegatesToApiClient()
        {
            var dto = new MonthlyTimeDto { PactStaffId = "S001", TimeCode = "TC1", Month = 6, ParentProject = "PP1", Hours = 7 };
            var expected = ApiResponseDto<MonthlyTimeDto>.SuccessResponse(dto);
            _pactMonthlyTimeApiClient.UpdateLiveAsync(dto).Returns(expected);

            var result = await _service.UpdateLiveAsync(dto);

            Assert.Same(expected, result);
            await _pactMonthlyTimeApiClient.Received(1).UpdateLiveAsync(dto);
        }

        #endregion

        #region ValidateLiveAsync Tests

        [Fact]
        public async Task ValidateLiveAsync_WithValidData_ReturnsNoErrors()
        {
            var dto = new MonthlyTimeDto
            {
                WorkGroup = "WG1",
                PactStaffId = "S001",
                TimeCode = "TC1",
                ParentProject = "PP1",
                Month = 6,
                Hours = 8
            };

            _workGroupService.GetAllWorkGroupsAsync().Returns(ApiResponseDto<List<WorkGroupDto>>.SuccessResponse(
            [
                new WorkGroupDto { WorkGroupName = "WG1" }
            ]));
            _timeCodeValidService.GetTimeCodeValidsByWorkGroupAsync("WG1").Returns(ApiResponseDto<List<TimeCodeValidDto>>.SuccessResponse(
            [
                new TimeCodeValidDto { WorkGroup = "WG1", TimeCode = "TC1", ParentProject = "PP1" }
            ]));
            _timeCodeValidService.GetTimeCodesProjectsByWorkGroupAndTimeCodeAsync("WG1", "TC1").Returns(ApiResponseDto<List<string>>.SuccessResponse(
            [
                "PP1"
            ]));
            _monthService.GetAllMonthsAsync().Returns(ApiResponseDto<List<MonthDto>>.SuccessResponse(
            [
                new MonthDto { Monthnumber = 6, Monthname = "June" }
            ]));

            var result = await _service.ValidateLiveAsync(dto);

            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task ValidateLiveAsync_WithInvalidData_ReturnsExpectedErrors()
        {
            var dto = new MonthlyTimeDto
            {
                WorkGroup = "BAD-WG",
                PactStaffId = "",
                TimeCode = "BAD-TC",
                ParentProject = "BAD-PP",
                Month = 99,
                Hours = 0
            };

            _workGroupService.GetAllWorkGroupsAsync().Returns(ApiResponseDto<List<WorkGroupDto>>.SuccessResponse(
            [
                new WorkGroupDto { WorkGroupName = "WG1" }
            ]));
            _timeCodeValidService.GetTimeCodeValidsByWorkGroupAsync("BAD-WG").Returns(ApiResponseDto<List<TimeCodeValidDto>>.SuccessResponse(
            [
                new TimeCodeValidDto { WorkGroup = "WG1", TimeCode = "TC1", ParentProject = "PP1" }
            ]));
            _timeCodeValidService.GetTimeCodesProjectsByWorkGroupAndTimeCodeAsync("BAD-WG", "BAD-TC").Returns(ApiResponseDto<List<string>>.SuccessResponse(
            [
                "PP1"
            ]));
            _monthService.GetAllMonthsAsync().Returns(ApiResponseDto<List<MonthDto>>.SuccessResponse(
            [
                new MonthDto { Monthnumber = 6, Monthname = "June" }
            ]));

            var result = await _service.ValidateLiveAsync(dto);

            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            var fields = result.Data!.Select(x => x.Field).ToList();
            Assert.Contains("Hours", fields);
            Assert.Contains("WorkGroup", fields);
            Assert.Contains("PactStaffId", fields);
            Assert.Contains("TimeCode", fields);
            Assert.Contains("ParentProject", fields);
            Assert.Contains("Month", fields);
        }

        [Fact]
        public async Task ValidateLiveAsync_NullHours_ReturnsHoursError()
        {
            var dto = new MonthlyTimeDto { WorkGroup = "WG1", PactStaffId = "S001", TimeCode = "TC1", ParentProject = "PP1", Month = 6, Hours = null };
            SetupValidationMocks();

            var result = await _service.ValidateLiveAsync(dto);

            Assert.True(result.Success);
            Assert.Contains(result.Data!, e => e.Field == "Hours");
        }

        [Fact]
        public async Task ValidateLiveAsync_BlankWorkGroup_ReturnsWorkGroupError()
        {
            var dto = new MonthlyTimeDto { WorkGroup = "  ", PactStaffId = "S001", TimeCode = "TC1", ParentProject = "PP1", Month = 6, Hours = 8 };
            _monthService.GetAllMonthsAsync().Returns(ApiResponseDto<List<MonthDto>>.SuccessResponse([new MonthDto { Monthnumber = 6, Monthname = "June" }]));

            var result = await _service.ValidateLiveAsync(dto);

            Assert.Contains(result.Data!, e => e.Field == "WorkGroup" && e.Message!.Contains("blank"));
        }

        [Fact]
        public async Task ValidateLiveAsync_WorkGroupServiceFailure_ReturnsInvalidWorkGroup()
        {
            var dto = new MonthlyTimeDto { WorkGroup = "WG1", PactStaffId = "S001", TimeCode = "TC1", ParentProject = "PP1", Month = 6, Hours = 8 };
            _workGroupService.GetAllWorkGroupsAsync().Returns(ApiResponseDto<List<WorkGroupDto>>.FailureResponse([], new ApiMetaDto()));
            _timeCodeValidService.GetTimeCodeValidsByWorkGroupAsync("WG1").Returns(ApiResponseDto<List<TimeCodeValidDto>>.SuccessResponse([new TimeCodeValidDto { TimeCode = "TC1" }]));
            _timeCodeValidService.GetTimeCodesProjectsByWorkGroupAndTimeCodeAsync("WG1", "TC1").Returns(ApiResponseDto<List<string>>.SuccessResponse(["PP1"]));
            _monthService.GetAllMonthsAsync().Returns(ApiResponseDto<List<MonthDto>>.SuccessResponse([new MonthDto { Monthnumber = 6, Monthname = "June" }]));

            var result = await _service.ValidateLiveAsync(dto);

            Assert.Contains(result.Data!, e => e.Field == "WorkGroup");
        }

        [Fact]
        public async Task ValidateLiveAsync_BlankStaffId_ReturnsStaffIdError()
        {
            var dto = new MonthlyTimeDto { WorkGroup = "WG1", PactStaffId = " ", TimeCode = "TC1", ParentProject = "PP1", Month = 6, Hours = 8 };
            SetupValidationMocks();

            var result = await _service.ValidateLiveAsync(dto);

            Assert.Contains(result.Data!, e => e.Field == "PactStaffId");
        }

        [Fact]
        public async Task ValidateLiveAsync_BlankTimeCode_ReturnsTimeCodeError()
        {
            var dto = new MonthlyTimeDto { WorkGroup = "WG1", PactStaffId = "S001", TimeCode = "", ParentProject = "PP1", Month = 6, Hours = 8 };
            SetupValidationMocks();

            var result = await _service.ValidateLiveAsync(dto);

            Assert.Contains(result.Data!, e => e.Field == "TimeCode" && e.Message!.Contains("blank"));
        }

        [Fact]
        public async Task ValidateLiveAsync_InvalidTimeCode_ReturnsTimeCodeError()
        {
            var dto = new MonthlyTimeDto { WorkGroup = "WG1", PactStaffId = "S001", TimeCode = "BADTC", ParentProject = "PP1", Month = 6, Hours = 8 };
            _workGroupService.GetAllWorkGroupsAsync().Returns(ApiResponseDto<List<WorkGroupDto>>.SuccessResponse([new WorkGroupDto { WorkGroupName = "WG1" }]));
            _timeCodeValidService.GetTimeCodeValidsByWorkGroupAsync("WG1").Returns(ApiResponseDto<List<TimeCodeValidDto>>.SuccessResponse([new TimeCodeValidDto { TimeCode = "TC1" }]));
            _timeCodeValidService.GetTimeCodesProjectsByWorkGroupAndTimeCodeAsync("WG1", "BADTC").Returns(ApiResponseDto<List<string>>.SuccessResponse(["PP1"]));
            _monthService.GetAllMonthsAsync().Returns(ApiResponseDto<List<MonthDto>>.SuccessResponse([new MonthDto { Monthnumber = 6, Monthname = "June" }]));

            var result = await _service.ValidateLiveAsync(dto);

            Assert.Contains(result.Data!, e => e.Field == "TimeCode" && e.Message!.Contains("not valid"));
        }

        [Fact]
        public async Task ValidateLiveAsync_TimeCodeServiceFailure_SkipsTimeCodeValidation()
        {
            var dto = new MonthlyTimeDto { WorkGroup = "WG1", PactStaffId = "S001", TimeCode = "TC1", ParentProject = "PP1", Month = 6, Hours = 8 };
            _workGroupService.GetAllWorkGroupsAsync().Returns(ApiResponseDto<List<WorkGroupDto>>.SuccessResponse([new WorkGroupDto { WorkGroupName = "WG1" }]));
            _timeCodeValidService.GetTimeCodeValidsByWorkGroupAsync("WG1").Returns(ApiResponseDto<List<TimeCodeValidDto>>.FailureResponse([], new ApiMetaDto()));
            _timeCodeValidService.GetTimeCodesProjectsByWorkGroupAndTimeCodeAsync("WG1", "TC1").Returns(ApiResponseDto<List<string>>.SuccessResponse(["PP1"]));
            _monthService.GetAllMonthsAsync().Returns(ApiResponseDto<List<MonthDto>>.SuccessResponse([new MonthDto { Monthnumber = 6, Monthname = "June" }]));

            var result = await _service.ValidateLiveAsync(dto);

            Assert.Contains(result.Data!, e => e.Field == "TimeCode");
        }

        [Fact]
        public async Task ValidateLiveAsync_BlankParentProject_ReturnsProjectError()
        {
            var dto = new MonthlyTimeDto { WorkGroup = "WG1", PactStaffId = "S001", TimeCode = "TC1", ParentProject = " ", Month = 6, Hours = 8 };
            SetupValidationMocks();

            var result = await _service.ValidateLiveAsync(dto);

            Assert.Contains(result.Data!, e => e.Field == "ParentProject" && e.Message!.Contains("blank"));
        }

        [Fact]
        public async Task ValidateLiveAsync_InvalidParentProject_ReturnsProjectError()
        {
            var dto = new MonthlyTimeDto { WorkGroup = "WG1", PactStaffId = "S001", TimeCode = "TC1", ParentProject = "BADPP", Month = 6, Hours = 8 };
            _workGroupService.GetAllWorkGroupsAsync().Returns(ApiResponseDto<List<WorkGroupDto>>.SuccessResponse([new WorkGroupDto { WorkGroupName = "WG1" }]));
            _timeCodeValidService.GetTimeCodeValidsByWorkGroupAsync("WG1").Returns(ApiResponseDto<List<TimeCodeValidDto>>.SuccessResponse([new TimeCodeValidDto { TimeCode = "TC1" }]));
            _timeCodeValidService.GetTimeCodesProjectsByWorkGroupAndTimeCodeAsync("WG1", "TC1").Returns(ApiResponseDto<List<string>>.SuccessResponse(["PP1"]));
            _monthService.GetAllMonthsAsync().Returns(ApiResponseDto<List<MonthDto>>.SuccessResponse([new MonthDto { Monthnumber = 6, Monthname = "June" }]));

            var result = await _service.ValidateLiveAsync(dto);

            Assert.Contains(result.Data!, e => e.Field == "ParentProject" && e.Message!.Contains("Not valid"));
        }

        [Fact]
        public async Task ValidateLiveAsync_ProjectServiceFailure_SkipsProjectValidation()
        {
            var dto = new MonthlyTimeDto { WorkGroup = "WG1", PactStaffId = "S001", TimeCode = "TC1", ParentProject = "PP1", Month = 6, Hours = 8 };
            _workGroupService.GetAllWorkGroupsAsync().Returns(ApiResponseDto<List<WorkGroupDto>>.SuccessResponse([new WorkGroupDto { WorkGroupName = "WG1" }]));
            _timeCodeValidService.GetTimeCodeValidsByWorkGroupAsync("WG1").Returns(ApiResponseDto<List<TimeCodeValidDto>>.SuccessResponse([new TimeCodeValidDto { TimeCode = "TC1" }]));
            _timeCodeValidService.GetTimeCodesProjectsByWorkGroupAndTimeCodeAsync("WG1", "TC1").Returns(ApiResponseDto<List<string>>.FailureResponse([], new ApiMetaDto()));
            _monthService.GetAllMonthsAsync().Returns(ApiResponseDto<List<MonthDto>>.SuccessResponse([new MonthDto { Monthnumber = 6, Monthname = "June" }]));

            var result = await _service.ValidateLiveAsync(dto);

            Assert.Contains(result.Data!, e => e.Field == "ParentProject");
        }

        [Fact]
        public async Task ValidateLiveAsync_InvalidMonth_ReturnsMonthError()
        {
            var dto = new MonthlyTimeDto { WorkGroup = "WG1", PactStaffId = "S001", TimeCode = "TC1", ParentProject = "PP1", Month = 99, Hours = 8 };
            _workGroupService.GetAllWorkGroupsAsync().Returns(ApiResponseDto<List<WorkGroupDto>>.SuccessResponse([new WorkGroupDto { WorkGroupName = "WG1" }]));
            _timeCodeValidService.GetTimeCodeValidsByWorkGroupAsync("WG1").Returns(ApiResponseDto<List<TimeCodeValidDto>>.SuccessResponse([new TimeCodeValidDto { TimeCode = "TC1" }]));
            _timeCodeValidService.GetTimeCodesProjectsByWorkGroupAndTimeCodeAsync("WG1", "TC1").Returns(ApiResponseDto<List<string>>.SuccessResponse(["PP1"]));
            _monthService.GetAllMonthsAsync().Returns(ApiResponseDto<List<MonthDto>>.SuccessResponse([new MonthDto { Monthnumber = 6, Monthname = "June" }]));

            var result = await _service.ValidateLiveAsync(dto);

            Assert.Contains(result.Data!, e => e.Field == "Month");
        }

        [Fact]
        public async Task ValidateLiveAsync_MonthServiceFailure_ReturnsMonthError()
        {
            var dto = new MonthlyTimeDto { WorkGroup = "WG1", PactStaffId = "S001", TimeCode = "TC1", ParentProject = "PP1", Month = 6, Hours = 8 };
            _workGroupService.GetAllWorkGroupsAsync().Returns(ApiResponseDto<List<WorkGroupDto>>.SuccessResponse([new WorkGroupDto { WorkGroupName = "WG1" }]));
            _timeCodeValidService.GetTimeCodeValidsByWorkGroupAsync("WG1").Returns(ApiResponseDto<List<TimeCodeValidDto>>.SuccessResponse([new TimeCodeValidDto { TimeCode = "TC1" }]));
            _timeCodeValidService.GetTimeCodesProjectsByWorkGroupAndTimeCodeAsync("WG1", "TC1").Returns(ApiResponseDto<List<string>>.SuccessResponse(["PP1"]));
            _monthService.GetAllMonthsAsync().Returns(ApiResponseDto<List<MonthDto>>.FailureResponse([], new ApiMetaDto()));

            var result = await _service.ValidateLiveAsync(dto);

            Assert.Contains(result.Data!, e => e.Field == "Month");
        }

        [Fact]
        public async Task ValidateLiveAsync_WorkGroupBlankSkipsTimeCodeAndProjectValidation()
        {
            var dto = new MonthlyTimeDto { WorkGroup = "", PactStaffId = "S001", TimeCode = "TC1", ParentProject = "PP1", Month = 6, Hours = 8 };
            _monthService.GetAllMonthsAsync().Returns(ApiResponseDto<List<MonthDto>>.SuccessResponse([new MonthDto { Monthnumber = 6, Monthname = "June" }]));

            var result = await _service.ValidateLiveAsync(dto);

            Assert.Contains(result.Data!, e => e.Field == "WorkGroup");
            Assert.DoesNotContain(result.Data!, e => e.Field == "TimeCode");
        }

        private void SetupValidationMocks()
        {
            _workGroupService.GetAllWorkGroupsAsync().Returns(ApiResponseDto<List<WorkGroupDto>>.SuccessResponse([new WorkGroupDto { WorkGroupName = "WG1" }]));
            _timeCodeValidService.GetTimeCodeValidsByWorkGroupAsync("WG1").Returns(ApiResponseDto<List<TimeCodeValidDto>>.SuccessResponse([new TimeCodeValidDto { TimeCode = "TC1" }]));
            _timeCodeValidService.GetTimeCodesProjectsByWorkGroupAndTimeCodeAsync("WG1", "TC1").Returns(ApiResponseDto<List<string>>.SuccessResponse(["PP1"]));
            _monthService.GetAllMonthsAsync().Returns(ApiResponseDto<List<MonthDto>>.SuccessResponse([new MonthDto { Monthnumber = 6, Monthname = "June" }]));
        }

        #endregion

        #region Staging Delegation Tests

        [Fact]
        public async Task GetStagingAsync_DelegatesToApiClient()
        {
            var query = new QueryParameters<string>();
            var expected = ApiResponseDto<List<StagingMonthlyTimeDto>>.SuccessResponse([]);
            _pactMonthlyTimeApiClient.GetStagingAsync(query, true).Returns(expected);

            var result = await _service.GetStagingAsync(query, true);

            Assert.Same(expected, result);
            await _pactMonthlyTimeApiClient.Received(1).GetStagingAsync(query, true);
        }

        [Fact]
        public async Task GetStagingByIdAsync_DelegatesToApiClient()
        {
            var dto = new StagingMonthlyTimeDto { Id = 5 };
            var expected = ApiResponseDto<StagingMonthlyTimeDto>.SuccessResponse(dto);
            _pactMonthlyTimeApiClient.GetStagingByIdAsync(5).Returns(expected);

            var result = await _service.GetStagingByIdAsync(5);

            Assert.Same(expected, result);
            await _pactMonthlyTimeApiClient.Received(1).GetStagingByIdAsync(5);
        }

        [Fact]
        public async Task CreateStagingAsync_DelegatesToApiClient()
        {
            var dto = new StagingMonthlyTimeDto { WorkGroup = "WG1" };
            var expected = ApiResponseDto<StagingMonthlyTimeDto>.SuccessResponse(dto);
            _pactMonthlyTimeApiClient.CreateStagingAsync(dto).Returns(expected);

            var result = await _service.CreateStagingAsync(dto);

            Assert.Same(expected, result);
            await _pactMonthlyTimeApiClient.Received(1).CreateStagingAsync(dto);
        }

        [Fact]
        public async Task UpdateStagingAsync_DelegatesToApiClient()
        {
            var dto = new StagingMonthlyTimeDto { Id = 5 };
            var expected = ApiResponseDto<StagingMonthlyTimeDto>.SuccessResponse(dto);
            _pactMonthlyTimeApiClient.UpdateStagingAsync(5, dto).Returns(expected);

            var result = await _service.UpdateStagingAsync(5, dto);

            Assert.Same(expected, result);
            await _pactMonthlyTimeApiClient.Received(1).UpdateStagingAsync(5, dto);
        }

        [Fact]
        public async Task BulkUpdateStagingNamesAsync_DelegatesToApiClient()
        {
            var dto = new BulkUpdateStagingMonthlyTimeNamesDto();
            var expected = ApiResponseDto<BulkUpdateStagingMonthlyTimeNamesResultDto>.SuccessResponse(new BulkUpdateStagingMonthlyTimeNamesResultDto());
            _pactMonthlyTimeApiClient.BulkUpdateStagingNamesAsync(dto).Returns(expected);

            var result = await _service.BulkUpdateStagingNamesAsync(dto);

            Assert.Same(expected, result);
            await _pactMonthlyTimeApiClient.Received(1).BulkUpdateStagingNamesAsync(dto);
        }

        [Fact]
        public async Task DeleteStagingAsync_DelegatesToApiClient()
        {
            var expected = ApiResponseDto<bool>.SuccessResponse(true);
            _pactMonthlyTimeApiClient.DeleteStagingAsync(5).Returns(expected);

            var result = await _service.DeleteStagingAsync(5);

            Assert.Same(expected, result);
            await _pactMonthlyTimeApiClient.Received(1).DeleteStagingAsync(5);
        }

        [Fact]
        public async Task DeleteAllStagingByUserAsync_DelegatesToApiClient()
        {
            var expected = ApiResponseDto<bool>.SuccessResponse(true);
            _pactMonthlyTimeApiClient.DeleteAllStagingByUserAsync().Returns(expected);

            var result = await _service.DeleteAllStagingByUserAsync();

            Assert.Same(expected, result);
            await _pactMonthlyTimeApiClient.Received(1).DeleteAllStagingByUserAsync();
        }

        [Fact]
        public async Task DeleteFailedStagingByUserAsync_DelegatesToApiClient()
        {
            var expected = ApiResponseDto<bool>.SuccessResponse(true);
            _pactMonthlyTimeApiClient.DeleteFailedStagingByUserAsync().Returns(expected);

            var result = await _service.DeleteFailedStagingByUserAsync();

            Assert.Same(expected, result);
            await _pactMonthlyTimeApiClient.Received(1).DeleteFailedStagingByUserAsync();
        }

        [Fact]
        public async Task ValidateStagingAsync_DelegatesToApiClient()
        {
            var expected = ApiResponseDto<MonthlyTimeValidateResultDto>.SuccessResponse(new MonthlyTimeValidateResultDto());
            _pactMonthlyTimeApiClient.ValidateStagingAsync().Returns(expected);

            var result = await _service.ValidateStagingAsync();

            Assert.Same(expected, result);
            await _pactMonthlyTimeApiClient.Received(1).ValidateStagingAsync();
        }

        [Fact]
        public async Task MakeLiveAsync_DelegatesToApiClient()
        {
            var expected = ApiResponseDto<MonthlyTimeMakeLiveResultDto>.SuccessResponse(new MonthlyTimeMakeLiveResultDto());
            _pactMonthlyTimeApiClient.MakeLiveAsync().Returns(expected);

            var result = await _service.MakeLiveAsync();

            Assert.Same(expected, result);
            await _pactMonthlyTimeApiClient.Received(1).MakeLiveAsync();
        }

        #endregion

        #region ImportMonthlyTimeAsync Tests

        [Fact]
        public async Task ImportMonthlyTimeAsync_UnsupportedType_ReturnsFailure()
        {
            var file = CreateMockFormFileWithExcel("test.xlsx");

            var result = await _service.ImportMonthlyTimeAsync(file, 99);

            Assert.False(result.Success);
            Assert.Contains(result.Errors!, e => e.Code == "INVALID_IMPORT_TYPE");
        }

        [Fact]
        public async Task ImportMonthlyTimeAsync_ImportFailure_ReturnsFailureWithoutUpload()
        {
            var file = CreateMockFormFileWithExcel("WG101TS.xlsx");
            _excelImportService.ReadExcel(
                Arg.Any<ClosedXML.Excel.IXLWorkbook>(),
                Arg.Any<Func<ClosedXML.Excel.IXLRangeRow, Dictionary<string, int>, MonthlyTimeImportRowDto>>(),
                Arg.Any<IEnumerable<string>>(),
                Arg.Any<int>(),
                Arg.Any<string>())
                .Returns(new ExcelImportResult<MonthlyTimeImportRowDto> { IsSuccess = false, ErrorMessage = "Bad template", MissingHeaders = ["Work Group"] });

            var result = await _service.ImportMonthlyTimeAsync(file, 2);

            Assert.False(result.Success);
            await _s3StorageService.DidNotReceive().UploadFileAsync(Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
        }

        [Fact]
        public async Task ImportMonthlyTimeAsync_SuccessfulImport_UploadsAuditFile()
        {
            var file = CreateMockFormFileWithExcel("WG101TS.xlsx");
            SetupSuccessfulFlatFileImport();
            _configuration["S3Storage:BucketName"].Returns("test-bucket");
            SetupHttpContextWithYear(2024);
            _s3StorageService.UploadFileAsync(Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
                .Returns(new S3UploadResult { Success = true, ObjectKey = "key" });

            var result = await _service.ImportMonthlyTimeAsync(file, 2);

            Assert.True(result.Success);
            await _s3StorageService.Received(1).UploadFileAsync(Arg.Any<Stream>(), "test-bucket", "FPS2024/MonthlyTime", Arg.Any<string>(), Arg.Any<string>());
        }

        [Fact]
        public async Task ImportMonthlyTimeAsync_S3UploadFails_LogsWarningAndReturnsSuccess()
        {
            var file = CreateMockFormFileWithExcel("WG101TS.xlsx");
            SetupSuccessfulFlatFileImport();
            _configuration["S3Storage:BucketName"].Returns("test-bucket");
            SetupHttpContextWithYear(2024);
            _s3StorageService.UploadFileAsync(Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
                .Returns(new S3UploadResult { Success = false, ErrorCode = "S3_ERR", Message = "Upload failed" });

            var result = await _service.ImportMonthlyTimeAsync(file, 2);

            Assert.True(result.Success);
        }

        [Fact]
        public async Task ImportMonthlyTimeAsync_S3UploadThrows_LogsWarningAndReturnsSuccess()
        {
            var file = CreateMockFormFileWithExcel("WG101TS.xlsx");
            SetupSuccessfulFlatFileImport();
            _configuration["S3Storage:BucketName"].Returns("test-bucket");
            SetupHttpContextWithYear(2024);
            _s3StorageService.UploadFileAsync(Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
                .ThrowsAsync(new Exception("S3 down"));

            var result = await _service.ImportMonthlyTimeAsync(file, 2);

            Assert.True(result.Success);
        }

        [Fact]
        public async Task ImportMonthlyTimeAsync_ImportReturnsNullData_ReturnsResponse()
        {
            var file = CreateMockFormFileWithExcel("WG101TS.xlsx");
            _excelImportService.ReadExcel(
                Arg.Any<ClosedXML.Excel.IXLWorkbook>(),
                Arg.Any<Func<ClosedXML.Excel.IXLRangeRow, Dictionary<string, int>, MonthlyTimeImportRowDto>>(),
                Arg.Any<IEnumerable<string>>(),
                Arg.Any<int>(),
                Arg.Any<string>())
                .Returns(new ExcelImportResult<MonthlyTimeImportRowDto> { IsSuccess = true, Rows = [new MonthlyTimeImportRowDto()] });
            _pactMonthlyTimeApiClient.ImportStagingAsync(Arg.Any<MonthlyTimeImportReqDto>())
                .Returns(ApiResponseDto<MonthlyTimeImportResultDto>.SuccessResponse(null!));

            var result = await _service.ImportMonthlyTimeAsync(file, 2);

            Assert.True(result.Success);
            await _s3StorageService.DidNotReceive().UploadFileAsync(Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
        }

        #endregion

        #region ImportFlatFileAsync Tests

        [Fact]
        public async Task ImportMonthlyTimeAsync_FlatFile_InvalidFilename_ReturnsFailure()
        {
            var file = CreateMockFormFileWithExcel("badname.xlsx");

            var result = await _service.ImportMonthlyTimeAsync(file, 2);

            Assert.False(result.Success);
            Assert.Contains(result.Errors!, e => e.Code == "INVALID_FILENAME");
        }

        [Fact]
        public async Task ImportMonthlyTimeAsync_FlatFile_EmptyFile_ReturnsFailure()
        {
            var file = CreateMockFormFileWithExcel("WG101TS.xlsx");
            _excelImportService.ReadExcel(
                Arg.Any<ClosedXML.Excel.IXLWorkbook>(),
                Arg.Any<Func<ClosedXML.Excel.IXLRangeRow, Dictionary<string, int>, MonthlyTimeImportRowDto>>(),
                Arg.Any<IEnumerable<string>>(),
                Arg.Any<int>(),
                Arg.Any<string>())
                .Returns(new ExcelImportResult<MonthlyTimeImportRowDto> { IsSuccess = false, ErrorMessage = "No data", MissingHeaders = [] });

            var result = await _service.ImportMonthlyTimeAsync(file, 2);

            Assert.False(result.Success);
            Assert.Contains(result.Errors!, e => e.Code == "EMPTY_FILE");
        }

        [Fact]
        public async Task ImportMonthlyTimeAsync_FlatFile_MissingHeaders_ReturnsFailure()
        {
            var file = CreateMockFormFileWithExcel("WG101TS.xlsx");
            _excelImportService.ReadExcel(
                Arg.Any<ClosedXML.Excel.IXLWorkbook>(),
                Arg.Any<Func<ClosedXML.Excel.IXLRangeRow, Dictionary<string, int>, MonthlyTimeImportRowDto>>(),
                Arg.Any<IEnumerable<string>>(),
                Arg.Any<int>(),
                Arg.Any<string>())
                .Returns(new ExcelImportResult<MonthlyTimeImportRowDto> { IsSuccess = false, ErrorMessage = "Bad template", MissingHeaders = ["Work Group"] });

            var result = await _service.ImportMonthlyTimeAsync(file, 2);

            Assert.False(result.Success);
            Assert.Contains(result.Errors!, e => e.Code == "INVALID_TEMPLATE");
        }

        [Fact]
        public async Task ImportMonthlyTimeAsync_FlatFile_Success_DelegatesToImportStaging()
        {
            var file = CreateMockFormFileWithExcel("WG101TS.xlsx");
            SetupSuccessfulFlatFileImport();
            SetupSuccessfulAuditUpload();

            var result = await _service.ImportMonthlyTimeAsync(file, 2);

            Assert.True(result.Success);
            await _pactMonthlyTimeApiClient.Received(1).ImportStagingAsync(Arg.Is<MonthlyTimeImportReqDto>(r => r.ImportType == 2));
        }

        #endregion

        #region ImportOtlDataAsync Tests

        [Fact]
        public async Task ImportMonthlyTimeAsync_OtlData_Success_DelegatesToImportStaging()
        {
            var file = CreateMockFormFileWithExcel("otl-data.xlsx");
            _excelImportService.ReadExcel(
                Arg.Any<ClosedXML.Excel.IXLWorkbook>(),
                Arg.Any<Func<ClosedXML.Excel.IXLRangeRow, Dictionary<string, int>, MonthlyTimeImportRowDto>>(),
                Arg.Any<IEnumerable<string>>(),
                Arg.Any<int>(),
                Arg.Any<string>())
                .Returns(new ExcelImportResult<MonthlyTimeImportRowDto> { IsSuccess = true, Rows = [new MonthlyTimeImportRowDto()] });
            _pactMonthlyTimeApiClient.ImportStagingAsync(Arg.Any<MonthlyTimeImportReqDto>())
                .Returns(ApiResponseDto<MonthlyTimeImportResultDto>.SuccessResponse(new MonthlyTimeImportResultDto()));
            SetupSuccessfulAuditUpload();

            var result = await _service.ImportMonthlyTimeAsync(file, 1);

            Assert.True(result.Success);
            await _pactMonthlyTimeApiClient.Received(1).ImportStagingAsync(Arg.Is<MonthlyTimeImportReqDto>(r => r.ImportType == 1));
        }

        [Fact]
        public async Task ImportMonthlyTimeAsync_OtlData_MissingHeaders_ReturnsFailure()
        {
            var file = CreateMockFormFileWithExcel("otl-data.xlsx");
            _excelImportService.ReadExcel(
                Arg.Any<ClosedXML.Excel.IXLWorkbook>(),
                Arg.Any<Func<ClosedXML.Excel.IXLRangeRow, Dictionary<string, int>, MonthlyTimeImportRowDto>>(),
                Arg.Any<IEnumerable<string>>(),
                Arg.Any<int>(),
                Arg.Any<string>())
                .Returns(new ExcelImportResult<MonthlyTimeImportRowDto> { IsSuccess = false, ErrorMessage = "Bad template", MissingHeaders = ["Work Group"] });

            var result = await _service.ImportMonthlyTimeAsync(file, 1);

            Assert.False(result.Success);
            Assert.Contains(result.Errors!, e => e.Code == "INVALID_TEMPLATE");
        }

        [Fact]
        public async Task ImportMonthlyTimeAsync_OtlData_EmptyFile_ReturnsFailure()
        {
            var file = CreateMockFormFileWithExcel("otl-data.xlsx");
            _excelImportService.ReadExcel(
                Arg.Any<ClosedXML.Excel.IXLWorkbook>(),
                Arg.Any<Func<ClosedXML.Excel.IXLRangeRow, Dictionary<string, int>, MonthlyTimeImportRowDto>>(),
                Arg.Any<IEnumerable<string>>(),
                Arg.Any<int>(),
                Arg.Any<string>())
                .Returns(new ExcelImportResult<MonthlyTimeImportRowDto> { IsSuccess = false, ErrorMessage = "No data", MissingHeaders = [] });

            var result = await _service.ImportMonthlyTimeAsync(file, 1);

            Assert.False(result.Success);
            Assert.Contains(result.Errors!, e => e.Code == "EMPTY_FILE");
        }

        #endregion

        #region ImportCrossTabAsync Tests

        [Fact]
        public async Task ImportMonthlyTimeAsync_CrossTab_InvalidFilename_ReturnsFailure()
        {
            var file = CreateMockFormFileWithExcel("badname.xlsx");

            var result = await _service.ImportMonthlyTimeAsync(file, 3);

            Assert.False(result.Success);
            Assert.Contains(result.Errors!, e => e.Code == "INVALID_FILENAME");
        }

        [Fact]
        public async Task ImportMonthlyTimeAsync_CrossTab_EmptyFile_ReturnsFailure()
        {
            var file = CreateMockFormFileWithExcelContent("WG101TS.xlsx", wb =>
            {
                var ws = wb.Worksheets.Add("Sheet1");
                ws.Cell(1, 1).Value = "Time Code";
            });

            var result = await _service.ImportMonthlyTimeAsync(file, 3);

            Assert.False(result.Success);
            Assert.Contains(result.Errors!, e => e.Code == "EMPTY_FILE");
        }

        [Fact]
        public async Task ImportMonthlyTimeAsync_CrossTab_MissingRequiredHeaders_ReturnsFailure()
        {
            var file = CreateMockFormFileWithExcelContent("WG101TS.xlsx", wb =>
            {
                var ws = wb.Worksheets.Add("Sheet1");
                ws.Cell(1, 1).Value = "SomeOtherHeader";
                ws.Cell(2, 1).Value = "data";
            });
            _excelImportService.BuildHeaderMap(Arg.Any<ClosedXML.Excel.IXLRangeRow>())
                .Returns(new Dictionary<string, int> { ["someotherheader"] = 1 });
            _excelImportService.GetMissingRequiredHeaders(Arg.Any<Dictionary<string, int>>(), Arg.Any<string[]>())
                .Returns(["Time Code"]);

            var result = await _service.ImportMonthlyTimeAsync(file, 3);

            Assert.False(result.Success);
            Assert.Contains(result.Errors!, e => e.Code == "INVALID_TEMPLATE");
        }

        [Fact]
        public async Task ImportMonthlyTimeAsync_CrossTab_DisallowedHeaders_ReturnsFailure()
        {
            var file = CreateMockFormFileWithExcelContent("WG101TS.xlsx", wb =>
            {
                var ws = wb.Worksheets.Add("Sheet1");
                ws.Cell(1, 1).Value = "Time Code";
                ws.Cell(1, 2).Value = "Parent Project";
                ws.Cell(1, 3).Value = "Hours";
                ws.Cell(2, 1).Value = "TC1";
                ws.Cell(2, 2).Value = "PP1";
                ws.Cell(2, 3).Value = "8";
            });
            _excelImportService.BuildHeaderMap(Arg.Any<ClosedXML.Excel.IXLRangeRow>())
                .Returns(new Dictionary<string, int> { ["timecode"] = 1, ["parentproject"] = 2, ["hours"] = 3 });
            _excelImportService.GetMissingRequiredHeaders(Arg.Any<Dictionary<string, int>>(), Arg.Any<string[]>())
                .Returns([]);
            _excelImportService.NormalizeHeader("Hours").Returns("hours");
            _excelImportService.NormalizeHeader("Month").Returns("month");
            _excelImportService.NormalizeHeader("Work Group").Returns("workgroup");
            _excelImportService.NormalizeHeader("Name").Returns("name");
            _excelImportService.NormalizeHeader("StagingId").Returns("stagingid");
            _excelImportService.NormalizeHeader("Pact Staff Id").Returns("pactstaffid");
            _excelImportService.NormalizeHeader("Employee/Supplier Number").Returns("employee/suppliernumber");
            _excelImportService.NormalizeHeader("Employee/Supplier").Returns("employee/supplier");
            _excelImportService.NormalizeHeader("Task Number").Returns("tasknumber");
            _excelImportService.NormalizeHeader("Project Code").Returns("projectcode");
            _excelImportService.NormalizeHeader("Period").Returns("period");
            _excelImportService.NormalizeHeader("Sum of Quantity").Returns("sumofquantity");
            _excelImportService.NormalizeHeader("Time Code").Returns("timecode");
            _excelImportService.NormalizeHeader("Parent Project").Returns("parentproject");
            _excelImportService.NormalizeHeader("Description").Returns("description");

            var result = await _service.ImportMonthlyTimeAsync(file, 3);

            Assert.False(result.Success);
            Assert.Contains(result.Errors!, e => e.Code == "INVALID_TEMPLATE" && e.Message!.Contains("different import type"));
        }

        [Fact]
        public async Task ImportMonthlyTimeAsync_CrossTab_ValidData_DelegatesToImportStaging()
        {
            var file = CreateMockFormFileWithExcelContent("WG101TS.xlsx", wb =>
            {
                var ws = wb.Worksheets.Add("Sheet1");
                ws.Cell(1, 1).Value = "Time Code";
                ws.Cell(1, 2).Value = "Parent Project";
                ws.Cell(1, 3).Value = "Staff1";
                ws.Cell(2, 1).Value = "TC1";
                ws.Cell(2, 2).Value = "PP1";
                ws.Cell(2, 3).Value = "8";
            });
            _excelImportService.BuildHeaderMap(Arg.Any<ClosedXML.Excel.IXLRangeRow>())
                .Returns(new Dictionary<string, int> { ["timecode"] = 1, ["parentproject"] = 2 });
            _excelImportService.GetMissingRequiredHeaders(Arg.Any<Dictionary<string, int>>(), Arg.Any<string[]>())
                .Returns([]);
            _excelImportService.NormalizeHeader("Time Code").Returns("timecode");
            _excelImportService.NormalizeHeader("Parent Project").Returns("parentproject");
            _excelImportService.NormalizeHeader("Month").Returns("month");
            _excelImportService.NormalizeHeader("Description").Returns("description");
            _excelImportService.NormalizeHeader("Hours").Returns("hours");
            _excelImportService.NormalizeHeader("Work Group").Returns("workgroup");
            _excelImportService.NormalizeHeader("Name").Returns("name");
            _excelImportService.NormalizeHeader("StagingId").Returns("stagingid");
            _excelImportService.NormalizeHeader("Pact Staff Id").Returns("pactstaffid");
            _excelImportService.NormalizeHeader("Employee/Supplier Number").Returns("employee/suppliernumber");
            _excelImportService.NormalizeHeader("Employee/Supplier").Returns("employee/supplier");
            _excelImportService.NormalizeHeader("Task Number").Returns("tasknumber");
            _excelImportService.NormalizeHeader("Project Code").Returns("projectcode");
            _excelImportService.NormalizeHeader("Period").Returns("period");
            _excelImportService.NormalizeHeader("Sum of Quantity").Returns("sumofquantity");
            _excelImportService.GetText(Arg.Any<ClosedXML.Excel.IXLCell>()).Returns("TC1", "PP1", "8");
            _pactMonthlyTimeApiClient.ImportStagingAsync(Arg.Any<MonthlyTimeImportReqDto>())
                .Returns(ApiResponseDto<MonthlyTimeImportResultDto>.SuccessResponse(new MonthlyTimeImportResultDto()));
            SetupSuccessfulAuditUpload();

            var result = await _service.ImportMonthlyTimeAsync(file, 3);

            Assert.True(result.Success);
            await _pactMonthlyTimeApiClient.Received(1).ImportStagingAsync(Arg.Is<MonthlyTimeImportReqDto>(r => r.ImportType == 3));
        }

        #endregion

        #region ImportExportedDataAsync Tests

        [Fact]
        public async Task ImportMonthlyTimeAsync_ExportedData_EmptyFile_ReturnsFailure()
        {
            var file = CreateMockFormFileWithExcelContent("exported.xlsx", wb =>
            {
                var ws = wb.Worksheets.Add("Sheet1");
                ws.Cell(1, 1).Value = "StagingId";
            });

            var result = await _service.ImportMonthlyTimeAsync(file, 4);

            Assert.False(result.Success);
            Assert.Contains(result.Errors!, e => e.Code == "EMPTY_FILE");
        }

        [Fact]
        public async Task ImportMonthlyTimeAsync_ExportedData_MissingStagingIdColumn_ReturnsFailure()
        {
            var file = CreateMockFormFileWithExcelContent("exported.xlsx", wb =>
            {
                var ws = wb.Worksheets.Add("Sheet1");
                ws.Cell(1, 1).Value = "Work Group";
                ws.Cell(2, 1).Value = "WG1";
            });
            _excelImportService.BuildHeaderMap(Arg.Any<ClosedXML.Excel.IXLRangeRow>())
                .Returns(new Dictionary<string, int> { ["workgroup"] = 1 });
            _excelImportService.GetMissingRequiredHeaders(Arg.Any<Dictionary<string, int>>(), Arg.Any<string[]>())
                .Returns(["StagingId"]);

            var result = await _service.ImportMonthlyTimeAsync(file, 4);

            Assert.False(result.Success);
            Assert.Contains(result.Errors!, e => e.Code == "INVALID_TEMPLATE" && e.Message!.Contains("correction file"));
        }

        [Fact]
        public async Task ImportMonthlyTimeAsync_ExportedData_Success_DelegatesToImportStaging()
        {
            var file = CreateMockFormFileWithExcelContent("exported.xlsx", wb =>
            {
                var ws = wb.Worksheets.Add("Sheet1");
                ws.Cell(1, 1).Value = "StagingId";
                ws.Cell(2, 1).Value = "1";
            });
            _excelImportService.BuildHeaderMap(Arg.Any<ClosedXML.Excel.IXLRangeRow>())
                .Returns(new Dictionary<string, int> { ["stagingid"] = 1 });
            _excelImportService.GetMissingRequiredHeaders(Arg.Any<Dictionary<string, int>>(), Arg.Any<string[]>())
                .Returns([]);
            _excelImportService.ReadExcel(
                Arg.Any<ClosedXML.Excel.IXLWorkbook>(),
                Arg.Any<Func<ClosedXML.Excel.IXLRangeRow, Dictionary<string, int>, MonthlyTimeImportRowDto>>(),
                Arg.Any<IEnumerable<string>>(),
                Arg.Any<int>(),
                Arg.Any<string>())
                .Returns(new ExcelImportResult<MonthlyTimeImportRowDto> { IsSuccess = true, Rows = [new MonthlyTimeImportRowDto()] });
            _pactMonthlyTimeApiClient.ImportStagingAsync(Arg.Any<MonthlyTimeImportReqDto>())
                .Returns(ApiResponseDto<MonthlyTimeImportResultDto>.SuccessResponse(new MonthlyTimeImportResultDto()));
            SetupSuccessfulAuditUpload();

            var result = await _service.ImportMonthlyTimeAsync(file, 4);

            Assert.True(result.Success);
            await _pactMonthlyTimeApiClient.Received(1).ImportStagingAsync(Arg.Is<MonthlyTimeImportReqDto>(r => r.ImportType == 4));
        }

        [Fact]
        public async Task ImportMonthlyTimeAsync_ExportedData_ReadFailure_ReturnsFailure()
        {
            var file = CreateMockFormFileWithExcelContent("exported.xlsx", wb =>
            {
                var ws = wb.Worksheets.Add("Sheet1");
                ws.Cell(1, 1).Value = "StagingId";
                ws.Cell(2, 1).Value = "1";
            });
            _excelImportService.BuildHeaderMap(Arg.Any<ClosedXML.Excel.IXLRangeRow>())
                .Returns(new Dictionary<string, int> { ["stagingid"] = 1 });
            _excelImportService.GetMissingRequiredHeaders(Arg.Any<Dictionary<string, int>>(), Arg.Any<string[]>())
                .Returns([]);
            _excelImportService.ReadExcel(
                Arg.Any<ClosedXML.Excel.IXLWorkbook>(),
                Arg.Any<Func<ClosedXML.Excel.IXLRangeRow, Dictionary<string, int>, MonthlyTimeImportRowDto>>(),
                Arg.Any<IEnumerable<string>>(),
                Arg.Any<int>(),
                Arg.Any<string>())
                .Returns(new ExcelImportResult<MonthlyTimeImportRowDto> { IsSuccess = false, ErrorMessage = "Bad data", MissingHeaders = [] });

            var result = await _service.ImportMonthlyTimeAsync(file, 4);

            Assert.False(result.Success);
            Assert.Contains(result.Errors!, e => e.Code == "EMPTY_FILE");
        }

        #endregion

        #region UploadAuditFileAsync Branch Tests

        [Fact]
        public async Task ImportMonthlyTimeAsync_EmptySourceFileName_UsesDefault()
        {
            var file = CreateMockFormFileWithExcel("");
            _excelImportService.ReadExcel(
                Arg.Any<ClosedXML.Excel.IXLWorkbook>(),
                Arg.Any<Func<ClosedXML.Excel.IXLRangeRow, Dictionary<string, int>, MonthlyTimeImportRowDto>>(),
                Arg.Any<IEnumerable<string>>(),
                Arg.Any<int>(),
                Arg.Any<string>())
                .Returns(new ExcelImportResult<MonthlyTimeImportRowDto> { IsSuccess = true, Rows = [new MonthlyTimeImportRowDto()] });
            _pactMonthlyTimeApiClient.ImportStagingAsync(Arg.Any<MonthlyTimeImportReqDto>())
                .Returns(ApiResponseDto<MonthlyTimeImportResultDto>.SuccessResponse(new MonthlyTimeImportResultDto()));
            _configuration["S3Storage:BucketName"].Returns("test-bucket");
            SetupHttpContextWithYear(2024);
            _s3StorageService.UploadFileAsync(Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
                .Returns(new S3UploadResult { Success = true });

            // ImportType 1 doesn't require special filename - OTL data
            var result = await _service.ImportMonthlyTimeAsync(file, 1);

            Assert.True(result.Success);
        }

        [Fact]
        public async Task ImportMonthlyTimeAsync_NoSelectedFPSYear_UsesCurrentYear()
        {
            var file = CreateMockFormFileWithExcel("WG101TS.xlsx");
            SetupSuccessfulFlatFileImport();
            _configuration["S3Storage:BucketName"].Returns("test-bucket");
            var httpContext = new DefaultHttpContext();
            _httpContextAccessor.HttpContext.Returns(httpContext);
            _s3StorageService.UploadFileAsync(Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
                .Returns(new S3UploadResult { Success = true });

            var result = await _service.ImportMonthlyTimeAsync(file, 2);

            Assert.True(result.Success);
            await _s3StorageService.Received(1).UploadFileAsync(
                Arg.Any<Stream>(), "test-bucket",
                Arg.Is<string>(s => s.StartsWith("FPS")),
                Arg.Any<string>(), Arg.Any<string>());
        }

        [Fact]
        public async Task ImportMonthlyTimeAsync_NullHttpContext_UsesCurrentYear()
        {
            var file = CreateMockFormFileWithExcel("WG101TS.xlsx");
            SetupSuccessfulFlatFileImport();
            _configuration["S3Storage:BucketName"].Returns("test-bucket");
            _httpContextAccessor.HttpContext.Returns((HttpContext?)null);
            _s3StorageService.UploadFileAsync(Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
                .Returns(new S3UploadResult { Success = true });

            var result = await _service.ImportMonthlyTimeAsync(file, 2);

            Assert.True(result.Success);
        }

        [Fact]
        public async Task ImportMonthlyTimeAsync_InvalidSelectedYear_UsesCurrentYear()
        {
            var file = CreateMockFormFileWithExcel("WG101TS.xlsx");
            SetupSuccessfulFlatFileImport();
            _configuration["S3Storage:BucketName"].Returns("test-bucket");
            var httpContext = new DefaultHttpContext();
            httpContext.Items["SelectedFPSYear"] = "notanumber";
            _httpContextAccessor.HttpContext.Returns(httpContext);
            _s3StorageService.UploadFileAsync(Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
                .Returns(new S3UploadResult { Success = true });

            var result = await _service.ImportMonthlyTimeAsync(file, 2);

            Assert.True(result.Success);
        }

        [Fact]
        public async Task ImportMonthlyTimeAsync_ZeroSelectedYear_UsesCurrentYear()
        {
            var file = CreateMockFormFileWithExcel("WG101TS.xlsx");
            SetupSuccessfulFlatFileImport();
            _configuration["S3Storage:BucketName"].Returns("test-bucket");
            var httpContext = new DefaultHttpContext();
            httpContext.Items["SelectedFPSYear"] = "0";
            _httpContextAccessor.HttpContext.Returns(httpContext);
            _s3StorageService.UploadFileAsync(Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
                .Returns(new S3UploadResult { Success = true });

            var result = await _service.ImportMonthlyTimeAsync(file, 2);

            Assert.True(result.Success);
        }

        [Fact]
        public async Task ImportMonthlyTimeAsync_FileWithNoExtension_UsesDefaultExtension()
        {
            var file = CreateMockFormFileWithExcel("WG101TS");
            _excelImportService.ReadExcel(
                Arg.Any<ClosedXML.Excel.IXLWorkbook>(),
                Arg.Any<Func<ClosedXML.Excel.IXLRangeRow, Dictionary<string, int>, MonthlyTimeImportRowDto>>(),
                Arg.Any<IEnumerable<string>>(),
                Arg.Any<int>(),
                Arg.Any<string>())
                .Returns(new ExcelImportResult<MonthlyTimeImportRowDto> { IsSuccess = true, Rows = [new MonthlyTimeImportRowDto()] });
            _pactMonthlyTimeApiClient.ImportStagingAsync(Arg.Any<MonthlyTimeImportReqDto>())
                .Returns(ApiResponseDto<MonthlyTimeImportResultDto>.SuccessResponse(new MonthlyTimeImportResultDto()));
            SetupSuccessfulAuditUpload();

            var result = await _service.ImportMonthlyTimeAsync(file, 2);

            Assert.True(result.Success);
        }

        #endregion

        #region GetAuditBucketName Tests

        [Fact]
        public async Task ImportMonthlyTimeAsync_MissingBucketConfig_LogsWarningAndReturnsSuccess()
        {
            var file = CreateMockFormFileWithExcel("WG101TS.xlsx");
            SetupSuccessfulFlatFileImport();
            _configuration["S3Storage:BucketName"].Returns((string?)null);
            SetupHttpContextWithYear(2024);

            var result = await _service.ImportMonthlyTimeAsync(file, 2);

            Assert.True(result.Success);
        }

        #endregion

        #region Helper Methods

        private static IFormFile CreateMockFormFileWithExcel(string fileName)
        {
            var workbook = new ClosedXML.Excel.XLWorkbook();
            var ws = workbook.Worksheets.Add("Sheet1");
            ws.Cell(1, 1).Value = "Placeholder";
            var ms = new MemoryStream();
            workbook.SaveAs(ms);
            ms.Position = 0;

            var formFile = Substitute.For<IFormFile>();
            formFile.FileName.Returns(fileName);
            formFile.OpenReadStream().Returns(_ =>
            {
                ms.Position = 0;
                return ms;
            });
            formFile.ContentType.Returns("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
            return formFile;
        }

        private static IFormFile CreateMockFormFileWithExcelContent(string fileName, Action<ClosedXML.Excel.XLWorkbook> configure)
        {
            var workbook = new ClosedXML.Excel.XLWorkbook();
            configure(workbook);
            var ms = new MemoryStream();
            workbook.SaveAs(ms);
            ms.Position = 0;

            var formFile = Substitute.For<IFormFile>();
            formFile.FileName.Returns(fileName);
            formFile.OpenReadStream().Returns(_ =>
            {
                ms.Position = 0;
                return ms;
            });
            formFile.ContentType.Returns("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
            return formFile;
        }

        private void SetupSuccessfulFlatFileImport()
        {
            _excelImportService.ReadExcel(
                Arg.Any<ClosedXML.Excel.IXLWorkbook>(),
                Arg.Any<Func<ClosedXML.Excel.IXLRangeRow, Dictionary<string, int>, MonthlyTimeImportRowDto>>(),
                Arg.Any<IEnumerable<string>>(),
                Arg.Any<int>(),
                Arg.Any<string>())
                .Returns(new ExcelImportResult<MonthlyTimeImportRowDto> { IsSuccess = true, Rows = [new MonthlyTimeImportRowDto()] });
            _pactMonthlyTimeApiClient.ImportStagingAsync(Arg.Any<MonthlyTimeImportReqDto>())
                .Returns(ApiResponseDto<MonthlyTimeImportResultDto>.SuccessResponse(new MonthlyTimeImportResultDto()));
        }

        private void SetupSuccessfulAuditUpload()
        {
            _configuration["S3Storage:BucketName"].Returns("test-bucket");
            SetupHttpContextWithYear(2024);
            _s3StorageService.UploadFileAsync(Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
                .Returns(new S3UploadResult { Success = true, ObjectKey = "key" });
        }

        private void SetupHttpContextWithYear(int year)
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Items["SelectedFPSYear"] = year.ToString();
            _httpContextAccessor.HttpContext.Returns(httpContext);
        }

        #endregion
    }
}
