namespace Apha.FPS.Application.Common.BulkRates
{
    /// <summary>
    /// The classification assigned to a staged Staff/Animal row:
    /// update-only, no `Insert`, no `ZeroRateWithdrawal` —
    /// a deliberately smaller vocabulary than FEC/AGRUP's calculated-action vocabulary
    /// (kept as its own type rather than sharing constants so the two domains'
    /// action sets can evolve independently).
    /// </summary>
    public static class StaffAnimalCalculatedAction
    {
        public const string NoChange = "NoChange";
        public const string Update = "Update";

        /// <summary>The staged business key (PcGrade/AnimalType) has no live counterpart. Hard failure — blocks the request, never silently skipped.</summary>
        public const string NotFound = "NotFound";

        /// <summary>The row itself fails a validation rule (e.g. negative rate, duplicate key, missing key) independently of whether a live counterpart exists.</summary>
        public const string Invalid = "Invalid";
    }
}
