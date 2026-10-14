namespace Apha.FPS.Core.Entities
{
    /// <summary>
    /// Read-only entity for mabarchive.tlkpyear (latest released month per year).
    /// </summary>
    public class ArchiveYear
    {
        public int Year { get; set; }
        public int? LatestMonthReleased { get; set; }
    }
}
