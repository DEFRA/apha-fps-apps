using Apha.Common.Utilities.ExcelImport;
using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.PACT;
using Apha.FPSApps.Application.Interfaces.PactApiClients;
using Apha.FPSApps.Application.Pagination;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Http;
using System.Text.RegularExpressions;

namespace Apha.FPSApps.Application.Services.PACT
{
    public class PactMonthlyTimeService : IPactMonthlyTimeService
    {
        private readonly IPactApiClient _pactApiClient;
        private readonly IExcelImportService _excelImportService;

        public PactMonthlyTimeService(IPactApiClient pactApiClient)
            : this(pactApiClient, new ExcelImportService())
        {
        }

        public PactMonthlyTimeService(IPactApiClient pactApiClient, IExcelImportService excelImportService)
        {
            _pactApiClient = pactApiClient;
            _excelImportService = excelImportService;
        }

        public async Task<ApiResponseDto<List<MonthlyTimeDto>>> GetLiveAsync(
            QueryParameters<string> query,
            string? workGroup,
            string? timeCode,
            string? pactStaffId,
            string? parentProject,
            double? month)
            => await _pactApiClient.PactMonthlyTime.GetLiveAsync(query, workGroup, timeCode, pactStaffId, parentProject, month);

        public async Task<ApiResponseDto<MonthlyTimeDto>> GetLiveByKeyAsync(string pactStaffId, string timeCode, double month, string parentProject)
            => await _pactApiClient.PactMonthlyTime.GetLiveByKeyAsync(pactStaffId, timeCode, month, parentProject);

        public async Task<ApiResponseDto<MonthlyTimeDto>> UpdateLiveAsync(MonthlyTimeDto dto)
            => await _pactApiClient.PactMonthlyTime.UpdateLiveAsync(dto);

        public async Task<ApiResponseDto<List<StagingMonthlyTimeDto>>> GetStagingAsync(QueryParameters<string> query, bool? passed)
            => await _pactApiClient.PactMonthlyTime.GetStagingAsync(query, passed);

        public async Task<ApiResponseDto<StagingMonthlyTimeDto>> GetStagingByIdAsync(int id)
            => await _pactApiClient.PactMonthlyTime.GetStagingByIdAsync(id);

        public async Task<ApiResponseDto<StagingMonthlyTimeDto>> CreateStagingAsync(StagingMonthlyTimeDto dto)
            => await _pactApiClient.PactMonthlyTime.CreateStagingAsync(dto);

        public async Task<ApiResponseDto<StagingMonthlyTimeDto>> UpdateStagingAsync(int id, StagingMonthlyTimeDto dto)
            => await _pactApiClient.PactMonthlyTime.UpdateStagingAsync(id, dto);

        public async Task<ApiResponseDto<BulkUpdateStagingMonthlyTimeNamesResultDto>> BulkUpdateStagingNamesAsync(BulkUpdateStagingMonthlyTimeNamesDto dto)
            => await _pactApiClient.PactMonthlyTime.BulkUpdateStagingNamesAsync(dto);

        public async Task<ApiResponseDto<bool>> DeleteStagingAsync(int id)
            => await _pactApiClient.PactMonthlyTime.DeleteStagingAsync(id);

        public async Task<ApiResponseDto<bool>> DeleteAllStagingByUserAsync()
            => await _pactApiClient.PactMonthlyTime.DeleteAllStagingByUserAsync();

        public async Task<ApiResponseDto<bool>> DeleteFailedStagingByUserAsync()
            => await _pactApiClient.PactMonthlyTime.DeleteFailedStagingByUserAsync();

        public async Task<ApiResponseDto<MonthlyTimeImportResultDto>> ImportMonthlyTimeAsync(IFormFile file, short importType)
        {
            using var stream = file.OpenReadStream();
            using var workbook = new XLWorkbook(stream);

            return importType switch
            {
                1 => await ImportOtlDataAsync(file.FileName, workbook),
                2 => await ImportFlatFileAsync(file.FileName, workbook),
                3 => await ImportCrossTabAsync(file.FileName, workbook),
                4 => await ImportExportedDataAsync(file.FileName, workbook),
                _ => ApiResponseDto<MonthlyTimeImportResultDto>.FailureResponse(
                    [new ApiErrorDto { Code = "INVALID_IMPORT_TYPE", Message = "Unsupported monthly time import type." }],
                    new ApiMetaDto { CorrelationId = Guid.NewGuid().ToString(), TimestampUtc = DateTime.UtcNow })
            };
        }

        public async Task<ApiResponseDto<MonthlyTimeValidateResultDto>> ValidateStagingAsync()
            => await _pactApiClient.PactMonthlyTime.ValidateStagingAsync();

        public async Task<ApiResponseDto<MonthlyTimeMakeLiveResultDto>> MakeLiveAsync()
            => await _pactApiClient.PactMonthlyTime.MakeLiveAsync();

        public async Task<ApiResponseDto<List<MonthlyTimeLogDto>>> SearchAsync(
            QueryParameters<string> query,
            MonthlyTimeLogFilterDto filter)
            => await _pactApiClient.PactMonthlyTime.SearchAsync(query, filter);

        private async Task<ApiResponseDto<MonthlyTimeImportResultDto>> ImportFlatFileAsync(string fileName, IXLWorkbook workbook)
        {
            if (!TryGetTimeFileMetadata(fileName, out var workGroupFromFile, out var monthFromFile))
            {
                return ApiResponseDto<MonthlyTimeImportResultDto>.FailureResponse(
                    [new ApiErrorDto { Code = "INVALID_FILENAME", Message = "Filename must contain WorkGroup and Month before TS (e.g. WorkGroup01TS.xlsx)." }],
                    new ApiMetaDto { CorrelationId = Guid.NewGuid().ToString(), TimestampUtc = DateTime.UtcNow });
            }

            var requiredHeaders = new[] { "Work Group", "Name", "Time Code", "Parent Project", "Month", "Hours" };
            var importResult = _excelImportService.ReadExcel(
                workbook,
                (row, headerMap) => MapFlatFileRow(row, headerMap, workGroupFromFile, monthFromFile),
                requiredHeaders,
                1,
                "The uploaded Excel file format is not correct. Please use the correct flat-file template.");
            if (!importResult.IsSuccess)
            {
                return BuildImportFailure(importResult, "INVALID_TEMPLATE", "EMPTY_FILE");
            }

            var request = new MonthlyTimeImportReqDto
            {
                FileName = fileName,
                ImportType = 2,
                Rows = importResult.Rows
            };

            return await _pactApiClient.PactMonthlyTime.ImportStagingAsync(request);
        }

        private async Task<ApiResponseDto<MonthlyTimeImportResultDto>> ImportCrossTabAsync(string fileName, IXLWorkbook workbook)
        {
            if (!TryGetTimeFileMetadata(fileName, out var workGroupFromFile, out var monthFromFile))
            {
                return ApiResponseDto<MonthlyTimeImportResultDto>.FailureResponse(
                    [new ApiErrorDto { Code = "INVALID_FILENAME", Message = "Filename must contain WorkGroup and Month before TS (e.g. WorkGroup01TS.xlsx)." }],
                    new ApiMetaDto { CorrelationId = Guid.NewGuid().ToString(), TimestampUtc = DateTime.UtcNow });
            }

            var rows = new List<MonthlyTimeImportRowDto>();
            var worksheet = workbook.Worksheet(1);
            var usedRows = worksheet.RangeUsed()?.RowsUsed().ToList() ?? [];

            if (usedRows.Count <= 1)
            {
                return ApiResponseDto<MonthlyTimeImportResultDto>.FailureResponse(
                    [new ApiErrorDto { Code = "EMPTY_FILE", Message = "No data rows found in the uploaded Excel file." }],
                    new ApiMetaDto { CorrelationId = Guid.NewGuid().ToString(), TimestampUtc = DateTime.UtcNow });
            }

            var headerMap = _excelImportService.BuildHeaderMap(usedRows[0]);
            var missingHeaders = _excelImportService.GetMissingRequiredHeaders(headerMap, new[] { "Time Code", "Parent Project" }).ToList();
            if (missingHeaders.Count > 0)
            {
                return ApiResponseDto<MonthlyTimeImportResultDto>.FailureResponse(
                    [new ApiErrorDto { Code = "INVALID_TEMPLATE", Message = "The uploaded Excel file format is not correct. Please use the correct cross-tab template." }],
                    new ApiMetaDto { CorrelationId = Guid.NewGuid().ToString(), TimestampUtc = DateTime.UtcNow });
            }

            var nonStaffMaxColumn = new[]
            {
                headerMap[_excelImportService.NormalizeHeader("Time Code")],
                headerMap[_excelImportService.NormalizeHeader("Parent Project")],
                headerMap.TryGetValue(_excelImportService.NormalizeHeader("Month"), out var monthColumn) ? monthColumn : 0,
                headerMap.TryGetValue(_excelImportService.NormalizeHeader("Description"), out var descriptionColumn) ? descriptionColumn : 0
            }.Max();

            var staffColumns = usedRows[0].CellsUsed()
                .Where(c => c.Address.ColumnNumber > nonStaffMaxColumn)
                .Select(c => new { c.Address.ColumnNumber, StaffName = c.GetString().Trim() })
                .Where(c => !string.IsNullOrWhiteSpace(c.StaffName))
                .ToList();

            var rowId = 1;
            foreach (var row in usedRows.Skip(1))
            {
                var timeCode = _excelImportService.GetText(row.Cell(headerMap[_excelImportService.NormalizeHeader("Time Code")]));
                var parentProject = _excelImportService.GetText(row.Cell(headerMap[_excelImportService.NormalizeHeader("Parent Project")]));

                foreach (var staffColumn in staffColumns)
                {
                    rows.Add(new MonthlyTimeImportRowDto
                    {
                        Id = rowId++,
                        WorkGroup = workGroupFromFile,
                        PactStaffId = staffColumn.StaffName,
                        Name = staffColumn.StaffName,
                        TimeCode = timeCode,
                        ParentProject = parentProject,
                        Month = monthFromFile,
                        Hours = _excelImportService.GetText(row.Cell(staffColumn.ColumnNumber))
                    });
                }
            }

            var request = new MonthlyTimeImportReqDto
            {
                FileName = fileName,
                ImportType = 3,
                Rows = rows
            };

            return await _pactApiClient.PactMonthlyTime.ImportStagingAsync(request);
        }

        private async Task<ApiResponseDto<MonthlyTimeImportResultDto>> ImportOtlDataAsync(string fileName, IXLWorkbook workbook)
        {
            var requiredHeaders = new[]
            {
                "Work Group",
                "Employee/Supplier Number",
                "Employee/Supplier",
                "Task Number",
                "Project Code",
                "Period",
                "Sum of Quantity"
            };

            var importResult = _excelImportService.ReadExcel(
                workbook,
                MapOtlDataRow,
                requiredHeaders,
                1,
                "The uploaded Excel file format is not correct. Please use the correct OTL Data template.");

            if (!importResult.IsSuccess)
            {
                return BuildImportFailure(importResult, "INVALID_TEMPLATE", "EMPTY_FILE");
            }

            var request = new MonthlyTimeImportReqDto
            {
                FileName = fileName,
                ImportType = 1,
                Rows = importResult.Rows
            };

            return await _pactApiClient.PactMonthlyTime.ImportStagingAsync(request);
        }

        private async Task<ApiResponseDto<MonthlyTimeImportResultDto>> ImportExportedDataAsync(string fileName, IXLWorkbook workbook)
        {
            var worksheet = workbook.Worksheet(1);
            var usedRows = worksheet.RangeUsed()?.RowsUsed().ToList() ?? [];
            if (usedRows.Count <= 1)
            {
                return ApiResponseDto<MonthlyTimeImportResultDto>.FailureResponse(
                    [new ApiErrorDto { Code = "EMPTY_FILE", Message = "No data rows found in the uploaded Excel file." }],
                    new ApiMetaDto { CorrelationId = Guid.NewGuid().ToString(), TimestampUtc = DateTime.UtcNow });
            }

            var headerMap = _excelImportService.BuildHeaderMap(usedRows[0]);
            var missingStagingIdColumn = _excelImportService.GetMissingRequiredHeaders(headerMap, new[] { "StagingId" }).Any();
            if (missingStagingIdColumn)
            {
                return ApiResponseDto<MonthlyTimeImportResultDto>.FailureResponse(
                    [new ApiErrorDto { Code = "INVALID_TEMPLATE", Message = "This file is not a valid correction file. Please use the exported file without removing the hidden StagingId column." }],
                    new ApiMetaDto { CorrelationId = Guid.NewGuid().ToString(), TimestampUtc = DateTime.UtcNow });
            }

            var requiredHeaders = new[]
            {
                "Work Group",
                "Pact Staff Id",
                "Name",
                "Time Code",
                "Parent Project",
                "Month",
                "Hours",
                "StagingId"
            };

            var importResult = _excelImportService.ReadExcel(
                workbook,
                MapExportedDataRow,
                requiredHeaders,
                1,
                "The uploaded Excel file format is not correct. Please use the correct exported file template.");

            if (!importResult.IsSuccess)
            {
                return BuildImportFailure(importResult, "INVALID_TEMPLATE", "EMPTY_FILE");
            }

            var request = new MonthlyTimeImportReqDto
            {
                FileName = fileName,
                ImportType = 4,
                Rows = importResult.Rows
            };

            return await _pactApiClient.PactMonthlyTime.ImportStagingAsync(request);
        }

        private MonthlyTimeImportRowDto MapFlatFileRow(
            IXLRangeRow row,
            Dictionary<string, int> headerMap,
            string workGroupFromFile,
            string monthFromFile)
        {
            return new MonthlyTimeImportRowDto
            {
                WorkGroup = workGroupFromFile,
                PactStaffId = _excelImportService.GetText(row.Cell(headerMap[_excelImportService.NormalizeHeader("Name")])),
                //Name = _excelImportService.GetText(row.Cell(headerMap[_excelImportService.NormalizeHeader("Name")])),
                TimeCode = _excelImportService.GetText(row.Cell(headerMap[_excelImportService.NormalizeHeader("Time Code")])),
                ParentProject = _excelImportService.GetText(row.Cell(headerMap[_excelImportService.NormalizeHeader("Parent Project")])),
                Month = monthFromFile,
                Hours = _excelImportService.GetText(row.Cell(headerMap[_excelImportService.NormalizeHeader("Hours")]))
            };
        }

        private MonthlyTimeImportRowDto MapOtlDataRow(IXLRangeRow row, Dictionary<string, int> headerMap)
        {
            return new MonthlyTimeImportRowDto
            {
                WorkGroup = _excelImportService.GetText(row.Cell(headerMap[_excelImportService.NormalizeHeader("Work Group")])) ,
                PactStaffId = _excelImportService.GetText(row.Cell(headerMap[_excelImportService.NormalizeHeader("Employee/Supplier Number")])) ,
                Name = _excelImportService.GetText(row.Cell(headerMap[_excelImportService.NormalizeHeader("Employee/Supplier")])) ,
                TimeCode = _excelImportService.GetText(row.Cell(headerMap[_excelImportService.NormalizeHeader("Task Number")])) ,
                ParentProject = _excelImportService.GetText(row.Cell(headerMap[_excelImportService.NormalizeHeader("Project Code")])) ,
                Month = _excelImportService.GetText(row.Cell(headerMap[_excelImportService.NormalizeHeader("Period")])) ,
                Hours = _excelImportService.GetText(row.Cell(headerMap[_excelImportService.NormalizeHeader("Sum of Quantity")]))
            };
        }

        private MonthlyTimeImportRowDto MapExportedDataRow(IXLRangeRow row, Dictionary<string, int> headerMap)
        {
            var stagingIdText = _excelImportService.GetText(row.Cell(headerMap[_excelImportService.NormalizeHeader("StagingId")])) ;
            return new MonthlyTimeImportRowDto
            {
                Id = int.TryParse(stagingIdText, out var parsedStagingId) ? parsedStagingId : 0,
                WorkGroup = _excelImportService.GetText(row.Cell(headerMap[_excelImportService.NormalizeHeader("Work Group")])) ,
                PactStaffId = _excelImportService.GetText(row.Cell(headerMap[_excelImportService.NormalizeHeader("Pact Staff Id")])) ,
                Name = _excelImportService.GetText(row.Cell(headerMap[_excelImportService.NormalizeHeader("Name")])) ,
                TimeCode = _excelImportService.GetText(row.Cell(headerMap[_excelImportService.NormalizeHeader("Time Code")])) ,
                ParentProject = _excelImportService.GetText(row.Cell(headerMap[_excelImportService.NormalizeHeader("Parent Project")])) ,
                Month = _excelImportService.GetText(row.Cell(headerMap[_excelImportService.NormalizeHeader("Month")])) ,
                Hours = _excelImportService.GetText(row.Cell(headerMap[_excelImportService.NormalizeHeader("Hours")]))
            };
        }

        private static bool TryGetTimeFileMetadata(string fileName, out string workGroup, out string month)
        {
            workGroup = string.Empty;
            month = string.Empty;

            var name = Path.GetFileNameWithoutExtension(fileName)?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(name))
            {
                return false;
            }

            var match = Regex.Match(name, "^(?<workGroup>[A-Za-z0-9]+?)(?<month>\\d{2})TS", RegexOptions.IgnoreCase);
            if (!match.Success)
            {
                return false;
            }

            workGroup = match.Groups["workGroup"].Value;
            month = match.Groups["month"].Value;
            return !string.IsNullOrWhiteSpace(workGroup) && !string.IsNullOrWhiteSpace(month);
        }

        private static ApiResponseDto<MonthlyTimeImportResultDto> BuildImportFailure<T>(ExcelImportResult<T> importResult, string invalidTemplateCode, string emptyFileCode)
        {
            return ApiResponseDto<MonthlyTimeImportResultDto>.FailureResponse(
                [new ApiErrorDto
                {
                    Code = importResult.MissingHeaders.Count > 0 ? invalidTemplateCode : emptyFileCode,
                    Message = importResult.ErrorMessage ?? "Import failed."
                }],
                new ApiMetaDto { CorrelationId = Guid.NewGuid().ToString(), TimestampUtc = DateTime.UtcNow });
        }
    }
}
