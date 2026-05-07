namespace Apha.PACT.Application.Dtos
{
    public class MonthDto
    {
        public required short MonthNumber { get; set; }
        public required string MonthName { get; set; }
        public short? AccntsPeriod { get; set; }
        public short? FQuarter { get; set; }
    }
}
