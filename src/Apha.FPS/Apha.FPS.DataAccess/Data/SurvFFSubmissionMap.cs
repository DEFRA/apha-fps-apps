using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    public class SurvFFSubmissionMap : IEntityTypeConfiguration<SurvFFSubmission>
    {
        public void Configure(EntityTypeBuilder<SurvFFSubmission> entity)
        {
            entity.HasKey(e => new { e.SdPactWg, e.Contract }).HasName("pk___1__12");

            entity.ToTable("tblsurvff_submissions", "fps");

            entity.Property(e => e.SdPactWg)
                .HasMaxLength(50)
                .HasColumnName("sd_pact_wg");
            entity.Property(e => e.Contract)
                .HasMaxLength(20)
                .HasColumnName("contract");
            entity.Property(e => e.CountOfJobName)
                .HasColumnName("countofjobname");
            entity.Property(e => e.FpsYear)
                .HasColumnName("fpsyear");
        }
    }
}
