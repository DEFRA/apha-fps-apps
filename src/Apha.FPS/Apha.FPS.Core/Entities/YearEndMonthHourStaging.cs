namespace Apha.FPS.Core.Entities
{
    public partial class YearEndMonthHourStaging
    {
        public Guid JobQueueId { get; set; }

        // Maps to fps.tlkpmonthhours.year on materialization. Named MonthYear, not Year, to stay
        // unambiguous next to JobQueue's FpsYear/TargetFpsYear on the same request.
        public short MonthYear { get; set; }
        public short Month { get; set; }
        public short Fmonth { get; set; }
        public decimal? Days { get; set; }
        public decimal? CvlHours { get; set; }
        public decimal? VidHours { get; set; }
    }
}
