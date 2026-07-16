namespace Apha.FPSApps.Application.Dtos.PACT
{
    public class TimeSaleProfitCentreDto
    {
        public string? ProfitCentre { get; set; }
        public string? WorkGroup { get; set; }
        public string? GradeCode { get; set; }
        public string? Name { get; set; }
        public string? ParentProject { get; set; }
        public string? JobCode { get; set; }
        public double? SumOfTime { get; set; }
        public double? SumOfCost { get; set; }
    }
}