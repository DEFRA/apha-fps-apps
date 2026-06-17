using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PACT;
using Apha.FPSApps.Application.Interfaces.PACT;
using Apha.FPSApps.Application.Interfaces.PactApiClients;
using Apha.FPSApps.Application.Pagination;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Http;

namespace Apha.FPSApps.Application.Services.PACT
{
    public class ProjectSubContractService : IProjectSubContractService
    {
        private readonly IPactApiClient _pactClient;

        public ProjectSubContractService(IPactApiClient pactClient)
        {
            _pactClient = pactClient;
        }

        public async Task<ApiResponseDto<List<ProjectSubContractDto>>> GetPagedProjectSubContractsAsync(QueryParameters<string> query, string? project)
            => await _pactClient.PactProjectSubContract.GetPagedProjectSubContractsAsync(query, project);

        public async Task<ApiResponseDto<List<ProjectSubContractDto>>> GetPagedProjectSubContractsManualAsync(QueryParameters<string> query, string? project)
            => await _pactClient.PactProjectSubContract.GetPagedProjectSubContractsManualAsync(query, project);

        public async Task<ApiResponseDto<decimal>> GetTotalAmountAsync(string? project)
            => await _pactClient.PactProjectSubContract.GetTotalAmountAsync(project);

        public async Task<ApiResponseDto<ProjectSubContractDto>> GetByIdAsync(int subContCounter)
            => await _pactClient.PactProjectSubContract.GetByIdAsync(subContCounter);

        public async Task<ApiResponseDto<ProjectSubContractDto>> CreateAsync(ProjectSubContractDto dto)
            => await _pactClient.PactProjectSubContract.CreateAsync(dto);

        public async Task<ApiResponseDto<ProjectSubContractDto>> UpdateAsync(int subContCounter, ProjectSubContractDto dto)
            => await _pactClient.PactProjectSubContract.UpdateAsync(subContCounter, dto);

        public async Task<ApiResponseDto<bool>> DeleteAsync(int subContCounter)
            => await _pactClient.PactProjectSubContract.DeleteAsync(subContCounter);

        public async Task<ApiResponseDto<List<ProjectSubContractDto>>> GetFpsProjectSubContractsAsync(QueryParameters<string> query, string? project, bool filterByAnimalAcctCodes = false)
            => await _pactClient.PactProjectSubContract.GetFpsProjectSubContractsAsync(query, project, filterByAnimalAcctCodes);

        public async Task<ApiResponseDto<decimal>> GetFpsProjectSubContractTotalAmountAsync(string? project, bool filterByAnimalAcctCodes = false)
            => await _pactClient.PactProjectSubContract.GetFpsProjectSubContractTotalAmountAsync(project, filterByAnimalAcctCodes);

        public async Task<ApiResponseDto<MonthlySubContractsPivotDto>> GetMonthlySubContractsSummaryAsync(QueryParameters<string> query)
           => await _pactClient.PactProjectSubContract.GetMonthlySubContractsSummaryAsync(query);

        public async Task<ApiResponseDto<List<SubContractRmsImportRowDto>>> GetFailedSubContractRmsAsync(QueryParameters<string> query)
            => await _pactClient.PactProjectSubContract.GetFailedSubContractRmsAsync(query);

        public async Task<ApiResponseDto<SubContractRmsImportResultDto>> ImportSubContractRmsAsync(IFormFile file)
        {
            using var stream = file.OpenReadStream();
            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheet(1);
            var usedRows = worksheet.RangeUsed()?.RowsUsed().ToList() ?? new List<IXLRangeRow>();

            if (usedRows.Count <= 1)
            {
                return ApiResponseDto<SubContractRmsImportResultDto>.FailureResponse(
                    new List<ApiErrorDto> { new ApiErrorDto { Code = "EMPTY_FILE", Message = "No data rows found in the uploaded Excel file." } },
                    new ApiMetaDto { CorrelationId = Guid.NewGuid().ToString(), TimestampUtc = DateTime.UtcNow });
            }

            var request = new SubContractRmsImportReqDto
            {
                FileName = file.FileName
            };

            var headerMap = BuildHeaderMap(usedRows.First());
            var missingHeaders = GetMissingRequiredHeaders(headerMap).ToList();
            if (missingHeaders.Count > 0)
            {
                return ApiResponseDto<SubContractRmsImportResultDto>.FailureResponse(
                    new List<ApiErrorDto>
                    {
                        new ApiErrorDto
                        {
                            Code = "INVALID_TEMPLATE",
                            Message = "The uploaded Excel file format is not correct please use the correct template."
                        }
                    },
                    new ApiMetaDto { CorrelationId = Guid.NewGuid().ToString(), TimestampUtc = DateTime.UtcNow });
            }

            foreach (var row in usedRows.Skip(1))
            {
                request.Rows.Add(new SubContractRmsImportRowDto
                {
                    Project = GetText(row, headerMap, "Project"),
                    TestJob = GetText(row, headerMap, "TestJob", "Test Job"),
                    Month = TryGetDouble(GetCell(row, headerMap, "Month")),
                    Amount = TryGetDecimal(GetCell(row, headerMap, "Amount")),
                    WorkGroup = GetText(row, headerMap, "WorkGroup", "Work Group"),
                    AcctCode = GetText(row, headerMap, "AcctCode", "Acct Code", "AccountCode", "Account Code"),
                    Supplier = GetText(row, headerMap, "Supplier"),
                    Description = GetText(row, headerMap, "Description"),
                    SupplierNumber = TryGetInt(GetCell(row, headerMap, "SupplierNumber", "Supplier Number")),
                    DailyRate = TryGetDecimal(GetCell(row, headerMap, "DailyRate", "Daily Rate")),
                    AnimalDays = TryGetInt(GetCell(row, headerMap, "AnimalDays", "Animal Days"))
                });
            }

            return await _pactClient.PactProjectSubContract.ImportSubContractRmsAsync(request);
        }

        public async Task<byte[]> ExportFailedSubContractRmsAsync()
            => await _pactClient.PactProjectSubContract.ExportFailedSubContractRmsAsync();

        public async Task<ApiResponseDto<bool>> DeleteFailedSubContractRmsByUserAsync()
            => await _pactClient.PactProjectSubContract.DeleteFailedSubContractRmsByUserAsync();

        private static Dictionary<string, int> BuildHeaderMap(IXLRangeRow headerRow)
        {
            var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (var cell in headerRow.CellsUsed())
            {
                var header = NormalizeHeader(cell.GetString());
                if (!string.IsNullOrWhiteSpace(header) && !map.ContainsKey(header))
                    map[header] = cell.Address.ColumnNumber;
            }
            return map;
        }

        private static IXLCell GetCell(IXLRangeRow row, Dictionary<string, int> headerMap, params string[] headerNames)
        {
            foreach (var headerName in headerNames)
            {
                var key = NormalizeHeader(headerName);
                if (headerMap.TryGetValue(key, out var col))
                    return row.Cell(col);
            }

            return row.Cell(1);
        }

        private static string? GetText(IXLRangeRow row, Dictionary<string, int> headerMap, params string[] headerNames)
        {
            var cell = GetCell(row, headerMap, headerNames);
            var text = cell.GetString()?.Trim();
            return string.IsNullOrWhiteSpace(text) ? null : text;
        }

        private static string NormalizeHeader(string value)
        {
            return new string((value ?? string.Empty).Where(char.IsLetterOrDigit).ToArray());
        }

        private static IEnumerable<string> GetMissingRequiredHeaders(Dictionary<string, int> headerMap)
        {
            var requiredHeaderGroups = new List<(string DisplayName, string[] Aliases)>
            {
                ("Project", new[] { "Project" }),
                ("Test Job", new[] { "TestJob", "Test Job" }),
                ("Month", new[] { "Month" }),
                ("Amount", new[] { "Amount" }),
                //("Work Group", new[] { "WorkGroup", "Work Group" }),
                ("Account Code", new[] { "AcctCode", "Acct Code", "AccountCode", "Account Code" }),
                ("Supplier", new[] { "Supplier" }),
                ("Description", new[] { "Description" }),
                ("Supplier Number", new[] { "SupplierNumber", "Supplier Number" }),
                ("Daily Rate", new[] { "DailyRate", "Daily Rate" }),
                ("Animal Days", new[] { "AnimalDays", "Animal Days" })
            };

            foreach (var (displayName, aliases) in requiredHeaderGroups)
            {
                var found = aliases.Any(a => headerMap.ContainsKey(NormalizeHeader(a)));
                if (!found)
                    yield return displayName;
            }
        }

        private static decimal? TryGetDecimal(IXLCell cell)
        {
            if (cell == null || cell.IsEmpty()) return null;

            var text = cell.GetString()?.Trim();
            if (decimal.TryParse(text, out var parsed)) return parsed;
            if (decimal.TryParse(text?.Replace(",", string.Empty), out parsed)) return parsed;
            return null;
        }

        private static double? TryGetDouble(IXLCell cell)
        {
            if (cell == null || cell.IsEmpty()) return null;

            var text = cell.GetString()?.Trim();
            if (double.TryParse(text, out var parsed)) return parsed;
            if (double.TryParse(text?.Replace(",", string.Empty), out parsed)) return parsed;
            return null;
        }

        private static int? TryGetInt(IXLCell cell)
        {
            if (cell == null || cell.IsEmpty()) return null;

            var text = cell.GetString()?.Trim();
            if (int.TryParse(text, out var parsed)) return parsed;
            return null;
        }
    }
}
