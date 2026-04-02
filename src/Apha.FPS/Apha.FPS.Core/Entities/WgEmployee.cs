
namespace Apha.FPS.Core.Entities
{
    public partial class WgEmployee
    {
        public string PactId { get; set; } = null!;

        public string SpNumber { get; set; } = null!;

        public string WorkGroupGrade { get; set; } = null!;

        public string PersonStatus { get; set; } = null!;

        public string? PersonClass { get; set; }

        public double HrsPaid { get; set; }

        public double Leave { get; set; }

        public double SickSpecial { get; set; }

        public double HrsAvail { get; set; }

        public int MakeAvailable { get; set; }

        public int TimeRecorder { get; set; }

        public DateOnly? StartDate { get; set; }

        public DateOnly? EndDate { get; set; }

        public double? HoursPerWeek { get; set; }

        public int? FpsYear { get; set; }
    }
}