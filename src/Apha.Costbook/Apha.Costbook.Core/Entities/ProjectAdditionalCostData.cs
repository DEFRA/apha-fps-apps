namespace Apha.Costbook.Core.Entities
{
    public class ProjectAdditionalCostRowData
    {
        public string? Directorate   { get; set; }
        public string? Programme     { get; set; }
        public string? ContractNumber { get; set; }
        public string? Project       { get; set; }
        public string? AccountCat    { get; set; }
        public string? Description   { get; set; }
        public double? ItemCost      { get; set; }
    }

    public class ProjectAdditionalCostData
    {
        public List<ProjectAdditionalCostRowData> Rows      { get; set; } = [];
        public int                               TotalCount { get; set; }
    }
}
