namespace Apha.FPS.Core.Entities
{
    /// <summary>
    /// LINQ join result shape for qryStaffEdit (WorkGroupEmployee joined with Employee to include computed Name).
    /// Not an EF entity — produced by a repository join projection.
    /// </summary>
    public class WorkGroupEmployeeView
    {
        public string PactId { get; set; } = null!;
        public string SpNumber { get; set; } = null!;
        public string WorkGroupGrade { get; set; } = null!;
        public string Name { get; set; } = null!;
        public string PersonStatus { get; set; } = null!;
        public string? PersonClass { get; set; }
        public double HrsPaid { get; set; }
        public double Leave { get; set; }
        public double SickSpecial { get; set; }
        public double HrsAvail { get; set; }
        public int MakeAvailable { get; set; }
    }
}
