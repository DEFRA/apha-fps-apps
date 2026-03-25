namespace Apha.Common.Contracts.FPS
{
    public class EmployeeReq
    {
        public string SPNumber { get; set; } = null!;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Title { get; set; }
        public int? FPSCalYear { get; set; }
    }
}
