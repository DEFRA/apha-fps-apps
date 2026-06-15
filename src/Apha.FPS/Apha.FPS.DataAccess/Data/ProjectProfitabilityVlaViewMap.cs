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
