namespace Apha.FPS.Api.TestTools
{
    /// <summary>
    /// Config gate for the temporary Year End test-reset tool. See <see cref="YearEndTestToolsController"/>
    /// for the full three-condition gate this feeds into. Deliberately absent (defaults to disabled)
    /// from the shared appsettings.json - only set true in a Local/Development-tier config file.
    /// </summary>
    public class YearEndTestToolsOptions
    {
        public const string SectionName = "YearEndTestTools";

        public bool Enabled { get; set; }
    }
}
