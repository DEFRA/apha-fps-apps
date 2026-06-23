/*
 * TRANSFORMENGINE MIGRATION — TestRequirementLogMap.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 4 — DataAccess Layer - DbContext + Map Files + Repository
 * Migrated : 2026-06-22
 *
 * CHANGED:
 *   - FIXED: ValueGeneratedOnAdd() removed from SequenceNo — DDL: sequenceno integer NOT NULL (plain integer, NOT IDENTITY)
 *   - FIXED: JobCode property mapping added — DDL column jobcode character varying(50) was missing from map
 *   - Added migration annotation header
 *
 * PRESERVED:
 *   - Composite PK (SequenceNo, FpsYear) — matches DDL CONSTRAINT pk_testreq_log PRIMARY KEY (sequenceno, fpsyear)
 *   - All other 12 column HasColumnName() mappings (lowercase) verified against DDL
 *   - ToTable("testreq_log", "fps") — lowercase preserved
 *   - Indexes: idx_testreqlog_sequenceno, testreq_log_ind_dt
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: unitprice DDL type is double precision; entity is decimal? — verify EF handles
 *     PostgreSQL float8 → decimal conversion without precision loss; may need HasColumnType("double precision")
 *   - TRANSFORMENGINE TODO: norequired DDL type is integer; entity is double? — verify whether widening
 *     is intentional or should be int?; may cause EF mapping issue
 *   - TRANSFORMENGINE TODO: jobcode DDL comment says 'Generated column based on projectbuyercode' — this is
 *     a comment-only annotation in PostgreSQL DDL (not a stored generated column); mapped as regular nullable string
 */
using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    public class TestRequirementLogMap : IEntityTypeConfiguration<TestRequirementLog>
    {
        public void Configure(EntityTypeBuilder<TestRequirementLog> entity)
        {
            entity.HasKey(e => new { e.SequenceNo, e.FpsYear }).HasName("pk_testreq_log");

            entity.ToTable("testreq_log", "fps");

            entity.HasIndex(e => e.SequenceNo, "idx_testreqlog_sequenceno");
            entity.HasIndex(e => e.DateTime, "testreq_log_ind_dt");

            // TRANSFORMENGINE: ValueGeneratedOnAdd removed — DDL: sequenceno integer NOT NULL (plain integer, not IDENTITY)
            entity.Property(e => e.SequenceNo)
                .HasColumnName("sequenceno");
            entity.Property(e => e.FpsYear).HasColumnName("fpsyear");
            entity.Property(e => e.TestCode)
                .HasMaxLength(20)
                .HasColumnName("testcode");
            entity.Property(e => e.Buyer)
                .HasMaxLength(20)
                .HasColumnName("buyer");
            entity.Property(e => e.UnitPrice).HasColumnName("unitprice");
            entity.Property(e => e.NoRequired).HasColumnName("norequired");
            entity.Property(e => e.ProjectBuyerCode)
                .HasMaxLength(50)
                .HasColumnName("projectbuyercode");
            entity.Property(e => e.TestBuyerCode)
                .HasMaxLength(50)
                .HasColumnName("testbuyercode");
            entity.Property(e => e.Active).HasColumnName("active");
            entity.Property(e => e.DateTime)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("date_time");
            entity.Property(e => e.UserId)
                .HasMaxLength(255)
                .HasColumnName("user_id");
            entity.Property(e => e.InsertDelete)
                .HasMaxLength(2)
                .IsFixedLength()
                .HasColumnName("insert_delete");
            // TRANSFORMENGINE: jobcode mapping added — DDL column was missing from original map; varchar(50) nullable
            entity.Property(e => e.JobCode)
                .HasMaxLength(50)
                .HasColumnName("jobcode");
        }
    }
}
