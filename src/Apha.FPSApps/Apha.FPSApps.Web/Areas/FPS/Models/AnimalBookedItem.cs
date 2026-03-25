namespace Apha.FPSApps.Web.Areas.FPS.Models
{
    public class AnimalBookedItem
    {
        /// <summary>
        /// Gets or sets the unique identifier for the animal booked record.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the animal type description.
        /// Example: "Animal_001, Type_A"
        /// </summary>
        public string AnimalType { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the number of days the animal is booked.
        /// </summary>
        public int Day { get; set; }

        /// <summary>
        /// Gets or sets the number of animals required.
        /// </summary>
        public int NoReq { get; set; }

        /// <summary>
        /// Gets or sets the daily rate for the animal.
        /// Stored as decimal for precise currency calculations.
        /// </summary>
        public decimal DailyRt { get; set; }

        /// <summary>
        /// Gets or sets the total cost for the animal booking.
        /// Calculated as: Day * NoReq * DailyRt
        /// Stored as decimal for precise currency calculations.
        /// </summary>
        public decimal Cost { get; set; }
    }
}
