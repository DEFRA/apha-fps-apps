namespace Apha.FPS.Core.Interfaces
{
    public interface IFpsRequestContext
    {
        int FpsYear { get; set; }
        string UserEmailId { get; set; }
        string CorrelationId { get; set; }
    }
}
