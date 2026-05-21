namespace Apha.Common.Contracts.PACT
{
    public class UpdateSendEmailFlagReq
    {
        /// <summary>
        /// When set, only work groups belonging to this profit centre are updated.
        /// Leave null to update ALL work groups regardless of profit centre.
        /// </summary>
        public string? ProfitCentre { get; set; }

        /// <summary>
        /// 1 = flag for email, 0 = clear flag.
        /// </summary>
        public short SendEmail { get; set; }
    }
}
