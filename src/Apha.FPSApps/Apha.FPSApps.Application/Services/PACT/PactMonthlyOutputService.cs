using Apha.Common.Utilities.ExcelImport;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.PACT;
using Apha.FPSApps.Application.Interfaces.PactApiClients;
using Apha.FPSApps.Application.Pagination;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Http;

namespace Apha.FPSApps.Application.Services.PACT
{
    public class PactMonthlyOutputService : IPactMonthlyOutputService
    {
        private readonly IPactApiClient _pactApiClient;
        private readonly IExcelImportService _excelImportService;

        private static readonly string[] RequiredHeaders =
            ["Work Group", "Test Code", "Buyer", "Month", "Volume"];

        public PactMonthlyOutputService(IPactApiClient pactApiClient)
            : this(pactApiClient, new ExcelImportService())
        {
        }

        public PactMonthlyOutputService(IPactApiClient pactApiClient, IExcelImportService excelImportService)
        {
            _pactApiClient = pactApiClient;
            _excelImportService = excelImportService;
        }

        // ── Log ──────────────────────────────────────────────────────────────────

        public async Task<ApiResponseDto<List<MonthlyOutputLogDto>>> SearchAsync(
            QueryParameters<string> query,
            MonthlyOutputLogFilterDto filter)
            => await _pactApiClient.PactMonthlyOutput.SearchAsync(query, filter);

        // ── Live ─────────────────────────────────────────────────────────────────

        public async Task<ApiResponseDto<List<PactMonthlyOutputDto>>> GetLiveAsync(
            QueryParameters<string> query,
            string? workGroup,
            string? testCode,
            string? buyer,
            double? month)
            => await _pactApiClient.PactMonthlyOutput.GetLiveAsync(query, workGroup, testCode, buyer, month);

        public async Task<ApiResponseDto<PactMonthlyOutputDto>> GetLiveByKeyAsync(string testCode, string buyer, double month, string workGroup)
            => await _pactApiClient.PactMonthlyOutput.GetLiveByKeyAsync(testCode, buyer, month, workGroup);

        public async Task<ApiResponseDto<PactMonthlyOutputDto>> UpdateLiveAsync(PactMonthlyOutputDto dto)
            => await _pactApiClient.PactMonthlyOutput.UpdateLiveAsync(dto);

        public async Task<ApiResponseDto<bool>> DeleteLiveAsync(string testCode, string buyer, double month, string workGroup)
            => await _pactApiClient.PactMonthlyOutput.DeleteLiveAsync(testCode, buyer, month, workGroup);

        // ── Staging ──────────────────────────────────────────────────────────────

        public async Task<ApiResponseDto<List<StagingMonthlyOutputDto>>> GetStagingAsync(QueryParameters<string> query, bool? passed)
            => await _pactApiClient.PactMonthlyOutput.GetStagingAsync(query, passed);

        public async Task<ApiResponseDto<StagingMonthlyOutputDto>> GetStagingByIdAsync(int id)
            => await _pactApiClient.PactMonthlyOutput.GetStagingByIdAsync(id);

        public async Task<ApiResponseDto<StagingMonthlyOutputDto>> CreateStagingAsync(StagingMonthlyOutputDto dto)
            => await _pactApiClient.PactMonthlyOutput.CreateStagingAsync(dto);

        public async Task<ApiResponseDto<StagingMonthlyOutputDto>> UpdateStagingAsync(int id, StagingMonthlyOutputDto dto)
            => await _pactApiClient.PactMonthlyOutput.UpdateStagingAsync(id, dto);

        public async Task<ApiResponseDto<bool>> DeleteStagingAsync(int id)
            => await _pactApiClient.PactMonthlyOutput.DeleteStagingAsync(id);

        public async Task<ApiResponseDto<bool>> DeleteAllStagingByUserAsync()
            => await _pactApiClient.PactMonthlyOutput.DeleteAllStagingByUserAsync();

        public async Task<ApiResponseDto<bool>> DeleteFailedStagingByUserAsync()
            => await _pactApiClient.PactMonthlyOutput.DeleteFailedStagingByUserAsync();

        // ── Import ───────────────────────────────────────────────────────────────

        public async Task<ApiResponseDto<MonthlyOutputImportResultDto>> ImportMonthlyOutputAsync(IFormFile file)
        {
            using var stream = file.OpenReadStream();
            using var workbook = new XLWorkbook(stream);

            var importResult = _excelImportService.ReadExcel(
                workbook,
                MapOutputRow,
                RequiredHeaders,
                1,
                "The uploaded Excel file format is not correct. Please use the correct PACT flat file template.");

            if (!importResult.IsSuccess)
            {
                var errors = new List<ApiErrorDto>();
                if (importResult.MissingHeaders?.Count > 0)
                    errors.Add(new ApiErrorDto
                    {
                        Code = "INVALID_TEMPLATE",
                        Message = $"Missing columns: {string.Join(", ", importResult.MissingHeaders)}. " +
                                  "Please use the correct PACT flat file template."
                    });
                else
                    errors.Add(new ApiErrorDto
                    {
                        Code = "EMPTY_FILE",
                        Message = importResult.ErrorMessage ?? "No data rows found in the uploaded Excel file."
                    });

                return ApiResponseDto<MonthlyOutputImportResultDto>.FailureResponse(
                    errors,
                    new ApiMetaDto { CorrelationId = Guid.NewGuid().ToString(), TimestampUtc = DateTime.UtcNow });
            }

            var request = new MonthlyOutputImportReqDto
            {
                FileName = file.FileName,
                Rows = importResult.Rows
            };

            return await _pactApiClient.PactMonthlyOutput.ImportStagingAsync(request);
        }

        public async Task<ApiResponseDto<MonthlyOutputValidateResultDto>> ValidateStagingAsync()
            => await _pactApiClient.PactMonthlyOutput.ValidateStagingAsync();

        public async Task<ApiResponseDto<MonthlyOutputMakeLiveResultDto>> MakeLiveAsync()
            => await _pactApiClient.PactMonthlyOutput.MakeLiveAsync();

        // ── Helpers ──────────────────────────────────────────────────────────────

        private MonthlyOutputImportRowDto MapOutputRow(IXLRangeRow row, Dictionary<string, int> headerMap)
        {
            return new MonthlyOutputImportRowDto
            {
                WorkGroup       = _excelImportService.GetText(row.Cell(headerMap[_excelImportService.NormalizeHeader("Work Group")])),
                TestCode        = _excelImportService.GetText(row.Cell(headerMap[_excelImportService.NormalizeHeader("Test Code")])),
                //ItemDescription = _excelImportService.GetText(row.Cell(headerMap[_excelImportService.NormalizeHeader("Item Description")])),
                Buyer           = _excelImportService.GetText(row.Cell(headerMap[_excelImportService.NormalizeHeader("Buyer")])),
                Month           = _excelImportService.GetText(row.Cell(headerMap[_excelImportService.NormalizeHeader("Month")])),
                Volume          = _excelImportService.GetText(row.Cell(headerMap[_excelImportService.NormalizeHeader("Volume")]))
            };
        }
    }
}

