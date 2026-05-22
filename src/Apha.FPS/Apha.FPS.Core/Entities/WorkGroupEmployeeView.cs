namespace Apha.FPS.Core.Entities
{
    /// <summary>
    /// EF entity mapped to view vtblwgemployee.
    /// Also used as LINQ join result shape (Name is populated by repository join with Employee).
    /// </summary>
    public class WorkGroupEmployeeView
    {
        public string? PactId { get; set; }
        public string? SpNumber { get; set; }
        public string? WorkGroupGrade { get; set; }
        public string? PersonStatus { get; set; }
        public string? PersonClass { get; set; }
        public double? HrsPaid { get; set; }
        public double? Leave { get; set; }
        public double? SickSpecial { get; set; }
        public double? HrsAvail { get; set; }
        public int? MakeAvailable { get; set; }
        public int? TimeRecorder { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public double? HoursPerWeek { get; set; }
        public int? FpsYear { get; set; }
        public int? UserId { get; set; }
        public string? Dt2Username { get; set; }
        public string? UserEmail { get; set; }

        /// <summary>Computed from Employee join; not mapped to the database view.</summary>
        public string? Name { get; set; }
    }
}
