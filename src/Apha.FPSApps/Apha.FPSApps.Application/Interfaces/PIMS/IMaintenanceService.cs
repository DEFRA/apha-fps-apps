/*
 * TRANSFORMENGINE MIGRATION — IMaintenanceService.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 8 — Frontend Service Interface + Implementation (Steps 12-13)
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New frontend aggregate service interface for frmMaintainance (PIMS Admin Maintenance)
 *   - Aggregates all Maintenance-form API surfaces: Report, ReportGroup, ReportGroupLink,
 *     ProjectManager, ProgramManagerLink, ProfitCentreManagerLink, Setting, AccessUser,
 *     AccessLevel, AccessUserLevel, AccessSystem (lookup), Frequency, ReviewItem, RadTrackProg
 *   - Method signatures mirror the corresponding IPimsXxxApiClient interfaces exactly
 *   - Composite and natural-PK variants preserved (reportid+groupid, program+manager, etc.)
 *
 * PRESERVED:
 *   - All PK types: int (Report, ReportGroup, Frequency, ReviewItem),
 *     string (ProjectManager, RadTrackProg), composite int+int (ReportGroupLink, AccessLevel),
 *     composite string+string (ProgramManagerLink, ProfitCentreManagerLink),
 *     composite int+string (AccessUser, AccessUserLevel)
 *   - Read-only surfaces for AccessSystem (lookup only — no create/update/delete)
 *   - Setting read/update only (no create/delete)
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: confirm role requirements for admin-gated endpoints (Setting update, AccessUser CRUD)
 *   - TRANSFORMENGINE TODO: confirm composite delete routes for ReportGroupLink, ProgramManagerLink, ProfitCentreManagerLink,
 *     AccessUserLevel are acceptable from MVC controller callers
 */

using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;

namespace Apha.FPSApps.Application.Interfaces.PIMS
{
    public interface IMaintenanceService
    {
        // ── Report ──────────────────────────────────────────────────────────────────
        // TRANSFORMENGINE: delegates to IPimsReportApiClient — GET/POST /api/v1/report, GET/PUT/DELETE /api/v1/report/{id:int}

        Task<ApiResponseDto<List<ReportDto>>> GetAllReportsAsync();
        Task<ApiResponseDto<ReportDto>> GetReportByIdAsync(int id);
        Task<ApiResponseDto<ReportDto>> CreateReportAsync(ReportDto dto);
        Task<ApiResponseDto<ReportDto>> UpdateReportAsync(int id, ReportDto dto);
        Task<ApiResponseDto<bool>> DeleteReportAsync(int id);

        // ── ReportGroup ─────────────────────────────────────────────────────────────
        // TRANSFORMENGINE: delegates to IPimsReportGroupApiClient — GET/POST /api/v1/reportgroup, GET/PUT/DELETE /api/v1/reportgroup/{groupid:int}
        // Also used as lookup source for Report dropdown

        Task<ApiResponseDto<List<ReportGroupDto>>> GetAllReportGroupsAsync();
        Task<ApiResponseDto<ReportGroupDto>> GetReportGroupByIdAsync(int groupid);
        Task<ApiResponseDto<ReportGroupDto>> CreateReportGroupAsync(ReportGroupDto dto);
        Task<ApiResponseDto<ReportGroupDto>> UpdateReportGroupAsync(int groupid, ReportGroupDto dto);
        Task<ApiResponseDto<bool>> DeleteReportGroupAsync(int groupid);

        // ── ReportGroupLink ─────────────────────────────────────────────────────────
        // TRANSFORMENGINE: delegates to IPimsReportGroupLinkApiClient — composite PK (reportid int + groupid int); no PUT

        Task<ApiResponseDto<List<ReportGroupLinkDto>>> GetAllReportGroupLinksAsync();
        Task<ApiResponseDto<List<ReportGroupLinkDto>>> GetReportGroupLinksByReportIdAsync(int reportid);
        Task<ApiResponseDto<ReportGroupLinkDto>> GetReportGroupLinkByIdAsync(int reportid, int groupid);
        Task<ApiResponseDto<ReportGroupLinkDto>> CreateReportGroupLinkAsync(ReportGroupLinkDto dto);
        Task<ApiResponseDto<bool>> DeleteReportGroupLinkAsync(int reportid, int groupid);

        // ── ProjectManager ──────────────────────────────────────────────────────────
        // TRANSFORMENGINE: delegates to IPimsProjectManagerApiClient — natural varchar PK (projectmanager)

        Task<ApiResponseDto<List<ProjectManagerDto>>> GetAllProjectManagersAsync();
        Task<ApiResponseDto<ProjectManagerDto>> GetProjectManagerByIdAsync(string projectmanager);
        Task<ApiResponseDto<ProjectManagerDto>> CreateProjectManagerAsync(ProjectManagerDto dto);
        Task<ApiResponseDto<ProjectManagerDto>> UpdateProjectManagerAsync(string projectmanager, ProjectManagerDto dto);
        Task<ApiResponseDto<bool>> DeleteProjectManagerAsync(string projectmanager);

        // ── ProgramManagerLink ──────────────────────────────────────────────────────
        // TRANSFORMENGINE: delegates to IPimsProgramManagerLinkApiClient — composite natural PK (program string + manager string); no PUT

        Task<ApiResponseDto<List<ProgramManagerLinkDto>>> GetAllProgramManagerLinksAsync();
        Task<ApiResponseDto<List<ProgramManagerLinkDto>>> GetProgramManagerLinksByProgramAsync(string program);
        Task<ApiResponseDto<ProgramManagerLinkDto>> GetProgramManagerLinkByIdAsync(string program, string manager);
        Task<ApiResponseDto<ProgramManagerLinkDto>> CreateProgramManagerLinkAsync(ProgramManagerLinkDto dto);
        Task<ApiResponseDto<bool>> DeleteProgramManagerLinkAsync(string program, string manager);

        // ── ProfitCentreManagerLink ─────────────────────────────────────────────────
        // TRANSFORMENGINE: delegates to IPimsProfitCentreManagerLinkApiClient — composite natural PK (profitcentre string + manager string); no PUT

        Task<ApiResponseDto<List<ProfitCentreManagerLinkDto>>> GetAllProfitCentreManagerLinksAsync();
        Task<ApiResponseDto<List<ProfitCentreManagerLinkDto>>> GetProfitCentreManagerLinksByProfitCentreAsync(string profitcentre);
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

        Task<ApiResponseDto<List<AccessUserLevelDto>>> GetAllAccessUserLevelsAsync();
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
        Task<ApiResponseDto<FrequencyDto>> GetFrequencyByIdAsync(int frequencyid);
        Task<ApiResponseDto<FrequencyDto>> CreateFrequencyAsync(FrequencyDto dto);
        Task<ApiResponseDto<FrequencyDto>> UpdateFrequencyAsync(int frequencyid, FrequencyDto dto);
        Task<ApiResponseDto<bool>> DeleteFrequencyAsync(int frequencyid);

        // ── ReviewItem ──────────────────────────────────────────────────────────────
        // TRANSFORMENGINE: delegates to IPimsReviewItemApiClient — integer PK (itemid); full CRUD; Other Tab lookup

        Task<ApiResponseDto<List<ReviewItemDto>>> GetAllReviewItemsAsync();
        Task<ApiResponseDto<ReviewItemDto>> GetReviewItemByIdAsync(int itemid);
        Task<ApiResponseDto<ReviewItemDto>> CreateReviewItemAsync(ReviewItemDto dto);
        Task<ApiResponseDto<ReviewItemDto>> UpdateReviewItemAsync(int itemid, ReviewItemDto dto);
        Task<ApiResponseDto<bool>> DeleteReviewItemAsync(int itemid);

        // ── RadTrackProg ────────────────────────────────────────────────────────────
        // TRANSFORMENGINE: delegates to IPimsRadTrackProgApiClient — natural string PK (program); full CRUD; Programme Tab

        Task<ApiResponseDto<List<RadTrackProgDto>>> GetAllRadTrackProgsAsync();
        Task<ApiResponseDto<RadTrackProgDto>> GetRadTrackProgByIdAsync(string program);
        Task<ApiResponseDto<RadTrackProgDto>> CreateRadTrackProgAsync(RadTrackProgDto dto);
        Task<ApiResponseDto<RadTrackProgDto>> UpdateRadTrackProgAsync(string program, RadTrackProgDto dto);
        Task<ApiResponseDto<bool>> DeleteRadTrackProgAsync(string program);
    }
}
