using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Validation
{
    /// <summary>
    /// Reusable range validation for non-financial numeric quantity fields
    /// (e.g. Hours, Days, Number, Freq, Time, Planned Hours, HrsPaid, Leave, etc.).
    /// Enforces -99999999999999.9999 to 99999999999999.9999 and builds the error message
    /// from the property's display name, e.g. "Hours must be between -99999999999999.9999 and 99999999999999.9999.".
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class NonFinancialRangeAttribute : RangeAttribute
    {
        private const double MinValue = -99999999999999.9999;
        private const double MaxValue = 99999999999999.9999;

        public NonFinancialRangeAttribute() : base(MinValue, MaxValue)
        {
        }

        public override string FormatErrorMessage(string name)
        {
            return $"{name} must be between {MinValue} and {MaxValue}.";
        }
    }
}
