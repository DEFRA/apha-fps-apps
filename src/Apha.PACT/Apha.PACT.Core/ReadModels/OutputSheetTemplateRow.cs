namespace Apha.PACT.Core.ReadModels
{
    /// <summary>
    /// Query projection for the blank output-sheet template sent to work group members.
    /// Mirrors Access ldoMakeOutputSheet SQL:
    ///   SELECT tlkpTestCapability.WorkGroup, tlkpTestCapability.TestCode,
    ///          TestOrProduct.ItemDescription, tlkpTestReqmt.Buyer,
    ///          &lt;month&gt; AS Month, Null AS Volume
    ///   FROM TestOrProduct
    ///        INNER JOIN (tlkpTestReqmt INNER JOIN tlkpTestCapability
    ///                    ON tlkpTestReqmt.TestCode = tlkpTestCapability.TestCode)
    ///        ON TestOrProduct.ItemCode = tlkpTestCapability.TestCode
    ///   WHERE tlkpTestReqmt.Active &lt;&gt; 0
    ///     AND tlkpTestCapability.WorkGroup = ?
    ///   ORDER BY WorkGroup, TestCode, Buyer
    /// </summary>
    public class OutputSheetTemplateRow
    {
        public string TestCode { get; set; } = string.Empty;
        public string? ItemDescription { get; set; }
        public string Buyer { get; set; } = string.Empty;
        public short Month { get; set; }
        public double? Volume { get; set; }  // always null — recipient fills in
    }
}
