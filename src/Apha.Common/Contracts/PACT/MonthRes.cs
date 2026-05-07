namespace Apha.Common.Contracts.PACT
{
    public class MonthRes
    {
        public required short MonthNumber { get; set; }
        public required string MonthName { get; set; }
        public short? AccntsPeriod { get; set; }
        public short? FQuarter { get; set; }
    }
}