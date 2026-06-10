namespace Apha.PIMS.Core.Entities
{
    public partial class FpsYearTotal
    {
        public short Year { get; set; }
        public string Parentproject { get; set; } = null!;

        public string Program { get; set; } = null!;

        public decimal? Totaladditionalcosts { get; set; }

        public double? Totalanimalcosts { get; set; }

        public double? Totalstaffcosts { get; set; }

        public double? Totaltestcosts { get; set; }

        public double? Totalcosts { get; set; }

        public decimal Custincome { get; set; }

        public decimal Transferincome { get; set; }

        public decimal Totalincome { get; set; }

        public decimal? BudgetCvl { get; set; }

        public decimal? Requiredprofit { get; set; }

        public string? Manager { get; set; }

        public string? Customer { get; set; }

        public string Projectstatus { get; set; } = null!;

        public decimal? Pvsincome { get; set; }

        public decimal? Plancaseworkdebit { get; set; }

        public double? Totalpaycosts { get; set; }
    }
}
