namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    public class TestBookedItem
    {
        /// <summary>
        /// Gets or sets the unique identifier for the test booked record.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the test code (e.g., TestCode_001, TestCode_002).
        /// </summary>
        public string Test { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the description of the test (e.g., Test Description A, Test Description B).
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the ReCUP (Recommended Unit Price) value with currency formatting.
        /// Represents the recommended unit price for the test.
        /// </summary>
        public decimal ReCUP { get; set; }

        /// <summary>
        /// Gets or sets the number of tests required.
        /// Represents the quantity of tests to be performed.
        /// </summary>
        public int Num { get; set; }

        /// <summary>
        /// Gets or sets the AgrUP (Agreed Unit Price) value with currency formatting.
        /// Represents the agreed unit price for the test.
        /// </summary>
        public decimal AgrUP { get; set; }

        /// <summary>
        /// Gets or sets the total test cost with currency formatting.
        /// Calculated as AgrUP * Num (Agreed Unit Price multiplied by Number of tests).
        /// </summary>
        public decimal TestCost { get; set; }
    }
}
