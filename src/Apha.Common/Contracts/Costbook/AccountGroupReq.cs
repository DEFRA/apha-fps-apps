/*
 * TRANSFORMENGINE MIGRATION — AccountGroupReq.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 14 — Pre-Build Security Review Gate
 * Migrated : 2026-06-23
 *
 * CHANGED:
 *   - New contract created from MS Access frmMaintainance Tab 3 (CSG7 Inflation Options) modal
 *   - Writable fields mapped from formTblCsg7: modal-csg7-group (text), modal-csg7-useinflation (checkbox)
 *   - Covers POST /api/v1/AccountGroup (create) and PUT /api/v1/AccountGroup/{csg7Group} (update)
 *   - Source table: mabarchive.tblcsg7_accountgroups (csg7group PK varchar(15), useinflation boolean)
 *   - Phase 14 security: added [Required] + [MaxLength(15)] on Csg7Group to enforce DB varchar(15)
 *     constraint at model-binding layer before service call
 *
 * PRESERVED:
 *   - Writable input fields only; PK csg7Group is sent on create and in route on update
 *   - No EF entity or repository concerns in contract
 *   - UseInflation default of true matches DB schema default
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: none — max-length annotation resolved in Phase 14.
 */

using System.ComponentModel.DataAnnotations;

namespace Apha.Common.Contracts.Costbook
{
    // TRANSFORMENGINE: Req contract for POST /api/v1/AccountGroup and PUT /api/v1/AccountGroup/{csg7Group}
    public class AccountGroupReq
    {
        // TRANSFORMENGINE: maps to modal-csg7-group text input; also serves as PK on create
        // TRANSFORMENGINE (Phase 14 security): [Required] + [MaxLength(15)] enforce DB varchar(15) constraint
        /// <summary>CSG7 group name. Acts as the natural key (varchar 15). Required on create; ignored on update (key is in route).</summary>
        [Required]
        [MaxLength(15)]
        public string Csg7Group { get; set; } = string.Empty;

        // TRANSFORMENGINE: maps to modal-csg7-useinflation checkbox — controls whether inflation applies to this CSG7 group
        /// <summary>Whether inflation is applied to costs in this CSG7 group. Defaults to true per DB schema.</summary>
        public bool UseInflation { get; set; } = true;
    }
}
