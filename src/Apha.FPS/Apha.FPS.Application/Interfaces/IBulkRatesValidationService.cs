using Apha.FPS.Application.Common.BulkRates;
using Apha.FPS.Application.Validation.BulkRates;

namespace Apha.FPS.Application.Interfaces
{
    /// <summary>
    /// The FEC/AGRUP domain validation service, called by <see cref="BulkRatesValidator"/>
    /// to validate a Bulk Rates upload/release. See ValidationContext/ValidationFinding
    /// for the contract this implements.
    /// </summary>
    public interface IBulkRatesValidationService
    {
        /// <summary>
        /// Deterministic and side-effect free: the same context always produces the
        /// same findings, and this never writes staging/audit/status — writing is the
        /// caller's job.
        /// </summary>
        IReadOnlyList<ValidationFinding> Validate(ValidationContext context);
    }
}
