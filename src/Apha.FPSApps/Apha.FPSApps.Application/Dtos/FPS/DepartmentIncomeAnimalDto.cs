namespace Apha.FPSApps.Application.Dtos.FPS
{
    public class DepartmentIncomeAnimalDto
    {
        public string Project { get; set; } = null!;

        public string? OracleProjectCode { get; set; }

        public string? SubAccountCode { get; set; }

        public string? DefraProject { get; set; }

        public string? OPC { get; set; }

        public string? OCC { get; set; }

        public int Month { get; set; }

        public string? SPC { get; set; }

        public string? SCC { get; set; }

        public string? AnimalType { get; set; }

        public decimal AnimalDays { get; set; }

        public decimal Rate { get; set; }

        public decimal TotalCost { get; set; }
    }
}
