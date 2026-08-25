namespace Apha.Common.Contracts.FPS.Email
{
    public class BulkRatesEmailSettings
    {
        public const string SectionName = "BulkRatesEmailSettings";

        // ── ReleasedForApproval ────────────────────────────────────────────────
        // Comma-separated list of approver addresses.
        public string ReleasedForApprovalRecipients { get; set; } = string.Empty;
        public string ReleasedForApprovalSubject    { get; set; } = string.Empty;
        public string ReleasedForApprovalBody       { get; set; } = string.Empty;

        // ── Approved ─────────────────────────────────────────────────────────
        // Comma-separated list of recipients (e.g. the initiator + a notification DL).
        public string ApprovedRecipients { get; set; } = string.Empty;
        public string ApprovedSubject    { get; set; } = string.Empty;
        public string ApprovedBody       { get; set; } = string.Empty;

        // ── Rejected ──────────────────────────────────────────────────────────
        // Sent to the original requester (RequestedBy address).
        public string RejectedSubject { get; set; } = string.Empty;
        public string RejectedBody    { get; set; } = string.Empty;

        // ── Cancelled ─────────────────────────────────────────────────────────
        // Sent to the original requester. Leave blank to suppress email.
        public string CancelledSubject { get; set; } = string.Empty;
        public string CancelledBody    { get; set; } = string.Empty;
    }
}
