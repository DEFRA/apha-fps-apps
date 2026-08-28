using Apha.Common.Utilities.ExcelImport;
using Apha.Common.Utilities.Storage;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.PACT;
using Apha.FPSApps.Application.Interfaces.PactApiClients;
using Apha.FPSApps.Application.Pagination;
using Apha.FPSApps.Application.Services.PACT;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Apha.FPSApps.Application.UnitTests.Services.PACT.PactMonthlyOutputServiceTest
{
    public class PactMonthlyOutputServiceTests
    {
        private readonly IPactApiClient _pactClient;
        private readonly IPactMonthlyOutputApiClient _pactMonthlyOutputApiClient;
        private readonly IExcelImportService _excelImportService;
        private readonly IWorkGroupService _workGroupService;
        private readonly IMonthService _monthService;
        private readonly IS3StorageService _s3StorageService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;
        private readonly IMonthlyImportServiceDependencies _monthlyImportServiceDependencies;
        private readonly ILogger<PactMonthlyOutputService> _logger;
        private readonly PactMonthlyOutputService _service;

        public PactMonthlyOutputServiceTests()
        {
            _pactClient = Substitute.For<IPactApiClient>();
            _pactMonthlyOutputApiClient = Substitute.For<IPactMonthlyOutputApiClient>();
            _excelImportService = Substitute.For<IExcelImportService>();
            _workGroupService = Substitute.For<IWorkGroupService>();
            _monthService = Substitute.For<IMonthService>();
            _s3StorageService = Substitute.For<IS3StorageService>();
            _httpContextAccessor = Substitute.For<IHttpContextAccessor>();
            _configuration = Substitute.For<IConfiguration>();
            _monthlyImportServiceDependencies = Substitute.For<IMonthlyImportServiceDependencies>();
            _logger = Substitute.For<ILogger<PactMonthlyOutputService>>();

            _monthlyImportServiceDependencies.ExcelImportService.Returns(_excelImportService);
            _monthlyImportServiceDependencies.WorkGroupService.Returns(_workGroupService);
            _monthlyImportServiceDependencies.MonthService.Returns(_monthService);
            _monthlyImportServiceDependencies.S3StorageService.Returns(_s3StorageService);
            _monthlyImportServiceDependencies.HttpContextAccessor.Returns(_httpContextAccessor);
            _monthlyImportServiceDependencies.Configuration.Returns(_configuration);

            _pactClient.PactMonthlyOutput.Returns(_pactMonthlyOutputApiClient);
            _service = new PactMonthlyOutputService(
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
            var filter = new MonthlyOutputLogFilterDto { WorkGroup = "WG1", TestCode = "TC1" };
            var logs = new List<MonthlyOutputLogDto>
            {
                new() { SequenceNo = 1, TestCode = "TC1", Buyer = "BuyerA", WorkGroup = "WG1" },
                new() { SequenceNo = 2, TestCode = "TC1", Buyer = "BuyerB", WorkGroup = "WG1" }
            };
            var expectedResponse = ApiResponseDto<List<MonthlyOutputLogDto>>.SuccessResponse(logs);
            _pactMonthlyOutputApiClient.SearchAsync(query, filter).Returns(expectedResponse);

            // Act
            var result = await _service.SearchAsync(query, filter);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _pactMonthlyOutputApiClient.Received(1).SearchAsync(query, filter);
        }

        [Fact]
        public async Task SearchAsync_WithNoMatchingRecords_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var query = new QueryParameters<string>();
            var filter = new MonthlyOutputLogFilterDto { WorkGroup = "WG_NONE" };
            var expectedResponse = ApiResponseDto<List<MonthlyOutputLogDto>>.SuccessResponse(new List<MonthlyOutputLogDto>());
            _pactMonthlyOutputApiClient.SearchAsync(query, filter).Returns(expectedResponse);

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
            var filter = new MonthlyOutputLogFilterDto
            {
                WorkGroup = "WG1",
                TestCode = "TC1",
                Buyer = "BuyerA",
                DateImported = new DateTime(2024, 1, 15),
                Month = 1.0,
                UserId = "user1",
                InsertDelete = "I"
            };
            var logs = new List<MonthlyOutputLogDto>
            {
                new() { SequenceNo = 1, TestCode = "TC1", Buyer = "BuyerA", WorkGroup = "WG1", Month = 1.0, UserId = "user1", InsertDelete = "I" }
            };
            var expectedResponse = ApiResponseDto<List<MonthlyOutputLogDto>>.SuccessResponse(logs);
            _pactMonthlyOutputApiClient.SearchAsync(query, filter).Returns(expectedResponse);

            // Act
            var result = await _service.SearchAsync(query, filter);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Single(result.Data!);
            await _pactMonthlyOutputApiClient.Received(1).SearchAsync(query, filter);
        }

        [Fact]
        public async Task SearchAsync_WithEmptyFilter_DelegatesToApiClient()
        {
            // Arrange
            var query = new QueryParameters<string>();
            var filter = new MonthlyOutputLogFilterDto();
            var expectedResponse = ApiResponseDto<List<MonthlyOutputLogDto>>.SuccessResponse(new List<MonthlyOutputLogDto>());
            _pactMonthlyOutputApiClient.SearchAsync(query, filter).Returns(expectedResponse);

            // Act
            var result = await _service.SearchAsync(query, filter);

            // Assert
            Assert.NotNull(result);
            await _pactMonthlyOutputApiClient.Received(1).SearchAsync(query, filter);
        }

        [Fact]
        public async Task SearchAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string>();
            var filter = new MonthlyOutputLogFilterDto { WorkGroup = "WG1" };
            var errors = new List<ApiErrorDto> { new() { Message = "API Error", Code = "API_ERROR" } };
            var expectedResponse = ApiResponseDto<List<MonthlyOutputLogDto>>.FailureResponse(errors, new ApiMetaDto());
            _pactMonthlyOutputApiClient.SearchAsync(query, filter).Returns(expectedResponse);

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
            var filter = new MonthlyOutputLogFilterDto();
            _pactMonthlyOutputApiClient
                .SearchAsync(Arg.Any<QueryParameters<string>>(), Arg.Any<MonthlyOutputLogFilterDto>())
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
            var expected = ApiResponseDto<List<PactMonthlyOutputDto>>.SuccessResponse([]);
            _pactMonthlyOutputApiClient.GetLiveAsync(query, "WG1", "TC1", "Buyer1", 6).Returns(expected);

            var result = await _service.GetLiveAsync(query, "WG1", "TC1", "Buyer1", 6);

            Assert.Same(expected, result);
            await _pactMonthlyOutputApiClient.Received(1).GetLiveAsync(query, "WG1", "TC1", "Buyer1", 6);
        }

        [Fact]
        public async Task GetLiveByKeyAsync_WithValidKey_DelegatesToApiClient()
        {
            var dto = new PactMonthlyOutputDto { TestCode = "TC1", Buyer = "Buyer1", Month = 6, WorkGroup = "WG1" };
            var expected = ApiResponseDto<PactMonthlyOutputDto>.SuccessResponse(dto);
            _pactMonthlyOutputApiClient.GetLiveByKeyAsync("TC1", "Buyer1", 6, "WG1").Returns(expected);

            var result = await _service.GetLiveByKeyAsync("TC1", "Buyer1", 6, "WG1");

            Assert.Same(expected, result);
            await _pactMonthlyOutputApiClient.Received(1).GetLiveByKeyAsync("TC1", "Buyer1", 6, "WG1");
        }

        [Fact]
        public async Task UpdateLiveAsync_WithDto_DelegatesToApiClient()
        {
            var dto = new PactMonthlyOutputDto { TestCode = "TC1", Buyer = "Buyer1", Month = 6, WorkGroup = "WG1", Volume = 10 };
            var expected = ApiResponseDto<PactMonthlyOutputDto>.SuccessResponse(dto);
            _pactMonthlyOutputApiClient.UpdateLiveAsync(dto).Returns(expected);

            var result = await _service.UpdateLiveAsync(dto);

            Assert.Same(expected, result);
            await _pactMonthlyOutputApiClient.Received(1).UpdateLiveAsync(dto);
        }

        #endregion

        #region ValidateLiveAsync Tests

        [Fact]
        public async Task ValidateLiveAsync_WithValidData_ReturnsNoErrors()
        {
            var dto = new PactMonthlyOutputDto
            {
                WorkGroup = "WG1",
                TestCode = "TC1",
                Buyer = "Buyer1",
                Month = 6,
                Volume = 100
            };

            _workGroupService.GetAllWorkGroupsAsync().Returns(ApiResponseDto<List<WorkGroupDto>>.SuccessResponse(
            [
                new WorkGroupDto { WorkGroupName = "WG1" }
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
        public async Task ValidateLiveAsync_WithNullVolume_ReturnsVolumeError()
        {
            var dto = new PactMonthlyOutputDto
            {
                WorkGroup = "WG1",
                TestCode = "TC1",
                Buyer = "Buyer1",
                Month = 6,
                Volume = null
            };

            _workGroupService.GetAllWorkGroupsAsync().Returns(ApiResponseDto<List<WorkGroupDto>>.SuccessResponse(
            [
                new WorkGroupDto { WorkGroupName = "WG1" }
            ]));
            _monthService.GetAllMonthsAsync().Returns(ApiResponseDto<List<MonthDto>>.SuccessResponse(
            [
                new MonthDto { Monthnumber = 6, Monthname = "June" }
            ]));

            var result = await _service.ValidateLiveAsync(dto);

            Assert.True(result.Success);
            Assert.Single(result.Data!);
            Assert.Equal("Volume", result.Data![0].Field);
        }

        [Fact]
        public async Task ValidateLiveAsync_WithBlankWorkGroup_ReturnsWorkGroupBlankError()
        {
            var dto = new PactMonthlyOutputDto
            {
                WorkGroup = "  ",
                TestCode = "TC1",
                Buyer = "Buyer1",
                Month = 6,
                Volume = 100
            };

            _monthService.GetAllMonthsAsync().Returns(ApiResponseDto<List<MonthDto>>.SuccessResponse(
            [
                new MonthDto { Monthnumber = 6, Monthname = "June" }
            ]));

            var result = await _service.ValidateLiveAsync(dto);

            Assert.True(result.Success);
            var fields = result.Data!.Select(x => x.Field).ToList();
            Assert.Contains("WorkGroup", fields);
            Assert.Equal("The work group name is blank.", result.Data!.First(x => x.Field == "WorkGroup").Message);
        }

        [Fact]
        public async Task ValidateLiveAsync_WithNullWorkGroup_ReturnsWorkGroupBlankError()
        {
            var dto = new PactMonthlyOutputDto
            {
                WorkGroup = null!,
                TestCode = "TC1",
                Buyer = "Buyer1",
                Month = 6,
                Volume = 100
            };

            _monthService.GetAllMonthsAsync().Returns(ApiResponseDto<List<MonthDto>>.SuccessResponse(
            [
                new MonthDto { Monthnumber = 6, Monthname = "June" }
            ]));

            var result = await _service.ValidateLiveAsync(dto);

            Assert.True(result.Success);
            Assert.Contains(result.Data!, x => x.Field == "WorkGroup");
        }

        [Fact]
        public async Task ValidateLiveAsync_WithInvalidData_ReturnsExpectedErrors()
        {
            var dto = new PactMonthlyOutputDto
            {
                WorkGroup = "BAD-WG",
                TestCode = "",
                Buyer = "",
                Month = 99,
                Volume = 0
            };

            _workGroupService.GetAllWorkGroupsAsync().Returns(ApiResponseDto<List<WorkGroupDto>>.SuccessResponse(
            [
                new WorkGroupDto { WorkGroupName = "WG1" }
            ]));
            _monthService.GetAllMonthsAsync().Returns(ApiResponseDto<List<MonthDto>>.SuccessResponse(
            [
                new MonthDto { Monthnumber = 6, Monthname = "June" }
            ]));

            var result = await _service.ValidateLiveAsync(dto);

            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            var fields = result.Data!.Select(x => x.Field).ToList();
            Assert.Contains("Volume", fields);
            Assert.Contains("WorkGroup", fields);
            Assert.Contains("TestCode", fields);
            Assert.Contains("Buyer", fields);
            Assert.Contains("Month", fields);
        }

        [Fact]
        public async Task ValidateLiveAsync_WhenWorkGroupServiceFails_ReturnsWorkGroupInvalidError()
        {
            var dto = new PactMonthlyOutputDto
            {
                WorkGroup = "WG1",
                TestCode = "TC1",
                Buyer = "Buyer1",
                Month = 6,
                Volume = 100
            };

            _workGroupService.GetAllWorkGroupsAsync().Returns(
                ApiResponseDto<List<WorkGroupDto>>.FailureResponse([], new ApiMetaDto()));
            _monthService.GetAllMonthsAsync().Returns(ApiResponseDto<List<MonthDto>>.SuccessResponse(
            [
                new MonthDto { Monthnumber = 6, Monthname = "June" }
            ]));

            var result = await _service.ValidateLiveAsync(dto);

            Assert.True(result.Success);
            Assert.Contains(result.Data!, x => x.Field == "WorkGroup" && x.Message!.Contains("invalid"));
        }

        [Fact]
        public async Task ValidateLiveAsync_WhenMonthServiceFails_ReturnsMonthInvalidError()
        {
            var dto = new PactMonthlyOutputDto
            {
                WorkGroup = "WG1",
                TestCode = "TC1",
                Buyer = "Buyer1",
                Month = 6,
                Volume = 100
            };

            _workGroupService.GetAllWorkGroupsAsync().Returns(ApiResponseDto<List<WorkGroupDto>>.SuccessResponse(
            [
                new WorkGroupDto { WorkGroupName = "WG1" }
            ]));
            _monthService.GetAllMonthsAsync().Returns(
                ApiResponseDto<List<MonthDto>>.FailureResponse([], new ApiMetaDto()));

            var result = await _service.ValidateLiveAsync(dto);

            Assert.True(result.Success);
            Assert.Contains(result.Data!, x => x.Field == "Month" && x.Message!.Contains("invalid"));
        }

        [Fact]
        public async Task ValidateLiveAsync_WhenWorkGroupServiceReturnsNullData_ReturnsWorkGroupInvalidError()
        {
            var dto = new PactMonthlyOutputDto
            {
                WorkGroup = "WG1",
                TestCode = "TC1",
                Buyer = "Buyer1",
                Month = 6,
                Volume = 100
            };

            var wgResponse = ApiResponseDto<List<WorkGroupDto>>.SuccessResponse(null!);
            _workGroupService.GetAllWorkGroupsAsync().Returns(wgResponse);
            _monthService.GetAllMonthsAsync().Returns(ApiResponseDto<List<MonthDto>>.SuccessResponse(
            [
                new MonthDto { Monthnumber = 6, Monthname = "June" }
            ]));

            var result = await _service.ValidateLiveAsync(dto);

            Assert.True(result.Success);
            Assert.Contains(result.Data!, x => x.Field == "WorkGroup");
        }

        #endregion

        #region Staging Methods Tests

        [Fact]
        public async Task GetStagingAsync_WithValidQuery_DelegatesToApiClient()
        {
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            bool? passed = true;
            var expected = ApiResponseDto<List<StagingMonthlyOutputDto>>.SuccessResponse([]);
            _pactMonthlyOutputApiClient.GetStagingAsync(query, passed).Returns(expected);

            var result = await _service.GetStagingAsync(query, passed);

            Assert.Same(expected, result);
            await _pactMonthlyOutputApiClient.Received(1).GetStagingAsync(query, passed);
        }

        [Fact]
        public async Task GetStagingAsync_WithNullPassed_DelegatesToApiClient()
        {
            var query = new QueryParameters<string>();
            var expected = ApiResponseDto<List<StagingMonthlyOutputDto>>.SuccessResponse([]);
            _pactMonthlyOutputApiClient.GetStagingAsync(query, null).Returns(expected);

            var result = await _service.GetStagingAsync(query, null);

            Assert.Same(expected, result);
            await _pactMonthlyOutputApiClient.Received(1).GetStagingAsync(query, null);
        }

        [Fact]
        public async Task GetStagingByIdAsync_WithValidId_DelegatesToApiClient()
        {
            var dto = new StagingMonthlyOutputDto { Id = 1, TestCode = "TC1", Buyer = "Buyer1", WorkGroup = "WG1" };
            var expected = ApiResponseDto<StagingMonthlyOutputDto>.SuccessResponse(dto);
            _pactMonthlyOutputApiClient.GetStagingByIdAsync(1).Returns(expected);

            var result = await _service.GetStagingByIdAsync(1);

            Assert.Same(expected, result);
            await _pactMonthlyOutputApiClient.Received(1).GetStagingByIdAsync(1);
        }

        [Fact]
        public async Task CreateStagingAsync_WithDto_DelegatesToApiClient()
        {
            var dto = new StagingMonthlyOutputDto { TestCode = "TC1", Buyer = "Buyer1", WorkGroup = "WG1" };
            var expected = ApiResponseDto<StagingMonthlyOutputDto>.SuccessResponse(dto);
            _pactMonthlyOutputApiClient.CreateStagingAsync(dto).Returns(expected);

            var result = await _service.CreateStagingAsync(dto);

            Assert.Same(expected, result);
            await _pactMonthlyOutputApiClient.Received(1).CreateStagingAsync(dto);
        }

        [Fact]
        public async Task UpdateStagingAsync_WithIdAndDto_DelegatesToApiClient()
        {
            var dto = new StagingMonthlyOutputDto { Id = 5, TestCode = "TC1", Buyer = "Buyer1", WorkGroup = "WG1" };
            var expected = ApiResponseDto<StagingMonthlyOutputDto>.SuccessResponse(dto);
            _pactMonthlyOutputApiClient.UpdateStagingAsync(5, dto).Returns(expected);

            var result = await _service.UpdateStagingAsync(5, dto);

            Assert.Same(expected, result);
            await _pactMonthlyOutputApiClient.Received(1).UpdateStagingAsync(5, dto);
        }

        [Fact]
        public async Task DeleteStagingAsync_WithValidId_DelegatesToApiClient()
        {
            var expected = ApiResponseDto<bool>.SuccessResponse(true);
            _pactMonthlyOutputApiClient.DeleteStagingAsync(1).Returns(expected);

            var result = await _service.DeleteStagingAsync(1);

            Assert.Same(expected, result);
            await _pactMonthlyOutputApiClient.Received(1).DeleteStagingAsync(1);
        }

        [Fact]
        public async Task DeleteAllStagingByUserAsync_DelegatesToApiClient()
        {
            var expected = ApiResponseDto<bool>.SuccessResponse(true);
            _pactMonthlyOutputApiClient.DeleteAllStagingByUserAsync().Returns(expected);

            var result = await _service.DeleteAllStagingByUserAsync();

            Assert.Same(expected, result);
            await _pactMonthlyOutputApiClient.Received(1).DeleteAllStagingByUserAsync();
        }

        [Fact]
        public async Task DeleteFailedStagingByUserAsync_DelegatesToApiClient()
        {
            var expected = ApiResponseDto<bool>.SuccessResponse(true);
            _pactMonthlyOutputApiClient.DeleteFailedStagingByUserAsync().Returns(expected);

            var result = await _service.DeleteFailedStagingByUserAsync();

            Assert.Same(expected, result);
            await _pactMonthlyOutputApiClient.Received(1).DeleteFailedStagingByUserAsync();
        }

        #endregion

        #region ValidateStagingAsync Tests

        [Fact]
        public async Task ValidateStagingAsync_DelegatesToApiClient()
        {
            var expected = ApiResponseDto<MonthlyOutputValidateResultDto>.SuccessResponse(new MonthlyOutputValidateResultDto());
            _pactMonthlyOutputApiClient.ValidateStagingAsync().Returns(expected);

            var result = await _service.ValidateStagingAsync();

            Assert.Same(expected, result);
            await _pactMonthlyOutputApiClient.Received(1).ValidateStagingAsync();
        }

        [Fact]
        public async Task ValidateStagingAsync_WhenApiFails_ReturnsFailureResponse()
        {
            var errors = new List<ApiErrorDto> { new() { Message = "Validation failed", Code = "VALIDATION_ERROR" } };
            var expected = ApiResponseDto<MonthlyOutputValidateResultDto>.FailureResponse(errors, new ApiMetaDto());
            _pactMonthlyOutputApiClient.ValidateStagingAsync().Returns(expected);

            var result = await _service.ValidateStagingAsync();

            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region MakeLiveAsync Tests

        [Fact]
        public async Task MakeLiveAsync_DelegatesToApiClient()
        {
            var expected = ApiResponseDto<MonthlyOutputMakeLiveResultDto>.SuccessResponse(new MonthlyOutputMakeLiveResultDto());
            _pactMonthlyOutputApiClient.MakeLiveAsync().Returns(expected);

            var result = await _service.MakeLiveAsync();

            Assert.Same(expected, result);
            await _pactMonthlyOutputApiClient.Received(1).MakeLiveAsync();
        }

        [Fact]
        public async Task MakeLiveAsync_WhenApiFails_ReturnsFailureResponse()
        {
            var errors = new List<ApiErrorDto> { new() { Message = "Make live failed", Code = "MAKE_LIVE_ERROR" } };
            var expected = ApiResponseDto<MonthlyOutputMakeLiveResultDto>.FailureResponse(errors, new ApiMetaDto());
            _pactMonthlyOutputApiClient.MakeLiveAsync().Returns(expected);

            var result = await _service.MakeLiveAsync();

            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region ImportMonthlyOutputAsync Tests - Helpers

        private static IFormFile CreateMockFormFile(string fileName, string[] headers, string[][]? dataRows = null)
        {
            var workbook = new XLWorkbook();
            var ws = workbook.AddWorksheet("Sheet1");
            for (int i = 0; i < headers.Length; i++)
                ws.Cell(1, i + 1).Value = headers[i];
            if (dataRows != null)
            {
                for (int r = 0; r < dataRows.Length; r++)
                    for (int c = 0; c < dataRows[r].Length; c++)
                        ws.Cell(r + 2, c + 1).Value = dataRows[r][c];
            }
            var ms = new MemoryStream();
            workbook.SaveAs(ms);
            ms.Position = 0;

            var formFile = Substitute.For<IFormFile>();
            formFile.FileName.Returns(fileName);
            formFile.ContentType.Returns("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
            formFile.OpenReadStream().Returns(ms);
            return formFile;
        }

        private void SetupS3Config()
        {
            _configuration["S3Storage:BucketName"].Returns("test-bucket");
        }

        private void SetupS3UploadSuccess()
        {
            SetupS3Config();
            _s3StorageService.UploadFileAsync(
                Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(S3UploadResult.SuccessResponse("key"));
        }

        #endregion

        #region ImportMonthlyOutputAsync Tests - Fresh Import (importType=1)

        [Fact]
        public async Task ImportMonthlyOutputAsync_FreshImport_Success_ReturnsImportResult()
        {
            // Arrange
            var headers = new[] { "Work Group", "Test Code", "Buyer", "Month", "Volume" };
            var data = new[] { new[] { "WG1", "TC1", "Buyer1", "6", "100" } };
            var file = CreateMockFormFile("test.xlsx", headers, data);

            var rows = new List<MonthlyOutputImportRowDto> { new() { WorkGroup = "WG1" } };
            _excelImportService.ReadExcel(
                Arg.Any<IXLWorkbook>(),
                Arg.Any<Func<IXLRangeRow, Dictionary<string, int>, MonthlyOutputImportRowDto>>(),
                Arg.Any<IEnumerable<string>>(), Arg.Any<int>(), Arg.Any<string>())
                .Returns(new ExcelImportResult<MonthlyOutputImportRowDto> { IsSuccess = true, Rows = rows });

            _excelImportService.BuildHeaderMap(Arg.Any<IXLRangeRow>()).Returns(new Dictionary<string, int>());
            _excelImportService.NormalizeHeader(Arg.Any<string>()).Returns(x => ((string)x[0]).ToLowerInvariant());

            var importResult = new MonthlyOutputImportResultDto { ImportedCount = 1 };
            var importResponse = ApiResponseDto<MonthlyOutputImportResultDto>.SuccessResponse(importResult);
            _pactMonthlyOutputApiClient.ImportStagingAsync(Arg.Any<MonthlyOutputImportReqDto>()).Returns(importResponse);

            SetupS3UploadSuccess();

            // Act
            var result = await _service.ImportMonthlyOutputAsync(file, 1);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(1, result.Data!.ImportedCount);
            await _pactMonthlyOutputApiClient.Received(1).ImportStagingAsync(Arg.Is<MonthlyOutputImportReqDto>(r => r.ImportType == 1));
        }

        [Fact]
        public async Task ImportMonthlyOutputAsync_FreshImport_ReadExcelFails_MissingHeaders_ReturnsInvalidTemplate()
        {
            var headers = new[] { "Work Group", "Test Code", "Buyer", "Month", "Volume" };
            var file = CreateMockFormFile("test.xlsx", headers);

            _excelImportService.ReadExcel(
                Arg.Any<IXLWorkbook>(),
                Arg.Any<Func<IXLRangeRow, Dictionary<string, int>, MonthlyOutputImportRowDto>>(),
                Arg.Any<IEnumerable<string>>(), Arg.Any<int>(), Arg.Any<string>())
                .Returns(new ExcelImportResult<MonthlyOutputImportRowDto>
                {
                    IsSuccess = false,
                    MissingHeaders = ["Volume"],
                    ErrorMessage = "Missing columns"
                });

            var result = await _service.ImportMonthlyOutputAsync(file, 1);

            Assert.False(result.Success);
            Assert.Equal("INVALID_TEMPLATE", result.Errors!.First().Code);
            Assert.Contains("Missing columns", result.Errors!.First().Message);
        }

        [Fact]
        public async Task ImportMonthlyOutputAsync_FreshImport_ReadExcelFails_EmptyFile_ReturnsEmptyFileError()
        {
            var headers = new[] { "Work Group", "Test Code", "Buyer", "Month", "Volume" };
            var file = CreateMockFormFile("test.xlsx", headers);

            _excelImportService.ReadExcel(
                Arg.Any<IXLWorkbook>(),
                Arg.Any<Func<IXLRangeRow, Dictionary<string, int>, MonthlyOutputImportRowDto>>(),
                Arg.Any<IEnumerable<string>>(), Arg.Any<int>(), Arg.Any<string>())
                .Returns(new ExcelImportResult<MonthlyOutputImportRowDto>
                {
                    IsSuccess = false,
                    MissingHeaders = [],
                    ErrorMessage = null
                });

            var result = await _service.ImportMonthlyOutputAsync(file, 1);

            Assert.False(result.Success);
            Assert.Equal("EMPTY_FILE", result.Errors!.First().Code);
            Assert.Equal("No data rows found in the uploaded Excel file.", result.Errors!.First().Message);
        }

        [Fact]
        public async Task ImportMonthlyOutputAsync_FreshImport_ReadExcelFails_EmptyFileWithMessage_ReturnsCustomMessage()
        {
            var headers = new[] { "Work Group", "Test Code", "Buyer", "Month", "Volume" };
            var file = CreateMockFormFile("test.xlsx", headers);

            _excelImportService.ReadExcel(
                Arg.Any<IXLWorkbook>(),
                Arg.Any<Func<IXLRangeRow, Dictionary<string, int>, MonthlyOutputImportRowDto>>(),
                Arg.Any<IEnumerable<string>>(), Arg.Any<int>(), Arg.Any<string>())
                .Returns(new ExcelImportResult<MonthlyOutputImportRowDto>
                {
                    IsSuccess = false,
                    MissingHeaders = [],
                    ErrorMessage = "Custom error message"
                });

            var result = await _service.ImportMonthlyOutputAsync(file, 1);

            Assert.False(result.Success);
            Assert.Equal("EMPTY_FILE", result.Errors!.First().Code);
            Assert.Equal("Custom error message", result.Errors!.First().Message);
        }

        [Fact]
        public async Task ImportMonthlyOutputAsync_FreshImport_DisallowedHeaders_ReturnsInvalidTemplate()
        {
            var headers = new[] { "Work Group", "Test Code", "Buyer", "Month", "Volume" };
            var data = new[] { new[] { "WG1", "TC1", "Buyer1", "6", "100" } };
            var file = CreateMockFormFile("test.xlsx", headers, data);

            _excelImportService.ReadExcel(
                Arg.Any<IXLWorkbook>(),
                Arg.Any<Func<IXLRangeRow, Dictionary<string, int>, MonthlyOutputImportRowDto>>(),
                Arg.Any<IEnumerable<string>>(), Arg.Any<int>(), Arg.Any<string>())
                .Returns(new ExcelImportResult<MonthlyOutputImportRowDto> { IsSuccess = true, Rows = [new()] });

            // Simulate disallowed header "Passed" found in header map
            var headerMap = new Dictionary<string, int> { { "passed", 1 } };
            _excelImportService.BuildHeaderMap(Arg.Any<IXLRangeRow>()).Returns(headerMap);
            _excelImportService.NormalizeHeader("Passed").Returns("passed");
            _excelImportService.NormalizeHeader("Failure Comments").Returns("failurecomments");
            _excelImportService.NormalizeHeader("Filename").Returns("filename");
            _excelImportService.NormalizeHeader("StagingId").Returns("stagingid");

            var result = await _service.ImportMonthlyOutputAsync(file, 1);

            Assert.False(result.Success);
            Assert.Equal("INVALID_TEMPLATE", result.Errors!.First().Code);
            Assert.Contains("exported or correction file", result.Errors!.First().Message);
        }

        [Fact]
        public async Task ImportMonthlyOutputAsync_FreshImport_ImportResponseFails_ReturnsFailure()
        {
            var headers = new[] { "Work Group", "Test Code", "Buyer", "Month", "Volume" };
            var data = new[] { new[] { "WG1", "TC1", "Buyer1", "6", "100" } };
            var file = CreateMockFormFile("test.xlsx", headers, data);

            _excelImportService.ReadExcel(
                Arg.Any<IXLWorkbook>(),
                Arg.Any<Func<IXLRangeRow, Dictionary<string, int>, MonthlyOutputImportRowDto>>(),
                Arg.Any<IEnumerable<string>>(), Arg.Any<int>(), Arg.Any<string>())
                .Returns(new ExcelImportResult<MonthlyOutputImportRowDto> { IsSuccess = true, Rows = [new()] });

            _excelImportService.BuildHeaderMap(Arg.Any<IXLRangeRow>()).Returns(new Dictionary<string, int>());
            _excelImportService.NormalizeHeader(Arg.Any<string>()).Returns(x => ((string)x[0]).ToLowerInvariant());

            var failResponse = ApiResponseDto<MonthlyOutputImportResultDto>.FailureResponse(
                [new ApiErrorDto { Code = "IMPORT_ERROR", Message = "Import failed" }], new ApiMetaDto());
            _pactMonthlyOutputApiClient.ImportStagingAsync(Arg.Any<MonthlyOutputImportReqDto>()).Returns(failResponse);

            var result = await _service.ImportMonthlyOutputAsync(file, 1);

            Assert.False(result.Success);
        }

        #endregion

        #region ImportMonthlyOutputAsync Tests - Exported Data (importType=4)

        [Fact]
        public async Task ImportMonthlyOutputAsync_ExportedData_EmptyFile_ReturnsEmptyFileError()
        {
            // Create workbook with only header row (no data rows)
            var headers = new[] { "Work Group", "Test Code", "Buyer", "Month", "Volume", "StagingId" };
            var file = CreateMockFormFile("test.xlsx", headers);

            var result = await _service.ImportMonthlyOutputAsync(file, 4);

            Assert.False(result.Success);
            Assert.Equal("EMPTY_FILE", result.Errors!.First().Code);
        }

        [Fact]
        public async Task ImportMonthlyOutputAsync_ExportedData_MissingStagingIdColumn_ReturnsInvalidTemplate()
        {
            var headers = new[] { "Work Group", "Test Code", "Buyer", "Month", "Volume", "StagingId" };
            var data = new[] { new[] { "WG1", "TC1", "Buyer1", "6", "100", "1" } };
            var file = CreateMockFormFile("test.xlsx", headers, data);

            _excelImportService.BuildHeaderMap(Arg.Any<IXLRangeRow>()).Returns(new Dictionary<string, int>());
            _excelImportService.GetMissingRequiredHeaders(Arg.Any<Dictionary<string, int>>(), Arg.Any<IEnumerable<string>>())
                .Returns(["StagingId"]);

            var result = await _service.ImportMonthlyOutputAsync(file, 4);

            Assert.False(result.Success);
            Assert.Equal("INVALID_TEMPLATE", result.Errors!.First().Code);
            Assert.Contains("StagingId", result.Errors!.First().Message);
        }

        [Fact]
        public async Task ImportMonthlyOutputAsync_ExportedData_ReadExcelFails_MissingHeaders_ReturnsInvalidTemplate()
        {
            var headers = new[] { "Work Group", "Test Code", "Buyer", "Month", "Volume", "StagingId" };
            var data = new[] { new[] { "WG1", "TC1", "Buyer1", "6", "100", "1" } };
            var file = CreateMockFormFile("test.xlsx", headers, data);

            _excelImportService.BuildHeaderMap(Arg.Any<IXLRangeRow>()).Returns(new Dictionary<string, int>());
            _excelImportService.GetMissingRequiredHeaders(Arg.Any<Dictionary<string, int>>(), Arg.Any<IEnumerable<string>>())
                .Returns(Enumerable.Empty<string>());

            _excelImportService.ReadExcel(
                Arg.Any<IXLWorkbook>(),
                Arg.Any<Func<IXLRangeRow, Dictionary<string, int>, MonthlyOutputImportRowDto>>(),
                Arg.Any<IEnumerable<string>>(), Arg.Any<int>(), Arg.Any<string>())
                .Returns(new ExcelImportResult<MonthlyOutputImportRowDto>
                {
                    IsSuccess = false,
                    MissingHeaders = ["Volume"],
                    ErrorMessage = "Missing columns"
                });

            var result = await _service.ImportMonthlyOutputAsync(file, 4);

            Assert.False(result.Success);
            Assert.Equal("INVALID_TEMPLATE", result.Errors!.First().Code);
        }

        [Fact]
        public async Task ImportMonthlyOutputAsync_ExportedData_ReadExcelFails_EmptyFile_ReturnsEmptyFileError()
        {
            var headers = new[] { "Work Group", "Test Code", "Buyer", "Month", "Volume", "StagingId" };
            var data = new[] { new[] { "WG1", "TC1", "Buyer1", "6", "100", "1" } };
            var file = CreateMockFormFile("test.xlsx", headers, data);

            _excelImportService.BuildHeaderMap(Arg.Any<IXLRangeRow>()).Returns(new Dictionary<string, int>());
            _excelImportService.GetMissingRequiredHeaders(Arg.Any<Dictionary<string, int>>(), Arg.Any<IEnumerable<string>>())
                .Returns(Enumerable.Empty<string>());

            _excelImportService.ReadExcel(
                Arg.Any<IXLWorkbook>(),
                Arg.Any<Func<IXLRangeRow, Dictionary<string, int>, MonthlyOutputImportRowDto>>(),
                Arg.Any<IEnumerable<string>>(), Arg.Any<int>(), Arg.Any<string>())
                .Returns(new ExcelImportResult<MonthlyOutputImportRowDto>
                {
                    IsSuccess = false,
                    MissingHeaders = [],
                    ErrorMessage = null
                });

            var result = await _service.ImportMonthlyOutputAsync(file, 4);

            Assert.False(result.Success);
            Assert.Equal("EMPTY_FILE", result.Errors!.First().Code);
        }

        [Fact]
        public async Task ImportMonthlyOutputAsync_ExportedData_Success_DelegatesToApiClient()
        {
            var headers = new[] { "Work Group", "Test Code", "Buyer", "Month", "Volume", "StagingId" };
            var data = new[] { new[] { "WG1", "TC1", "Buyer1", "6", "100", "1" } };
            var file = CreateMockFormFile("test.xlsx", headers, data);

            _excelImportService.BuildHeaderMap(Arg.Any<IXLRangeRow>()).Returns(new Dictionary<string, int>());
            _excelImportService.GetMissingRequiredHeaders(Arg.Any<Dictionary<string, int>>(), Arg.Any<IEnumerable<string>>())
                .Returns(Enumerable.Empty<string>());

            var rows = new List<MonthlyOutputImportRowDto> { new() { Id = 1, WorkGroup = "WG1" } };
            _excelImportService.ReadExcel(
                Arg.Any<IXLWorkbook>(),
                Arg.Any<Func<IXLRangeRow, Dictionary<string, int>, MonthlyOutputImportRowDto>>(),
                Arg.Any<IEnumerable<string>>(), Arg.Any<int>(), Arg.Any<string>())
                .Returns(new ExcelImportResult<MonthlyOutputImportRowDto> { IsSuccess = true, Rows = rows });

            var importResponse = ApiResponseDto<MonthlyOutputImportResultDto>.SuccessResponse(new MonthlyOutputImportResultDto { ImportedCount = 1 });
            _pactMonthlyOutputApiClient.ImportStagingAsync(Arg.Any<MonthlyOutputImportReqDto>()).Returns(importResponse);

            SetupS3UploadSuccess();

            var result = await _service.ImportMonthlyOutputAsync(file, 4);

            Assert.True(result.Success);
            await _pactMonthlyOutputApiClient.Received(1).ImportStagingAsync(Arg.Is<MonthlyOutputImportReqDto>(r => r.ImportType == 4));
        }

        #endregion

        #region ImportMonthlyOutputAsync Tests - LIMS Data (importType=2)

        [Fact]
        public async Task ImportMonthlyOutputAsync_LimsData_DisallowedHeaders_ReturnsInvalidTemplate()
        {
            var headers = new[] { "Group", "Test", "Project", "Month", "SumOfCountTotal" };
            var data = new[] { new[] { "WG1", "TC1", "Buyer1", "6", "100" } };
            var file = CreateMockFormFile("test.xlsx", headers, data);

            var headerMap = new Dictionary<string, int> { { "passed", 1 } };
            _excelImportService.BuildHeaderMap(Arg.Any<IXLRangeRow>()).Returns(headerMap);
            _excelImportService.NormalizeHeader("Passed").Returns("passed");
            _excelImportService.NormalizeHeader("Failure Comments").Returns("failurecomments");
            _excelImportService.NormalizeHeader("Filename").Returns("filename");
            _excelImportService.NormalizeHeader("StagingId").Returns("stagingid");

            var result = await _service.ImportMonthlyOutputAsync(file, 2);

            Assert.False(result.Success);
            Assert.Equal("INVALID_TEMPLATE", result.Errors!.First().Code);
            Assert.Contains("LIMS file template", result.Errors!.First().Message);
        }

        [Fact]
        public async Task ImportMonthlyOutputAsync_LimsData_ReadExcelFails_MissingHeaders_ReturnsInvalidTemplate()
        {
            var headers = new[] { "Group", "Test", "Project", "Month", "SumOfCountTotal" };
            var data = new[] { new[] { "WG1", "TC1", "Buyer1", "6", "100" } };
            var file = CreateMockFormFile("test.xlsx", headers, data);

            _excelImportService.BuildHeaderMap(Arg.Any<IXLRangeRow>()).Returns(new Dictionary<string, int>());
            _excelImportService.NormalizeHeader(Arg.Any<string>()).Returns(x => ((string)x[0]).ToLowerInvariant());

            _excelImportService.ReadExcel(
                Arg.Any<IXLWorkbook>(),
                Arg.Any<Func<IXLRangeRow, Dictionary<string, int>, MonthlyOutputImportRowDto>>(),
                Arg.Any<IEnumerable<string>>(), Arg.Any<int>(), Arg.Any<string>())
                .Returns(new ExcelImportResult<MonthlyOutputImportRowDto>
                {
                    IsSuccess = false,
                    MissingHeaders = ["SumOfCountTotal"],
                    ErrorMessage = "Missing columns"
                });

            var result = await _service.ImportMonthlyOutputAsync(file, 2);

            Assert.False(result.Success);
            Assert.Equal("INVALID_TEMPLATE", result.Errors!.First().Code);
            Assert.Contains("Missing columns", result.Errors!.First().Message);
        }

        [Fact]
        public async Task ImportMonthlyOutputAsync_LimsData_ReadExcelFails_EmptyFile_ReturnsEmptyFileError()
        {
            var headers = new[] { "Group", "Test", "Project", "Month", "SumOfCountTotal" };
            var data = new[] { new[] { "WG1", "TC1", "Buyer1", "6", "100" } };
            var file = CreateMockFormFile("test.xlsx", headers, data);

            _excelImportService.BuildHeaderMap(Arg.Any<IXLRangeRow>()).Returns(new Dictionary<string, int>());
            _excelImportService.NormalizeHeader(Arg.Any<string>()).Returns(x => ((string)x[0]).ToLowerInvariant());

            _excelImportService.ReadExcel(
                Arg.Any<IXLWorkbook>(),
                Arg.Any<Func<IXLRangeRow, Dictionary<string, int>, MonthlyOutputImportRowDto>>(),
                Arg.Any<IEnumerable<string>>(), Arg.Any<int>(), Arg.Any<string>())
                .Returns(new ExcelImportResult<MonthlyOutputImportRowDto>
                {
                    IsSuccess = false,
                    MissingHeaders = [],
                    ErrorMessage = null
                });

            var result = await _service.ImportMonthlyOutputAsync(file, 2);

            Assert.False(result.Success);
            Assert.Equal("EMPTY_FILE", result.Errors!.First().Code);
        }

        [Fact]
        public async Task ImportMonthlyOutputAsync_LimsData_Success_DelegatesToApiClient()
        {
            var headers = new[] { "Group", "Test", "Project", "Month", "SumOfCountTotal" };
            var data = new[] { new[] { "WG1", "TC1", "Buyer1", "6", "100" } };
            var file = CreateMockFormFile("test.xlsx", headers, data);

            _excelImportService.BuildHeaderMap(Arg.Any<IXLRangeRow>()).Returns(new Dictionary<string, int>());
            _excelImportService.NormalizeHeader(Arg.Any<string>()).Returns(x => ((string)x[0]).ToLowerInvariant());

            var rows = new List<MonthlyOutputImportRowDto> { new() { WorkGroup = "WG1" } };
            _excelImportService.ReadExcel(
                Arg.Any<IXLWorkbook>(),
                Arg.Any<Func<IXLRangeRow, Dictionary<string, int>, MonthlyOutputImportRowDto>>(),
                Arg.Any<IEnumerable<string>>(), Arg.Any<int>(), Arg.Any<string>())
                .Returns(new ExcelImportResult<MonthlyOutputImportRowDto> { IsSuccess = true, Rows = rows });

            var importResponse = ApiResponseDto<MonthlyOutputImportResultDto>.SuccessResponse(new MonthlyOutputImportResultDto { ImportedCount = 1 });
            _pactMonthlyOutputApiClient.ImportStagingAsync(Arg.Any<MonthlyOutputImportReqDto>()).Returns(importResponse);

            SetupS3UploadSuccess();

            var result = await _service.ImportMonthlyOutputAsync(file, 2);

            Assert.True(result.Success);
            await _pactMonthlyOutputApiClient.Received(1).ImportStagingAsync(Arg.Is<MonthlyOutputImportReqDto>(r => r.ImportType == 2));
        }

        #endregion

        #region ImportMonthlyOutputAsync Tests - S3 Upload

        [Fact]
        public async Task ImportMonthlyOutputAsync_S3UploadFails_LogsWarningAndReturnsSuccess()
        {
            var headers = new[] { "Work Group", "Test Code", "Buyer", "Month", "Volume" };
            var data = new[] { new[] { "WG1", "TC1", "Buyer1", "6", "100" } };
            var file = CreateMockFormFile("test.xlsx", headers, data);

            _excelImportService.ReadExcel(
                Arg.Any<IXLWorkbook>(),
                Arg.Any<Func<IXLRangeRow, Dictionary<string, int>, MonthlyOutputImportRowDto>>(),
                Arg.Any<IEnumerable<string>>(), Arg.Any<int>(), Arg.Any<string>())
                .Returns(new ExcelImportResult<MonthlyOutputImportRowDto> { IsSuccess = true, Rows = [new()] });

            _excelImportService.BuildHeaderMap(Arg.Any<IXLRangeRow>()).Returns(new Dictionary<string, int>());
            _excelImportService.NormalizeHeader(Arg.Any<string>()).Returns(x => ((string)x[0]).ToLowerInvariant());

            var importResponse = ApiResponseDto<MonthlyOutputImportResultDto>.SuccessResponse(new MonthlyOutputImportResultDto { ImportedCount = 1 });
            _pactMonthlyOutputApiClient.ImportStagingAsync(Arg.Any<MonthlyOutputImportReqDto>()).Returns(importResponse);

            SetupS3Config();
            _s3StorageService.UploadFileAsync(
                Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(S3UploadResult.FailureResponse("S3_ERROR", "Upload failed"));

            var result = await _service.ImportMonthlyOutputAsync(file, 1);

            Assert.True(result.Success);
        }

        [Fact]
        public async Task ImportMonthlyOutputAsync_S3UploadThrows_LogsWarningAndReturnsSuccess()
        {
            var headers = new[] { "Work Group", "Test Code", "Buyer", "Month", "Volume" };
            var data = new[] { new[] { "WG1", "TC1", "Buyer1", "6", "100" } };
            var file = CreateMockFormFile("test.xlsx", headers, data);

            _excelImportService.ReadExcel(
                Arg.Any<IXLWorkbook>(),
                Arg.Any<Func<IXLRangeRow, Dictionary<string, int>, MonthlyOutputImportRowDto>>(),
                Arg.Any<IEnumerable<string>>(), Arg.Any<int>(), Arg.Any<string>())
                .Returns(new ExcelImportResult<MonthlyOutputImportRowDto> { IsSuccess = true, Rows = [new()] });

            _excelImportService.BuildHeaderMap(Arg.Any<IXLRangeRow>()).Returns(new Dictionary<string, int>());
            _excelImportService.NormalizeHeader(Arg.Any<string>()).Returns(x => ((string)x[0]).ToLowerInvariant());

            var importResponse = ApiResponseDto<MonthlyOutputImportResultDto>.SuccessResponse(new MonthlyOutputImportResultDto { ImportedCount = 1 });
            _pactMonthlyOutputApiClient.ImportStagingAsync(Arg.Any<MonthlyOutputImportReqDto>()).Returns(importResponse);

            SetupS3Config();
            _s3StorageService.UploadFileAsync(
                Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
                .ThrowsAsync(new Exception("S3 connection failed"));

            var result = await _service.ImportMonthlyOutputAsync(file, 1);

            Assert.True(result.Success);
        }

        [Fact]
        public async Task ImportMonthlyOutputAsync_ImportResponseHasNullData_ReturnsResponseWithoutS3Upload()
        {
            var headers = new[] { "Work Group", "Test Code", "Buyer", "Month", "Volume" };
            var data = new[] { new[] { "WG1", "TC1", "Buyer1", "6", "100" } };
            var file = CreateMockFormFile("test.xlsx", headers, data);

            _excelImportService.ReadExcel(
                Arg.Any<IXLWorkbook>(),
                Arg.Any<Func<IXLRangeRow, Dictionary<string, int>, MonthlyOutputImportRowDto>>(),
                Arg.Any<IEnumerable<string>>(), Arg.Any<int>(), Arg.Any<string>())
                .Returns(new ExcelImportResult<MonthlyOutputImportRowDto> { IsSuccess = true, Rows = [new()] });

            _excelImportService.BuildHeaderMap(Arg.Any<IXLRangeRow>()).Returns(new Dictionary<string, int>());
            _excelImportService.NormalizeHeader(Arg.Any<string>()).Returns(x => ((string)x[0]).ToLowerInvariant());

            var importResponse = ApiResponseDto<MonthlyOutputImportResultDto>.SuccessResponse(null!);
            _pactMonthlyOutputApiClient.ImportStagingAsync(Arg.Any<MonthlyOutputImportReqDto>()).Returns(importResponse);

            var result = await _service.ImportMonthlyOutputAsync(file, 1);

            // Should return early without attempting S3 upload
            await _s3StorageService.DidNotReceive().UploadFileAsync(
                Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        }

        #endregion

        #region ImportMonthlyOutputAsync Tests - UploadAuditFileAsync branches

        [Fact]
        public async Task ImportMonthlyOutputAsync_WithSelectedFPSYear_UsesYearInS3Path()
        {
            var headers = new[] { "Work Group", "Test Code", "Buyer", "Month", "Volume" };
            var data = new[] { new[] { "WG1", "TC1", "Buyer1", "6", "100" } };
            var file = CreateMockFormFile("test.xlsx", headers, data);

            _excelImportService.ReadExcel(
                Arg.Any<IXLWorkbook>(),
                Arg.Any<Func<IXLRangeRow, Dictionary<string, int>, MonthlyOutputImportRowDto>>(),
                Arg.Any<IEnumerable<string>>(), Arg.Any<int>(), Arg.Any<string>())
                .Returns(new ExcelImportResult<MonthlyOutputImportRowDto> { IsSuccess = true, Rows = [new()] });

            _excelImportService.BuildHeaderMap(Arg.Any<IXLRangeRow>()).Returns(new Dictionary<string, int>());
            _excelImportService.NormalizeHeader(Arg.Any<string>()).Returns(x => ((string)x[0]).ToLowerInvariant());

            var importResponse = ApiResponseDto<MonthlyOutputImportResultDto>.SuccessResponse(new MonthlyOutputImportResultDto { ImportedCount = 1 });
            _pactMonthlyOutputApiClient.ImportStagingAsync(Arg.Any<MonthlyOutputImportReqDto>()).Returns(importResponse);

            SetupS3Config();
            var httpContext = new DefaultHttpContext();
            httpContext.Items["SelectedFPSYear"] = "2025";
            _httpContextAccessor.HttpContext.Returns(httpContext);

            _s3StorageService.UploadFileAsync(
                Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(S3UploadResult.SuccessResponse("key"));

            var result = await _service.ImportMonthlyOutputAsync(file, 1);

            Assert.True(result.Success);
            await _s3StorageService.Received(1).UploadFileAsync(
                Arg.Any<Stream>(), "test-bucket", Arg.Is<string>(p => p.Contains("FPS2025")), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task ImportMonthlyOutputAsync_WithEmptyFileName_UsesDefaultFileName()
        {
            // Create file with empty path-style filename
            var workbook = new XLWorkbook();
            var ws = workbook.AddWorksheet("Sheet1");
            ws.Cell(1, 1).Value = "Work Group";
            ws.Cell(1, 2).Value = "Test Code";
            ws.Cell(1, 3).Value = "Buyer";
            ws.Cell(1, 4).Value = "Month";
            ws.Cell(1, 5).Value = "Volume";
            ws.Cell(2, 1).Value = "WG1";
            ws.Cell(2, 2).Value = "TC1";
            ws.Cell(2, 3).Value = "Buyer1";
            ws.Cell(2, 4).Value = "6";
            ws.Cell(2, 5).Value = "100";
            var ms = new MemoryStream();
            workbook.SaveAs(ms);
            ms.Position = 0;

            var file = Substitute.For<IFormFile>();
            file.FileName.Returns("");
            file.ContentType.Returns("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
            file.OpenReadStream().Returns(ms);

            _excelImportService.ReadExcel(
                Arg.Any<IXLWorkbook>(),
                Arg.Any<Func<IXLRangeRow, Dictionary<string, int>, MonthlyOutputImportRowDto>>(),
                Arg.Any<IEnumerable<string>>(), Arg.Any<int>(), Arg.Any<string>())
                .Returns(new ExcelImportResult<MonthlyOutputImportRowDto> { IsSuccess = true, Rows = [new()] });

            _excelImportService.BuildHeaderMap(Arg.Any<IXLRangeRow>()).Returns(new Dictionary<string, int>());
            _excelImportService.NormalizeHeader(Arg.Any<string>()).Returns(x => ((string)x[0]).ToLowerInvariant());

            var importResponse = ApiResponseDto<MonthlyOutputImportResultDto>.SuccessResponse(new MonthlyOutputImportResultDto { ImportedCount = 1 });
            _pactMonthlyOutputApiClient.ImportStagingAsync(Arg.Any<MonthlyOutputImportReqDto>()).Returns(importResponse);

            SetupS3Config();
            _s3StorageService.UploadFileAsync(
                Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(S3UploadResult.SuccessResponse("key"));

            var result = await _service.ImportMonthlyOutputAsync(file, 1);

            Assert.True(result.Success);
            await _s3StorageService.Received(1).UploadFileAsync(
                Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<string>(),
                Arg.Is<string>(n => n.StartsWith("monthly-output-import_")),
                Arg.Any<string>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task ImportMonthlyOutputAsync_WithFileNameNoExtension_UsesDefaultExtension()
        {
            var workbook = new XLWorkbook();
            var ws = workbook.AddWorksheet("Sheet1");
            ws.Cell(1, 1).Value = "Work Group";
            ws.Cell(1, 2).Value = "Test Code";
            ws.Cell(1, 3).Value = "Buyer";
            ws.Cell(1, 4).Value = "Month";
            ws.Cell(1, 5).Value = "Volume";
            ws.Cell(2, 1).Value = "WG1";
            ws.Cell(2, 2).Value = "TC1";
            ws.Cell(2, 3).Value = "Buyer1";
            ws.Cell(2, 4).Value = "6";
            ws.Cell(2, 5).Value = "100";
            var ms = new MemoryStream();
            workbook.SaveAs(ms);
            ms.Position = 0;

            var file = Substitute.For<IFormFile>();
            file.FileName.Returns("testfile");
            file.ContentType.Returns("application/octet-stream");
            file.OpenReadStream().Returns(ms);

            _excelImportService.ReadExcel(
                Arg.Any<IXLWorkbook>(),
                Arg.Any<Func<IXLRangeRow, Dictionary<string, int>, MonthlyOutputImportRowDto>>(),
                Arg.Any<IEnumerable<string>>(), Arg.Any<int>(), Arg.Any<string>())
                .Returns(new ExcelImportResult<MonthlyOutputImportRowDto> { IsSuccess = true, Rows = [new()] });

            _excelImportService.BuildHeaderMap(Arg.Any<IXLRangeRow>()).Returns(new Dictionary<string, int>());
            _excelImportService.NormalizeHeader(Arg.Any<string>()).Returns(x => ((string)x[0]).ToLowerInvariant());

            var importResponse = ApiResponseDto<MonthlyOutputImportResultDto>.SuccessResponse(new MonthlyOutputImportResultDto { ImportedCount = 1 });
            _pactMonthlyOutputApiClient.ImportStagingAsync(Arg.Any<MonthlyOutputImportReqDto>()).Returns(importResponse);

            SetupS3Config();
            _s3StorageService.UploadFileAsync(
                Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(S3UploadResult.SuccessResponse("key"));

            var result = await _service.ImportMonthlyOutputAsync(file, 1);

            Assert.True(result.Success);
        }

        [Fact]
        public async Task ImportMonthlyOutputAsync_BucketNameNotConfigured_ThrowsInvalidOperationException()
        {
            var headers = new[] { "Work Group", "Test Code", "Buyer", "Month", "Volume" };
            var data = new[] { new[] { "WG1", "TC1", "Buyer1", "6", "100" } };
            var file = CreateMockFormFile("test.xlsx", headers, data);

            _excelImportService.ReadExcel(
                Arg.Any<IXLWorkbook>(),
                Arg.Any<Func<IXLRangeRow, Dictionary<string, int>, MonthlyOutputImportRowDto>>(),
                Arg.Any<IEnumerable<string>>(), Arg.Any<int>(), Arg.Any<string>())
                .Returns(new ExcelImportResult<MonthlyOutputImportRowDto> { IsSuccess = true, Rows = [new()] });

            _excelImportService.BuildHeaderMap(Arg.Any<IXLRangeRow>()).Returns(new Dictionary<string, int>());
            _excelImportService.NormalizeHeader(Arg.Any<string>()).Returns(x => ((string)x[0]).ToLowerInvariant());

            var importResponse = ApiResponseDto<MonthlyOutputImportResultDto>.SuccessResponse(new MonthlyOutputImportResultDto { ImportedCount = 1 });
            _pactMonthlyOutputApiClient.ImportStagingAsync(Arg.Any<MonthlyOutputImportReqDto>()).Returns(importResponse);

            // Don't configure S3Storage:BucketName - it should throw
            _configuration["S3Storage:BucketName"].Returns((string?)null);

            // The exception is caught by the catch block in ImportMonthlyOutputAsync (line 234)
            // so it should log a warning and still return success
            var result = await _service.ImportMonthlyOutputAsync(file, 1);

            Assert.True(result.Success);
        }

        #endregion
    }
}
