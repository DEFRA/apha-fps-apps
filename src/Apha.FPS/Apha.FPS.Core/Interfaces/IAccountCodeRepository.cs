using Apha.FPS.Core.Entities;

namespace Apha.FPS.Core.Interfaces
{
    public interface IAccountCodeRepository
    {
        Task<IEnumerable<AccountCode>> GetAllAccountCodeAsync();
    }
}
