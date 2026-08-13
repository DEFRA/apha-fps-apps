namespace Apha.FPS.Application.Dtos.BulkRates
{
    /// <summary>
    /// Staged data for a Bulk Rates request's staging grid(s) — "FEC Data (Staging)" +
    /// "Agrup Details" for FEC requests, "Staff Data (Staging)" for Staff requests, or
    /// "Animal Data (Staging)" for Animal requests. Only the relevant list is populated.
    /// </summary>
    public class BulkRatesStagingDataDto
    {
        public IReadOnlyList<BulkRatesFecStagingRowDto> FecRows { get; set; } = [];
        public IReadOnlyList<BulkRatesAgrupStagingRowDto> AgrupRows { get; set; } = [];
        public IReadOnlyList<BulkRatesStaffStagingRowDto> StaffRows { get; set; } = [];
        public IReadOnlyList<BulkRatesAnimalStagingRowDto> AnimalRows { get; set; } = [];
    }
}
