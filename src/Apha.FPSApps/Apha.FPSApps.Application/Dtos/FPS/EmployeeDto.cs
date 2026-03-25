namespace Apha.FPSApps.Application.Dtos.FPS
{
    public class EmployeeDto
    {
        public string SPNumber { get; set; } = null!;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Title { get; set; }
        public int? FPSCalYear { get; set; }
    }
}
