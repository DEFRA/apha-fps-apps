// TRANSFORMENGINE: human_review — verify before running

/*
 * TRANSFORMENGINE MIGRATION — WorkGroupEmployeeRes.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 1 — Apha.Common - Shared Contracts (Step 1)
 *            Phase 6 — Backend Readiness Gate - Route + Contract + Mapper Confirmation
 * Migrated : 2026-06-11
 *
 * CHANGED:
 *   - Added TimeRecorder (int) field to match full API output surface required by CRUD responses
 *   - Added StartDate (DateTime?) field to match full API output surface required by CRUD responses
 *   - Added EndDate (DateTime?) field to match full API output surface required by CRUD responses
 *   - Added HoursPerWeek (double?) field to match full API output surface required by CRUD responses
 *
 * PRESERVED:
 *   - All original RecordSource-surface fields: PactId, SpNumber, WorkGroupGrade, Name,
 *     PersonStatus, PersonClass, HrsPaid, Leave, SickSpecial, HrsAvail, MakeAvailable
 *   - Namespace Apha.Common.Contracts.FPS
 *   - Res model scope: full API output surface; no EF entity or repository concerns
 *
 * PHASE 6 GATE — VERIFIED 2026-06-11:
 *   Contract role    : Response body for GET /api/v1/wgstaff (list, inside PaginationRes<WorkGroupEmployeeRes>),
 *                      GET /api/v1/wgstaff/{pactId} (single), POST /api/v1/wgstaff (201 body),
 *                      and PUT /api/v1/wgstaff (200 body)
 *   Field coverage   : All 15 response fields present:
 *                        PactId, SpNumber, WorkGroupGrade, Name, PersonStatus, PersonClass,
 *                        HrsPaid, Leave, SickSpecial, HrsAvail, MakeAvailable (original 11)
 *                        TimeRecorder, StartDate, EndDate, HoursPerWeek (Phase 1 additions)
 *   HrsAvail         : Present in Res only (computed server-side = HrsPaid - Leave - SickSpecial);
 *                      intentionally absent from Req
 *   Mapper coverage  : RequestMapper.cs CreateMap<WorkGroupEmployeeDto, WorkGroupEmployeeRes>().ReverseMap()
 *                      resolves all 15 fields by AutoMapper name convention — no ForMember needed
 *   Lookup endpoints : No separate lookup response contract needed for this form
 *   Frontend binding : Frontend reads this contract in grid display (WorkGroupEmployeeItem) and
 *                      Edit modal pre-population; all 15 fields must be mapped in FpsApiDtoMapper
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Confirm TimeRecorder int column name/alias in the RecordSource query
 *     or stored procedure that backs this response
 *   - TRANSFORMENGINE TODO: Confirm StartDate and EndDate nullable semantics match what the
 *     backend query returns (NULL vs min-date convention)
 *   - TRANSFORMENGINE TODO: Confirm HoursPerWeek nullable double matches the source column type;
 *     verify no precision loss vs. the stored procedure output
 */

namespace Apha.Common.Contracts.FPS
{
    public class WorkGroupEmployeeRes
    {
        // TRANSFORMENGINE: original RecordSource-surface fields preserved verbatim
        public string PactId { get; set; } = null!;
        public string SpNumber { get; set; } = null!;
        public string WorkGroupGrade { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string PersonStatus { get; set; } = null!;
        public string? PersonClass { get; set; }
        public double HrsPaid { get; set; }
        public double Leave { get; set; }
        public double SickSpecial { get; set; }
        public double HrsAvail { get; set; }
        public int MakeAvailable { get; set; }

        // TRANSFORMENGINE: new fields added per Phase 1 plan — TimeRecorder, StartDate, EndDate, HoursPerWeek
        public int TimeRecorder { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public double? HoursPerWeek { get; set; }
    }
}
