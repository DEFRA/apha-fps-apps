namespace Apha.Common.Contracts.PACT
{
    public class CalenderMonthRes
    {
        public required short MonthNumber { get; set; }
        public required string MonthName { get; set; }
        public short? AccntsPeriod { get; set; }
    }
}