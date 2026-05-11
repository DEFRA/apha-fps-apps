using Apha.PIMS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Apha.PIMS.DataAccess.Data
{
    public partial class PimsDbContext : DbContext
    {
        
        public PimsDbContext(DbContextOptions<PimsDbContext> options)
        : base(options)
        {
        }

        public virtual DbSet<Project> Projects { get; set; }
        public virtual DbSet<Projects> MyTlkpProjects { get; set; }
        public virtual DbSet<Comment> Comments { get; set; }
        public virtual DbSet<CommentTopic> CommentTopics { get; set; }
        public virtual DbSet<ProposedProject> ProposedProjects { get; set; }
        public virtual DbSet<RadtrackProg> RadtrackProgs { get; set; }
        public virtual DbSet<ProjectDetail> ProjectDetails { get; set; }
        public virtual DbSet<ProjectLatestDetail> ProjectLatestDetails { get; set; } // New DbSet for ProjectLatestDetail
                                                                                     // Add these 4 new DbSets alongside the existing ones
        public virtual DbSet<ProjectRadTrackData> ProjectRadtrackdata { get; set; }
        public virtual DbSet<Risk> Risks { get; set; }
        public virtual DbSet<ProjectStatus> ProjectStatuses { get; set; }
        public virtual DbSet<Year> Years { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasPostgresExtension("partman", "pg_partman")
                .HasPostgresExtension("citext")
                .HasAnnotation("Npgsql:CollationDefinition:public.latin1_general_ci_as", "en-US-u-ks-level2,en-US-u-ks-level2,icu,False");
            
            modelBuilder.ApplyConfiguration(new ProjectMap());
            modelBuilder.ApplyConfiguration(new ProjectsMap());
            modelBuilder.ApplyConfiguration(new CommentMap());
            modelBuilder.ApplyConfiguration(new CommentTopicMap());
            modelBuilder.ApplyConfiguration(new ProposedProjectMap());
            modelBuilder.ApplyConfiguration(new RadtrackProgMap());
            modelBuilder.ApplyConfiguration(new ProjectDetailMap());
            modelBuilder.ApplyConfiguration(new ProjectLatestDetailMap());
            modelBuilder.ApplyConfiguration(new ProjectRadTrackDataMap());
            modelBuilder.ApplyConfiguration(new RiskMap());
            modelBuilder.ApplyConfiguration(new ProjectStatusMap());
            modelBuilder.ApplyConfiguration(new YearMap());           

        }        
    }
}
