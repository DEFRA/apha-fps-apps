using System.ComponentModel.DataAnnotations;

namespace Apha.FPSApps.Web.Areas.FPS.Validation
{
    /// <summary>
    /// Reusable range validation for currency/decimal fields.
    /// Enforces -999999999999999.9999 to 999999999999999.9999 using decimal
    /// (exact) comparison on the server. It intentionally does NOT emit the
    /// jQuery unobtrusive data-val-range attributes, because these bounds
    /// exceed JavaScript's number precision (~15-16 significant digits) and
    /// cause false client-side validation failures near the boundaries.
    /// The message is built from the property's display name, e.g.
    /// "Budget must be between -999999999999999.9999 and 999999999999999.9999.".
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class CurrencyRangeAttribute : ValidationAttribute
    {
        private const decimal MinValue = -99999999999999.9999m;
        private const decimal MaxValue = 99999999999999.9999m;
                                         

        public override bool IsValid(object? value)
        {
            // Let [Required] handle null/empty; treat missing value as valid here.
            if (value is null)
            {
                return true;
            }

            if (value is decimal dec)
            {
                return dec >= MinValue && dec <= MaxValue;
            }

            // Fallback for values arriving as strings or other numeric types.
            if (decimal.TryParse(value.ToString(), out var parsed))
            {
                return parsed >= MinValue && parsed <= MaxValue;
            }

            // Not a number — let the type/number validation report the issue.
            return true;
        }

        public override string FormatErrorMessage(string name)
        {
            return $"{name} must be between {MinValue} and {MaxValue}.";
        }
    }
}