namespace Apha.Common.Contracts.Email
{
    public class WorkGroupReportEmailSettings
    {
        public const string SectionName = "WorkGroupReportEmailSettings";

        public string GatekeeperMailbox { get; set; } = string.Empty;
        public string EmailBodyTemplate { get; set; } = string.Empty;
    }
}
