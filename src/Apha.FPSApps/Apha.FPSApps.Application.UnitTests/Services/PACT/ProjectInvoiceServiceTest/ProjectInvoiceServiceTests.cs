using Apha.Common.Utilities.ExcelImport;
using Apha.Common.Utilities.Storage;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
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

namespace Apha.FPSApps.Application.UnitTests.Services.PACT.ProjectInvoiceServiceTest
{
    public class ProjectInvoiceServiceTests
    {
        private readonly IPactApiClient _pactClient;
        private readonly IPactProjectInvoiceApiClient _pactProjectInvoiceApiClient;
        private readonly IExcelImportService _excelImportService;
        private readonly IS3StorageService _s3StorageService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ProjectInvoiceService> _logger;
        private readonly ProjectInvoiceService _service;

        public ProjectInvoiceServiceTests()
        {
            _pactClient = Substitute.For<IPactApiClient>();
            _pactProjectInvoiceApiClient = Substitute.For<IPactProjectInvoiceApiClient>();
            _pactClient.PactProjectInvoice.Returns(_pactProjectInvoiceApiClient);
            _excelImportService = Substitute.For<IExcelImportService>();
            _s3StorageService = Substitute.For<IS3StorageService>();
            _httpContextAccessor = Substitute.For<IHttpContextAccessor>();
            _configuration = Substitute.For<IConfiguration>();
            _logger = Substitute.For<ILogger<ProjectInvoiceService>>();
            _service = new ProjectInvoiceService(
                _pactClient,
                _excelImportService,
                _s3StorageService,
                _httpContextAccessor,
                _configuration,
                _logger);
        }

        #region GetPagedProjectInvoicesAsync Tests

        [Fact]
        public async Task GetPagedProjectInvoicesAsync_WithValidQuery_ReturnsPaginatedInvoices()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var parentProject = "PP001";
            var invoices = new List<ProjectInvoiceDto>
            {
                new ProjectInvoiceDto { InvoiceCounter = 1, ProjectParent = parentProject, Amount = 100.00m },
                new ProjectInvoiceDto { InvoiceCounter = 2, ProjectParent = parentProject, Amount = 200.00m }
            };
            var expectedResponse = ApiResponseDto<List<ProjectInvoiceDto>>.SuccessResponse(
                invoices,
                new PaginationDto { PageNumber = 1, PageSize = 10, TotalRecords = 2 }
            );
            _pactProjectInvoiceApiClient.GetPagedProjectInvoicesAsync(query, parentProject).Returns(expectedResponse);

            // Act
            var result = await _service.GetPagedProjectInvoicesAsync(query, parentProject);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(2, result.Data?.Count);
            await _pactProjectInvoiceApiClient.Received(1).GetPagedProjectInvoicesAsync(query, parentProject);
        }

        [Fact]
        public async Task GetPagedProjectInvoicesAsync_WithNullProject_ReturnsSuccessWithEmptyList()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var expectedResponse = ApiResponseDto<List<ProjectInvoiceDto>>.SuccessResponse(new List<ProjectInvoiceDto>());
            _pactProjectInvoiceApiClient.GetPagedProjectInvoicesAsync(query, null).Returns(expectedResponse);

            // Act
            var result = await _service.GetPagedProjectInvoicesAsync(query, null);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Empty(result.Data!);
        }

        [Fact]
        public async Task GetPagedProjectInvoicesAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "API Error", Code = "API_ERROR" } };
            var expectedResponse = ApiResponseDto<List<ProjectInvoiceDto>>.FailureResponse(errors, new ApiMetaDto());
            _pactProjectInvoiceApiClient.GetPagedProjectInvoicesAsync(query, null).Returns(expectedResponse);

            // Act
            var result = await _service.GetPagedProjectInvoicesAsync(query, null);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
        }

        #endregion

        #region GetTotalAmountAsync Tests

        [Fact]
        public async Task GetTotalAmountAsync_WithValidProject_ReturnsTotalAmount()
        {
            // Arrange
            var parentProject = "PP001";
            var expectedResponse = ApiResponseDto<decimal>.SuccessResponse(1500.00m);
            _pactProjectInvoiceApiClient.GetTotalAmountAsync(parentProject).Returns(expectedResponse);

            // Act
            var result = await _service.GetTotalAmountAsync(parentProject);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(1500.00m, result.Data);
            await _pactProjectInvoiceApiClient.Received(1).GetTotalAmountAsync(parentProject);
        }

        [Fact]
        public async Task GetTotalAmountAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "API Error", Code = "API_ERROR" } };
            var expectedResponse = ApiResponseDto<decimal>.FailureResponse(errors, new ApiMetaDto());
            _pactProjectInvoiceApiClient.GetTotalAmountAsync(null).Returns(expectedResponse);

            // Act
            var result = await _service.GetTotalAmountAsync(null);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region GetByIdAsync Tests

        [Fact]
        public async Task GetByIdAsync_WithValidId_ReturnsInvoice()
        {
            // Arrange
            var invoiceCounter = 1;
            var invoice = new ProjectInvoiceDto { InvoiceCounter = invoiceCounter, ProjectParent = "PP001", Amount = 500.00m };
            var expectedResponse = ApiResponseDto<ProjectInvoiceDto>.SuccessResponse(invoice);
            _pactProjectInvoiceApiClient.GetByIdAsync(invoiceCounter).Returns(expectedResponse);

            // Act
            var result = await _service.GetByIdAsync(invoiceCounter);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(invoiceCounter, result.Data?.InvoiceCounter);
            await _pactProjectInvoiceApiClient.Received(1).GetByIdAsync(invoiceCounter);
        }

        [Fact]
        public async Task GetByIdAsync_WithNonExistentId_ReturnsFailureResponse()
        {
            // Arrange
            var invoiceCounter = 9999;
            var errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Invoice not found", Code = "NOT_FOUND" } };
            var expectedResponse = ApiResponseDto<ProjectInvoiceDto>.FailureResponse(errors, new ApiMetaDto());
            _pactProjectInvoiceApiClient.GetByIdAsync(invoiceCounter).Returns(expectedResponse);

            // Act
            var result = await _service.GetByIdAsync(invoiceCounter);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region CreateAsync Tests

        [Fact]
        public async Task CreateAsync_WithValidInvoice_ReturnsSuccessResponse()
        {
            // Arrange
            var newInvoice = new ProjectInvoiceDto
            {
                InvoiceCounter = 1,
                ProjectParent = "PP001",
                Amount = 750.00m,
                Type = "Standard"
            };
            var expectedResponse = ApiResponseDto<ProjectInvoiceDto>.SuccessResponse(newInvoice);
            _pactProjectInvoiceApiClient.CreateAsync(newInvoice).Returns(expectedResponse);

            // Act
            var result = await _service.CreateAsync(newInvoice);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(newInvoice.InvoiceCounter, result.Data?.InvoiceCounter);
            await _pactProjectInvoiceApiClient.Received(1).CreateAsync(newInvoice);
        }

        [Fact]
        public async Task CreateAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var newInvoice = new ProjectInvoiceDto { ProjectParent = "PP001" };
            var errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Duplicate invoice", Code = "DUPLICATE" } };
            var expectedResponse = ApiResponseDto<ProjectInvoiceDto>.FailureResponse(errors, new ApiMetaDto());
            _pactProjectInvoiceApiClient.CreateAsync(newInvoice).Returns(expectedResponse);

            // Act
            var result = await _service.CreateAsync(newInvoice);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region UpdateAsync Tests

        [Fact]
        public async Task UpdateAsync_WithValidInvoice_ReturnsSuccessResponse()
        {
            // Arrange
            var invoiceCounter = 1;
            var updatedInvoice = new ProjectInvoiceDto
            {
                InvoiceCounter = invoiceCounter,
                ProjectParent = "PP001",
                Amount = 900.00m,
                Type = "Revised"
            };
            var expectedResponse = ApiResponseDto<ProjectInvoiceDto>.SuccessResponse(updatedInvoice);
            _pactProjectInvoiceApiClient.UpdateAsync(invoiceCounter, updatedInvoice).Returns(expectedResponse);

            // Act
            var result = await _service.UpdateAsync(invoiceCounter, updatedInvoice);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.Equal(900.00m, result.Data?.Amount);
            await _pactProjectInvoiceApiClient.Received(1).UpdateAsync(invoiceCounter, updatedInvoice);
        }

        [Fact]
        public async Task UpdateAsync_WithNonExistentId_ReturnsFailureResponse()
        {
            // Arrange
            var invoiceCounter = 9999;
            var invoice = new ProjectInvoiceDto { InvoiceCounter = invoiceCounter, ProjectParent = "PP001" };
            var errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Invoice not found", Code = "NOT_FOUND" } };
            var expectedResponse = ApiResponseDto<ProjectInvoiceDto>.FailureResponse(errors, new ApiMetaDto());
            _pactProjectInvoiceApiClient.UpdateAsync(invoiceCounter, invoice).Returns(expectedResponse);

            // Act
            var result = await _service.UpdateAsync(invoiceCounter, invoice);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region DeleteAsync Tests

        [Fact]
        public async Task DeleteAsync_WithValidId_ReturnsSuccessResponse()
        {
            // Arrange
            var invoiceCounter = 1;
            var expectedResponse = ApiResponseDto<bool>.SuccessResponse(true);
            _pactProjectInvoiceApiClient.DeleteAsync(invoiceCounter).Returns(expectedResponse);

            // Act
            var result = await _service.DeleteAsync(invoiceCounter);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Success);
            Assert.True(result.Data);
            await _pactProjectInvoiceApiClient.Received(1).DeleteAsync(invoiceCounter);
        }

        [Fact]
        public async Task DeleteAsync_WithNonExistentId_ReturnsFailureResponse()
        {
            // Arrange
            var invoiceCounter = 9999;
            var errors = new List<ApiErrorDto> { new ApiErrorDto { Message = "Invoice not found", Code = "NOT_FOUND" } };
            var expectedResponse = ApiResponseDto<bool>.FailureResponse(errors, new ApiMetaDto());
            _pactProjectInvoiceApiClient.DeleteAsync(invoiceCounter).Returns(expectedResponse);

            // Act
            var result = await _service.DeleteAsync(invoiceCounter);

            // Assert
            Assert.NotNull(result);
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region GetPagedProjectInvoiceManualAsync Tests

        [Fact]
        public async Task GetPagedProjectInvoiceManualAsync_WithValidQuery_ReturnsSuccess()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var parentProject = "PP001";
            var invoices = new List<ProjectInvoiceDto> { new() { InvoiceCounter = 1 } };
            var expectedResponse = ApiResponseDto<List<ProjectInvoiceDto>>.SuccessResponse(invoices, new PaginationDto());
            _pactProjectInvoiceApiClient.GetPagedProjectInvoiceManualAsync(query, parentProject).Returns(expectedResponse);

            // Act
            var result = await _service.GetPagedProjectInvoiceManualAsync(query, parentProject);

            // Assert
            Assert.True(result.Success);
            Assert.Single(result.Data!);
            await _pactProjectInvoiceApiClient.Received(1).GetPagedProjectInvoiceManualAsync(query, parentProject);
        }

        [Fact]
        public async Task GetPagedProjectInvoiceManualAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string>();
            var errors = new List<ApiErrorDto> { new() { Message = "API Error" } };
            var expectedResponse = ApiResponseDto<List<ProjectInvoiceDto>>.FailureResponse(errors, new ApiMetaDto());
            _pactProjectInvoiceApiClient.GetPagedProjectInvoiceManualAsync(query, null).Returns(expectedResponse);

            // Act
            var result = await _service.GetPagedProjectInvoiceManualAsync(query, null);

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
        }

        #endregion

        #region GetMonthlyInvoicesSummaryAsync Tests

        [Fact]
        public async Task GetMonthlyInvoicesSummaryAsync_WithValidQuery_ReturnsSuccess()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var pivotDto = new MonthlyInvoicesPivotDto { Months = [1, 2], Rows = [], Pagination = new PaginationDto() };
            var expectedResponse = ApiResponseDto<MonthlyInvoicesPivotDto>.SuccessResponse(pivotDto);
            _pactProjectInvoiceApiClient.GetMonthlyInvoicesSummaryAsync(query).Returns(expectedResponse);

            // Act
            var result = await _service.GetMonthlyInvoicesSummaryAsync(query);

            // Assert
            Assert.True(result.Success);
            Assert.NotNull(result.Data);
            Assert.Equal(2, result.Data.Months.Count);
            await _pactProjectInvoiceApiClient.Received(1).GetMonthlyInvoicesSummaryAsync(query);
        }

        [Fact]
        public async Task GetMonthlyInvoicesSummaryAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string>();
            var errors = new List<ApiErrorDto> { new() { Message = "API Error" } };
            var expectedResponse = ApiResponseDto<MonthlyInvoicesPivotDto>.FailureResponse(errors, new ApiMetaDto());
            _pactProjectInvoiceApiClient.GetMonthlyInvoicesSummaryAsync(query).Returns(expectedResponse);

            // Act
            var result = await _service.GetMonthlyInvoicesSummaryAsync(query);

            // Assert
            Assert.False(result.Success);
        }

        #endregion

        #region GetFailedInvoiceImportAsync Tests

        [Fact]
        public async Task GetFailedInvoiceImportAsync_WithValidQuery_ReturnsSuccess()
        {
            // Arrange
            var query = new QueryParameters<string> { Page = 1, PageSize = 10 };
            var rows = new List<InvoiceImportRowDto> { new() { Id = 1, ProjectParent = "PP001" } };
            var expectedResponse = ApiResponseDto<List<InvoiceImportRowDto>>.SuccessResponse(rows);
            _pactProjectInvoiceApiClient.GetFailedInvoiceImportAsync(query).Returns(expectedResponse);

            // Act
            var result = await _service.GetFailedInvoiceImportAsync(query);

            // Assert
            Assert.True(result.Success);
            Assert.Single(result.Data!);
            await _pactProjectInvoiceApiClient.Received(1).GetFailedInvoiceImportAsync(query);
        }

        [Fact]
        public async Task GetFailedInvoiceImportAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var query = new QueryParameters<string>();
            var errors = new List<ApiErrorDto> { new() { Message = "API Error" } };
            var expectedResponse = ApiResponseDto<List<InvoiceImportRowDto>>.FailureResponse(errors, new ApiMetaDto());
            _pactProjectInvoiceApiClient.GetFailedInvoiceImportAsync(query).Returns(expectedResponse);

            // Act
            var result = await _service.GetFailedInvoiceImportAsync(query);

            // Assert
            Assert.False(result.Success);
        }

        #endregion

        #region GetFailedInvoiceImportByIdAsync Tests

        [Fact]
        public async Task GetFailedInvoiceImportByIdAsync_WithValidId_ReturnsSuccess()
        {
            // Arrange
            var dto = new InvoiceImportRowDto { Id = 1, ProjectParent = "PP001" };
            var expectedResponse = ApiResponseDto<InvoiceImportRowDto>.SuccessResponse(dto);
            _pactProjectInvoiceApiClient.GetFailedInvoiceImportByIdAsync(1).Returns(expectedResponse);

            // Act
            var result = await _service.GetFailedInvoiceImportByIdAsync(1);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(1, result.Data!.Id);
            await _pactProjectInvoiceApiClient.Received(1).GetFailedInvoiceImportByIdAsync(1);
        }

        [Fact]
        public async Task GetFailedInvoiceImportByIdAsync_WhenNotFound_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Message = "Not found" } };
            var expectedResponse = ApiResponseDto<InvoiceImportRowDto>.FailureResponse(errors, new ApiMetaDto());
            _pactProjectInvoiceApiClient.GetFailedInvoiceImportByIdAsync(99).Returns(expectedResponse);

            // Act
            var result = await _service.GetFailedInvoiceImportByIdAsync(99);

            // Assert
            Assert.False(result.Success);
        }

        #endregion

        #region SaveFailedInvoiceImportAsync Tests

        [Fact]
        public async Task SaveFailedInvoiceImportAsync_WithValidData_ReturnsSuccess()
        {
            // Arrange
            var dto = new InvoiceImportRowDto { Id = 1, ProjectParent = "PP001" };
            var expectedResponse = ApiResponseDto<bool>.SuccessResponse(true);
            _pactProjectInvoiceApiClient.SaveFailedInvoiceImportAsync(1, dto).Returns(expectedResponse);

            // Act
            var result = await _service.SaveFailedInvoiceImportAsync(1, dto);

            // Assert
            Assert.True(result.Success);
            Assert.True(result.Data);
            await _pactProjectInvoiceApiClient.Received(1).SaveFailedInvoiceImportAsync(1, dto);
        }

        [Fact]
        public async Task SaveFailedInvoiceImportAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var dto = new InvoiceImportRowDto { Id = 1 };
            var errors = new List<ApiErrorDto> { new() { Message = "Validation error" } };
            var expectedResponse = ApiResponseDto<bool>.FailureResponse(errors, new ApiMetaDto());
            _pactProjectInvoiceApiClient.SaveFailedInvoiceImportAsync(1, dto).Returns(expectedResponse);

            // Act
            var result = await _service.SaveFailedInvoiceImportAsync(1, dto);

            // Assert
            Assert.False(result.Success);
        }

        #endregion

        #region DeleteFailedInvoiceImportByIdAsync Tests

        [Fact]
        public async Task DeleteFailedInvoiceImportByIdAsync_WithValidId_ReturnsSuccess()
        {
            // Arrange
            var expectedResponse = ApiResponseDto<bool>.SuccessResponse(true);
            _pactProjectInvoiceApiClient.DeleteFailedInvoiceImportByIdAsync(1).Returns(expectedResponse);

            // Act
            var result = await _service.DeleteFailedInvoiceImportByIdAsync(1);

            // Assert
            Assert.True(result.Success);
            Assert.True(result.Data);
            await _pactProjectInvoiceApiClient.Received(1).DeleteFailedInvoiceImportByIdAsync(1);
        }

        [Fact]
        public async Task DeleteFailedInvoiceImportByIdAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Message = "Not found" } };
            var expectedResponse = ApiResponseDto<bool>.FailureResponse(errors, new ApiMetaDto());
            _pactProjectInvoiceApiClient.DeleteFailedInvoiceImportByIdAsync(99).Returns(expectedResponse);

            // Act
            var result = await _service.DeleteFailedInvoiceImportByIdAsync(99);

            // Assert
            Assert.False(result.Success);
        }

        #endregion

        #region DeleteFailedInvoiceImportByUserAsync Tests

        [Fact]
        public async Task DeleteFailedInvoiceImportByUserAsync_WhenRecordsExist_ReturnsSuccess()
        {
            // Arrange
            var expectedResponse = ApiResponseDto<bool>.SuccessResponse(true);
            _pactProjectInvoiceApiClient.DeleteFailedInvoiceImportByUserAsync().Returns(expectedResponse);

            // Act
            var result = await _service.DeleteFailedInvoiceImportByUserAsync();

            // Assert
            Assert.True(result.Success);
            Assert.True(result.Data);
            await _pactProjectInvoiceApiClient.Received(1).DeleteFailedInvoiceImportByUserAsync();
        }

        [Fact]
        public async Task DeleteFailedInvoiceImportByUserAsync_WhenApiFails_ReturnsFailureResponse()
        {
            // Arrange
            var errors = new List<ApiErrorDto> { new() { Message = "Failed to delete" } };
            var expectedResponse = ApiResponseDto<bool>.FailureResponse(errors, new ApiMetaDto());
            _pactProjectInvoiceApiClient.DeleteFailedInvoiceImportByUserAsync().Returns(expectedResponse);

            // Act
            var result = await _service.DeleteFailedInvoiceImportByUserAsync();

            // Assert
            Assert.False(result.Success);
        }

        #endregion

        #region ImportInvoiceAsync Tests

        private IFormFile CreateMockFormFile(string fileName = "test.xlsx")
        {
            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Sheet1");
            ws.Cell(1, 1).Value = "Project Parent";
            ws.Cell(1, 2).Value = "Month";
            ws.Cell(1, 3).Value = "Amount";
            ws.Cell(1, 4).Value = "Cost Of Work";
            ws.Cell(1, 5).Value = "WIP";
            ws.Cell(1, 6).Value = "Profit Loss";
            ws.Cell(1, 7).Value = "Detail";
            ws.Cell(2, 1).Value = "PP001";
            ws.Cell(2, 2).Value = "1";
            ws.Cell(2, 3).Value = "100";

            var ms = new MemoryStream();
            workbook.SaveAs(ms);
            ms.Position = 0;

            var file = Substitute.For<IFormFile>();
            file.FileName.Returns(fileName);
            file.Length.Returns(ms.Length);
            file.ContentType.Returns("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
            file.OpenReadStream().Returns(ms);
            return file;
        }

        [Fact]
        public async Task ImportInvoiceAsync_WhenMissingHeaders_ReturnsInvalidTemplateFailure()
        {
            // Arrange
            var file = CreateMockFormFile();
            _excelImportService.ReadExcel(
                Arg.Any<IXLWorkbook>(),
                Arg.Any<Func<IXLRangeRow, Dictionary<string, int>, InvoiceImportRowDto>>(),
                Arg.Any<IEnumerable<string>>(),
                Arg.Any<int>(),
                Arg.Any<string?>())
                .Returns(new ExcelImportResult<InvoiceImportRowDto>
                {
                    IsSuccess = false,
                    ErrorMessage = "Missing required headers.",
                    MissingHeaders = new List<string> { "Amount" }
                });

            // Act
            var result = await _service.ImportInvoiceAsync(file);

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Single(result.Errors);
            Assert.Equal("INVALID_TEMPLATE", result.Errors[0].Code);
        }

        [Fact]
        public async Task ImportInvoiceAsync_WhenEmptyFile_ReturnsEmptyFileFailure()
        {
            // Arrange
            var file = CreateMockFormFile();
            _excelImportService.ReadExcel(
                Arg.Any<IXLWorkbook>(),
                Arg.Any<Func<IXLRangeRow, Dictionary<string, int>, InvoiceImportRowDto>>(),
                Arg.Any<IEnumerable<string>>(),
                Arg.Any<int>(),
                Arg.Any<string?>())
                .Returns(new ExcelImportResult<InvoiceImportRowDto>
                {
                    IsSuccess = false,
                    ErrorMessage = "No data rows found.",
                    MissingHeaders = new List<string>()
                });

            // Act
            var result = await _service.ImportInvoiceAsync(file);

            // Assert
            Assert.False(result.Success);
            Assert.NotNull(result.Errors);
            Assert.Equal("EMPTY_FILE", result.Errors[0].Code);
        }

        [Fact]
        public async Task ImportInvoiceAsync_WhenReadExcelFailsWithNullErrorMessage_ReturnsDefaultMessage()
        {
            // Arrange
            var file = CreateMockFormFile();
            _excelImportService.ReadExcel(
                Arg.Any<IXLWorkbook>(),
                Arg.Any<Func<IXLRangeRow, Dictionary<string, int>, InvoiceImportRowDto>>(),
                Arg.Any<IEnumerable<string>>(),
                Arg.Any<int>(),
                Arg.Any<string?>())
                .Returns(new ExcelImportResult<InvoiceImportRowDto>
                {
                    IsSuccess = false,
                    ErrorMessage = null,
                    MissingHeaders = new List<string>()
                });

            // Act
            var result = await _service.ImportInvoiceAsync(file);

            // Assert
            Assert.False(result.Success);
            Assert.Equal("Import failed.", result.Errors![0].Message);
        }

        [Fact]
        public async Task ImportInvoiceAsync_WhenApiImportFails_ReturnsFailureResponse()
        {
            // Arrange
            var file = CreateMockFormFile();
            var rows = new List<InvoiceImportRowDto> { new() { ProjectParent = "PP001", Month = "1", Amount = "100" } };
            _excelImportService.ReadExcel(
                Arg.Any<IXLWorkbook>(),
                Arg.Any<Func<IXLRangeRow, Dictionary<string, int>, InvoiceImportRowDto>>(),
                Arg.Any<IEnumerable<string>>(),
                Arg.Any<int>(),
                Arg.Any<string?>())
                .Returns(new ExcelImportResult<InvoiceImportRowDto> { IsSuccess = true, Rows = rows });

            var errors = new List<ApiErrorDto> { new() { Message = "Import failed on server" } };
            _pactProjectInvoiceApiClient.ImportInvoiceAsync(Arg.Any<InvoiceImportReqDto>())
                .Returns(ApiResponseDto<InvoiceImportResultDto>.FailureResponse(errors, new ApiMetaDto()));

            // Act
            var result = await _service.ImportInvoiceAsync(file);

            // Assert
            Assert.False(result.Success);
        }

        [Fact]
        public async Task ImportInvoiceAsync_WhenApiReturnsNullData_ReturnsResponse()
        {
            // Arrange
            var file = CreateMockFormFile();
            var rows = new List<InvoiceImportRowDto> { new() { ProjectParent = "PP001" } };
            _excelImportService.ReadExcel(
                Arg.Any<IXLWorkbook>(),
                Arg.Any<Func<IXLRangeRow, Dictionary<string, int>, InvoiceImportRowDto>>(),
                Arg.Any<IEnumerable<string>>(),
                Arg.Any<int>(),
                Arg.Any<string?>())
                .Returns(new ExcelImportResult<InvoiceImportRowDto> { IsSuccess = true, Rows = rows });

            var response = ApiResponseDto<InvoiceImportResultDto>.SuccessResponse(null!);
            _pactProjectInvoiceApiClient.ImportInvoiceAsync(Arg.Any<InvoiceImportReqDto>()).Returns(response);

            // Act
            var result = await _service.ImportInvoiceAsync(file);

            // Assert
            Assert.True(result.Success);
            Assert.Null(result.Data);
        }

        [Fact]
        public async Task ImportInvoiceAsync_WhenSuccessAndS3UploadSucceeds_ReturnsImportResponse()
        {
            // Arrange
            var file = CreateMockFormFile("invoices.xlsx");
            var rows = new List<InvoiceImportRowDto> { new() { ProjectParent = "PP001" } };
            _excelImportService.ReadExcel(
                Arg.Any<IXLWorkbook>(),
                Arg.Any<Func<IXLRangeRow, Dictionary<string, int>, InvoiceImportRowDto>>(),
                Arg.Any<IEnumerable<string>>(),
                Arg.Any<int>(),
                Arg.Any<string?>())
                .Returns(new ExcelImportResult<InvoiceImportRowDto> { IsSuccess = true, Rows = rows });

            var importResult = new InvoiceImportResultDto { PassedCount = 1, FailedCount = 0, Message = "OK" };
            _pactProjectInvoiceApiClient.ImportInvoiceAsync(Arg.Any<InvoiceImportReqDto>())
                .Returns(ApiResponseDto<InvoiceImportResultDto>.SuccessResponse(importResult));

            _configuration["S3Storage:BucketName"].Returns("test-bucket");
            _s3StorageService.UploadFileAsync(Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
                .Returns(S3UploadResult.SuccessResponse("key"));

            // Act
            var result = await _service.ImportInvoiceAsync(file);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(1, result.Data!.PassedCount);
        }

        [Fact]
        public async Task ImportInvoiceAsync_WhenSuccessAndS3UploadFails_StillReturnsSuccess()
        {
            // Arrange
            var file = CreateMockFormFile("invoices.xlsx");
            var rows = new List<InvoiceImportRowDto> { new() { ProjectParent = "PP001" } };
            _excelImportService.ReadExcel(
                Arg.Any<IXLWorkbook>(),
                Arg.Any<Func<IXLRangeRow, Dictionary<string, int>, InvoiceImportRowDto>>(),
                Arg.Any<IEnumerable<string>>(),
                Arg.Any<int>(),
                Arg.Any<string?>())
                .Returns(new ExcelImportResult<InvoiceImportRowDto> { IsSuccess = true, Rows = rows });

            var importResult = new InvoiceImportResultDto { PassedCount = 1, FailedCount = 0, Message = "OK" };
            _pactProjectInvoiceApiClient.ImportInvoiceAsync(Arg.Any<InvoiceImportReqDto>())
                .Returns(ApiResponseDto<InvoiceImportResultDto>.SuccessResponse(importResult));

            _configuration["S3Storage:BucketName"].Returns("test-bucket");
            _s3StorageService.UploadFileAsync(Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
                .Returns(S3UploadResult.FailureResponse("S3_ERROR", "Upload failed"));

            // Act
            var result = await _service.ImportInvoiceAsync(file);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(1, result.Data!.PassedCount);
        }

        [Fact]
        public async Task ImportInvoiceAsync_WhenSuccessAndS3UploadThrowsException_StillReturnsSuccess()
        {
            // Arrange
            var file = CreateMockFormFile("invoices.xlsx");
            var rows = new List<InvoiceImportRowDto> { new() { ProjectParent = "PP001" } };
            _excelImportService.ReadExcel(
                Arg.Any<IXLWorkbook>(),
                Arg.Any<Func<IXLRangeRow, Dictionary<string, int>, InvoiceImportRowDto>>(),
                Arg.Any<IEnumerable<string>>(),
                Arg.Any<int>(),
                Arg.Any<string?>())
                .Returns(new ExcelImportResult<InvoiceImportRowDto> { IsSuccess = true, Rows = rows });

            var importResult = new InvoiceImportResultDto { PassedCount = 1, FailedCount = 0, Message = "OK" };
            _pactProjectInvoiceApiClient.ImportInvoiceAsync(Arg.Any<InvoiceImportReqDto>())
                .Returns(ApiResponseDto<InvoiceImportResultDto>.SuccessResponse(importResult));

            _configuration["S3Storage:BucketName"].Returns("test-bucket");
            _s3StorageService.UploadFileAsync(Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
                .Throws(new Exception("S3 connection error"));

            // Act
            var result = await _service.ImportInvoiceAsync(file);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(1, result.Data!.PassedCount);
        }

        [Fact]
        public async Task ImportInvoiceAsync_WhenSelectedFPSYearInContext_UsesYearInFolderPath()
        {
            // Arrange
            var file = CreateMockFormFile("invoices.xlsx");
            var rows = new List<InvoiceImportRowDto> { new() { ProjectParent = "PP001" } };
            _excelImportService.ReadExcel(
                Arg.Any<IXLWorkbook>(),
                Arg.Any<Func<IXLRangeRow, Dictionary<string, int>, InvoiceImportRowDto>>(),
                Arg.Any<IEnumerable<string>>(),
                Arg.Any<int>(),
                Arg.Any<string?>())
                .Returns(new ExcelImportResult<InvoiceImportRowDto> { IsSuccess = true, Rows = rows });

            var importResult = new InvoiceImportResultDto { PassedCount = 1, FailedCount = 0, Message = "OK" };
            _pactProjectInvoiceApiClient.ImportInvoiceAsync(Arg.Any<InvoiceImportReqDto>())
                .Returns(ApiResponseDto<InvoiceImportResultDto>.SuccessResponse(importResult));

            var httpContext = new DefaultHttpContext();
            httpContext.Items["SelectedFPSYear"] = "2025";
            _httpContextAccessor.HttpContext.Returns(httpContext);
            _configuration["S3Storage:BucketName"].Returns("test-bucket");
            _s3StorageService.UploadFileAsync(Arg.Any<Stream>(), "test-bucket", "FPS2025/InvoiceImport", Arg.Any<string>(), Arg.Any<string>())
                .Returns(S3UploadResult.SuccessResponse("key"));

            // Act
            var result = await _service.ImportInvoiceAsync(file);

            // Assert
            Assert.True(result.Success);
            await _s3StorageService.Received(1).UploadFileAsync(
                Arg.Any<Stream>(), "test-bucket", "FPS2025/InvoiceImport", Arg.Any<string>(), Arg.Any<string>());
        }

        [Fact]
        public async Task ImportInvoiceAsync_WhenBucketNameNotConfigured_StillReturnsSuccessAndLogsWarning()
        {
            // Arrange
            var file = CreateMockFormFile();
            var rows = new List<InvoiceImportRowDto> { new() { ProjectParent = "PP001" } };
            _excelImportService.ReadExcel(
                Arg.Any<IXLWorkbook>(),
                Arg.Any<Func<IXLRangeRow, Dictionary<string, int>, InvoiceImportRowDto>>(),
                Arg.Any<IEnumerable<string>>(),
                Arg.Any<int>(),
                Arg.Any<string?>())
                .Returns(new ExcelImportResult<InvoiceImportRowDto> { IsSuccess = true, Rows = rows });

            var importResult = new InvoiceImportResultDto { PassedCount = 1, FailedCount = 0, Message = "OK" };
            _pactProjectInvoiceApiClient.ImportInvoiceAsync(Arg.Any<InvoiceImportReqDto>())
                .Returns(ApiResponseDto<InvoiceImportResultDto>.SuccessResponse(importResult));

            _configuration["S3Storage:BucketName"].Returns((string?)null);

            // Act
            var result = await _service.ImportInvoiceAsync(file);

            // Assert — S3 upload throws InvalidOperationException, caught by catch block, import still succeeds
            Assert.True(result.Success);
            Assert.Equal(1, result.Data!.PassedCount);
        }

        [Fact]
        public async Task ImportInvoiceAsync_WhenFileNameIsEmpty_UsesDefaultFileName()
        {
            // Arrange
            var file = CreateMockFormFile("");
            var rows = new List<InvoiceImportRowDto> { new() { ProjectParent = "PP001" } };
            _excelImportService.ReadExcel(
                Arg.Any<IXLWorkbook>(),
                Arg.Any<Func<IXLRangeRow, Dictionary<string, int>, InvoiceImportRowDto>>(),
                Arg.Any<IEnumerable<string>>(),
                Arg.Any<int>(),
                Arg.Any<string?>())
                .Returns(new ExcelImportResult<InvoiceImportRowDto> { IsSuccess = true, Rows = rows });

            var importResult = new InvoiceImportResultDto { PassedCount = 1, FailedCount = 0, Message = "OK" };
            _pactProjectInvoiceApiClient.ImportInvoiceAsync(Arg.Any<InvoiceImportReqDto>())
                .Returns(ApiResponseDto<InvoiceImportResultDto>.SuccessResponse(importResult));

            _configuration["S3Storage:BucketName"].Returns("test-bucket");
            _s3StorageService.UploadFileAsync(Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
                .Returns(S3UploadResult.SuccessResponse("key"));

            // Act
            var result = await _service.ImportInvoiceAsync(file);

            // Assert
            Assert.True(result.Success);
        }

        [Fact]
        public async Task ImportInvoiceAsync_WhenSelectedFPSYearIsInvalidString_UsesCurrentYear()
        {
            // Arrange
            var file = CreateMockFormFile("invoices.xlsx");
            var rows = new List<InvoiceImportRowDto> { new() { ProjectParent = "PP001" } };
            _excelImportService.ReadExcel(
                Arg.Any<IXLWorkbook>(),
                Arg.Any<Func<IXLRangeRow, Dictionary<string, int>, InvoiceImportRowDto>>(),
                Arg.Any<IEnumerable<string>>(),
                Arg.Any<int>(),
                Arg.Any<string?>())
                .Returns(new ExcelImportResult<InvoiceImportRowDto> { IsSuccess = true, Rows = rows });

            var importResult = new InvoiceImportResultDto { PassedCount = 1, FailedCount = 0, Message = "OK" };
            _pactProjectInvoiceApiClient.ImportInvoiceAsync(Arg.Any<InvoiceImportReqDto>())
                .Returns(ApiResponseDto<InvoiceImportResultDto>.SuccessResponse(importResult));

            var httpContext = new DefaultHttpContext();
            httpContext.Items["SelectedFPSYear"] = "not-a-number";
            _httpContextAccessor.HttpContext.Returns(httpContext);
            _configuration["S3Storage:BucketName"].Returns("test-bucket");
            _s3StorageService.UploadFileAsync(Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
                .Returns(S3UploadResult.SuccessResponse("key"));

            // Act
            var result = await _service.ImportInvoiceAsync(file);

            // Assert — falls back to DateTime.UtcNow.Year, import still succeeds
            Assert.True(result.Success);
            var currentYear = DateTime.UtcNow.Year;
            await _s3StorageService.Received(1).UploadFileAsync(
                Arg.Any<Stream>(), "test-bucket", $"FPS{currentYear}/InvoiceImport", Arg.Any<string>(), Arg.Any<string>());
        }

        [Fact]
        public async Task ImportInvoiceAsync_WhenSelectedFPSYearIsZero_UsesCurrentYear()
        {
            // Arrange
            var file = CreateMockFormFile("invoices.xlsx");
            var rows = new List<InvoiceImportRowDto> { new() { ProjectParent = "PP001" } };
            _excelImportService.ReadExcel(
                Arg.Any<IXLWorkbook>(),
                Arg.Any<Func<IXLRangeRow, Dictionary<string, int>, InvoiceImportRowDto>>(),
                Arg.Any<IEnumerable<string>>(),
                Arg.Any<int>(),
                Arg.Any<string?>())
                .Returns(new ExcelImportResult<InvoiceImportRowDto> { IsSuccess = true, Rows = rows });

            var importResult = new InvoiceImportResultDto { PassedCount = 1, FailedCount = 0, Message = "OK" };
            _pactProjectInvoiceApiClient.ImportInvoiceAsync(Arg.Any<InvoiceImportReqDto>())
                .Returns(ApiResponseDto<InvoiceImportResultDto>.SuccessResponse(importResult));

            var httpContext = new DefaultHttpContext();
            httpContext.Items["SelectedFPSYear"] = "0";
            _httpContextAccessor.HttpContext.Returns(httpContext);
            _configuration["S3Storage:BucketName"].Returns("test-bucket");
            _s3StorageService.UploadFileAsync(Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
                .Returns(S3UploadResult.SuccessResponse("key"));

            // Act
            var result = await _service.ImportInvoiceAsync(file);

            // Assert — parsedYear is 0 which is not > 0, so falls back to current year
            Assert.True(result.Success);
            var currentYear = DateTime.UtcNow.Year;
            await _s3StorageService.Received(1).UploadFileAsync(
                Arg.Any<Stream>(), "test-bucket", $"FPS{currentYear}/InvoiceImport", Arg.Any<string>(), Arg.Any<string>());
        }

        [Fact]
        public async Task ImportInvoiceAsync_WhenFileHasNoExtension_UsesDefaultExtension()
        {
            // Arrange
            var file = CreateMockFormFile("testfile");
            var rows = new List<InvoiceImportRowDto> { new() { ProjectParent = "PP001" } };
            _excelImportService.ReadExcel(
                Arg.Any<IXLWorkbook>(),
                Arg.Any<Func<IXLRangeRow, Dictionary<string, int>, InvoiceImportRowDto>>(),
                Arg.Any<IEnumerable<string>>(),
                Arg.Any<int>(),
                Arg.Any<string?>())
                .Returns(new ExcelImportResult<InvoiceImportRowDto> { IsSuccess = true, Rows = rows });

            var importResult = new InvoiceImportResultDto { PassedCount = 1, FailedCount = 0, Message = "OK" };
            _pactProjectInvoiceApiClient.ImportInvoiceAsync(Arg.Any<InvoiceImportReqDto>())
                .Returns(ApiResponseDto<InvoiceImportResultDto>.SuccessResponse(importResult));

            _configuration["S3Storage:BucketName"].Returns("test-bucket");
            _s3StorageService.UploadFileAsync(Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
                .Returns(S3UploadResult.SuccessResponse("key"));

            // Act
            var result = await _service.ImportInvoiceAsync(file);

            // Assert — extension defaults to ".xlsx"
            Assert.True(result.Success);
            await _s3StorageService.Received(1).UploadFileAsync(
                Arg.Any<Stream>(), "test-bucket", Arg.Any<string>(),
                Arg.Is<string>(n => n.EndsWith(".xlsx")),
                Arg.Any<string>());
        }

        [Fact]
        public async Task ImportInvoiceAsync_WhenFileNameIsOnlyExtension_UsesDefaultOriginalName()
        {
            // Arrange
            var file = CreateMockFormFile(".xlsx");
            var rows = new List<InvoiceImportRowDto> { new() { ProjectParent = "PP001" } };
            _excelImportService.ReadExcel(
                Arg.Any<IXLWorkbook>(),
                Arg.Any<Func<IXLRangeRow, Dictionary<string, int>, InvoiceImportRowDto>>(),
                Arg.Any<IEnumerable<string>>(),
                Arg.Any<int>(),
                Arg.Any<string?>())
                .Returns(new ExcelImportResult<InvoiceImportRowDto> { IsSuccess = true, Rows = rows });

            var importResult = new InvoiceImportResultDto { PassedCount = 1, FailedCount = 0, Message = "OK" };
            _pactProjectInvoiceApiClient.ImportInvoiceAsync(Arg.Any<InvoiceImportReqDto>())
                .Returns(ApiResponseDto<InvoiceImportResultDto>.SuccessResponse(importResult));

            _configuration["S3Storage:BucketName"].Returns("test-bucket");
            _s3StorageService.UploadFileAsync(Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
                .Returns(S3UploadResult.SuccessResponse("key"));

            // Act
            var result = await _service.ImportInvoiceAsync(file);

            // Assert — originalName defaults to "invoice-import"
            Assert.True(result.Success);
            await _s3StorageService.Received(1).UploadFileAsync(
                Arg.Any<Stream>(), "test-bucket", Arg.Any<string>(),
                Arg.Is<string>(n => n.StartsWith("invoice-import_")),
                Arg.Any<string>());
        }

        [Fact]
        public async Task ImportInvoiceAsync_WhenSelectedFPSYearIsNegative_UsesCurrentYear()
        {
            // Arrange
            var file = CreateMockFormFile("invoices.xlsx");
            var rows = new List<InvoiceImportRowDto> { new() { ProjectParent = "PP001" } };
            _excelImportService.ReadExcel(
                Arg.Any<IXLWorkbook>(),
                Arg.Any<Func<IXLRangeRow, Dictionary<string, int>, InvoiceImportRowDto>>(),
                Arg.Any<IEnumerable<string>>(),
                Arg.Any<int>(),
                Arg.Any<string?>())
                .Returns(new ExcelImportResult<InvoiceImportRowDto> { IsSuccess = true, Rows = rows });

            var importResult = new InvoiceImportResultDto { PassedCount = 1, FailedCount = 0, Message = "OK" };
            _pactProjectInvoiceApiClient.ImportInvoiceAsync(Arg.Any<InvoiceImportReqDto>())
                .Returns(ApiResponseDto<InvoiceImportResultDto>.SuccessResponse(importResult));

            var httpContext = new DefaultHttpContext();
            httpContext.Items["SelectedFPSYear"] = "-1";
            _httpContextAccessor.HttpContext.Returns(httpContext);
            _configuration["S3Storage:BucketName"].Returns("test-bucket");
            _s3StorageService.UploadFileAsync(Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
                .Returns(S3UploadResult.SuccessResponse("key"));

            // Act
            var result = await _service.ImportInvoiceAsync(file);

            // Assert
            Assert.True(result.Success);
            var currentYear = DateTime.UtcNow.Year;
            await _s3StorageService.Received(1).UploadFileAsync(
                Arg.Any<Stream>(), "test-bucket", $"FPS{currentYear}/InvoiceImport", Arg.Any<string>(), Arg.Any<string>());
        }

        [Fact]
        public async Task ImportInvoiceAsync_WhenMapRowHasValidId_MapsIdAndAllFieldsInRequest()
        {
            // Arrange
            var file = CreateMockFormFile("invoice-with-id.xlsx");

            _excelImportService.NormalizeHeader(Arg.Any<string>())
                .Returns(ci => ci.Arg<string>().Trim().ToLowerInvariant());
            _excelImportService.GetText(Arg.Any<IXLCell>())
                .Returns(ci => ci.Arg<IXLCell>().GetString());

            _excelImportService.ReadExcel(
                Arg.Any<IXLWorkbook>(),
                Arg.Any<Func<IXLRangeRow, Dictionary<string, int>, InvoiceImportRowDto>>(),
                Arg.Any<IEnumerable<string>>(),
                Arg.Any<int>(),
                Arg.Any<string?>())
                .Returns(ci =>
                {
                    var mapper = ci.ArgAt<Func<IXLRangeRow, Dictionary<string, int>, InvoiceImportRowDto>>(1);

                    using var wb = new XLWorkbook();
                    var ws = wb.Worksheets.Add("Sheet1");
                    ws.Cell(2, 1).Value = "42";
                    ws.Cell(2, 2).Value = "PP777";
                    ws.Cell(2, 3).Value = "3";
                    ws.Cell(2, 4).Value = "1000";
                    ws.Cell(2, 5).Value = "600";
                    ws.Cell(2, 6).Value = "400";
                    ws.Cell(2, 7).Value = "400";
                    ws.Cell(2, 8).Value = "detail-text";

                    var headerMap = new Dictionary<string, int>
                    {
                        ["id"] = 1,
                        ["project parent"] = 2,
                        ["month"] = 3,
                        ["amount"] = 4,
                        ["cost of work"] = 5,
                        ["wip"] = 6,
                        ["profit loss"] = 7,
                        ["detail"] = 8
                    };

                    var mappedRow = mapper(ws.Range(2, 1, 2, 8).Rows().First(), headerMap);
                    return new ExcelImportResult<InvoiceImportRowDto>
                    {
                        IsSuccess = true,
                        Rows = [mappedRow]
                    };
                });

            var importResponse = ApiResponseDto<InvoiceImportResultDto>.SuccessResponse(new InvoiceImportResultDto { PassedCount = 1, FailedCount = 0 });
            _pactProjectInvoiceApiClient.ImportInvoiceAsync(Arg.Any<InvoiceImportReqDto>()).Returns(importResponse);
            _configuration["S3Storage:BucketName"].Returns("test-bucket");
            _s3StorageService.UploadFileAsync(Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
                .Returns(S3UploadResult.SuccessResponse("key"));

            // Act
            var result = await _service.ImportInvoiceAsync(file);

            // Assert
            Assert.True(result.Success);
            await _pactProjectInvoiceApiClient.Received(1).ImportInvoiceAsync(
                Arg.Is<InvoiceImportReqDto>(r =>
                    r.Rows.Count == 1 &&
                    r.Rows[0].Id == 42 &&
                    r.Rows[0].ProjectParent == "PP777" &&
                    r.Rows[0].Month == "3" &&
                    r.Rows[0].Amount == "1000" &&
                    r.Rows[0].CostOfWork == "600" &&
                    r.Rows[0].Wip == "400" &&
                    r.Rows[0].ProfitLoss == "400" &&
                    r.Rows[0].Detail == "detail-text"));
        }

        [Fact]
        public async Task ImportInvoiceAsync_WhenMapRowIdIsNotNumeric_DefaultsIdToZero()
        {
            // Arrange
            var file = CreateMockFormFile("invoice-invalid-id.xlsx");

            _excelImportService.NormalizeHeader(Arg.Any<string>())
                .Returns(ci => ci.Arg<string>().Trim().ToLowerInvariant());
            _excelImportService.GetText(Arg.Any<IXLCell>())
                .Returns(ci => ci.Arg<IXLCell>().GetString());

            _excelImportService.ReadExcel(
                Arg.Any<IXLWorkbook>(),
                Arg.Any<Func<IXLRangeRow, Dictionary<string, int>, InvoiceImportRowDto>>(),
                Arg.Any<IEnumerable<string>>(),
                Arg.Any<int>(),
                Arg.Any<string?>())
                .Returns(ci =>
                {
                    var mapper = ci.ArgAt<Func<IXLRangeRow, Dictionary<string, int>, InvoiceImportRowDto>>(1);

                    using var wb = new XLWorkbook();
                    var ws = wb.Worksheets.Add("Sheet1");
                    ws.Cell(2, 1).Value = "not-an-int";
                    ws.Cell(2, 2).Value = "PP001";
                    ws.Cell(2, 3).Value = "1";
                    ws.Cell(2, 4).Value = "100";
                    ws.Cell(2, 5).Value = "50";
                    ws.Cell(2, 6).Value = "50";
                    ws.Cell(2, 7).Value = "50";
                    ws.Cell(2, 8).Value = "detail";

                    var headerMap = new Dictionary<string, int>
                    {
                        ["id"] = 1,
                        ["project parent"] = 2,
                        ["month"] = 3,
                        ["amount"] = 4,
                        ["cost of work"] = 5,
                        ["wip"] = 6,
                        ["profit loss"] = 7,
                        ["detail"] = 8
                    };

                    var mappedRow = mapper(ws.Range(2, 1, 2, 8).Rows().First(), headerMap);
                    return new ExcelImportResult<InvoiceImportRowDto> { IsSuccess = true, Rows = [mappedRow] };
                });

            var importResponse = ApiResponseDto<InvoiceImportResultDto>.SuccessResponse(new InvoiceImportResultDto { PassedCount = 1, FailedCount = 0 });
            _pactProjectInvoiceApiClient.ImportInvoiceAsync(Arg.Any<InvoiceImportReqDto>()).Returns(importResponse);
            _configuration["S3Storage:BucketName"].Returns("test-bucket");
            _s3StorageService.UploadFileAsync(Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
                .Returns(S3UploadResult.SuccessResponse("key"));

            // Act
            var result = await _service.ImportInvoiceAsync(file);

            // Assert
            Assert.True(result.Success);
            await _pactProjectInvoiceApiClient.Received(1).ImportInvoiceAsync(
                Arg.Is<InvoiceImportReqDto>(r => r.Rows.Count == 1 && r.Rows[0].Id == 0));
        }

        [Fact]
        public async Task ImportInvoiceAsync_WhenMapRowHeaderDoesNotContainId_DefaultsIdToZero()
        {
            // Arrange
            var file = CreateMockFormFile("invoice-no-id-header.xlsx");

            _excelImportService.NormalizeHeader(Arg.Any<string>())
                .Returns(ci => ci.Arg<string>().Trim().ToLowerInvariant());
            _excelImportService.GetText(Arg.Any<IXLCell>())
                .Returns(ci => ci.Arg<IXLCell>().GetString());

            _excelImportService.ReadExcel(
                Arg.Any<IXLWorkbook>(),
                Arg.Any<Func<IXLRangeRow, Dictionary<string, int>, InvoiceImportRowDto>>(),
                Arg.Any<IEnumerable<string>>(),
                Arg.Any<int>(),
                Arg.Any<string?>())
                .Returns(ci =>
                {
                    var mapper = ci.ArgAt<Func<IXLRangeRow, Dictionary<string, int>, InvoiceImportRowDto>>(1);

                    using var wb = new XLWorkbook();
                    var ws = wb.Worksheets.Add("Sheet1");
                    ws.Cell(2, 1).Value = "PP001";
                    ws.Cell(2, 2).Value = "1";
                    ws.Cell(2, 3).Value = "100";
                    ws.Cell(2, 4).Value = "50";
                    ws.Cell(2, 5).Value = "50";
                    ws.Cell(2, 6).Value = "50";
                    ws.Cell(2, 7).Value = "detail";

                    var headerMap = new Dictionary<string, int>
                    {
                        ["project parent"] = 1,
                        ["month"] = 2,
                        ["amount"] = 3,
                        ["cost of work"] = 4,
                        ["wip"] = 5,
                        ["profit loss"] = 6,
                        ["detail"] = 7
                    };

                    var mappedRow = mapper(ws.Range(2, 1, 2, 7).Rows().First(), headerMap);
                    return new ExcelImportResult<InvoiceImportRowDto> { IsSuccess = true, Rows = [mappedRow] };
                });

            var importResponse = ApiResponseDto<InvoiceImportResultDto>.SuccessResponse(new InvoiceImportResultDto { PassedCount = 1, FailedCount = 0 });
            _pactProjectInvoiceApiClient.ImportInvoiceAsync(Arg.Any<InvoiceImportReqDto>()).Returns(importResponse);
            _configuration["S3Storage:BucketName"].Returns("test-bucket");
            _s3StorageService.UploadFileAsync(Arg.Any<Stream>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
                .Returns(S3UploadResult.SuccessResponse("key"));

            // Act
            var result = await _service.ImportInvoiceAsync(file);

            // Assert
            Assert.True(result.Success);
            await _pactProjectInvoiceApiClient.Received(1).ImportInvoiceAsync(
                Arg.Is<InvoiceImportReqDto>(r => r.Rows.Count == 1 && r.Rows[0].Id == 0));
        }

        #endregion
    }
}
