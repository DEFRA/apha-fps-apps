namespace Apha.Common.Contracts.PIMS
{
    public class LogMilestoneRes
    {
        public string? Project { get; set; }
        public string? Number { get; set; }
        public string? Description { get; set; }
        public DateTime? DateDue { get; set; }
        public DateTime? DateCompleted { get; set; }
        public short? UnderSdReview { get; set; }
        public short? OnTarget { get; set; }
        public string? ProjectLeaderComment { get; set; }
        public string? CapsComment { get; set; }
        public string? IdType { get; set; }
        public DateTime? DateChanged { get; set; }
        public string? ChangedBy { get; set; }
        public string? UpdateType { get; set; }
    }
}
