namespace Apha.PIMS.Application.Dtos
{
    public class FpsYearTotalsDto
    {
        public short Year { get; set; }
        public string? Parentproject { get; set; }
        public decimal? Totaladditionalcosts { get; set; }
        public double? Totalanimalcosts { get; set; }
        public double? Totalstaffcosts { get; set; }
        public double? Totaltestcosts { get; set; }
        public double? Totalcosts { get; set; }
        public decimal Custincome { get; set; }
        public decimal Transferincome { get; set; }
        public decimal Totalincome { get; set; }
        public decimal? BudgetCvl { get; set; }
    }
}
