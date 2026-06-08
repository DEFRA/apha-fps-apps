namespace Apha.FPSApps.Application.Dtos.PIMS
{
    public class AdditionalCostDto
    {
        // Shared
        public short Year { get; set; }
        public string? Description { get; set; }

        // Actuals (from my_proj_subcontract)
        public int SubContCounter { get; set; }
        public string? Project { get; set; }
        public double? Month { get; set; }
        public string? AcctCode { get; set; }
        public decimal? Amount { get; set; }
        public string? Supplier { get; set; }
        public int? SupplierNumber { get; set; }

        // Plan (from my_tbladditionalcosts)
        public string? JobCode { get; set; }
        public string? Account { get; set; }
        public decimal? ItemCost { get; set; }
    }
}
