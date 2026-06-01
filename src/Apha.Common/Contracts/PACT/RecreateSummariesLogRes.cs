namespace Apha.Common.Contracts.PACT
{
    public class RecreateSummariesLogRes
    {
        public int Id { get; set; }
        public string? UserId { get; set; }
        public string? UserName { get; set; }
        public short? Period { get; set; }
        public DateTime? DateDone { get; set; }
        public int FpsYear { get; set; }
    }
}
