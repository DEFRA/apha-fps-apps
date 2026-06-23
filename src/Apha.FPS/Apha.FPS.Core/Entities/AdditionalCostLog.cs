/*
 * TRANSFORMENGINE MIGRATION — AdditionalCostLog.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 2 — Core Layer - Entities + Repository Interfaces + Pagination
 * Migrated : 2026-06-22
 *
 * CHANGED:
 *   - Added TransformEngine migration header (annotation only — no structural changes)
 *
 * PRESERVED:
 *   - All 11 column mappings verified against fps.additionalcosts_log DDL: sequenceno, jobcode, account,
 *     description, itemcost, freq, supplier, date_time, user_id, insert_delete, fpsyear
 *   - Nullability is correct: Freq?, Supplier?, DateTime?, UserId?, InsertDelete? match DDL nullable columns
 *   - Non-nullable required fields (JobCode, Account, Description, ItemCost, FpsYear) match DDL NOT NULL constraints
 *   - partial class declaration preserved
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: fpsyear partition pruning — ensure FilterFpsYear in FpsDbContext is applied at query time
 */
namespace Apha.FPS.Core.Entities
{
    public partial class AdditionalCostLog
    {
        public int SequenceNo { get; set; }

        public string JobCode { get; set; } = null!;

        public string Account { get; set; } = null!;

        public string Description { get; set; } = null!;

        public decimal ItemCost { get; set; }

        public string? Freq { get; set; }

        public string? Supplier { get; set; }

        public DateTime? DateTime { get; set; }

        public string? UserId { get; set; }

        public string? InsertDelete { get; set; }

        public int FpsYear { get; set; }
    }
}
