/*
 * TRANSFORMENGINE MIGRATION — ProjectLogMap.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 4 — DataAccess Layer - DbContext + Map Files + Repository
 * Migrated : 2026-06-22
 *
 * CHANGED:
 *   - FIXED: HasKey changed from single-column (SequenceNo) to composite (SequenceNo, FpsYear)
 *     — DDL defines CONSTRAINT pk_project_log PRIMARY KEY (sequenceno, fpsyear); partition key must be part of PK
 *   - FIXED: SequenceNo.ValueGeneratedOnAdd() added — DDL: sequenceno GENERATED ALWAYS AS IDENTITY
 *   - FIXED: InsertDelete IsFixedLength() added — DDL: insert_delete character(2) (fixed-length char, not varchar)
 *   - Added migration annotation header
 *
 * PRESERVED:
 *   - All 41 column HasColumnName() mappings (lowercase) verified against DDL
 *   - ToTable("project_log", "fps") — lowercase preserved
 *   - All HasColumnType("timestamp without time zone") declarations
 *   - All HasMaxLength() declarations
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: caseworksub is numeric(5,4) in DDL — verify decimal precision is handled correctly by EF default
 *   - TRANSFORMENGINE TODO: transferincome, custincome, wip_eoy etc. are PostgreSQL money type — verify EF handles
 *     money → decimal conversion without precision loss (HasColumnType("money") may be required)
 */
using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    public class ProjectLogMap : IEntityTypeConfiguration<ProjectLog>
    {
        public void Configure(EntityTypeBuilder<ProjectLog> entity)
        {
            // TRANSFORMENGINE: composite PK fixed — DDL CONSTRAINT pk_project_log PRIMARY KEY (sequenceno, fpsyear)
            entity.HasKey(e => new { e.SequenceNo, e.FpsYear }).HasName("pk_project_log");

            entity.ToTable("project_log", "fps");

            // TRANSFORMENGINE: ValueGeneratedOnAdd added — DDL: sequenceno GENERATED ALWAYS AS IDENTITY
            entity.Property(e => e.SequenceNo)
                .ValueGeneratedOnAdd()
                .HasColumnName("sequenceno");
            entity.Property(e => e.ParentProject).HasMaxLength(20).HasColumnName("parentproject");
            entity.Property(e => e.ProjectTitle).HasColumnName("projecttitle");
            entity.Property(e => e.Program).HasColumnName("program");
            entity.Property(e => e.Customer).HasColumnName("customer");
            entity.Property(e => e.Manager).HasColumnName("manager");
            entity.Property(e => e.TransferIncome).HasColumnName("transferincome");
            entity.Property(e => e.CustIncome).HasColumnName("custincome");
            entity.Property(e => e.WipEoy).HasColumnName("wip_eoy");
            entity.Property(e => e.WipLimit).HasColumnName("wip_limit");
            entity.Property(e => e.WipCurrent).HasColumnName("wip_current");
            entity.Property(e => e.ProjectStatus).HasColumnName("projectstatus");
            entity.Property(e => e.CostBookNo).HasColumnName("costbookno");
            entity.Property(e => e.DateCreated)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("datecreated");
            entity.Property(e => e.FecCost).HasColumnName("feccost");
            entity.Property(e => e.Profit).HasColumnName("profit");
            entity.Property(e => e.BudgetCvl).HasColumnName("budget_cvl");
            entity.Property(e => e.DateCosted)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("datecosted");
            entity.Property(e => e.Disease).HasColumnName("disease");
            entity.Property(e => e.Contract).HasColumnName("contract");
            entity.Property(e => e.ProjectParent).HasColumnName("projectparent");
            entity.Property(e => e.ShortTitle).HasColumnName("shorttitle");
            entity.Property(e => e.CaseWorkSub).HasColumnName("caseworksub");
            entity.Property(e => e.PvsIncome).HasColumnName("pvsincome");
            entity.Property(e => e.PlanCaseWorkDebit).HasColumnName("plancaseworkdebit");
            entity.Property(e => e.Finished).HasColumnName("finished");
            entity.Property(e => e.OwningRc).HasColumnName("owningrc");
            entity.Property(e => e.Comments).HasColumnName("comments");
            entity.Property(e => e.CarryOver).HasColumnName("carryover");
            entity.Property(e => e.CarryOverSeed).HasColumnName("carryoverseed");
            entity.Property(e => e.DateTime)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("date_time");
            entity.Property(e => e.UserId).HasMaxLength(255).HasColumnName("user_id");
            // TRANSFORMENGINE: IsFixedLength added — DDL: insert_delete character(2) (char not varchar)
            entity.Property(e => e.InsertDelete)
                .HasMaxLength(2)
                .IsFixedLength()
                .HasColumnName("insert_delete");
            entity.Property(e => e.JobCode).HasMaxLength(20).HasColumnName("jobcode");
            entity.Property(e => e.IsDefraProject).HasColumnName("isdefraproject");
            entity.Property(e => e.CostCentre).HasColumnName("costcentre");
            entity.Property(e => e.OracleProjectCode).HasColumnName("oracleprojectcode");
            entity.Property(e => e.SubAccountCode).HasColumnName("subaccountcode");
            entity.Property(e => e.ProjectGroup).HasColumnName("projectgroup");
            entity.Property(e => e.IncomeAccountCode).HasColumnName("incomeaccountcode");
            entity.Property(e => e.FpsYear).HasColumnName("fpsyear");
        }
    }
}