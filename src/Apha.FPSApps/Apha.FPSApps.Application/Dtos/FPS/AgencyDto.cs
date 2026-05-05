namespace Apha.FPSApps.Application.Dtos.FPS
{
    /// <summary>
    /// Frontend DTO for Agency data.
    /// </summary>
    public class AgencyDto
    {
        /// <summary>
        /// Agency identifier.
        /// </summary>
        public int AgencyId { get; set; }

        /// <summary>
        /// Agency name or description.
        /// </summary>
        public string? AgencyName { get; set; }
    }
}
