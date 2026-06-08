namespace Apha.Common.Contracts.PIMS
{
    public class AnimalCostRes
    {
        // Actuals (from my_proj_subcontract)
        public short Year { get; set; }
        public string? Project { get; set; }
        public double? Month { get; set; }
        public string? AcctCode { get; set; }
        public decimal? Amount { get; set; }
        public string? Description { get; set; }
        public decimal? DailyRate { get; set; }
        public int? AnimalDays { get; set; }

        // Plan (from vmy_projectanimalplan)
        public string? ParentProject { get; set; }
        public string? AnimalType { get; set; }
        public double? NumberOfDays { get; set; }
        public double? NumberOfAnimals { get; set; }
        public decimal? Rate { get; set; }
        public double? Cost { get; set; }
    }
}
