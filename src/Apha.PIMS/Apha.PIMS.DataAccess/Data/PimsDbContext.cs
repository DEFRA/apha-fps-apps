using Apha.PIMS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Apha.PIMS.DataAccess.Data
{
    public partial class PimsDbContext : DbContext
    {
        private const string MabArchiveSchema = "mabarchive";
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

            modelBuilder.Entity<Project>(entity =>
            {
                entity.HasKey(e => e.Parentproject).HasName("pk_g_tlkpproject");

                entity.ToTable("g_tlkpproject", MabArchiveSchema);

                entity.Property(e => e.Parentproject)
                    .HasMaxLength(20)
                    .HasColumnName("parentproject");
                entity.Property(e => e.Contract)
                    .HasMaxLength(10)
                    .HasColumnName("contract");
                entity.Property(e => e.Costbookno)
                    .HasMaxLength(50)
                    .HasColumnName("costbookno");
                entity.Property(e => e.Disease)
                    .HasMaxLength(50)
                    .HasColumnName("disease");
                entity.Property(e => e.Projectstatus)
                    .HasMaxLength(50)
                    .HasColumnName("projectstatus");
                entity.Property(e => e.Projecttitle)
                    .HasMaxLength(200)
                    .HasColumnName("projecttitle");
                entity.Property(e => e.Shorttitle)
                    .HasMaxLength(30)
                    .HasColumnName("shorttitle");
            });

            modelBuilder.Entity<Projects>(entity =>
            {
                entity.HasKey(e => new { e.Year, e.Parentproject }).HasName("pk_my_tlkpproject");

                entity.ToTable("my_tlkpproject", MabArchiveSchema);

                entity.HasIndex(e => e.Year, "my_p_year");

                entity.Property(e => e.Year).HasColumnName("year");
                entity.Property(e => e.Parentproject)
                    .HasMaxLength(20)
                    .HasColumnName("parentproject");
                entity.Property(e => e.BudgetCvl)
                    .HasColumnType("money")
                    .HasColumnName("budget_cvl");
                entity.Property(e => e.Carryover)
                    .HasColumnType("money")
                    .HasColumnName("carryover");
                entity.Property(e => e.Caseworksub)
                    .HasPrecision(5, 4)
                    .HasColumnName("caseworksub");
                entity.Property(e => e.Comments).HasColumnName("comments");
                entity.Property(e => e.Contract)
                    .HasMaxLength(10)
                    .HasColumnName("contract");
                entity.Property(e => e.Costcentre).HasColumnName("costcentre");
                entity.Property(e => e.Custincome)
                    .HasColumnType("money")
                    .HasColumnName("custincome");
                entity.Property(e => e.Customer)
                    .HasMaxLength(50)
                    .HasColumnName("customer");
                entity.Property(e => e.Datecreated)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("datecreated");
                entity.Property(e => e.Disease)
                    .HasMaxLength(50)
                    .HasColumnName("disease");
                entity.Property(e => e.Feccost)
                    .HasColumnType("money")
                    .HasColumnName("feccost");
                entity.Property(e => e.Finished).HasColumnName("finished");
                entity.Property(e => e.Incomeaccountcode)
                    .HasMaxLength(50)
                    .HasColumnName("incomeaccountcode");
                entity.Property(e => e.Isdefraproject).HasColumnName("isdefraproject");
                entity.Property(e => e.Manager)
                    .HasMaxLength(50)
                    .HasColumnName("manager");
                entity.Property(e => e.Oracleprojectcode)
                    .HasMaxLength(50)
                    .HasColumnName("oracleprojectcode");
                entity.Property(e => e.Plancaseworkdebit)
                    .HasColumnType("money")
                    .HasColumnName("plancaseworkdebit");
                entity.Property(e => e.Profit)
                    .HasColumnType("money")
                    .HasColumnName("profit");
                entity.Property(e => e.Program)
                    .HasMaxLength(10)
                    .HasColumnName("program");
                entity.Property(e => e.Projectgroup)
                    .HasMaxLength(50)
                    .HasColumnName("projectgroup");
                entity.Property(e => e.Projectstatus)
                    .HasMaxLength(50)
                    .HasColumnName("projectstatus");
                entity.Property(e => e.Pvsincome)
                    .HasColumnType("money")
                    .HasColumnName("pvsincome");
                entity.Property(e => e.Source)
                    .HasMaxLength(5)
                    .IsFixedLength()
                    .HasColumnName("source");
                entity.Property(e => e.Subaccountcode)
                    .HasMaxLength(50)
                    .HasColumnName("subaccountcode");
                entity.Property(e => e.Transferincome)
                    .HasColumnType("money")
                    .HasColumnName("transferincome");
                entity.Property(e => e.WipCurrent)
                    .HasColumnType("money")
                    .HasColumnName("wip_current");
                entity.Property(e => e.WipEoy)
                    .HasColumnType("money")
                    .HasColumnName("wip_eoy");
                entity.Property(e => e.WipLimit)
                    .HasColumnType("money")
                    .HasColumnName("wip_limit");
            });

            modelBuilder.Entity<Comment>(entity =>
            {
                entity.HasKey(e => e.Commentno).HasName("pk_tblcomments");

                entity.ToTable("tblcomments", MabArchiveSchema);

                entity.HasIndex(e => new { e.Project, e.Year, e.Topic }, "ix_tblcomments").IsUnique();

                entity.HasIndex(e => e.Commentno, "tblcomments_commentno_idx");

                entity.Property(e => e.Commentno).HasColumnName("commentno");
                entity.Property(e => e.Commenttext)
                    .UseCollation("und-x-icu")
                    .HasColumnName("comment");
                entity.Property(e => e.Dateentered)
                    .HasColumnType("timestamp without time zone")
                    .HasColumnName("dateentered");
                entity.Property(e => e.Madeby)
                    .HasMaxLength(20)
                    .IsFixedLength()
                    .UseCollation("und-x-icu")
                    .HasColumnName("madeby");
                entity.Property(e => e.Project)
                    .HasMaxLength(20)
                    .UseCollation("und-x-icu")
                    .HasColumnName("project");
                entity.Property(e => e.Topic)
                    .HasMaxLength(25)
                    .UseCollation("und-x-icu")
                    .HasColumnName("topic");
                entity.Property(e => e.Year).HasColumnName("year");
            });

            modelBuilder.Entity<CommentTopic>(entity =>
            {
                entity.HasKey(e => e.Topic).HasName("pk_tlkpcommenttopics");

                entity.ToTable("tlkpcommenttopics", MabArchiveSchema);

                entity.Property(e => e.Topic)
                    .HasMaxLength(50)
                    .HasColumnName("topic");
            });

            modelBuilder.Entity<ProposedProject>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("pk_tblproposedproject");

                entity.ToTable("tblproposedproject", MabArchiveSchema);

                entity.HasIndex(e => e.Parentproject, "project_index").IsUnique();

                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Costbookno)
                    .HasMaxLength(50)
                    .HasColumnName("costbookno");
                entity.Property(e => e.Customer)
                    .HasMaxLength(50)
                    .HasColumnName("customer");
                entity.Property(e => e.Disease)
                    .HasMaxLength(50)
                    .HasColumnName("disease");
                entity.Property(e => e.Manager)
                    .HasMaxLength(50)
                    .HasColumnName("manager");
                entity.Property(e => e.Parentproject)
                    .HasMaxLength(20)
                    .HasColumnName("parentproject");
                entity.Property(e => e.Program)
                    .HasMaxLength(10)
                    .HasColumnName("program");
                entity.Property(e => e.Projectstatus)
                    .HasMaxLength(50)
                    .HasColumnName("projectstatus");
                entity.Property(e => e.Projecttitle)
                    .HasMaxLength(200)
                    .HasColumnName("projecttitle");
                entity.Property(e => e.Reason)
                    .HasMaxLength(250)
                    .HasColumnName("reason");
            });


            modelBuilder.Entity<RadtrackProg>(entity =>
            {
                entity.HasKey(e => e.Program).HasName("pk_tblradtrackprog");

                entity.ToTable("tblradtrackprog", MabArchiveSchema);

                entity.Property(e => e.Program)
                    .HasMaxLength(10)
                    .HasColumnName("program");
                entity.Property(e => e.Publicationprefix)
                    .HasMaxLength(5)
                    .HasColumnName("publicationprefix");
                entity.Property(e => e.Radtrackprog).HasColumnName("radtrackprog");
            });

            modelBuilder.Entity<ProjectDetail>(entity =>
            {
                entity.HasNoKey();
                entity.Property(e => e.Parentproject).HasMaxLength(20).HasColumnName("parentproject");
                entity.Property(e => e.Version).HasMaxLength(20).HasColumnName("version");
                entity.Property(e => e.FileRef).HasMaxLength(50).HasColumnName("fileref");
                entity.Property(e => e.CustomerRef).HasMaxLength(50).HasColumnName("customerref");
                entity.Property(e => e.StartDate).HasColumnName("startdate");
                entity.Property(e => e.EndDate).HasColumnName("enddate");
                entity.Property(e => e.CostbookNumber).HasMaxLength(50).HasColumnName("costbooknumber");
                entity.Property(e => e.Riskid).HasColumnName("riskid");
                entity.Property(e => e.UseProjectYears).HasColumnName("useprojectyears");
                entity.Property(e => e.RevisedEndDate).HasColumnName("revisedenddate");
                entity.Property(e => e.ClosedDate).HasColumnName("closeddate");
            });

            modelBuilder.Entity<ProjectLatestDetail>(entity => // Configuration for ProjectLatestDetail
            {
                entity.HasNoKey();
                entity.ToView("vprojectlatestdetails", MabArchiveSchema); // Ensure the schema is correct
                entity.Property(e => e.ParentProject).HasColumnName("parentproject");
                entity.Property(e => e.Program).HasColumnName("program");
                entity.Property(e => e.Customer).HasColumnName("customer");
                entity.Property(e => e.Active).HasColumnName("active");
            });

            // ── ProjectRadTrackData → g_tlkpproject_radtrackdata ──────────────────
            modelBuilder.Entity<ProjectRadTrackData>(entity =>
            {
                entity.HasKey(e => e.Parentproject).HasName("pk_g_tlkpproject_radtrackdata");
                entity.ToTable("g_tlkpproject_radtrackdata", MabArchiveSchema);
                entity.Property(e => e.Parentproject).HasMaxLength(20).HasColumnName("parentproject");
                entity.Property(e => e.Version).HasMaxLength(20).HasColumnName("version");
                entity.Property(e => e.Fileref).HasMaxLength(50).HasColumnName("fileref");
                entity.Property(e => e.Customerref).HasMaxLength(50).HasColumnName("customerref");
                entity.Property(e => e.Startdate).HasColumnName("startdate");
                entity.Property(e => e.Enddate).HasColumnName("enddate");
                entity.Property(e => e.Finalreportreceived).HasColumnName("finalreportreceived");
                entity.Property(e => e.Finalreportsent).HasColumnName("finalreportsent");
                entity.Property(e => e.Inflation).HasColumnName("inflation");
                entity.Property(e => e.Closeddate).HasColumnName("closeddate");
                entity.Property(e => e.Useprojectyear).HasColumnName("useprojectyear");
                entity.Property(e => e.Status).HasMaxLength(50).HasColumnName("status");
                entity.Property(e => e.Pcforecastspend).HasColumnName("pcforecastspend");
                entity.Property(e => e.Riskid).HasColumnName("riskid");
                entity.Property(e => e.Costbooknumber).HasMaxLength(50).HasColumnName("costbooknumber");
                entity.Property(e => e.Revisedenddate).HasColumnName("revisedenddate");
                entity.Property(e => e.Formrequired).HasColumnName("formrequired");
                entity.Property(e => e.Overallcustincome).HasColumnName("overallcustincome");
            });

            // ── Risk → tlkprisk ───────────────────────────────────────────────────
            modelBuilder.Entity<Risk>(entity =>
            {
                entity.HasKey(e => e.Riskid).HasName("pk_tlkprisk");
                entity.ToTable("tlkprisk", MabArchiveSchema);
                entity.Property(e => e.Riskid).HasColumnName("riskid");
                entity.Property(e => e.Riskrating).HasMaxLength(50).HasColumnName("riskrating");
            });

            // ── ProjectStatus → tlkpprojectstatus ─────────────────────────────────
            modelBuilder.Entity<ProjectStatus>(entity =>
            {
                entity.HasKey(e => e.Projectstatus).HasName("pk_tlkpprojectstatus");
                entity.ToTable("tlkpprojectstatus", MabArchiveSchema);
                entity.Property(e => e.Projectstatus).HasMaxLength(50).HasColumnName("projectstatus");
                entity.Property(e => e.IsFps).HasColumnName("is_fps");
                entity.Property(e => e.IsPims).HasColumnName("is_pims");
            });

            // ── Year → tlkpyear  (property renamed Value to avoid class/member clash) ──
            modelBuilder.Entity<Year>(entity =>
            {
                entity.HasKey(e => e.Value).HasName("pk_tlkpyear");
                entity.ToTable("tlkpyear", MabArchiveSchema);
                entity.Property(e => e.Value).HasColumnName("year");             // ← Value maps to DB column "year"
                entity.Property(e => e.Latestmonthreleased).HasColumnName("latestmonthreleased");
            });
           

        }        
    }
}
