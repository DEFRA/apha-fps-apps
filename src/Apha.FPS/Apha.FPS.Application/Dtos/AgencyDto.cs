namespace Apha.FPS.Application.Dtos
{
    /// <summary>
    /// Data transfer object for Agency.
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
