namespace Apha.Common.Contracts.Email
{
    public class YearEndEmailSettings
    {
        public const string SectionName = "YearEndEmailSettings";

        public string DataSetupInitiatedEmailRecipient { get; set; } = string.Empty;
        public string DataSetupInitiatedEmailSubject { get; set; } = string.Empty;
        public string DataSetupInitiatedEmailBody { get; set; } = string.Empty;
        public string DataSetupApprovalEmailRecipient { get; set; } = string.Empty;
        public string DataSetupApprovalEmailSubject { get; set; } = string.Empty;
        public string DataSetupApprovalEmailBody { get; set; } = string.Empty;

        public string CutOverInitiatedEmailRecipient { get; set; } = string.Empty;
        public string CutOverInitiatedEmailSubject { get; set; } = string.Empty;
        public string CutOverInitiatedEmailBody { get; set; } = string.Empty;
        public string CutOverApprovalEmailRecipient { get; set; } = string.Empty;
        public string CutOverApprovalEmailSubject { get; set; } = string.Empty;
        public string CutOverApprovalEmailBody { get; set; } = string.Empty;
    }
}