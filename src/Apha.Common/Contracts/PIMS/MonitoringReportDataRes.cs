namespace Apha.Common.Contracts.PIMS
{
    public class MonitoringReportDataRes
    {
        public short? Year { get; set; }

        public string? Project { get; set; }

        public string? Program { get; set; }

        public string? ParentProject { get; set; }

        public string? ProjectTitle { get; set; }

        public string? Manager { get; set; }

        public string? ProjectStatus { get; set; }

        public string? Contract { get; set; }

        public decimal? TotalPlanCosts { get; set; }

        public decimal? TotalYtdCosts { get; set; }

        public string? MonitoringComment { get; set; }
    }
}
