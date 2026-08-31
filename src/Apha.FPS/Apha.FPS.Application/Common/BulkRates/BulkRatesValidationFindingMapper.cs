using Apha.FPS.Core.Entities;

namespace Apha.FPS.Application.Common.BulkRates
{
    /// <summary>
    /// Maps a <see cref="ValidationFinding"/> onto <see cref="StagingValidationError"/> — identical
    /// logic today called from all three process services' upload validation (mirroring
    /// <c>BulkRatesValidator.MapFinding</c>/<c>SplitBusinessKey</c>, which the old validator path
    /// keeps as its own private copy). AGRUP findings carry "TestCode/Buyer" as a single
    /// BusinessKey string; every other sheet (FEC, Staff, Animal) is a plain business key.
    /// </summary>
    public static class BulkRatesValidationFindingMapper
    {
        public static StagingValidationError MapFinding(ValidationFinding finding, Guid jobQueueId, int uploadVersion)
        {
            var (testCode, buyer) = SplitBusinessKey(finding.Sheet, finding.BusinessKey);
            return new StagingValidationError
            {
                JobQueueId = jobQueueId,
                UploadVersion = uploadVersion,
                SourceRowNumber = finding.SourceRow ?? 0,
                FieldName = finding.Field,
                ValidationCode = finding.ValidationCode,
                Severity = finding.Severity,
                ValidationMessage = finding.Message,
                SheetName = finding.Sheet,
                TestCode = testCode,
                Buyer = buyer,
                IsRequestLevel = finding.IsRequestLevel
            };
        }

        /// <summary>
        /// Also called directly (not just from <see cref="MapFinding"/>) wherever a caller needs
        /// to split a ROW_CLASSIFIED finding's AGRUP business key back into TestCode/Buyer to
        /// look up the matching live row — e.g. BulkTestRatesService.PrepareForReleaseAsync.
        /// </summary>
        public static (string? TestCode, string? Buyer) SplitBusinessKey(string sheet, string? businessKey)
        {
            if (businessKey is null) return (null, null);
            if (!string.Equals(sheet, "AGRUP", StringComparison.OrdinalIgnoreCase)) return (businessKey, null);
            var parts = businessKey.Split('/', 2);
            return parts.Length == 2 ? (parts[0], parts[1]) : (businessKey, null);
        }
    }
}
