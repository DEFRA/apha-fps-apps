/*
 * TRANSFORMENGINE MIGRATION — TestRequirementLog.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 2 — Core Layer - Entities + Repository Interfaces + Pagination
 * Migrated : 2026-06-22
 *
 * CHANGED:
 *   - Added TransformEngine migration header (annotation only — no structural changes)
 *
 * PRESERVED:
 *   - All 13 column mappings verified against fps.testreq_log DDL: sequenceno, testcode, buyer, unitprice, norequired,
 *     projectbuyercode, testbuyercode, active, date_time, user_id, insert_delete, jobcode, fpsyear
 *   - FpsYear is int (NOT NULL in DDL) — correct
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: unitprice is decimal? in entity but double precision in DDL — verify EF map handles PostgreSQL money→decimal conversion correctly
 *   - TRANSFORMENGINE TODO: norequired is double? in entity but integer in DDL — verify EF map handles integer→double mapping or correct to int?
 *   - TRANSFORMENGINE TODO: jobcode is annotated in DDL as 'Generated column based on projectbuyercode' — verify if EF map marks this as a computed column
 *   - TRANSFORMENGINE TODO: fpsyear partition pruning — ensure FilterFpsYear in FpsDbContext is applied at query time
 */
namespace Apha.FPS.Core.Entities
{
    public class TestRequirementLog
    {
        public int SequenceNo { get; set; }
        public string? TestCode { get; set; }
        public string? Buyer { get; set; }
        // TRANSFORMENGINE TODO: unitprice DDL type is double precision — decimal? maps via EF but verify no precision loss
        public decimal? UnitPrice { get; set; }
        // TRANSFORMENGINE TODO: norequired DDL type is integer — double? is widening, verify intent vs int?
        public double? NoRequired { get; set; }
        public string? ProjectBuyerCode { get; set; }
        public string? TestBuyerCode { get; set; }
        public short? Active { get; set; }
        public DateTime? DateTime { get; set; }
        public string? UserId { get; set; }
        public string? InsertDelete { get; set; }
        // TRANSFORMENGINE TODO: jobcode is a DDL-level generated/derived column (based on projectbuyercode) — verify EF map handles ValueGeneratedOnAddOrUpdate or HasComputedColumnSql
        public string? JobCode { get; set; }
        public int FpsYear { get; set; }
    }
}
