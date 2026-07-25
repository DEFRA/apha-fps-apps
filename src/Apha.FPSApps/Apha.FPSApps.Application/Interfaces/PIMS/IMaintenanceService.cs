using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Interfaces.PIMS
{
    public interface IMaintenanceService
    {
        Task<ApiResponseDto<List<ReportDto>>> GetAllReportsAsync();

        Task<ApiResponseDto<PaginatedResult<ReportDto>>> GetPagedReportsAsync(QueryParameters<string> query);
        Task<ApiResponseDto<ReportDto>> GetReportByIdAsync(int id);
        Task<ApiResponseDto<ReportDto>> CreateReportAsync(ReportDto dto);
        Task<ApiResponseDto<ReportDto>> UpdateReportAsync(int id, ReportDto dto);
        Task<ApiResponseDto<bool>> DeleteReportAsync(int id);

        Task<ApiResponseDto<List<ReportGroupDto>>> GetAllReportGroupsAsync();
        Task<ApiResponseDto<PaginatedResult<ReportGroupDto>>> GetPagedReportGroupsAsync(QueryParameters<string> query, int? reportId = null);
        Task<ApiResponseDto<List<ReportGroupDto>>> GetReportGroupsByReportIdAsync(int reportId);
        Task<ApiResponseDto<ReportGroupDto>> GetReportGroupByIdAsync(int groupId);
        Task<ApiResponseDto<ReportGroupDto>> CreateReportGroupAsync(ReportGroupDto dto);
        Task<ApiResponseDto<ReportGroupDto>> UpdateReportGroupAsync(int groupId, ReportGroupDto dto);
        Task<ApiResponseDto<bool>> DeleteReportGroupAsync(int groupId);

        // ── ReportGroupLink ─────────────────────────────────────────────────────────
        // TRANSFORMENGINE: delegates to IPimsReportGroupLinkApiClient — composite PK (reportid int + groupid int); no PUT

        Task<ApiResponseDto<List<ReportGroupLinkDto>>> GetAllReportGroupLinksAsync();
        Task<ApiResponseDto<List<ReportGroupLinkDto>>> GetReportGroupLinksByReportIdAsync(int reportid);
        Task<ApiResponseDto<ReportGroupLinkDto>> GetReportGroupLinkByIdAsync(int reportid, int groupid);
        Task<ApiResponseDto<ReportGroupLinkDto>> CreateReportGroupLinkAsync(ReportGroupLinkDto dto);
        Task<ApiResponseDto<bool>> DeleteReportGroupLinkAsync(int reportid, int groupid);

        // ── ProjectManager ──────────────────────────────────────────────────────────
        // TRANSFORMENGINE: delegates to IPimsProjectManagerApiClient — natural varchar PK (projectmanager)

        Task<ApiResponseDto<List<ProjectManagerDto>>> GetAllProjectManagersAsync(QueryParameters<string>? query = null);
        Task<ApiResponseDto<PaginatedResult<ProjectManagerDto>>> GetPagedProjectManagersAsync(QueryParameters<string> query);
        Task<ApiResponseDto<List<string>>> GetManagerNamesAsync();
        Task<ApiResponseDto<ProjectManagerDto>> GetProjectManagerByIdAsync(string projectmanager);
        Task<ApiResponseDto<ProjectManagerDto>> CreateProjectManagerAsync(ProjectManagerDto dto);
        Task<ApiResponseDto<ProjectManagerDto>> UpdateProjectManagerAsync(string projectmanager, ProjectManagerDto dto);
        Task<ApiResponseDto<bool>> DeleteProjectManagerAsync(string projectmanager);

        // ── ProgramManagerLink ──────────────────────────────────────────────────────
        // TRANSFORMENGINE: delegates to IPimsProgramManagerLinkApiClient — composite natural PK (program string + manager string); no PUT

        Task<ApiResponseDto<List<ProgramManagerLinkDto>>> GetAllProgramManagerLinksAsync();
        Task<ApiResponseDto<PaginatedResult<ProgramManagerLinkDto>>> GetPagedProgramManagerLinksByManagerAsync(QueryParameters<string> query, string manager);
        Task<ApiResponseDto<List<ProgramManagerLinkDto>>> GetProgramManagerLinksByProgramAsync(string program);
        Task<ApiResponseDto<List<ProgramManagerLinkDto>>> GetProgramManagerLinksByManagerAsync(string manager);
        Task<ApiResponseDto<ProgramManagerLinkDto>> GetProgramManagerLinkByIdAsync(string program, string manager);
        Task<ApiResponseDto<ProgramManagerLinkDto>> CreateProgramManagerLinkAsync(ProgramManagerLinkDto dto);
        Task<ApiResponseDto<bool>> DeleteProgramManagerLinkAsync(string program, string manager);
        Task<ApiResponseDto<List<ProgramLookupDto>>> GetProgramsAsync();

        // ── ProfitCentreManagerLink ─────────────────────────────────────────────────
        // TRANSFORMENGINE: delegates to IPimsProfitCentreManagerLinkApiClient — composite natural PK (profitcentre string + manager string); no PUT

        Task<ApiResponseDto<List<ProfitCentreManagerLinkDto>>> GetAllProfitCentreManagerLinksAsync();
        Task<ApiResponseDto<PaginatedResult<ProfitCentreManagerLinkDto>>> GetPagedProfitCentreManagerLinksByManagerAsync(QueryParameters<string> query, string manager);
        Task<ApiResponseDto<List<ProfitCentreLookupDto>>> GetProfitCentresAsync();
        Task<ApiResponseDto<List<ProfitCentreManagerLinkDto>>> GetProfitCentreManagerLinksByProfitCentreAsync(string profitcentre);
        Task<ApiResponseDto<List<ProfitCentreManagerLinkDto>>> GetProfitCentreManagerLinksByManagerAsync(string manager);
        Task<ApiResponseDto<ProfitCentreManagerLinkDto>> GetProfitCentreManagerLinkByIdAsync(string profitcentre, string manager);
        Task<ApiResponseDto<ProfitCentreManagerLinkDto>> CreateProfitCentreManagerLinkAsync(ProfitCentreManagerLinkDto dto);
        Task<ApiResponseDto<bool>> DeleteProfitCentreManagerLinkAsync(string profitcentre, string manager);

        // ── Setting ─────────────────────────────────────────────────────────────────
        // TRANSFORMENGINE: delegates to IPimsSettingApiClient — read/update only; string PK; no create/delete

        Task<ApiResponseDto<List<SettingDto>>> GetAllSettingsAsync();
        Task<ApiResponseDto<List<SettingDto>>> GetAllUserUpdateableSettingsAsync();
        Task<ApiResponseDto<SettingDto>> GetSettingByIdAsync(string id);
        Task<ApiResponseDto<SettingDto>> UpdateSettingAsync(string id, SettingDto dto);

        // ── AccessUser ──────────────────────────────────────────────────────────────
        // TRANSFORMENGINE: delegates to IPimsAccessUserApiClient — composite PK (systemid int + ntlogin string)

        Task<ApiResponseDto<List<AccessUserDto>>> GetAllAccessUsersAsync();
        Task<ApiResponseDto<List<AccessUserDto>>> GetAccessUsersBySystemIdAsync(int systemid);
        Task<ApiResponseDto<AccessUserDto>> GetAccessUserByIdAsync(int systemid, string ntlogin);
        Task<ApiResponseDto<AccessUserDto>> CreateAccessUserAsync(AccessUserDto dto);
        Task<ApiResponseDto<AccessUserDto>> UpdateAccessUserAsync(int systemid, string ntlogin, AccessUserDto dto);
        Task<ApiResponseDto<bool>> DeleteAccessUserAsync(int systemid, string ntlogin);

        // ── AccessLevel ─────────────────────────────────────────────────────────────
        // TRANSFORMENGINE: delegates to IPimsAccessLevelApiClient — composite PK (systemid int + accesslevelid int)

        Task<ApiResponseDto<List<AccessLevelDto>>> GetAllAccessLevelsAsync();
        Task<ApiResponseDto<List<AccessLevelDto>>> GetAccessLevelsBySystemIdAsync(int systemid);
        Task<ApiResponseDto<AccessLevelDto>> GetAccessLevelByIdAsync(int systemid, int accesslevelid);
        Task<ApiResponseDto<AccessLevelDto>> CreateAccessLevelAsync(AccessLevelDto dto);
        Task<ApiResponseDto<AccessLevelDto>> UpdateAccessLevelAsync(int systemid, int accesslevelid, AccessLevelDto dto);
        Task<ApiResponseDto<bool>> DeleteAccessLevelAsync(int systemid, int accesslevelid);

        // ── AccessUserLevel ─────────────────────────────────────────────────────────
        // TRANSFORMENGINE: delegates to IPimsAccessUserLevelApiClient — triple composite PK (systemid int + ntlogin string + accesslevelid int); no PUT

        Task<ApiResponseDto<PaginatedResult<AccessUserLevelDto>>> GetPagedAccessUserLevelsAsync(QueryParameters<string> request);
        Task<ApiResponseDto<List<AccessUserLevelDto>>> GetAccessUserLevelsBySystemIdAsync(int systemid);
        Task<ApiResponseDto<List<AccessUserLevelDto>>> GetAccessUserLevelsByUserAsync(int systemid, string ntlogin);
        Task<ApiResponseDto<AccessUserLevelDto>> GetAccessUserLevelByIdAsync(int systemid, string ntlogin, int accesslevelid);
        Task<ApiResponseDto<AccessUserLevelDto>> CreateAccessUserLevelAsync(AccessUserLevelDto dto);
        Task<ApiResponseDto<bool>> DeleteAccessUserLevelAsync(int systemid, string ntlogin, int accesslevelid);

        // ── AccessSystem (lookup — read-only) ───────────────────────────────────────
        // TRANSFORMENGINE: delegates to IPimsAccessSystemApiClient — reference lookup data; no create/update/delete

        Task<ApiResponseDto<List<AccessSystemDto>>> GetAllAccessSystemsAsync();
        Task<ApiResponseDto<AccessSystemDto>> GetAccessSystemByIdAsync(int systemid);

        // ── Frequency ───────────────────────────────────────────────────────────────
        // TRANSFORMENGINE: delegates to IPimsFrequencyApiClient — integer PK (frequencyid); full CRUD

        Task<ApiResponseDto<List<FrequencyDto>>> GetAllFrequenciesAsync();
        Task<ApiResponseDto<PaginatedResult<FrequencyDto>>> GetPagedFrequenciesAsync(QueryParameters<string> query);
        Task<ApiResponseDto<FrequencyDto>> GetFrequencyByIdAsync(int frequencyId);
        Task<ApiResponseDto<FrequencyDto>> CreateFrequencyAsync(FrequencyDto dto);
        Task<ApiResponseDto<FrequencyDto>> UpdateFrequencyAsync(int frequencyId, FrequencyDto dto);
        Task<ApiResponseDto<bool>> DeleteFrequencyAsync(int frequencyId);

        // ── ReviewItem ──────────────────────────────────────────────────────────────
        // TRANSFORMENGINE: delegates to IPimsReviewItemApiClient — integer PK (itemid); full CRUD; Other Tab lookup

        Task<ApiResponseDto<List<ReviewItemDto>>> GetAllReviewItemsAsync();
        Task<ApiResponseDto<PaginatedResult<ReviewItemDto>>> GetPagedReviewItemsAsync(QueryParameters<string> query);
        Task<ApiResponseDto<ReviewItemDto>> GetReviewItemByIdAsync(int itemId);
        Task<ApiResponseDto<ReviewItemDto>> CreateReviewItemAsync(ReviewItemDto dto);
        Task<ApiResponseDto<ReviewItemDto>> UpdateReviewItemAsync(int itemId, ReviewItemDto dto);
        Task<ApiResponseDto<bool>> DeleteReviewItemAsync(int itemId);

        // ── Risk ──────────────────────────────────────────────────────────────────────────────────────
        // delegates to IPimsRiskApiClient — integer PK (riskid); full CRUD; Other Tab lookup

        Task<ApiResponseDto<List<RiskDto>>> GetAllRiskRatingsAsync();
        Task<ApiResponseDto<PaginatedResult<RiskDto>>> GetPagedRiskRatingsAsync(QueryParameters<string> query);
        Task<ApiResponseDto<RiskDto>> GetRiskRatingByIdAsync(int riskId);
        Task<ApiResponseDto<RiskDto>> CreateRiskRatingAsync(RiskDto dto);
        Task<ApiResponseDto<RiskDto>> UpdateRiskRatingAsync(int riskId, RiskDto dto);
        Task<ApiResponseDto<bool>> DeleteRiskRatingAsync(int riskId);

        // ── PublicationType ───────────────────────────────────────────────────────────────────────────
        // delegates to IPimsPublicationTypeApiClient — string PK (type); full CRUD; Other Tab lookup

        Task<ApiResponseDto<List<PublicationTypeDto>>> GetAllPublicationTypesAsync();
        Task<ApiResponseDto<PaginatedResult<PublicationTypeDto>>> GetPagedPublicationTypesAsync(QueryParameters<string> query);
        Task<ApiResponseDto<PublicationTypeDto>> GetPublicationTypeByCodeAsync(string type);
        Task<ApiResponseDto<PublicationTypeDto>> CreatePublicationTypeAsync(PublicationTypeDto dto);
        Task<ApiResponseDto<PublicationTypeDto>> UpdatePublicationTypeAsync(string type, PublicationTypeDto dto);
        Task<ApiResponseDto<bool>> DeletePublicationTypeAsync(string type);

        // ── RadTrackProg
        // TRANSFORMENGINE: delegates to IPimsRadTrackProgApiClient — natural string PK (program); full CRUD; Programme Tab

        Task<ApiResponseDto<List<RadTrackProgDto>>> GetAllRadTrackProgsAsync();
        Task<ApiResponseDto<PaginatedResult<RadTrackProgDto>>> GetPagedRadTrackProgsAsync(QueryParameters<string> query);
        Task<ApiResponseDto<RadTrackProgDto>> GetRadTrackProgByIdAsync(string program);
        Task<ApiResponseDto<RadTrackProgDto>> CreateRadTrackProgAsync(RadTrackProgDto dto);
        Task<ApiResponseDto<RadTrackProgDto>> UpdateRadTrackProgAsync(string program, RadTrackProgDto dto);
        Task<ApiResponseDto<bool>> DeleteRadTrackProgAsync(string program);

        // Returns distinct non-null Programme names from MY_tlkpProject for dropdown binding
        Task<ApiResponseDto<List<string>>> GetRadTrackProgProgramsAsync();
    }
}
