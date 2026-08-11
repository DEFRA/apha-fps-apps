namespace Apha.FPSApps.Application.Dtos.FPS
{
    public class DepartmentIncomeTestDto
    {
        public string Project { get; set; } = null!;

        public string? OracleProjectCode { get; set; }

        public string? SubAccountCode { get; set; }

        public string? DefraProject { get; set; }

        public string? OPC { get; set; }

        public string? OCC { get; set; }

        public int Month { get; set; }

        public string? SPC { get; set; }

        public string? WorkGroup { get; set; }

        public string? SCC { get; set; }

        public string? TestCode { get; set; }

        public decimal Volume { get; set; }

        public decimal TestPrice { get; set; }

        public decimal TotalCost { get; set; }
    }
}
