// TRANSFORMENGINE: human_review — verify before running

/*
 * TRANSFORMENGINE MIGRATION — WorkGroupEmployeeReq.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 1 — Apha.Common - Shared Contracts (Step 1)
 *            Phase 6 — Backend Readiness Gate - Route + Contract + Mapper Confirmation
 * Migrated : 2026-06-11
 *
 * CHANGED:
 *   - Added TimeRecorder (int) field to support Create/Update operations aligned with form prototype
 *   - Added StartDate (DateTime?) field to support Create/Update operations aligned with form prototype
 *   - Added EndDate (DateTime?) field to support Create/Update operations aligned with form prototype
 *   - Added HoursPerWeek (double?) field to support Create/Update operations aligned with form prototype
 *
 * PRESERVED:
 *   - All original writable ControlSource-bound fields: PactId, HrsPaid, Leave, SickSpecial,
 *     PersonStatus, PersonClass, MakeAvailable
 *   - Namespace Apha.Common.Contracts.FPS
 *   - Req model scope: writable input fields only, no response-only or EF entity members
 *
 * PHASE 6 GATE — VERIFIED 2026-06-11:
 *   Contract role    : POST (create) and PUT (update) request body for POST /api/v1/wgstaff and
 *                      PUT /api/v1/wgstaff — consumed by WorkGroupEmployeeController.CreateWorkGroupEmployeeAsync
 *                      and WorkGroupEmployeeController.UpdateWorkGroupEmployeeAsync
 *   Field coverage   : All 11 writable prototype fields present:
 *                        PactId, HrsPaid, Leave, SickSpecial, PersonStatus, PersonClass, MakeAvailable (original)
 *                        TimeRecorder, StartDate, EndDate, HoursPerWeek (Phase 1 additions)
 *   Mapper coverage  : RequestMapper.cs CreateMap<WorkGroupEmployeeDto, WorkGroupEmployeeReq>().ReverseMap()
 *                      resolves all fields by AutoMapper name convention — no ForMember needed
 *   HrsAvail         : Intentionally absent from Req (computed server-side); present only in Res
 *   Lookup endpoints : No lookup contract needed — wgGrade is a query param, not a field in Req
 *   Frontend binding : Frontend POSTs/PUTs this contract; all 11 fields must be bound in Add/Edit forms
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Confirm TimeRecorder maps to the correct int column in the WorkGroupEmployee
 *     source form / stored procedure (verify ControlSource binding name)
 *   - TRANSFORMENGINE TODO: Confirm StartDate and EndDate nullable semantics match the form's
 *     allow-null behaviour for those date fields
 *   - TRANSFORMENGINE TODO: Confirm HoursPerWeek nullable double matches the source form field type
 */

namespace Apha.Common.Contracts.FPS
{
    public class WorkGroupEmployeeReq
    {
        // TRANSFORMENGINE: original writable ControlSource-bound fields preserved verbatim
        public string PactId { get; set; } = null!;
        public double HrsPaid { get; set; }
        public double Leave { get; set; }
        public double SickSpecial { get; set; }
        public string PersonStatus { get; set; } = null!;
        public string? PersonClass { get; set; }
        public int MakeAvailable { get; set; }

        // TRANSFORMENGINE: new fields added per Phase 1 plan — TimeRecorder, StartDate, EndDate, HoursPerWeek
        public int TimeRecorder { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public double? HoursPerWeek { get; set; }
    }
}
