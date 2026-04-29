namespace Apha.Common.Contracts.FPS
{
    /// <summary>
    /// Response contract for Division data.
    /// </summary>
    public class DivisionRes
    {
        /// <summary>
        /// Division identifier (regular integer field, not auto-generated).
        /// </summary>
        public int? DivisionId { get; set; }

        /// <summary>
        /// Parent agency identifier (foreign key to fps.tlkpagency).
        /// </summary>
        public int AgencyId { get; set; }

        /// <summary>
        /// Division name (primary key - case-insensitive text).
        /// </summary>
        public string DivName { get; set; } = null!;

        /// <summary>
        /// Central overhead cost allocation.
        /// </summary>
        public decimal? CentOverhead { get; set; }

        /// <summary>
        /// Parent agency name for display.
        /// </summary>
        public string? AgencyName { get; set; }
    }
}
