namespace Apha.Costbook.Core.Entities
{
    public class ProjectSummaryExportData
    {
        public Project? Project { get; set; }
        public List<ProjectYear> Years { get; set; } = [];
        public List<StaffRequirement> StaffRequirements { get; set; } = [];
        public List<TestRequirement> TestRequirements { get; set; } = [];
        public List<AnimalRequirement> AnimalRequirements { get; set; } = [];
        public List<AdditionalCost> AdditionalCosts { get; set; } = [];
    }
}
