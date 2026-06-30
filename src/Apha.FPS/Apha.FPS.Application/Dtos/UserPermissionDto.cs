namespace Apha.FPS.Application.Dtos
{
    public class UserPermissionDto
    {
        public int UserId { get; set; }
        public List<string> ProfitCentres { get; set; } = [];
        public List<string> Programs { get; set; } = [];
        public List<string> Categories { get; set; } = [];
        public List<string> TestOwners { get; set; } = [];
        public List<string> ProjectGroups { get; set; } = [];
    }
}
