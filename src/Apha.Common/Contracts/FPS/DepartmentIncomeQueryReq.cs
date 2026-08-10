namespace Apha.Common.Contracts.FPS
{
    public class DepartmentIncomeQueryReq
    {
        public DepartmentIncomeQueryType QueryType { get; set; }

        public string? Project { get; set; }

        public int? MonthFrom { get; set; }

        public int? MonthTo { get; set; }
    }
}
