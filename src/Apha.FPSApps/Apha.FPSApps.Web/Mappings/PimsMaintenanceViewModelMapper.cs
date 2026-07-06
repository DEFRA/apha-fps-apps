/*
 * TRANSFORMENGINE MIGRATION — PimsMaintenanceViewModelMapper.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 11 — ViewModels + MVC Controller (Steps 16-17)
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - Phase 11: CreateMap entries added for all 12 Maintenance Item types created in this phase
 *   - Maps DTO ↔ Item for grid round-trips and DTO → SettingItem for Time Tab binding
 *   - Explicit ForMember for FrequencyItem.FrequencyValue ↔ FrequencyDto.FrequencyValue (same name,
 *     preserved from ApiDtoMapper; no custom mapping needed here)
 *   - AccessUserLevel CreateMap uses all three composite-PK fields; no ReverseMap on AccessSystemDto
 *     (read-only lookup surface)
 *
 * PRESERVED:
 *   - Profile is registered with AutoMapper via DI in Phase 10 — no further DI changes required
 *   - Pattern matches existing PimsViewModelMapper.cs structure
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: ReportGroupLinkItem and AccessLevelItem are not in scope for Phase 11
 *     batch; CreateMap entries for those types should be added when their Item classes are created
 *   - TRANSFORMENGINE TODO: AccessUserLevelItem.AccessLevelName is a display-only field populated
 *     by the controller from a lookup — not mapped from DTO; confirm this is handled correctly
 *     at controller level and not expected from AutoMapper
 */

using Apha.FPSApps.Application.Dtos.PIMS;
using Apha.FPSApps.Web.Areas.PIMS.Models;
using AutoMapper;

namespace Apha.FPSApps.Web.Mappings
{
    // TRANSFORMENGINE: Maintenance mapper profile — Phase 11 CreateMap entries added for all Item types
    public class PimsMaintenanceViewModelMapper : Profile
    {
        public PimsMaintenanceViewModelMapper()
        {
            // ── Reports Tab ──────────────────────────────────────────────────────────

            // TRANSFORMENGINE: ReportItem ↔ ReportDto — all boolean Allowpick* fields auto-mapped by name
            CreateMap<ReportItem, ReportDto>().ReverseMap();

            // TRANSFORMENGINE: ReportGroupItem ↔ ReportGroupDto — Groupid + Description
            CreateMap<ReportGroupItem, ReportGroupDto>().ReverseMap();

            // ── Programme Tab ────────────────────────────────────────────────────────

            // TRANSFORMENGINE: RadTrackProgItem ↔ RadTrackProgDto — Program (PK), Radtrackprog, Publicationprefix
            CreateMap<RadTrackProgItem, RadTrackProgDto>().ReverseMap();

            // ── Manager Tab ──────────────────────────────────────────────────────────

            // TRANSFORMENGINE: ProjectManagerItem ↔ ProjectManagerDto — Projectmanager (PK), Email, Mnumber, Disable
            CreateMap<ProjectManagerItem, ProjectManagerDto>().ReverseMap();

            // TRANSFORMENGINE: ProgramManagerLinkItem ↔ ProgramManagerLinkDto — composite PK: Program + Manager
            CreateMap<ProgramManagerLinkItem, ProgramManagerLinkDto>().ReverseMap();

            // TRANSFORMENGINE: ProfitCentreManagerLinkItem ↔ ProfitCentreManagerLinkDto — composite PK: Profitcentre + Manager
            CreateMap<ProfitCentreManagerLinkItem, ProfitCentreManagerLinkDto>().ReverseMap();

            // ── Time Tab ─────────────────────────────────────────────────────────────

            // TRANSFORMENGINE: SettingItem ↔ SettingDto — Id (PK), SettingValue, Notes, Testsetting, Userupdateable
            CreateMap<SettingItem, SettingDto>().ReverseMap();

            // ── Admin Maintenance Tab ────────────────────────────────────────────────

            // TRANSFORMENGINE: AccessUserItem ↔ AccessUserDto — composite PK: Systemid + Ntlogin
            CreateMap<AccessUserItem, AccessUserDto>().ReverseMap();

            // TRANSFORMENGINE: AccessUserLevelItem ↔ AccessUserLevelDto — triple composite PK: Systemid + Ntlogin + Accesslevelid
            // AccessLevelName is display-only (not in DTO); it is populated by the controller
            CreateMap<AccessUserLevelItem, AccessUserLevelDto>()
                .ForMember(dest => dest.Systemid, opt => opt.MapFrom(src => src.Systemid))
                .ForMember(dest => dest.Ntlogin, opt => opt.MapFrom(src => src.Ntlogin))
                .ForMember(dest => dest.Accesslevelid, opt => opt.MapFrom(src => src.Accesslevelid));
            CreateMap<AccessUserLevelDto, AccessUserLevelItem>()
                .ForMember(dest => dest.AccessLevelName, opt => opt.Ignore()); // populated by controller

            // ── Other Tab ────────────────────────────────────────────────────────────

            // TRANSFORMENGINE: FrequencyItem ↔ FrequencyDto — Frequencyid (PK), FrequencyValue
            CreateMap<FrequencyItem, FrequencyDto>().ReverseMap();

            // TRANSFORMENGINE: ReviewItemItem ↔ ReviewItemDto — Itemid (PK), Item
            CreateMap<ReviewItemItem, ReviewItemDto>().ReverseMap();
        }
    }
}
