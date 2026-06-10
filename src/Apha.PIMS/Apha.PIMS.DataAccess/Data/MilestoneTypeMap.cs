using Apha.PIMS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Apha.PIMS.DataAccess.Data
{
    public class MilestoneTypeMap : IEntityTypeConfiguration<MilestoneType>
    {
        public void Configure(EntityTypeBuilder<MilestoneType> entity)
        {
            entity.ToTable("tlkpmilestonetype", "mabarchive");
            entity.HasKey(e => e.IdType);

            entity.Property(e => e.IdType).HasMaxLength(1).HasColumnName("idtype").IsFixedLength();
            entity.Property(e => e.Type).HasMaxLength(50).HasColumnName("type");
            entity.Property(e => e.MilestoneDeliverable).HasMaxLength(1).HasColumnName("milestonedeliverable").IsFixedLength();
        }
    }
}
