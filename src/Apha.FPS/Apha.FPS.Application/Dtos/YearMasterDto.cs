namespace Apha.FPS.Application.Dtos
{
    public class YearMasterDto
    {
        public int FpsYear { get; set; }

        public string FpsYearCode { get; set; } = null!;

        public string YearStatus { get; set; } = null!;

        public string? Remarks { get; set; }

        public bool Active { get; set; }

        public DateTime CreatedOn { get; set; }

        public string? CreatedBy { get; set; }
    }
}
