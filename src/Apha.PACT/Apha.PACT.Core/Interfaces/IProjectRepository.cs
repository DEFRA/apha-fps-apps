namespace Apha.PACT.Core.Interfaces
{
    public interface IProjectRepository
    {
        Task<bool> ExistsAsync(string parentProject);
    }
}
