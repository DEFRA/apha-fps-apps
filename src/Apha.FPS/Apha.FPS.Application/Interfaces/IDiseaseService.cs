namespace Apha.FPS.Application.Interfaces
{
    public interface IDiseaseService
    {
        Task<IEnumerable<string>> GetAllDiseasesAsync();
    }
}
