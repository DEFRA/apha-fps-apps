/*
 * TRANSFORMENGINE MIGRATION — WorkgroupMap.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 4 — DataAccess Layer - DbContext + Map Files + Repository
 * Migrated : 2026-06-23
 *
 * CHANGED:
 *   - Annotation header added for Phase 4 batch verification pass
 *
 * PRESERVED:
 *   - Composite PK (WorkGroupName + FpsYear) with constraint name pk_workgroup
 *   - ToTable("workgroup", "fps") — lowercase per project convention
 *   - HasIndex on ProfitCentre ("workgroup_profitcentre")
 *   - All 11 column mappings (workgroup, centraloverhead, cos90, costcentre, costcentreold,
 *     description, email_recipient, fpsyear, owner, profitcentre, sendemail) — all lowercase
 *   - HasMaxLength and HasColumnType constraints preserved from DDL
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: SysTimestamp property on Workgroup entity has no matching column
 *     in the fps.workgroup DDL and is intentionally unmapped here — confirm the property
 *     can be removed from the entity or document why it is kept unmapped
 */
using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    public class WorkgroupMap : IEntityTypeConfiguration<Workgroup>
    {


        public void Configure(EntityTypeBuilder<Workgroup> entity)
        {
            entity.HasKey(e => new { e.WorkGroupName, e.FpsYear }).HasName("pk_workgroup");

            entity.ToTable("workgroup", "fps");

            entity.HasIndex(e => e.ProfitCentre, "workgroup_profitcentre");

            entity.Property(e => e.WorkGroupName)
                .HasMaxLength(50)
                .HasColumnName("workgroup");
            entity.Property(e => e.CentralOverhead)
                .HasDefaultValueSql("0")
                .HasColumnType("money")
                .HasColumnName("centraloverhead");
            entity.Property(e => e.Cos90).HasColumnName("cos90");
            entity.Property(e => e.CostCentre).HasColumnName("costcentre");
            entity.Property(e => e.CostCentreOld).HasColumnName("costcentreold");
            entity.Property(e => e.Description)
                .HasMaxLength(45)
                .HasColumnName("description");
            entity.Property(e => e.EmailRecipient)
                .HasMaxLength(50)
                .HasColumnName("email_recipient");
            entity.Property(e => e.FpsYear).HasColumnName("fpsyear");
            entity.Property(e => e.Owner)
                .HasMaxLength(50)
                .HasColumnName("owner");
            entity.Property(e => e.ProfitCentre)
                .HasMaxLength(50)
                .HasColumnName("profitcentre");
            entity.Property(e => e.SendEmail).HasColumnName("sendemail");
            }
    }
}
