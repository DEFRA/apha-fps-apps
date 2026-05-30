using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    public class AccountCategoryMap : IEntityTypeConfiguration<AccountCategory>
    {
        public void Configure(EntityTypeBuilder<AccountCategory> entity)
        {
            entity.HasKey(e => new { e.AccShortName, e.FpsYear })
                  .HasName("pk_tblkpaccountcategory");

            entity.ToTable("tblkpaccountcategory", "fps");

            entity.HasIndex(e => e.AccountType, "accounttype");

            entity.Property(e => e.AccShortName)
                .HasMaxLength(50)
                .HasColumnName("accshortname");

            entity.Property(e => e.AccountDescription)
                .HasMaxLength(50)
                .HasColumnName("accountdescription");

            entity.Property(e => e.AccountType)
                .HasMaxLength(10)
                .HasColumnName("accounttype");

            entity.Property(e => e.ConstituentAccountCodes)
                .HasMaxLength(100)
                .HasColumnName("constituentaccountcodes");

            entity.Property(e => e.Csg7Group)
                .HasMaxLength(15)
                .IsFixedLength()
                .HasColumnName("csg7_group");

            entity.Property(e => e.ProjectSpecific)
                .HasColumnName("projectspecific");

            entity.Property(e => e.RcSpecific)
                .HasColumnName("rcspecific");

            entity.Property(e => e.FpsYear)
                .HasColumnName("fpsyear");
        }
    }
}
