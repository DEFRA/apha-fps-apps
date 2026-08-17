namespace Apha.FPS.Core.Entities
{
    public class DepartmentIncomeAdditional
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
