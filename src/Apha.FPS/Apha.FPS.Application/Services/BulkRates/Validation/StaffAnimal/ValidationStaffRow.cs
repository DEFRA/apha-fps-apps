namespace Apha.FPS.Application.Services.BulkRates.Validation.StaffAnimal
{
    /// <summary>
    /// A staged Staff row, in a neutral shape decoupled from
    /// <see cref="Apha.FPS.Core.Entities.BulkRates.StaffStagingRow"/> — the caller maps its
    /// repository entity into this before calling IStaffAnimalValidationService, and maps
    /// the result back out.
    /// </summary>
    public sealed record ValidationStaffRow
    {
        public required string PcGrade { get; init; }
        public decimal? PayRate { get; init; }
        public decimal? Npr { get; init; }
        public decimal? Ohr { get; init; }

        /// <summary>1-based worksheet row number (row 1 is the header), for finding attribution.</summary>
        public required int SourceRow { get; init; }
    }
}
