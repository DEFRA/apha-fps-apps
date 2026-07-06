/*
 * TRANSFORMENGINE MIGRATION — PimsMaintenanceApiDtoMapper.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 10 — AutoMapper Profiles + DI Registration (Step 15)
 * Migrated : 2026-07-06
 *
 * CHANGED:
 *   - New AutoMapper Profile for frmMaintenance (PIMS Admin Maintenance) API contracts
 *   - Maps all 14 maintenance entity Res/Req contracts to/from their frontend DTOs
 *   - Explicit ForMember required for two name mismatches:
 *       FrequencyDto.FrequencyValue <-> FrequencyRes/Req.Frequency
 *       SettingDto.SettingValue     <-> SettingRes/Req.Setting
 *   - All other property differences are case-only (AutoMapper resolves case-insensitively)
 *   - AccessSystem is read-only (Res -> Dto only, no ReverseMap)
 *   - AccessLevel has no Req contract (Res <-> Dto only)
 *   - Link/junction tables (ReportGroupLink, ProgramManagerLink,
 *     ProfitCentreManagerLink, AccessUserLevel) have no PUT — Req contracts are
 *     used for POST (create) only
 *
 * PRESERVED:
 *   - All 14 entity surfaces: Report, ReportGroup, ReportGroupLink,
 *     ProjectManager, ProgramManagerLink, ProfitCentreManagerLink, Setting,
 *     AccessUser, AccessLevel, AccessUserLevel, AccessSystem, Frequency,
 *     ReviewItem, RadTrackProg
 *   - Composite and natural-PK shapes preserved (no flattening)
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: verify AutoMapper resolves AccessLevelRes.AccessLevel
 *     (nullable string) -> AccessLevelDto.Accesslevel correctly at runtime — both
 *     are nullable but naming differs only in case, which should be fine
 *   - TRANSFORMENGINE TODO: if a future AccessLevelReq contract is added, register
 *     CreateMap<AccessLevelDto, AccessLevelReq>().ReverseMap() here
 */

using Apha.Common.Contracts.PIMS;
using Apha.FPSApps.Application.Dtos.PIMS;
using AutoMapper;

namespace Apha.FPSApps.Infrastructure.Mappings
{
    public class PimsMaintenanceApiDtoMapper : Profile
    {
        public PimsMaintenanceApiDtoMapper()
        {
            // ── Report ──────────────────────────────────────────────────────────────
            // TRANSFORMENGINE: ReportRes <-> ReportDto; all property diffs are case-only
            CreateMap<ReportRes, ReportDto>().ReverseMap();
            // TRANSFORMENGINE: ReportDto -> ReportReq (POST/PUT body; Id excluded from Req)
            CreateMap<ReportDto, ReportReq>().ReverseMap();

            // ── ReportGroup ─────────────────────────────────────────────────────────
            // TRANSFORMENGINE: ReportGroupRes <-> ReportGroupDto; also used as lookup dropdown source
            CreateMap<ReportGroupRes, ReportGroupDto>().ReverseMap();
            // TRANSFORMENGINE: ReportGroupDto -> ReportGroupReq (only Description is writable; GroupId is IDENTITY)
            CreateMap<ReportGroupDto, ReportGroupReq>().ReverseMap();

            // ── ReportGroupLink ─────────────────────────────────────────────────────
            // TRANSFORMENGINE: composite PK (Reportid+Groupid); no PUT — Req used for POST only
            CreateMap<ReportGroupLinkRes, ReportGroupLinkDto>().ReverseMap();
            CreateMap<ReportGroupLinkDto, ReportGroupLinkReq>().ReverseMap();

            // ── ProjectManager ──────────────────────────────────────────────────────
            // TRANSFORMENGINE: natural varchar PK (Projectmanager); all diffs are case-only
            CreateMap<ProjectManagerRes, ProjectManagerDto>().ReverseMap();
            CreateMap<ProjectManagerDto, ProjectManagerReq>().ReverseMap();

            // ── ProgramManagerLink ──────────────────────────────────────────────────
            // TRANSFORMENGINE: composite natural PK (Program+Manager); no PUT — Req used for POST only
            CreateMap<ProgramManagerLinkRes, ProgramManagerLinkDto>().ReverseMap();
            CreateMap<ProgramManagerLinkDto, ProgramManagerLinkReq>().ReverseMap();

            // ── ProfitCentreManagerLink ─────────────────────────────────────────────
            // TRANSFORMENGINE: composite natural PK (ProfitCentre+Manager); no PUT
            // ProfitCentreManagerLinkRes.ProfitCentre <-> ProfitCentreManagerLinkDto.Profitcentre — case-insensitive match
            CreateMap<ProfitCentreManagerLinkRes, ProfitCentreManagerLinkDto>().ReverseMap();
            CreateMap<ProfitCentreManagerLinkDto, ProfitCentreManagerLinkReq>().ReverseMap();

            // ── Setting ─────────────────────────────────────────────────────────────
            // TRANSFORMENGINE: read/update only (no create/delete); natural varchar PK
            // EXPLICIT ForMember required: SettingRes.Setting <-> SettingDto.SettingValue (name mismatch)
            CreateMap<SettingRes, SettingDto>()
                .ForMember(dest => dest.SettingValue, opt => opt.MapFrom(src => src.Setting))
                .ReverseMap()
                .ForMember(dest => dest.Setting, opt => opt.MapFrom(src => src.SettingValue));

            // TRANSFORMENGINE: SettingReq.Setting <-> SettingDto.SettingValue (same name mismatch as Res)
            CreateMap<SettingDto, SettingReq>()
                .ForMember(dest => dest.Setting, opt => opt.MapFrom(src => src.SettingValue))
                .ReverseMap()
                .ForMember(dest => dest.SettingValue, opt => opt.MapFrom(src => src.Setting));

            // ── AccessUser ──────────────────────────────────────────────────────────
            // TRANSFORMENGINE: composite PK (Systemid+Ntlogin); all diffs are case-only
            CreateMap<AccessUserRes, AccessUserDto>().ReverseMap();
            CreateMap<AccessUserDto, AccessUserReq>().ReverseMap();

            // ── AccessLevel ─────────────────────────────────────────────────────────
            // TRANSFORMENGINE: composite PK (Systemid+Accesslevelid); no AccessLevelReq contract exists
            // Res <-> Dto only; all diffs are case-only
            CreateMap<AccessLevelRes, AccessLevelDto>().ReverseMap();

            // ── AccessUserLevel ─────────────────────────────────────────────────────
            // TRANSFORMENGINE: triple composite PK (Systemid+Ntlogin+Accesslevelid); no PUT
            CreateMap<AccessUserLevelRes, AccessUserLevelDto>().ReverseMap();
            CreateMap<AccessUserLevelDto, AccessUserLevelReq>().ReverseMap();

            // ── AccessSystem (read-only lookup) ─────────────────────────────────────
            // TRANSFORMENGINE: reference lookup data — Res -> Dto only; no Req contract
            CreateMap<AccessSystemRes, AccessSystemDto>();

            // ── Frequency ───────────────────────────────────────────────────────────
            // TRANSFORMENGINE: integer PK (Frequencyid); full CRUD
            // EXPLICIT ForMember required: FrequencyRes.Frequency <-> FrequencyDto.FrequencyValue (name mismatch)
            CreateMap<FrequencyRes, FrequencyDto>()
                .ForMember(dest => dest.FrequencyValue, opt => opt.MapFrom(src => src.Frequency))
                .ReverseMap()
                .ForMember(dest => dest.Frequency, opt => opt.MapFrom(src => src.FrequencyValue));

            // TRANSFORMENGINE: FrequencyReq.Frequency <-> FrequencyDto.FrequencyValue (same name mismatch as Res)
            CreateMap<FrequencyDto, FrequencyReq>()
                .ForMember(dest => dest.Frequency, opt => opt.MapFrom(src => src.FrequencyValue))
                .ReverseMap()
                .ForMember(dest => dest.FrequencyValue, opt => opt.MapFrom(src => src.Frequency));

            // ── ReviewItem ──────────────────────────────────────────────────────────
            // TRANSFORMENGINE: integer PK (Itemid); full CRUD; Other Tab lookup
            // ReviewItemRes.ItemId <-> ReviewItemDto.Itemid — case-insensitive match
            CreateMap<ReviewItemRes, ReviewItemDto>().ReverseMap();
            CreateMap<ReviewItemDto, ReviewItemReq>().ReverseMap();

            // ── RadTrackProg ────────────────────────────────────────────────────────
            // TRANSFORMENGINE: natural string PK (Program); full CRUD; Programme Tab
            // RadTrackProgRes.RadTrackProg <-> RadTrackProgDto.Radtrackprog — case-insensitive match
            CreateMap<RadTrackProgRes, RadTrackProgDto>().ReverseMap();
            CreateMap<RadTrackProgDto, RadTrackProgReq>().ReverseMap();
        }
    }
}
