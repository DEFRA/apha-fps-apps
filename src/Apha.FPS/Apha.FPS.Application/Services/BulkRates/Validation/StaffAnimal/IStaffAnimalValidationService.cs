namespace Apha.FPS.Application.Services.BulkRates.Validation.StaffAnimal
{
    /// <summary>
    /// The Staff/Animal domain validation service, called by <see cref="BulkRatesValidator"/>
    /// to validate a Bulk Rates upload/release — a parallel service to
    /// <see cref="IBulkRatesValidationService"/>, deliberately not forced into that
    /// FEC/AGRUP-shaped context (different action vocabulary, no Insert/ZeroRateWithdrawal, no
    /// project/capability routing concerns).
    /// </summary>
    public interface IStaffAnimalValidationService
    {
        /// <summary>
        /// Deterministic and side-effect free: the same context always produces the same
        /// result, and this never writes staging/audit/status — writing is the caller's job.
        /// </summary>
        StaffAnimalValidationResult Validate(StaffAnimalValidationContext context);
    }
}
