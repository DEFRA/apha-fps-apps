namespace Apha.Common.Contracts.FPS
{
    /// <summary>
    /// Request payload for creating or updating a value in a generic master lookup table.
    /// </summary>
    public class LookupItemReq
    {
        public string Value { get; set; } = string.Empty;

        /// <summary>
        /// The current value being edited. Only used for update operations.
        /// </summary>
        public string? OriginalValue { get; set; }
    }
}
