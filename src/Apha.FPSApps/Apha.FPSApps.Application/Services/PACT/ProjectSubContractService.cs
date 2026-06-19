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
            var hasMissingHeaders = GetMissingRequiredHeaders(headerMap).Any();
            if (hasMissingHeaders)
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

            var colProject = headerMap[NormalizeHeader("Project")];
            var colTestJob = headerMap[NormalizeHeader("Test Job")];
            var colMonth = headerMap[NormalizeHeader("Month")];
            var colAmount = headerMap[NormalizeHeader("Amount")];
            //var colWorkGroup = headerMap[NormalizeHeader("Work Group")];
            var colAccountCode = headerMap[NormalizeHeader("Account Code")];
            var colSupplier = headerMap[NormalizeHeader("Supplier")];
            var colDescription = headerMap[NormalizeHeader("Description")];
            var colSupplierNumber = headerMap[NormalizeHeader("Supplier Number")];
            var colDailyRate = headerMap[NormalizeHeader("Daily Rate")];
            var colAnimalDays = headerMap[NormalizeHeader("Animal Days")];

            request.Rows = new List<SubContractRmsImportRowDto>(Math.Max(usedRows.Count - 1, 0));

            foreach (var row in usedRows.Skip(1))
            {
                request.Rows.Add(new SubContractRmsImportRowDto
                {
                    Project = GetText(row.Cell(colProject)),
                    TestJob = GetText(row.Cell(colTestJob)),
                    Month = GetText(row.Cell(colMonth)),
                    Amount = GetText(row.Cell(colAmount)),
                    //WorkGroup = GetText(row.Cell(colWorkGroup)),
                    AcctCode = GetText(row.Cell(colAccountCode)),
                    Supplier = GetText(row.Cell(colSupplier)),
                    Description = GetText(row.Cell(colDescription)),
                    SupplierNumber = GetText(row.Cell(colSupplierNumber)),
                    DailyRate = GetText(row.Cell(colDailyRate)),
                    AnimalDays = GetText(row.Cell(colAnimalDays))
                });
            }

            return await _pactClient.PactProjectSubContract.ImportSubContractRmsAsync(request);
        }

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

        private static string? GetText(IXLCell cell)
        {
            if (cell == null || cell.IsEmpty()) return null;
            var text = cell.GetString()?.Trim();
            return string.IsNullOrWhiteSpace(text) ? null : text;
        }

        private static string NormalizeHeader(string value)
        {
            return new string((value ?? string.Empty).Where(char.IsLetterOrDigit).ToArray());
        }

        private static readonly string[] TemplateHeaders =
        {
            "Project",
            "Test Job",
            "Month",
            "Amount",
            //"Work Group",
            "Account Code",
            "Supplier",
            "Description",
            "Supplier Number",
            "Daily Rate",
            "Animal Days"
        };

        private static IEnumerable<string> GetMissingRequiredHeaders(Dictionary<string, int> headerMap)
        {
            foreach (var header in TemplateHeaders)
            {
                if (!headerMap.ContainsKey(NormalizeHeader(header)))
                    yield return header;
            }
        }


    }
}
