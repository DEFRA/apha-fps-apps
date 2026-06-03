namespace Apha.PACT.Core.Entities
{
    public partial class WgSummarisedStaffTimeUsageView
    {
        public int FpsYear { get; set; }

        public string? MonthName { get; set; }

        public string? ProfitCentre { get; set; }

        public string? WorkGroup { get; set; }

        public string? WgGrade { get; set; }

        public string? Name { get; set; }

        public double? HrsPaid { get; set; }

        public string? ParentProject { get; set; }

        public string? JobCode { get; set; }

        public string? JobTitle { get; set; }

        public int? UtFlag { get; set; }

        public double? TotalTime { get; set; }

        public double? TotalCost { get; set; }
    }
}