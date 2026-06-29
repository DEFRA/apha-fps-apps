namespace Apha.FPSApps.Application.Dtos.FPS
{
    public class PermissionOptionsDto
    {
        public List<string> ProfitCentres { get; set; } = [];
        public List<string> Programs { get; set; } = [];
        public List<string> Categories { get; set; } = [];
        public List<string> TestOwners { get; set; } = [];
        public List<string> ProjectGroups { get; set; } = [];
    }
}
