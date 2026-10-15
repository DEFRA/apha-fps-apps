namespace Apha.FPS.Core.Entities
{
    /// <summary>
    /// Read-only entity for mabarchive.my_monthlyoutput (historic monthly test volumes).
    /// </summary>
    public class ArchiveMonthlyOutput
    {
        public short Year { get; set; }
        public string TestCode { get; set; } = null!;
        public string Buyer { get; set; } = null!;
        public double Month { get; set; }
        public string WorkGroup { get; set; } = null!;
        public double? Volume { get; set; }
        public string? WgBuyer { get; set; }
    }
}
