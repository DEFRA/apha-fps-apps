/*
 * TRANSFORMENGINE MIGRATION — CommentTopicMap.cs
 * Pattern  : stack-upgrade/msaccess-frm-to-dotnet10-mvc-e2e  Phase 4 — DataAccess Layer - DbContext + Map Files + Repository
 * Migrated : 2026-07-22
 *
 * CHANGED:
 *   - IEntityTypeConfiguration<CommentTopic> map created from mabarchive.tlkpcommenttopics PostgreSQL DDL
 *   - ToTable("tlkpcommenttopics", "mabarchive") — lowercase schema and table names per phase rule
 *   - HasColumnName("topic") lowercase to match PostgreSQL DDL column name
 *   - HasKey named "pk_tlkpcommenttopics" matches DDL constraint name
 *
 * PRESERVED:
 *   - Topic is both PK and the only column — single-column lookup table structure preserved exactly
 *   - HasMaxLength(25) matches DDL `character varying(25)`
 *
 * DEFERRED / REQUIRES HUMAN REVIEW:
 *   - DEFERRED: none — fully automated.
 */
using Apha.PIMS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Apha.PIMS.DataAccess.Data
{
    public class CommentTopicMap : IEntityTypeConfiguration<CommentTopic>
    {
        public void Configure(EntityTypeBuilder<CommentTopic> entity)
        {
            entity.HasKey(e => e.Topic).HasName("pk_tlkpcommenttopics");

            // TRANSFORMENGINE: single-column lookup table; topic is both PK and only data column
            entity.ToTable("tlkpcommenttopics", "mabarchive");

            entity.Property(e => e.Topic)
                .HasMaxLength(25)
                .HasColumnName("topic");
        }
    }
}

