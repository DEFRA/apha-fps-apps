using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Apha.FPS.Core.Entities
{
    /// <summary>
    /// Agency entity mapped to fps.tlkpagency table.
    /// Represents agencies that can have divisions.
    /// </summary>
    [Table("tlkpagency", Schema = "fps")]
    public partial class Agency
    {
        /// <summary>
        /// Agency identifier. Primary key.
        /// </summary>
        [Key]
        [Column("agencyid")]
        public int AgencyId { get; set; }

        /// <summary>
        /// Agency name or description.
        /// </summary>
        [Column("agencyname")]
        [StringLength(255)]
        public string? AgencyName { get; set; }
    }
}
