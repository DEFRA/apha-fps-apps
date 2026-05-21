namespace Apha.PACT.Application.Dtos
{
    public class WorkGroupTimeCodeDto
    {
        public string PACTStaffID { get; set; } = null!;
        public string ParentProject { get; set; } = null!;
        public string? WorkGroup { get; set; }
        public string? Name { get; set; }
        public string TimeCode { get; set; } = null!;
        public double Month { get; set; }
        public double Hours { get; set; }
    }
}