using Apha.FPS.Core.Entities;

namespace Apha.FPS.Application.Common.BulkRates
{
    /// <summary>
    /// Staff/Animal row-count bucketing — identical logic needed by both
    /// <c>BulkStaffRatesService</c> and <c>BulkAnimalRatesService</c> (mirroring
    /// <c>BulkRatesValidator.ComputeStaffAnimalRowCounts</c>, which the old validator path keeps
    /// as its own private copy). No Insert bucket (Staff/Animal are update-only, gating decision
    /// #3): NotFound/Invalid rows both carry an Error-severity finding, so they're counted the
    /// same way FEC/AGRUP's Invalid bucket already is, from the mapped errors rather than from
    /// <see cref="StaffAnimalCalculatedAction"/> directly.
    /// </summary>
    public static class StaffAnimalRowCountHelper
    {
        public static BulkRatesRowCounts ComputeRowCounts(
            int total, IEnumerable<string> actions, IReadOnlyList<StagingValidationError> errors)
        {
            int update = 0, unchanged = 0;
            foreach (var action in actions)
            {
                if (action == StaffAnimalCalculatedAction.Update) update++;
                else if (action == StaffAnimalCalculatedAction.NoChange) unchanged++;
            }

            var invalid = errors.Count(e => e.Severity == ValidationSeverity.Error);
            return new BulkRatesRowCounts
            {
                Total = total,
                Update = update,
                Unchanged = unchanged,
                Invalid = invalid,
                Valid = total - invalid
            };
        }
    }
}
