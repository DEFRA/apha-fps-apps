using System.Text.RegularExpressions;

namespace Apha.FPSApps.Web.Common.Validation
{
    /// <summary>
    /// Centralised, compile-time generated regular expressions that can be reused across pages/controllers.
    /// </summary>
    public static partial class ValidationRegexPatterns
    {
        /// <summary>
        /// Matches a string containing only alphanumeric characters (A-Z, a-z, 0-9).
        /// </summary>
        [GeneratedRegex("^[A-Za-z0-9]+$")]
        public static partial Regex AlphaNumeric();
    }
}
