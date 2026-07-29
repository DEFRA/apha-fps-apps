namespace Apha.FPS.Application.Dtos
{
    public class DepartmentIncomeTimeDto
    {
        public string Project { get; set; } = null!;

        public string? OracleProjectCode { get; set; }

        public string? SubAccountCode { get; set; }

        public int Month { get; set; }

        public string? DefraProject { get; set; }

        public string? OCC { get; set; }

        public string? OPC { get; set; }

        public string? SPC { get; set; }

        public string? SCC { get; set; }

        public string? Name { get; set; }

        public string? GradeCode { get; set; }

        public string? SpNumber { get; set; }

        public decimal ChargeRate { get; set; }

        public decimal Pay { get; set; }

        public decimal NonPay { get; set; }

        public decimal Overhead { get; set; }

        public decimal Time { get; set; }

        public decimal TotalCost { get; set; }
    }
}
