namespace Apha.FPS.Application.Dtos
{
    public class DepartmentIncomeTotalsDto
    {
        public string Project { get; set; } = null!;

        public string? OracleProjectCode { get; set; }

        public decimal TotalCosts { get; set; }

        public decimal? TimeCost { get; set; }

        public decimal? TestsCost { get; set; }

        public decimal? AnimalsCost { get; set; }

        public decimal? ProjectSpecificsCost { get; set; }
    }
}
