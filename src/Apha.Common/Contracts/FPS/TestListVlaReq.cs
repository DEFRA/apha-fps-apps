namespace Apha.Common.Contracts.FPS
{
    /// <summary>
    /// Request contract for create/update operations on TestOrProduct VLA entries.
    /// Contains only writable ControlSource-bound fields from frmTestList / fsubTest_MainList.
    /// Route keys: ItemCode + FpsYear (composite PK on fps.testorproduct).
    /// </summary>
    public class TestListVlaReq
    {
        public string ItemCode { get; set; } = null!;
        public int FpsYear { get; set; }

        public string? ItemDescription { get; set; }
        public string? TestManager { get; set; }
        public string? JobStatus { get; set; }

        public decimal? UnitPriceVla { get; set; }
        public decimal? PriceAhvg { get; set; }

        //   partial-update patterns — service must validate before persisting
        public string? Owner { get; set; }
        public string? ChargeMethod { get; set; }
        public string? ShortDescription { get; set; }

        public decimal DefraUnitPrice { get; set; }
    }
}
