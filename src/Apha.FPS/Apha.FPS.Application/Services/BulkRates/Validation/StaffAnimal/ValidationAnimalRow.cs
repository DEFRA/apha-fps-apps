namespace Apha.FPS.Application.Services.BulkRates.Validation.StaffAnimal
{
    /// <summary>
    /// A staged Animal row, in a neutral shape decoupled from
    /// <see cref="Apha.FPS.Core.Entities.BulkRates.AnimalStagingRow"/> — the caller maps its
    /// repository entity into this before calling IStaffAnimalValidationService, and maps
    /// the result back out.
    /// </summary>
    public sealed record ValidationAnimalRow
    {
        public required string AnimalType { get; init; }
        public decimal? DailyRate { get; init; }
        public decimal? DefraDailyRate { get; init; }
        public bool? PlanByWeek { get; init; }
        public string? Species { get; init; }
        public string? SecurityLevel { get; init; }

        /// <summary>1-based worksheet row number (row 1 is the header), for finding attribution.</summary>
        public required int SourceRow { get; init; }
    }
}
