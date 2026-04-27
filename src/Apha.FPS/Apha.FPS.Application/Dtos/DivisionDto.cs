namespace Apha.FPS.Application.Dtos
{
    /// <summary>
    /// Data transfer object for Division entity.
    /// </summary>
    public class DivisionDto
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
        /// Parent agency name for display purposes.
        /// </summary>
        public string? AgencyName { get; set; }
    }
}
