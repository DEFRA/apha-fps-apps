namespace Apha.FPS.Core.Entities
{
    public partial class PactStaff
    {
        public string? PactId { get; set; }

        public string? SpNumber { get; set; }

        public string? Name { get; set; }

        public string? WorkGroupGrade { get; set; }

        public string? Title { get; set; }

        public string? PersonStatus { get; set; }

        public string? PersonClass { get; set; }

        public double? HrsPaid { get; set; }

        public double? Leave { get; set; }

        public double? SickSpecial { get; set; }

        public double? HrsAvail { get; set; }
        public int? FpsYear { get; set; }
    }
}