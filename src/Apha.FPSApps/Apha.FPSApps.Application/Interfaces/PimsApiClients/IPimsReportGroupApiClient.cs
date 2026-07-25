using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.PimsApiClients
{
    // TRANSFORMENGINE: mirrors ReportGroupController — GET/POST /api/v1/reportgroup, GET/PUT/DELETE /api/v1/reportgroup/{groupid:int}
    public interface IPimsReportGroupApiClient
    {
        // TRANSFORMENGINE: GET /api/v1/reportgroup — full lookup list (also used as Report dropdown source)
        Task<ApiResponseDto<List<ReportGroupDto>>> GetAllReportGroupsAsync();

        // TRANSFORMENGINE: GET /api/v1/reportgroup/paged (+ optional reportid) — paged/sorted/filterable list
        Task<ApiResponseDto<PaginatedResult<ReportGroupDto>>> GetPagedReportGroupsAsync(QueryParameters<string> query, int? reportId = null);

        // TRANSFORMENGINE: GET /api/v1/reportgroup/byreport/{reportid:int} — groups linked to a specific report
        Task<ApiResponseDto<List<ReportGroupDto>>> GetReportGroupsByReportIdAsync(int reportId);

        // TRANSFORMENGINE: GET /api/v1/reportgroup/{groupid:int}
        Task<ApiResponseDto<ReportGroupDto>> GetReportGroupByIdAsync(int groupId);

        // TRANSFORMENGINE: POST /api/v1/reportgroup
        Task<ApiResponseDto<ReportGroupDto>> CreateReportGroupAsync(ReportGroupDto dto);

        // TRANSFORMENGINE: PUT /api/v1/reportgroup/{groupid:int}
        Task<ApiResponseDto<ReportGroupDto>> UpdateReportGroupAsync(int groupId, ReportGroupDto dto);

        // TRANSFORMENGINE: DELETE /api/v1/reportgroup/{groupid:int}
        Task<ApiResponseDto<bool>> DeleteReportGroupAsync(int groupId);
    }
}
