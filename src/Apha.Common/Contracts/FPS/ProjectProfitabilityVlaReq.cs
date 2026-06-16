using System.ComponentModel.DataAnnotations;

namespace Apha.Common.Contracts.FPS
{
    /// <summary>
    /// Filter request for the Project Profitability VLA list endpoint
    /// (<c>GET /api/v1/project/profitability-vla</c>).
    /// All filter fields are optional; omitting a field returns all rows for that dimension.
    /// </summary>
    public class ProjectProfitabilityVlaReq
    {
        // TRANSFORMENGINE: maps HTML filterProjectStatus — static options: Approved, Completed, Not Approved
        /// <summary>
        /// Optional filter by project status (e.g. "Approved", "Completed", "Not Approved").
        /// </summary>
        [MaxLength(50)]
        public string? ProjectStatus { get; set; }

        // TRANSFORMENGINE: maps HTML filterProgram — dynamically populated from data
        /// <summary>
        /// Optional filter by program number / name.
        /// </summary>
        [MaxLength(50)]
        public string? ProgramNo { get; set; }

        // TRANSFORMENGINE: maps HTML filterManager — dynamically populated from data
        /// <summary>
        /// Optional filter by manager name.
        /// </summary>
        [MaxLength(100)]
        public string? Manager { get; set; }

        // TRANSFORMENGINE: maps HTML filterCustomer — dynamically populated from data
        /// <summary>
        /// Optional filter by customer name.
        /// </summary>
        [MaxLength(100)]
        public string? Customer { get; set; }

        }
}
