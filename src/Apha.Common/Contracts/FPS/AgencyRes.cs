namespace Apha.Common.Contracts.FPS
{
    /// <summary>
    /// Response contract for Agency.
    /// </summary>
    public class AgencyRes
    {
        /// <summary>
        /// Agency identifier.
        /// </summary>
        public int AgencyId { get; set; }

        /// <summary>
        /// Agency name or description.
        /// </summary>
        public string? AgencyName { get; set; }
    }
}
