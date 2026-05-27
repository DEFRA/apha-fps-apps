namespace Apha.Costbook.Application.Dtos
{
    public class StaffEffortRowDto
    {
        public string Project { get; set; } = null!;
        public string WorkGroup { get; set; } = null!;
        public string GradeCode { get; set; } = null!;
        public string Name { get; set; } = null!;
        public double Total { get; set; }
        public Dictionary<int, double> YearlyAmounts { get; set; } = [];
    }
}
