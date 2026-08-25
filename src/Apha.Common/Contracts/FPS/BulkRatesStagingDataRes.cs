namespace Apha.Common.Contracts.FPS
{
    /// <summary>
    /// Wire contract for a Bulk Rates request's staging grid(s) — only the relevant list is
    /// populated (FEC/Agrup for FEC requests, Staff for Staff requests, Animal for Animal
    /// requests).
    /// </summary>
    public class BulkRatesStagingDataRes
    {
        public IReadOnlyList<BulkRatesFecStagingRowRes> FecRows { get; set; } = [];
        public IReadOnlyList<BulkRatesAgrupStagingRowRes> AgrupRows { get; set; } = [];
        public IReadOnlyList<BulkRatesStaffStagingRowRes> StaffRows { get; set; } = [];
        public IReadOnlyList<BulkRatesAnimalStagingRowRes> AnimalRows { get; set; } = [];
    }
}
