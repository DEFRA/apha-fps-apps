namespace Apha.FPSApps.Application.Dtos.FPS
{
    public class BulkRatesRowCountsDto
    {
        public int Total { get; set; }
        public int Valid { get; set; }
        public int Invalid { get; set; }
        public int Insert { get; set; }
        public int Update { get; set; }
        public int Unchanged { get; set; }
    }
}
