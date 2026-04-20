using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    public class WorkgroupMap : IEntityTypeConfiguration<Workgroup>
    {


        public void Configure(EntityTypeBuilder<Workgroup> entity)
        {
            entity.HasKey(e => e.WorkgroupName).HasName("workgroup_pk__workgroup__25518c17");

            entity.ToTable("workgroup", "fps");

            entity.HasIndex(e => e.ProfitCentre, "dbo_workgroup_profitcentre");

            entity.Property(e => e.WorkgroupName)
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
            entity.Property(e => e.SysTimestamp)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("systimestamp");
        }
    }
}
