using Apha.FPS.Application.Dtos;

namespace Apha.FPS.Application.Interfaces
{
    public interface IAccountCodeService
    {
        Task<IEnumerable<AccountCodeDto>> GetAllAccountCodeAsync();
    }
}
