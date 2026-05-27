namespace Apha.Common.Contracts.PACT;

public class SummarisedWgTimePivotRes
{
    public List<int> Months { get; set; } = [];
    public List<SummarisedWgTimeRes> Rows { get; set; } = [];
    public Pagination Pagination { get; set; } = new();
}
