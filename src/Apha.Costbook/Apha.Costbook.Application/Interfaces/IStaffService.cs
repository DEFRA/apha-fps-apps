using Apha.Costbook.Application.Dtos;

namespace Apha.Costbook.Application.Interfaces
{
    public interface IStaffService
    {
        Task<List<StaffDto>> GetAllStaffAsync();
    }
}
