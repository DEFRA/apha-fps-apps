/*
 * TRANSFORMENGINE MIGRATION — DepartmentIncomeQueryTypeEnum.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 1 — Apha.Common - Shared Contracts (Step 1)
 * Migrated : 2026-07-10
 *
 * CHANGED:
 *   - New enum created to replace implicit VBA query-type branching in frmDepartmentIncome form
 *   - Values align with the five distinct qryDeptIncome* MS Access queries:
 *     Time=1, Tests=2, Animals=3, Additional=4, Totals=5
 *
 * PRESERVED:
 *   - Semantic ordering matches the tab/option sequence in the legacy Access form
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: confirm integer values match any persisted query-type references in stored config or URL bookmarks
 */

namespace Apha.Common.Contracts.FPS
{
    // TRANSFORMENGINE: Enum replaces VBA conditional branching across five qryDeptIncome* Access queries
    public enum DepartmentIncomeQueryType
    {
        Time       = 1,
        Tests      = 2,
        Animals    = 3,
        Additional = 4,
        Totals     = 5
    }
}
