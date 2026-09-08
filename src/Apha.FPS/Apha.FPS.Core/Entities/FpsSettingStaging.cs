namespace Apha.FPS.Core.Entities
{
    public partial class FpsSettingStaging
    {
        public string Id { get; set; } = null!;
        public string? Setting { get; set; }
        public string? Notes { get; set; }

        // Not a scoping key — staging is a singleton (at most one non-terminal Year End DataSetup
        // request exists at a time, enforced by YearEndRepository.CanInitiateRequest). This carries
        // the request's target_fpsyear so the shape matches fps.tblsettings's own (id, fpsyear)
        // identity, letting the UI combine live and staged rows without a special-cased projection.
        public int FpsYear { get; set; }

        // Who/when confirmed this value, carried through to fps.tblsettings.updated_by/updated_at on
        // materialization instead of the Worker regenerating them.
        public string? UpdatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
