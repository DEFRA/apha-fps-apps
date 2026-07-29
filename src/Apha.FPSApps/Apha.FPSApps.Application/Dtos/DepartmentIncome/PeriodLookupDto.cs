namespace Apha.FPSApps.Application.Dtos.DepartmentIncome
{
    // Dedicated lookup type — never reuse for CRUD operations
    public class PeriodLookupDto
    {
        public int AccntsPeriod { get; set; }

        public string MonthName { get; set; } = null!;

        public int MonthNumber { get; set; }
    }
}
