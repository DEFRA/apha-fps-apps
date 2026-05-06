using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    public class AccountCategoryMap : IEntityTypeConfiguration<AccountCategory>
    {
        public void Configure(EntityTypeBuilder<AccountCategory> entity)
        {
            entity.HasKey(e => e.AccShortName)
                  .HasName("pk__tblkpaccountcate__02dc7882");

            entity.ToTable("tblkpaccountcategory", "fps");

            entity.Property(e => e.AccShortName)
                .HasColumnType("citext")
                .HasColumnName("accshortname");

            entity.Property(e => e.AccountDescription)
                .HasMaxLength(50)
                .HasColumnName("accountdescription");

            entity.Property(e => e.ConstituentAccountCodes)
                .HasMaxLength(100)
                .HasColumnName("constituentaccountcodes");

            entity.Property(e => e.ProjectSpecific)
                .HasColumnName("projectspecific");

            entity.Property(e => e.FpsYear)
                .HasColumnName("fpsyear");
        }
    }
}
