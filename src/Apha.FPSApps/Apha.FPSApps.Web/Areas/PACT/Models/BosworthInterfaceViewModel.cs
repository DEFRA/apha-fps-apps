namespace Apha.FPSApps.Web.Areas.PACT.Models
{
    public class BosworthInterfaceViewModel
    {
        public List<WorkGroup> WorkGroupOptions { get; set; } = [];
        public List<Project> ProjectOptions { get; set; } = [];
        public List<ProfitCentre> ProfitCentreOptions { get; set; } = [];
    }
}