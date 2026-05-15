namespace Apha.FPSApps.Web.Areas.CostBook.Models
{
    public class StaffEffortPivotRow
    {
        public string Project { get; set; } = null!;
        public string WorkGroup { get; set; } = null!;
        public string GradeCode { get; set; } = null!;
        public string Name { get; set; } = null!;
        public decimal? Y1  { get; set; }
        public decimal? Y2  { get; set; }
        public decimal? Y3  { get; set; }
        public decimal? Y4  { get; set; }
        public decimal? Y5  { get; set; }
        public decimal? Y6  { get; set; }
        public decimal? Y7  { get; set; }
        public decimal? Y8  { get; set; }
        public decimal? Y9  { get; set; }
        public decimal? Y10 { get; set; }
        public decimal Total { get; set; }
    }
}
