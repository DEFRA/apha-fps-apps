using Apha.FPS.Application.Dtos;

namespace Apha.FPS.Application.Interfaces
{
    public interface ISubAccountService
    {
        Task<IEnumerable<SubAccountDto>> GetAllSubAccountsAsync();
    }
}
