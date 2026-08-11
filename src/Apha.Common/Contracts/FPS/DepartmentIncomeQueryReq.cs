namespace Apha.Common.Contracts.FPS
{
       public class DepartmentIncomeQueryReq
    {
        public DepartmentIncomeQueryType QueryType { get; set; }

        public string? Project { get; set; }

        public int? MonthFrom { get; set; }

        public int? MonthTo { get; set; }
    }
    public enum DepartmentIncomeQueryType
    {
        Time = 1,
        Tests = 2,
        Animals = 3,
        Additional = 4,
        Totals = 5
    }
}
