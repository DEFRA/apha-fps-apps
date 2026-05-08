using Apha.PIMS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.PIMS.DataAccess.Data
{
    public class CommentMap : IEntityTypeConfiguration<Comment>
    {
        private const string Collation = "und-x-icu";
        public void Configure(EntityTypeBuilder<Comment> entity)
        {
            entity.HasKey(e => e.Commentno).HasName("pk_tblcomments");

            entity.ToTable("tblcomments", "mabarchive");

            entity.HasIndex(e => new { e.Project, e.Year, e.Topic }, "ix_tblcomments").IsUnique();

            entity.HasIndex(e => e.Commentno, "tblcomments_commentno_idx");

            entity.Property(e => e.Commentno).HasColumnName("commentno");
            entity.Property(e => e.Commenttext)
                .UseCollation(Collation)
                .HasColumnName("comment");
            entity.Property(e => e.Dateentered)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("dateentered");
            entity.Property(e => e.Madeby)
                .HasMaxLength(20)
                .IsFixedLength()
                .UseCollation(Collation)
                .HasColumnName("madeby");
            entity.Property(e => e.Project)
                .HasMaxLength(20)
                .UseCollation(Collation)
                .HasColumnName("project");
            entity.Property(e => e.Topic)
                .HasMaxLength(25)
                .UseCollation(Collation)
                .HasColumnName("topic");
            entity.Property(e => e.Year).HasColumnName("year");
        }
    }
}

