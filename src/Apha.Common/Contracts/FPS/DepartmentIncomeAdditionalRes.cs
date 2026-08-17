namespace Apha.Common.Contracts.FPS
{
    public class DepartmentIncomeAdditionalRes
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
