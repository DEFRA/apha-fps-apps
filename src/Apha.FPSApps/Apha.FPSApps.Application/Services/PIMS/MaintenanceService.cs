/*
 * TRANSFORMENGINE MIGRATION — MaintenanceService.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 8 — Frontend Service Interface + Implementation (Steps 12-13)
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New thin-delegate frontend aggregate service for frmMaintainance (PIMS Admin Maintenance)
 *   - Implements IMaintenanceService by forwarding every call to the corresponding
 *     IPimsApiClient sub-client (PimsReport, PimsReportGroup, PimsReportGroupLink,
 *     PimsProjectManager, PimsProgramManagerLink, PimsProfitCentreManagerLink, PimsSetting,
 *     PimsAccessUser, PimsAccessLevel, PimsAccessUserLevel, PimsAccessSystem,
 *     PimsFrequency, PimsReviewItem, PimsRadTrackProg)
 *   - No business logic — each method body is a single return await delegation
 *   - _client field is private readonly (Sonar S2933)
 *
 * PRESERVED:
 *   - All PK types and composite-PK parameter signatures from IMaintenanceService
 *   - Read-only surfaces (AccessSystem) and read/update-only surfaces (Setting)
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: verify IPimsApiClient is registered in DI container (ServiceCollectionExtension) before first use
 *   - TRANSFORMENGINE TODO: confirm role requirements for admin-gated endpoints (Setting update, AccessUser CRUD)
 */

using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Application.Interfaces.PIMS;
using Apha.FPSApps.Application.Interfaces.PimsApiClients;

namespace Apha.FPSApps.Application.Services.PIMS
{
    public class MaintenanceService : IMaintenanceService
    {
        // TRANSFORMENGINE: S2933 — private readonly; injected via constructor DI
        private readonly IPimsApiClient _client;

        public MaintenanceService(IPimsApiClient client)
        {
            _client = client;
        }

        // ── Report ──────────────────────────────────────────────────────────────────
        // TRANSFORMENGINE: thin delegates — PimsReport sub-client; integer PK (id)

        public async Task<ApiResponseDto<List<ReportDto>>> GetAllReportsAsync()
            => await _client.PimsReport.GetAllAsync();

        public async Task<ApiResponseDto<ReportDto>> GetReportByIdAsync(int id)
            => await _client.PimsReport.GetByIdAsync(id);

        public async Task<ApiResponseDto<ReportDto>> CreateReportAsync(ReportDto dto)
            => await _client.PimsReport.CreateAsync(dto);

        public async Task<ApiResponseDto<ReportDto>> UpdateReportAsync(int id, ReportDto dto)
            => await _client.PimsReport.UpdateAsync(id, dto);

        public async Task<ApiResponseDto<bool>> DeleteReportAsync(int id)
            => await _client.PimsReport.DeleteAsync(id);

        // ── ReportGroup ─────────────────────────────────────────────────────────────
        // TRANSFORMENGINE: thin delegates — PimsReportGroup sub-client; integer PK (groupid); also serves as Report dropdown lookup

        public async Task<ApiResponseDto<List<ReportGroupDto>>> GetAllReportGroupsAsync()
            => await _client.PimsReportGroup.GetAllAsync();

        public async Task<ApiResponseDto<ReportGroupDto>> GetReportGroupByIdAsync(int groupid)
            => await _client.PimsReportGroup.GetByIdAsync(groupid);

        public async Task<ApiResponseDto<ReportGroupDto>> CreateReportGroupAsync(ReportGroupDto dto)
            => await _client.PimsReportGroup.CreateAsync(dto);

        public async Task<ApiResponseDto<ReportGroupDto>> UpdateReportGroupAsync(int groupid, ReportGroupDto dto)
            => await _client.PimsReportGroup.UpdateAsync(groupid, dto);

        public async Task<ApiResponseDto<bool>> DeleteReportGroupAsync(int groupid)
            => await _client.PimsReportGroup.DeleteAsync(groupid);

        // ── ReportGroupLink ─────────────────────────────────────────────────────────
        // TRANSFORMENGINE: thin delegates — PimsReportGroupLink sub-client; composite PK (reportid int + groupid int); no PUT

        public async Task<ApiResponseDto<List<ReportGroupLinkDto>>> GetAllReportGroupLinksAsync()
            => await _client.PimsReportGroupLink.GetAllAsync();

        public async Task<ApiResponseDto<List<ReportGroupLinkDto>>> GetReportGroupLinksByReportIdAsync(int reportid)
            => await _client.PimsReportGroupLink.GetByReportIdAsync(reportid);

        public async Task<ApiResponseDto<ReportGroupLinkDto>> GetReportGroupLinkByIdAsync(int reportid, int groupid)
            => await _client.PimsReportGroupLink.GetByIdAsync(reportid, groupid);

        public async Task<ApiResponseDto<ReportGroupLinkDto>> CreateReportGroupLinkAsync(ReportGroupLinkDto dto)
            => await _client.PimsReportGroupLink.CreateAsync(dto);

        public async Task<ApiResponseDto<bool>> DeleteReportGroupLinkAsync(int reportid, int groupid)
            => await _client.PimsReportGroupLink.DeleteAsync(reportid, groupid);

        // ── ProjectManager ──────────────────────────────────────────────────────────
        // TRANSFORMENGINE: thin delegates — PimsProjectManager sub-client; natural varchar PK (projectmanager)

        public async Task<ApiResponseDto<List<ProjectManagerDto>>> GetAllProjectManagersAsync()
            => await _client.PimsProjectManager.GetAllAsync();

        public async Task<ApiResponseDto<ProjectManagerDto>> GetProjectManagerByIdAsync(string projectmanager)
            => await _client.PimsProjectManager.GetByIdAsync(projectmanager);

        public async Task<ApiResponseDto<ProjectManagerDto>> CreateProjectManagerAsync(ProjectManagerDto dto)
            => await _client.PimsProjectManager.CreateAsync(dto);

        public async Task<ApiResponseDto<ProjectManagerDto>> UpdateProjectManagerAsync(string projectmanager, ProjectManagerDto dto)
            => await _client.PimsProjectManager.UpdateAsync(projectmanager, dto);

        public async Task<ApiResponseDto<bool>> DeleteProjectManagerAsync(string projectmanager)
            => await _client.PimsProjectManager.DeleteAsync(projectmanager);

        // ── ProgramManagerLink ──────────────────────────────────────────────────────
        // TRANSFORMENGINE: thin delegates — PimsProgramManagerLink sub-client; composite natural PK (program string + manager string); no PUT

        public async Task<ApiResponseDto<List<ProgramManagerLinkDto>>> GetAllProgramManagerLinksAsync()
            => await _client.PimsProgramManagerLink.GetAllAsync();

        public async Task<ApiResponseDto<List<ProgramManagerLinkDto>>> GetProgramManagerLinksByProgramAsync(string program)
            => await _client.PimsProgramManagerLink.GetByProgramAsync(program);

        public async Task<ApiResponseDto<ProgramManagerLinkDto>> GetProgramManagerLinkByIdAsync(string program, string manager)
            => await _client.PimsProgramManagerLink.GetByIdAsync(program, manager);

        public async Task<ApiResponseDto<ProgramManagerLinkDto>> CreateProgramManagerLinkAsync(ProgramManagerLinkDto dto)
            => await _client.PimsProgramManagerLink.CreateAsync(dto);

        public async Task<ApiResponseDto<bool>> DeleteProgramManagerLinkAsync(string program, string manager)
            => await _client.PimsProgramManagerLink.DeleteAsync(program, manager);

        // ── ProfitCentreManagerLink ─────────────────────────────────────────────────
        // TRANSFORMENGINE: thin delegates — PimsProfitCentreManagerLink sub-client; composite natural PK (profitcentre string + manager string); no PUT

        public async Task<ApiResponseDto<List<ProfitCentreManagerLinkDto>>> GetAllProfitCentreManagerLinksAsync()
            => await _client.PimsProfitCentreManagerLink.GetAllAsync();

        public async Task<ApiResponseDto<List<ProfitCentreManagerLinkDto>>> GetProfitCentreManagerLinksByProfitCentreAsync(string profitcentre)
            => await _client.PimsProfitCentreManagerLink.GetByProfitCentreAsync(profitcentre);

        public async Task<ApiResponseDto<ProfitCentreManagerLinkDto>> GetProfitCentreManagerLinkByIdAsync(string profitcentre, string manager)
            => await _client.PimsProfitCentreManagerLink.GetByIdAsync(profitcentre, manager);

        public async Task<ApiResponseDto<ProfitCentreManagerLinkDto>> CreateProfitCentreManagerLinkAsync(ProfitCentreManagerLinkDto dto)
            => await _client.PimsProfitCentreManagerLink.CreateAsync(dto);

        public async Task<ApiResponseDto<bool>> DeleteProfitCentreManagerLinkAsync(string profitcentre, string manager)
            => await _client.PimsProfitCentreManagerLink.DeleteAsync(profitcentre, manager);

        // ── Setting ─────────────────────────────────────────────────────────────────
        // TRANSFORMENGINE: thin delegates — PimsSetting sub-client; read/update only; string PK; no create/delete

        public async Task<ApiResponseDto<List<SettingDto>>> GetAllSettingsAsync()
            => await _client.PimsSetting.GetAllAsync();

        public async Task<ApiResponseDto<List<SettingDto>>> GetAllUserUpdateableSettingsAsync()
            => await _client.PimsSetting.GetAllUserUpdateableAsync();

        public async Task<ApiResponseDto<SettingDto>> GetSettingByIdAsync(string id)
            => await _client.PimsSetting.GetByIdAsync(id);

        public async Task<ApiResponseDto<SettingDto>> UpdateSettingAsync(string id, SettingDto dto)
            => await _client.PimsSetting.UpdateAsync(id, dto);

        // ── AccessUser ──────────────────────────────────────────────────────────────
        // TRANSFORMENGINE: thin delegates — PimsAccessUser sub-client; composite PK (systemid int + ntlogin string)

        public async Task<ApiResponseDto<List<AccessUserDto>>> GetAllAccessUsersAsync()
            => await _client.PimsAccessUser.GetAllAsync();

        public async Task<ApiResponseDto<List<AccessUserDto>>> GetAccessUsersBySystemIdAsync(int systemid)
            => await _client.PimsAccessUser.GetBySystemIdAsync(systemid);

        public async Task<ApiResponseDto<AccessUserDto>> GetAccessUserByIdAsync(int systemid, string ntlogin)
            => await _client.PimsAccessUser.GetByIdAsync(systemid, ntlogin);

        public async Task<ApiResponseDto<AccessUserDto>> CreateAccessUserAsync(AccessUserDto dto)
            => await _client.PimsAccessUser.CreateAsync(dto);

        public async Task<ApiResponseDto<AccessUserDto>> UpdateAccessUserAsync(int systemid, string ntlogin, AccessUserDto dto)
            => await _client.PimsAccessUser.UpdateAsync(systemid, ntlogin, dto);

        public async Task<ApiResponseDto<bool>> DeleteAccessUserAsync(int systemid, string ntlogin)
            => await _client.PimsAccessUser.DeleteAsync(systemid, ntlogin);

        // ── AccessLevel ─────────────────────────────────────────────────────────────
        // TRANSFORMENGINE: thin delegates — PimsAccessLevel sub-client; composite PK (systemid int + accesslevelid int)

        public async Task<ApiResponseDto<List<AccessLevelDto>>> GetAllAccessLevelsAsync()
            => await _client.PimsAccessLevel.GetAllAsync();

        public async Task<ApiResponseDto<List<AccessLevelDto>>> GetAccessLevelsBySystemIdAsync(int systemid)
            => await _client.PimsAccessLevel.GetBySystemIdAsync(systemid);

        public async Task<ApiResponseDto<AccessLevelDto>> GetAccessLevelByIdAsync(int systemid, int accesslevelid)
            => await _client.PimsAccessLevel.GetByIdAsync(systemid, accesslevelid);

        public async Task<ApiResponseDto<AccessLevelDto>> CreateAccessLevelAsync(AccessLevelDto dto)
            => await _client.PimsAccessLevel.CreateAsync(dto);

        public async Task<ApiResponseDto<AccessLevelDto>> UpdateAccessLevelAsync(int systemid, int accesslevelid, AccessLevelDto dto)
            => await _client.PimsAccessLevel.UpdateAsync(systemid, accesslevelid, dto);

        public async Task<ApiResponseDto<bool>> DeleteAccessLevelAsync(int systemid, int accesslevelid)
            => await _client.PimsAccessLevel.DeleteAsync(systemid, accesslevelid);

        // ── AccessUserLevel ─────────────────────────────────────────────────────────
        // TRANSFORMENGINE: thin delegates — PimsAccessUserLevel sub-client; triple composite PK (systemid int + ntlogin string + accesslevelid int); no PUT

        public async Task<ApiResponseDto<List<AccessUserLevelDto>>> GetAllAccessUserLevelsAsync()
            => await _client.PimsAccessUserLevel.GetAllAsync();

        public async Task<ApiResponseDto<List<AccessUserLevelDto>>> GetAccessUserLevelsBySystemIdAsync(int systemid)
            => await _client.PimsAccessUserLevel.GetBySystemIdAsync(systemid);

        public async Task<ApiResponseDto<List<AccessUserLevelDto>>> GetAccessUserLevelsByUserAsync(int systemid, string ntlogin)
            => await _client.PimsAccessUserLevel.GetByUserAsync(systemid, ntlogin);

        public async Task<ApiResponseDto<AccessUserLevelDto>> GetAccessUserLevelByIdAsync(int systemid, string ntlogin, int accesslevelid)
            => await _client.PimsAccessUserLevel.GetByIdAsync(systemid, ntlogin, accesslevelid);

        public async Task<ApiResponseDto<AccessUserLevelDto>> CreateAccessUserLevelAsync(AccessUserLevelDto dto)
            => await _client.PimsAccessUserLevel.CreateAsync(dto);

        public async Task<ApiResponseDto<bool>> DeleteAccessUserLevelAsync(int systemid, string ntlogin, int accesslevelid)
            => await _client.PimsAccessUserLevel.DeleteAsync(systemid, ntlogin, accesslevelid);

        // ── AccessSystem (lookup — read-only) ───────────────────────────────────────
        // TRANSFORMENGINE: thin delegates — PimsAccessSystem sub-client; reference lookup; integer PK (systemid); no create/update/delete

        public async Task<ApiResponseDto<List<AccessSystemDto>>> GetAllAccessSystemsAsync()
            => await _client.PimsAccessSystem.GetAllAsync();

        public async Task<ApiResponseDto<AccessSystemDto>> GetAccessSystemByIdAsync(int systemid)
            => await _client.PimsAccessSystem.GetByIdAsync(systemid);

        // ── Frequency ───────────────────────────────────────────────────────────────
        // TRANSFORMENGINE: thin delegates — PimsFrequency sub-client; integer PK (frequencyid); full CRUD

        public async Task<ApiResponseDto<List<FrequencyDto>>> GetAllFrequenciesAsync()
            => await _client.PimsFrequency.GetAllAsync();

        public async Task<ApiResponseDto<FrequencyDto>> GetFrequencyByIdAsync(int frequencyid)
            => await _client.PimsFrequency.GetByIdAsync(frequencyid);

        public async Task<ApiResponseDto<FrequencyDto>> CreateFrequencyAsync(FrequencyDto dto)
            => await _client.PimsFrequency.CreateAsync(dto);

        public async Task<ApiResponseDto<FrequencyDto>> UpdateFrequencyAsync(int frequencyid, FrequencyDto dto)
            => await _client.PimsFrequency.UpdateAsync(frequencyid, dto);

        public async Task<ApiResponseDto<bool>> DeleteFrequencyAsync(int frequencyid)
            => await _client.PimsFrequency.DeleteAsync(frequencyid);

        // ── ReviewItem ──────────────────────────────────────────────────────────────
        // TRANSFORMENGINE: thin delegates — PimsReviewItem sub-client; integer PK (itemid); full CRUD; Other Tab lookup

        public async Task<ApiResponseDto<List<ReviewItemDto>>> GetAllReviewItemsAsync()
            => await _client.PimsReviewItem.GetAllAsync();

        public async Task<ApiResponseDto<ReviewItemDto>> GetReviewItemByIdAsync(int itemid)
            => await _client.PimsReviewItem.GetByIdAsync(itemid);

        public async Task<ApiResponseDto<ReviewItemDto>> CreateReviewItemAsync(ReviewItemDto dto)
            => await _client.PimsReviewItem.CreateAsync(dto);

        public async Task<ApiResponseDto<ReviewItemDto>> UpdateReviewItemAsync(int itemid, ReviewItemDto dto)
            => await _client.PimsReviewItem.UpdateAsync(itemid, dto);

        public async Task<ApiResponseDto<bool>> DeleteReviewItemAsync(int itemid)
            => await _client.PimsReviewItem.DeleteAsync(itemid);

        // ── RadTrackProg ────────────────────────────────────────────────────────────
        // TRANSFORMENGINE: thin delegates — PimsRadTrackProg sub-client; natural string PK (program); full CRUD; Programme Tab

        public async Task<ApiResponseDto<List<RadTrackProgDto>>> GetAllRadTrackProgsAsync()
            => await _client.PimsRadTrackProg.GetAllAsync();

        public async Task<ApiResponseDto<RadTrackProgDto>> GetRadTrackProgByIdAsync(string program)
            => await _client.PimsRadTrackProg.GetByIdAsync(program);

        public async Task<ApiResponseDto<RadTrackProgDto>> CreateRadTrackProgAsync(RadTrackProgDto dto)
            => await _client.PimsRadTrackProg.CreateAsync(dto);

        public async Task<ApiResponseDto<RadTrackProgDto>> UpdateRadTrackProgAsync(string program, RadTrackProgDto dto)
            => await _client.PimsRadTrackProg.UpdateAsync(program, dto);

        public async Task<ApiResponseDto<bool>> DeleteRadTrackProgAsync(string program)
            => await _client.PimsRadTrackProg.DeleteAsync(program);
    }
}
