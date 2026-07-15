namespace Apha.PACT.Core.Entities
{
    public partial class TimeCostCalcs
    {
        public string WorkGroup { get; set; } = null!;

        public string JobCode { get; set; } = null!;

        public string Project { get; set; } = null!;

        public double Month { get; set; }

        public string StaffId { get; set; } = null!;

        public int FpsYear { get; set; }

        public string? GradeCode { get; set; }

        public string? Name { get; set; }

        public decimal? ChargeRate { get; set; }

        public string? Class { get; set; }

        public double? Time { get; set; }

        public double? Cost { get; set; }

        public string? Division { get; set; }

        public string? JobCodeOld { get; set; }

        public decimal? Pay { get; set; }

        public decimal? NonPay { get; set; }

        public decimal? Overhead { get; set; }
    }
}
