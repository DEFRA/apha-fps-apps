namespace Apha.BatchJobs.Infrastructure.Email
{
    public class GraphEmailSettings
    {
        public string TenantId { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
        public string SharedMailbox { get; set; } = string.Empty;
    }
}
