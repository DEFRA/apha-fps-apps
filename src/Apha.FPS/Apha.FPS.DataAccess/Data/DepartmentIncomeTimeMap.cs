using Apha.FPS.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apha.FPS.DataAccess.Data
{
    public class DepartmentIncomeTimeMap : IEntityTypeConfiguration<DepartmentIncomeTime>
    {
        public void Configure(EntityTypeBuilder<DepartmentIncomeTime> builder)
        {
            builder.HasNoKey();
            builder.ToView("vw_dept_income_time", "fps");

            builder.Property(e => e.Project)
                .HasMaxLength(20)
                .HasColumnName("project");

            builder.Property(e => e.OracleProjectCode)
                .HasMaxLength(50)
                .HasColumnName("oracleprojectcode");

            builder.Property(e => e.SubAccountCode)
                .HasMaxLength(50)
                .HasColumnName("subaccountcode");

            builder.Property(e => e.Month)
                .HasColumnName("month");

            builder.Property(e => e.DefraProject)
                .HasMaxLength(3)
                .HasColumnName("defraproject");

            builder.Property(e => e.OCC)
                .HasMaxLength(50)
                .HasColumnName("occ");

            builder.Property(e => e.OPC)
                .HasMaxLength(50)
                .HasColumnName("opc");

            builder.Property(e => e.SPC)
                .HasMaxLength(50)
                .HasColumnName("spc");

            builder.Property(e => e.SCC)
                .HasMaxLength(50)
                .HasColumnName("scc");

            builder.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");

            builder.Property(e => e.GradeCode)
                .HasMaxLength(10)
                .HasColumnName("gradecode");

            builder.Property(e => e.SpNumber)
                .HasMaxLength(10)
                .HasColumnName("spnumber");

            builder.Property(e => e.ChargeRate)
                .HasColumnType("money")
                .HasColumnName("chargerate");

            builder.Property(e => e.Pay)
                .HasColumnType("money")
                .HasColumnName("pay");

            builder.Property(e => e.NonPay)
                .HasColumnType("money")
                .HasColumnName("nonpay");

            builder.Property(e => e.Overhead)
                .HasColumnType("money")
                .HasColumnName("overhead");

            builder.Property(e => e.Time)
                .HasColumnName("time");

            builder.Property(e => e.TotalCost)
                .HasColumnType("money")
                .HasColumnName("totalcost");
        }
    }
}
