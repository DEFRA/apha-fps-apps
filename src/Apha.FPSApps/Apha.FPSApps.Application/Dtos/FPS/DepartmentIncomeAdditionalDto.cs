namespace Apha.FPSApps.Application.Dtos.FPS
{
    public class DepartmentIncomeAdditionalDto
    {
        public string Project { get; set; } = null!;

        public string? OracleProjectCode { get; set; }

        public string? SubAccountCode { get; set; }

        public string? DefraProject { get; set; }

        public string? OPC { get; set; }

        public string? OCC { get; set; }

        public int Month { get; set; }

        public decimal TotalCost { get; set; }
    }
}
