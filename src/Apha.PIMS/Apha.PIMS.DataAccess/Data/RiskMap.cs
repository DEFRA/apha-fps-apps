using Apha.PIMS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Apha.PIMS.DataAccess.Data
{
    public class RiskMap : IEntityTypeConfiguration<Risk>
    {
        public void Configure(EntityTypeBuilder<Risk> entity)
        {
            entity.HasKey(e => e.Riskid).HasName("pk_tlkprisk");
            entity.ToTable("tlkprisk", "mabarchive");
            entity.Property(e => e.Riskid).HasColumnName("riskid");
            entity.Property(e => e.Riskrating).HasMaxLength(50).HasColumnName("riskrating");
        }
    }
}
