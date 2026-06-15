namespace Apha.PIMS.Core.Entities
{
    public partial class AdditionalCosts
    {
        public short Year { get; set; }

        public string Jobcode { get; set; } = null!;

        public string Account { get; set; } = null!;

        public string Description { get; set; } = null!;

        public decimal Itemcost { get; set; }

        public string? Freq { get; set; }

        public string? Supplier { get; set; }

        public int AcCounter { get; set; }
    }
}
