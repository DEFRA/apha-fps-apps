using Apha.PACT.Core.Entities;
using Apha.PACT.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.PACT.DataAccess.Data
{
    public class WorkGroupMap : IEntityTypeConfiguration<WorkGroup>
    {
        private readonly IFpsYearContext _fPSYearContext;

        public WorkGroupMap(IFpsYearContext fPSYearContext)
        {
            _fPSYearContext = fPSYearContext;
        }

        public void Configure(EntityTypeBuilder<WorkGroup> entity)
        {
            entity.HasKey(e => new { e.WorkGroupName, e.FpsYear }).HasName("pk_workgroup");

            entity.ToTable("workgroup", "fps");

            entity.HasIndex(e => e.ProfitCentre, "workgroup_profitcentre");

            entity.Property(e => e.WorkGroupName)
                .HasColumnType("citext")
                .HasColumnName("workgroup");
            entity.Property(e => e.FpsYear).HasColumnName("fpsyear");
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
            entity.Property(e => e.Owner)
                .HasMaxLength(50)
                .HasColumnName("owner");
            entity.Property(e => e.ProfitCentre)
                .HasColumnType("citext")
                .HasColumnName("profitcentre");
            entity.Property(e => e.SendEmail).HasColumnName("sendemail");
            entity.HasQueryFilter(e => e.FpsYear == _fPSYearContext.FPSYear);
        }
    }
}
