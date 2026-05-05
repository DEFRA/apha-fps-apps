namespace Apha.Common.Contracts.FPS
{
    /// <summary>
    /// Request contract for Division maintenance operations.
    /// </summary>
    public class DivisionReq
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
    }
}
