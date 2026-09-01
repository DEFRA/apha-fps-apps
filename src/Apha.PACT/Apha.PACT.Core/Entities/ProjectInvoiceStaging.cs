namespace Apha.PACT.Core.Entities
{
    public partial class ProjectInvoiceStaging
    {
        public int Id { get; set; }
        public string? ProjectParent { get; set; }
        public string? Month { get; set; }
        public string? Amount { get; set; }
        public string? CostOfWork { get; set; }
        public string? Wip { get; set; }
        public string? ProfitLoss { get; set; }
        public string? Detail { get; set; }
        public string? Type { get; set; }
        public string? Filename { get; set; }
        public string? ImportedBy { get; set; }
        public DateTime? ImportedDate { get; set; }
        public string? ValidationFailure { get; set; }
        public bool? IsExported { get; set; }
        public bool? IsPassed { get; set; }
    }
}
