namespace Apha.FPSApps.Application.Dtos.PACT
{
    public class ProjectSubContractDto
    {
        public int SubContCounter { get; set; }
        public string? Project { get; set; }
        public string? TestJob { get; set; }
        public double? Month { get; set; }
        public decimal? Amount { get; set; }
        public string? WorkGroup { get; set; }
        public string? AcctCode { get; set; }
        public string? Supplier { get; set; }
        public string? Description { get; set; }
        public int? SupplierNumber { get; set; }
        public decimal? DailyRate { get; set; }
        public int? AnimalDays { get; set; }
    }
}
