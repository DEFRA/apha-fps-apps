namespace Apha.FPSApps.Application.Dtos.PACT
{
    public class CalenderMonthDto
    {
        public required short MonthNumber { get; set; }
        public required string MonthName { get; set; }
        public short? AccntsPeriod { get; set; }
        public short? Fquarter { get; set; }
    }
}