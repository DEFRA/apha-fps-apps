namespace Apha.Common.Contracts.PACT
{
    public class TestPriceCheckReq
    {
        public short IsDefraProject { get; set; }
        public decimal? TestPrice { get; set; }
        public decimal? DefraUnitPrice { get; set; }
    }
}
