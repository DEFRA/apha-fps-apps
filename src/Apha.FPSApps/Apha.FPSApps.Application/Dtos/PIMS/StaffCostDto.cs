namespace Apha.FPSApps.Application.Dtos.PIMS
{
    public class StaffCostDto
    {
        // Plan (from vmy_projectstaffplan)
        public short? Year { get; set; }
        public string? ParentProject { get; set; }
        public string? WgGrade { get; set; }
        public string? Name { get; set; }
        public double? PlannedHours { get; set; }
        public decimal? Rate { get; set; }
        public decimal? Cost { get; set; }

        // Actuals (from my_timecostcalcs)
        public string? JobCode { get; set; }
        public string? WorkGroup { get; set; }
        public string? GradeCode { get; set; }
        public double? Month { get; set; }
        public double? Time { get; set; }
        public decimal? ChargeRate { get; set; }
        public decimal? ActualCost { get; set; }
    }
}
