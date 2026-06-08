namespace Apha.PIMS.Core.Entities
{
    public class ProjectMonthFinal
    {
        public short Year { get; set; }
        public string Project { get; set; } = null!;
        public double Monthno { get; set; }
        public string? Periodname { get; set; }
        public decimal? Subcontracts { get; set; }
        public decimal? Nonanimals { get; set; }
        public decimal? Animals { get; set; }
        public decimal? Timecosts { get; set; }
        public decimal? Transfercosts { get; set; }
        public decimal? Totalcost { get; set; }
        public double? Totalhours { get; set; }
        public decimal? Invoices { get; set; }
        public decimal? Coiw { get; set; }
    }
}
