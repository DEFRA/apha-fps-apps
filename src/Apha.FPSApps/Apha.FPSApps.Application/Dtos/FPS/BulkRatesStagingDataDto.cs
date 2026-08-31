namespace Apha.FPSApps.Application.Dtos.FPS
{
    /// <summary>Mirrors <c>BulkRatesStagingDataDto</c> (Apha.FPS.Application) as serialised over the wire.</summary>
    public class BulkRatesStagingDataDto
    {
        public List<BulkRatesFecStagingRowDto> FecRows { get; set; } = [];
        public List<BulkRatesAgrupStagingRowDto> AgrupRows { get; set; } = [];
        public List<BulkRatesStaffStagingRowDto> StaffRows { get; set; } = [];
        public List<BulkRatesAnimalStagingRowDto> AnimalRows { get; set; } = [];
    }
}
