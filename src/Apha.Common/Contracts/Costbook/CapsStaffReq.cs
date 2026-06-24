/*
 * TRANSFORMENGINE MIGRATION — CapsStaffReq.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 14 — Pre-Build Security Review Gate
 * Migrated : 2026-06-23
 *
 * CHANGED:
 *   - New contract created from MS Access frmMaintainance Tab 5 (CAPS Staff) modal (formTblCapsStaff)
 *   - Writable fields mapped from: modal-capsstaff-mnumber (text), modal-capsstaff-name (text)
 *   - Covers POST /api/v1/CapsStaff (create) and PUT /api/v1/CapsStaff/{mNumber} (update)
 *   - Source table: mabarchive.tblcapsstaff (mnumber PK varchar(50), name varchar(50), dt2number varchar(50))
 *   - Dt2Number is not shown in the HTML prototype modal — excluded from request contract
 *   - Phase 14 security: added [Required] + [MaxLength(50)] on MNumber and [MaxLength(50)] on Name
 *     to enforce DB varchar(50) constraints at model-binding layer before service call
 *
 * PRESERVED:
 *   - Writable input fields only; MNumber is PK on create and in route on update
 *   - No EF entity or repository concerns in contract
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Confirm whether Dt2Number should be exposed as a writable field (not present in HTML prototype modal)
 */

using System.ComponentModel.DataAnnotations;

namespace Apha.Common.Contracts.Costbook
{
    // TRANSFORMENGINE: Req contract for POST /api/v1/CapsStaff and PUT /api/v1/CapsStaff/{mNumber}
    public class CapsStaffReq
    {
        // TRANSFORMENGINE: maps to modal-capsstaff-mnumber text input; serves as PK on create; in route on update
        // TRANSFORMENGINE (Phase 14 security): [Required] + [MaxLength(50)] enforce DB varchar(50) constraint
        /// <summary>Staff member M-Number identifier. Required on create; ignored on update (key is in route).</summary>
        [Required]
        [MaxLength(50)]
        public string MNumber { get; set; } = string.Empty;

        // TRANSFORMENGINE: maps to modal-capsstaff-name text input
        // TRANSFORMENGINE (Phase 14 security): [MaxLength(50)] enforces DB varchar(50) constraint
        /// <summary>Staff member full name.</summary>
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;
    }
}
