/*
 * TRANSFORMENGINE MIGRATION — CommentMap.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 4 — DataAccess Layer - DbContext + Map Files + Repository
 * Migrated : 2026-07-22
 *
 * CHANGED:
 *   - IEntityTypeConfiguration<Comment> map created from mabarchive.tblcomments PostgreSQL DDL
 *   - ToTable("tblcomments", "mabarchive") — lowercase schema and table names per phase rule
 *   - HasColumnName values all lowercase to match PostgreSQL DDL column names
 *   - CommentText mapped to DB column "comment" (renamed on entity to avoid reserved-word collision)
 *   - commentno uses auto-sequence default (ValueGeneratedOnAdd via identity sequence tblcomments_commentno_seq)
 *   - MadeBy: HasMaxLength corrected to 20 to match DDL `character(20)` (was incorrectly 50)
 *   - HasKey named "pk_tblcomments" matches DDL constraint name
 *
 * PRESERVED:
 *   - All column nullability semantics: project/year/topic NOT NULL; dateentered/comment/madeby nullable
 *   - HasColumnType for DateEntered ("timestamp without time zone") matches PostgreSQL DDL exactly
 *   - HasColumnType for CommentText ("character varying") matches DDL `text` promoted to varchar storage
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - TRANSFORMENGINE TODO: Verify PostgreSQL collation "und-x-icu" on project/topic/comment/madeby columns
 *     is handled by database-level default (modelBuilder.UseCollation in PimsDbContext) — no per-property mapping needed.
 */
using Apha.PIMS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.PIMS.DataAccess.Data
{
    public class CommentMap : IEntityTypeConfiguration<Comment>
    {

        public void Configure(EntityTypeBuilder<Comment> entity)
        {
            entity.HasKey(e => e.CommentNo).HasName("pk_tblcomments");

            entity.ToTable("tblcomments", "mabarchive");

            // TRANSFORMENGINE: commentno uses sequence tblcomments_commentno_seq (auto-generated PK on insert)
            entity.Property(e => e.CommentNo)
                .HasColumnName("commentno")
                .ValueGeneratedOnAdd();

            entity.Property(e => e.CommentText)
                .HasColumnType("character varying")
                .HasColumnName("comment");
            entity.Property(e => e.DateEntered)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("dateentered");
            // TRANSFORMENGINE: MadeBy max length corrected to 20 — DDL defines madeby as character(20), not varchar(50)
            entity.Property(e => e.MadeBy)
                .HasMaxLength(20)
                .HasColumnName("madeby");
            entity.Property(e => e.Project)
                .HasMaxLength(20)
                .HasColumnName("project");
            entity.Property(e => e.Topic)
                .HasMaxLength(25)
                .HasColumnName("topic");
            entity.Property(e => e.Year).HasColumnName("year");

            // TRANSFORMENGINE: Unique index ix_tblcomments (project, year, topic) from DDL — enforced by ExistsAsync guard in repository
            entity.HasIndex(e => new { e.Project, e.Year, e.Topic })
                .IsUnique()
                .HasDatabaseName("ix_tblcomments");
        }
    }
}

