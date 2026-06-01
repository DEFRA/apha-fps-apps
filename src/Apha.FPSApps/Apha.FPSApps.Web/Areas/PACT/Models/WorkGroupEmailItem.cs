namespace Apha.FPSApps.Web.Areas.PACT.Models
{
    public class WorkGroupEmailItem
    {
        public string WorkGroupName { get; set; } = string.Empty;
        public string? EmailRecipient { get; set; }
        public bool FlaggedForEmail { get; set; }
        public bool SendEmailYes => FlaggedForEmail;
        public bool SendEmailNo  => !FlaggedForEmail;
    }
}
