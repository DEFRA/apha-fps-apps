using Apha.FPS.Core.Enities;

namespace Apha.FPS.Core.Interfaces
{
    public interface IAccountCodeRepository
    {
        Task<IEnumerable<AccountCode>> GetAllAccountCodeAsync();
    }
}
