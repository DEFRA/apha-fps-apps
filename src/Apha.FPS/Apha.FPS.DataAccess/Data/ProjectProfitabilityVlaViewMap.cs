// TRANSFORMENGINE: human_review — verify before running

/*
 * TRANSFORMENGINE MIGRATION — ProjectProfitabilityVlaViewMap.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 4 — DataAccess Layer - DbContext + Map Files + Repository (Steps 7-7a)
 * Migrated : 2026-06-15
 *
 * CHANGED:
 *   - New file: IEntityTypeConfiguration<ProjectProfitabilityVlaView> for the
 *     vprojectprofitabilityvla PostgreSQL view in the fps schema.
 *   - HasNoKey() — keyless view; no primary key defined on the PostgreSQL view.
 *   - ToView("vprojectprofitabilityvla", "fps") — lowercase per Phase 4 rules.
 *   - All 15 entity properties mapped with lowercase HasColumnName() per rules.
 *   - Financial decimal columns (StaffCosts, TestCost, AnimalCosts,
 *     AdditionalCosts, TotalCosts, Budget, Profit, TargetProfit, OffTarget)
 *     mapped with HasColumnType("money") to align with FPS schema convention.
 *   - String dimension columns (JobCode, Program, Customer, Manager, Status)
 *     mapped with HasMaxLength() derived from tlkpProject / tlkpProgram DDL
 *     used in existing ProjectViewMap / ProgramViewMap for consistency.
 *   - Id mapped as optional int; verify whether the view exposes ROW_NUMBER()
 *     or a surrogate id column.
 *   - No HasQueryFilter — entity has no FpsYear column; year scoping is
 *     expected to be embedded in the view definition itself via its join to
 *     tlkpProject (which is year-scoped in the fps schema).
 *
 * PRESERVED:
 *   - Property-to-column mapping aligned with ProjectProfitabilityVlaView
 *     entity (Phase 2) and ProjectProfitabilityVlaRes contract (Phase 1).
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: confirm view name — vprojectprofitabilityvla must
 *     exist in the fps schema before this mapping is active; view may need
 *     creating from qryJobCodeTotals + qryJobCodeTotals2 aggregation logic.
 *   - TRANSFORMENGINE TODO: confirm Id column — remove Id mapping if the
 *     PostgreSQL view has no id / row_number column; the Id property on the
 *     entity is nullable (int?) and is only present to satisfy the Res contract.
 *   - TRANSFORMENGINE TODO: confirm column name for Status — mapped to
 *     "projectstatus" based on tlkpProject.ProjectStatus source field; rename
 *     to "status" if the view aliases the column differently.
 *   - TRANSFORMENGINE TODO: confirm all remaining column names match the final
 *     PostgreSQL view DDL — particularly jobcode vs parentproject for the
 *     project code column.
 *   - TRANSFORMENGINE TODO: confirm decimal column types — money vs numeric(x,y)
 *     per final view DDL; replace HasColumnType("money") if the view exposes
 *     numeric types for cost columns.
 *   - TRANSFORMENGINE TODO: confirm Budget nullability — if the view column is
 *     defined NOT NULL, update entity property from decimal? to decimal and
 *     remove nullable mapping here.
 *   - TRANSFORMENGINE TODO: confirm FpsYear scoping — if the view does NOT
 *     embed year filtering internally, add a year-filter column to the entity
 *     and apply HasQueryFilter in FpsDbContext.
 */

using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    /// <summary>
    /// EF Core entity type configuration for <see cref="ProjectProfitabilityVlaView"/>.
    /// Maps to the <c>vprojectprofitabilityvla</c> PostgreSQL view in the <c>fps</c> schema.
    /// </summary>
    public class ProjectProfitabilityVlaViewMap : IEntityTypeConfiguration<ProjectProfitabilityVlaView>
    {
        public void Configure(EntityTypeBuilder<ProjectProfitabilityVlaView> entity)
        {
            // TRANSFORMENGINE: keyless — aggregation view; no PK; maps to vprojectprofitabilityvla in fps schema
            entity
                .HasNoKey()
                .ToView("vprojectprofitabilityvla", "fps");

            // TRANSFORMENGINE: optional numeric row identifier — verify view exposes ROW_NUMBER() or surrogate id;
            //   remove this mapping if the view has no id column
            entity.Property(e => e.Id)
                .HasColumnName("id");

            // TRANSFORMENGINE: JobCode maps qryJobCodeTotals.JobCode (tlkpProject.ParentProject)
            entity.Property(e => e.JobCode)
                .HasMaxLength(20)
                .HasColumnName("jobcode");

            // TRANSFORMENGINE: Program maps qryJobCodeTotals.Program (tlkpProject.Program / ProgramNo)
            entity.Property(e => e.Program)
                .HasMaxLength(10)
                .HasColumnName("program");

            // TRANSFORMENGINE: Customer maps qryJobCodeTotals.Customer (tlkpProject.Customer) — VLA-specific
            entity.Property(e => e.Customer)
                .HasMaxLength(50)
                .HasColumnName("customer");

            // TRANSFORMENGINE: Manager maps qryJobCodeTotals2.Manager (tlkpProgram.Manager) — VLA-specific
            entity.Property(e => e.Manager)
                .HasMaxLength(50)
                .HasColumnName("manager");

            // TRANSFORMENGINE: Status maps qryJobCodeTotals.ProjectStatus (tlkpProject.ProjectStatus);
            //   column name "projectstatus" — rename to "status" if the view aliases it
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasColumnName("projectstatus");

            // ── Financial columns — money type aligned with FPS schema convention ──────────────────

            // TRANSFORMENGINE: StaffCosts maps JCTotalStaffCosts from qryJobCodeTotals
            entity.Property(e => e.StaffCosts)
                .HasColumnType("money")
                .HasColumnName("staffcosts");

            // TRANSFORMENGINE: TestCost maps JCTotalTestCosts from qryJobCodeTotals
            entity.Property(e => e.TestCost)
                .HasColumnType("money")
                .HasColumnName("testcost");

            // TRANSFORMENGINE: AnimalCosts maps JCTotalAnimalCosts from qryJobCodeTotals
            entity.Property(e => e.AnimalCosts)
                .HasColumnType("money")
                .HasColumnName("animalcosts");

            // TRANSFORMENGINE: AdditionalCosts maps JCTotalAdditionalCosts from qryJobCodeTotals
            entity.Property(e => e.AdditionalCosts)
                .HasColumnType("money")
                .HasColumnName("additionalcosts");

            // TRANSFORMENGINE: TotalCosts = JCTotalAnimalCosts + JCTotalAdditionalCosts + JCTotalStaffCosts + JCTotalTestCosts
            entity.Property(e => e.TotalCosts)
                .HasColumnType("money")
                .HasColumnName("totalcosts");

            // TRANSFORMENGINE: Budget maps Budget_CVL (tlkpProject.Budget_CVL); nullable — may be NULL in source data
            entity.Property(e => e.Budget)
                .HasColumnType("money")
                .HasColumnName("budget");

            // TRANSFORMENGINE: Profit maps JCProfit = Budget_CVL - TotalCosts
            entity.Property(e => e.Profit)
                .HasColumnType("money")
                .HasColumnName("profit");

            // TRANSFORMENGINE: TargetProfit maps tlkpProgram.Target (via qryJobCodeTotals2)
            entity.Property(e => e.TargetProfit)
                .HasColumnType("money")
                .HasColumnName("targetprofit");

            // TRANSFORMENGINE: OffTarget = Profit - TargetProfit; computed column in view or Application layer
            entity.Property(e => e.OffTarget)
                .HasColumnType("money")
                .HasColumnName("offtarget");
        }
    }
}
