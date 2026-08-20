namespace Apha.PIMS.Core.Entities
{
    /// <summary>
    /// Projection entity for monitoring report data that combines RadTrack contract, project details,
    /// year totals, project month-final costs, and monitoring comments.
    /// Maps to the SQL query joining tblRadtrackContract, MY_tlkpProject, G_tlkpProject,
    /// MY_FPSYearTotals, MY_ProjectMonthFinal, and qryPMonitoringComments.
    /// </summary>
    public class MonitoringReportData
    {
        /// <summary>Report year from MY_tlkpProject</summary>
        public short? Year { get; set; }

        /// <summary>Project ID alias from MY_tlkpProject.ParentProject</summary>
        public string? Project { get; set; }

        /// <summary>Program code from MY_tlkpProject / G_tlkpProject</summary>
        public string? Program { get; set; }

        /// <summary>Parent project ID from MY_tlkpProject</summary>
        public string? ParentProject { get; set; }

        /// <summary>Project title from G_tlkpProject</summary>
        public string? ProjectTitle { get; set; }

        /// <summary>Project manager from MY_tlkpProject</summary>
        public string? Manager { get; set; }

        /// <summary>Project status from MY_tlkpProject</summary>
        public string? ProjectStatus { get; set; }

        /// <summary>Contract code from G_tlkpProject</summary>
        public string? Contract { get; set; }

        /// <summary>Total planned costs for the year from MY_FPSYearTotals</summary>
        public decimal? TotalPlanCosts { get; set; }

        /// <summary>Cumulative cost year-to-date from MY_ProjectMonthFinal (CumCost field)</summary>
        public decimal? TotalYtdCosts { get; set; }

        /// <summary>Monitoring comment text from qryPMonitoringComments</summary>
        public string? MonitoringComment { get; set; }
    }
}
