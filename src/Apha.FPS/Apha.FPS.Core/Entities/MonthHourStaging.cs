namespace Apha.FPS.Core.Entities
{
    public partial class MonthHourStaging
    {
        // Maps to fps.tlkpmonthhours.year on materialization. Not always equal to FpsYear below —
        // entries can span into target_fpsyear + 1 (MonthHourRepository.GetMonthHourKeys).
        public short Year { get; set; }
        public short Month { get; set; }
        public short Fmonth { get; set; }
        public decimal? Days { get; set; }
        public decimal? CvlHours { get; set; }
        public decimal? VidHours { get; set; }

        // Not a scoping key — staging is a singleton (at most one non-terminal Year End DataSetup
        // request exists at a time, enforced by YearEndRepository.CanInitiateRequest). This carries
        // the request's target_fpsyear so the shape matches fps.tlkpmonthhours's own
        // (year, month, fpsyear) identity, letting the UI combine live and staged rows without a
        // special-cased projection.
        public int FpsYear { get; set; }
    }
}
