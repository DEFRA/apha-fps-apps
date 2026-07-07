namespace Apha.FPSApps.Web.Areas.PACT.Models
{
    public class Project
    {
        public string ParentProject { get; set; } = null!;
        public string? Manager { get; set; }
    }
}