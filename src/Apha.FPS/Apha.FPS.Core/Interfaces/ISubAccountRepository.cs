using Apha.FPS.Core.Entities;

namespace Apha.FPS.Core.Interfaces
{
    public interface ISubAccountRepository
    {
        Task<IEnumerable<SubAccount>> GetAllSubAccountsAsync();
    }
}
