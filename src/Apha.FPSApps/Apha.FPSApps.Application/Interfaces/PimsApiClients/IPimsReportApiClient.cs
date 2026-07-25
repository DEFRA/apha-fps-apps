using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.PimsApiClients
{
    // TRANSFORMENGINE: mirrors ReportController — GET/POST /api/v1/report, GET/PUT/DELETE /api/v1/report/{id:int}
    public interface IPimsReportApiClient
    {
        // TRANSFORMENGINE: GET /api/v1/report — no required params; full list for Reports Tab grid
        Task<ApiResponseDto<List<ReportDto>>> GetAllReportsAsync();

        // GET /api/v1/report/paged — paged/sorted/filterable list
        Task<ApiResponseDto<PaginatedResult<ReportDto>>> GetPagedReportsAsync(QueryParameters<string> query);

        // TRANSFORMENGINE: GET /api/v1/report/{id:int}
        Task<ApiResponseDto<ReportDto>> GetReportByIdAsync(int id);

        // TRANSFORMENGINE: POST /api/v1/report
        Task<ApiResponseDto<ReportDto>> CreateReportAsync(ReportDto dto);

        // TRANSFORMENGINE: PUT /api/v1/report/{id:int}
        Task<ApiResponseDto<ReportDto>> UpdateReportAsync(int id, ReportDto dto);

        // TRANSFORMENGINE: DELETE /api/v1/report/{id:int}
        Task<ApiResponseDto<bool>> DeleteReportAsync(int id);
    }
}
