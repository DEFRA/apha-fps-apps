using Apha.Common.Utilities.ExcelImport;
using Apha.Common.Utilities.Storage;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.PACT;
using Apha.FPSApps.Application.Interfaces.PactApiClients;
using Apha.FPSApps.Application.Pagination;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Apha.FPSApps.Application.Services.PACT
{
    public class ProjectInvoiceService : IProjectInvoiceService
    {
        private readonly IPactApiClient _pactClient;
        private readonly IExcelImportService _excelImportService;
        private readonly IS3StorageService _s3StorageService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ProjectInvoiceService> _logger;

        public ProjectInvoiceService(
            IPactApiClient pactClient,
            IExcelImportService excelImportService,
            IS3StorageService s3StorageService,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration,
            ILogger<ProjectInvoiceService> logger)
        {
            _pactClient = pactClient;
            _excelImportService = excelImportService;
            _s3StorageService = s3StorageService;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<ApiResponseDto<List<ProjectInvoiceDto>>> GetPagedProjectInvoicesAsync(QueryParameters<string> query, string? parentProject)
            => await _pactClient.PactProjectInvoice.GetPagedProjectInvoicesAsync(query, parentProject);
        public async Task<ApiResponseDto<List<ProjectInvoiceDto>>> GetPagedProjectInvoiceManualAsync(QueryParameters<string> query, string? parentProject)
            => await _pactClient.PactProjectInvoice.GetPagedProjectInvoiceManualAsync(query, parentProject);

        public async Task<ApiResponseDto<decimal>> GetTotalAmountAsync(string? parentProject)
            => await _pactClient.PactProjectInvoice.GetTotalAmountAsync(parentProject);

        public async Task<ApiResponseDto<ProjectInvoiceDto>> GetByIdAsync(int invoiceCounter)
            => await _pactClient.PactProjectInvoice.GetByIdAsync(invoiceCounter);

        public async Task<ApiResponseDto<ProjectInvoiceDto>> CreateAsync(ProjectInvoiceDto dto)
            => await _pactClient.PactProjectInvoice.CreateAsync(dto);

        public async Task<ApiResponseDto<ProjectInvoiceDto>> UpdateAsync(int invoiceCounter, ProjectInvoiceDto dto)
            => await _pactClient.PactProjectInvoice.UpdateAsync(invoiceCounter, dto);

        public async Task<ApiResponseDto<bool>> DeleteAsync(int invoiceCounter)
            => await _pactClient.PactProjectInvoice.DeleteAsync(invoiceCounter);

        public async Task<ApiResponseDto<MonthlyInvoicesPivotDto>> GetMonthlyInvoicesSummaryAsync(QueryParameters<string> query)
            => await _pactClient.PactProjectInvoice.GetMonthlyInvoicesSummaryAsync(query);

        public async Task<ApiResponseDto<List<InvoiceImportRowDto>>> GetFailedInvoiceImportAsync(QueryParameters<string> query)
            => await _pactClient.PactProjectInvoice.GetFailedInvoiceImportAsync(query);

        public async Task<ApiResponseDto<InvoiceImportRowDto>> GetFailedInvoiceImportByIdAsync(int id)
            => await _pactClient.PactProjectInvoice.GetFailedInvoiceImportByIdAsync(id);

        public async Task<ApiResponseDto<bool>> SaveFailedInvoiceImportAsync(int id, InvoiceImportRowDto dto)
            => await _pactClient.PactProjectInvoice.SaveFailedInvoiceImportAsync(id, dto);

        public async Task<ApiResponseDto<bool>> DeleteFailedInvoiceImportByIdAsync(int id)
            => await _pactClient.PactProjectInvoice.DeleteFailedInvoiceImportByIdAsync(id);

        public async Task<ApiResponseDto<bool>> DeleteFailedInvoiceImportByUserAsync()
            => await _pactClient.PactProjectInvoice.DeleteFailedInvoiceImportByUserAsync();

        public async Task<ApiResponseDto<InvoiceImportResultDto>> ImportInvoiceAsync(IFormFile file)
        {
            using var originalFileStream = file.OpenReadStream();
            using var bufferStream = new MemoryStream();
            await originalFileStream.CopyToAsync(bufferStream);

            bufferStream.Position = 0;
            using var workbook = new XLWorkbook(bufferStream);

            var requiredHeaders = new[]
            {
                "Project Parent",
                "Month",
                "Amount",
                "Cost Of Work",
                "WIP",
                "Profit Loss",
                "Detail"
            };

            var importResult = _excelImportService.ReadExcel(
                workbook,
                MapRowToDto,
                requiredHeaders,
                worksheetIndex: 1);

            if (!importResult.IsSuccess)
            {
                return ApiResponseDto<InvoiceImportResultDto>.FailureResponse(
                    new List<ApiErrorDto>
                    {
                        new ApiErrorDto
                        {
                            Code = importResult.MissingHeaders.Count > 0 ? "INVALID_TEMPLATE" : "EMPTY_FILE",
                            Message = importResult.ErrorMessage ?? "Import failed."
                        }
                    },
                    new ApiMetaDto { CorrelationId = Guid.NewGuid().ToString(), TimestampUtc = DateTime.UtcNow });
            }

            var request = new InvoiceImportReqDto
            {
                FileName = file.FileName,
                Rows = importResult.Rows
            };

            var importResponse = await _pactClient.PactProjectInvoice.ImportInvoiceAsync(request);
            if (!importResponse.Success || importResponse.Data == null)
            {
                return importResponse;
            }

            bufferStream.Position = 0;
            try
            {
                var uploadResult = await UploadAuditFileAsync(file, bufferStream);
                if (!uploadResult.Success)
                {
                    _logger.LogWarning(
                        "Invoice import succeeded but S3 audit upload failed. FileName: {FileName}, ErrorCode: {ErrorCode}, Message: {Message}",
                        file.FileName,
                        uploadResult.ErrorCode,
                        uploadResult.Message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Invoice import succeeded but S3 audit upload threw an exception. FileName: {FileName}",
                    file.FileName);
            }

            return importResponse;
        }

        private InvoiceImportRowDto MapRowToDto(IXLRangeRow row, Dictionary<string, int> headerMap)
        {
            var normalizedIdHeader = _excelImportService.NormalizeHeader("Id");
            var stagingId = 0;
            if (headerMap.TryGetValue(normalizedIdHeader, out var idCol))
            {
                var idText = _excelImportService.GetText(row.Cell(idCol));
                _ = int.TryParse(idText, out stagingId);
            }

            return new InvoiceImportRowDto
            {
                Id = stagingId,
                ProjectParent = _excelImportService.GetText(row.Cell(headerMap[_excelImportService.NormalizeHeader("Project Parent")])),
                Month = _excelImportService.GetText(row.Cell(headerMap[_excelImportService.NormalizeHeader("Month")])),
                Amount = _excelImportService.GetText(row.Cell(headerMap[_excelImportService.NormalizeHeader("Amount")])),
                CostOfWork = _excelImportService.GetText(row.Cell(headerMap[_excelImportService.NormalizeHeader("Cost Of Work")])),
                Wip = _excelImportService.GetText(row.Cell(headerMap[_excelImportService.NormalizeHeader("WIP")])),
                ProfitLoss = _excelImportService.GetText(row.Cell(headerMap[_excelImportService.NormalizeHeader("Profit Loss")])),
                Detail = _excelImportService.GetText(row.Cell(headerMap[_excelImportService.NormalizeHeader("Detail")])),

            };
        }

        private async Task<S3UploadResult> UploadAuditFileAsync(IFormFile file, Stream fileStream)
        {
            var sourceFileName = Path.GetFileName(file.FileName);
            if (string.IsNullOrWhiteSpace(sourceFileName))
                sourceFileName = "invoice-import.xlsx";

            var timestamp = DateTime.UtcNow;
            var selectedYear = timestamp.Year;
            var selectedYearItem = _httpContextAccessor.HttpContext?.Items["SelectedFPSYear"];
            if (selectedYearItem != null && int.TryParse(selectedYearItem.ToString(), out var parsedYear) && parsedYear > 0)
                selectedYear = parsedYear;

            var folderPath = $"FPS{selectedYear}/InvoiceImport";

            var originalName = Path.GetFileNameWithoutExtension(sourceFileName);
            if (string.IsNullOrWhiteSpace(originalName))
                originalName = "invoice-import";

            var extension = Path.GetExtension(sourceFileName);
            if (string.IsNullOrWhiteSpace(extension))
                extension = ".xlsx";

            var auditFileName = $"{originalName}_{timestamp:yyyyMMddHHmmss}{extension}";

            return await _s3StorageService.UploadFileAsync(
                fileStream,
                GetAuditBucketName(),
                folderPath,
                auditFileName,
                file.ContentType);
        }

        private string GetAuditBucketName()
            => _configuration["S3Storage:BucketName"]
               ?? throw new InvalidOperationException("S3Storage:BucketName is not configured.");
    }
}
