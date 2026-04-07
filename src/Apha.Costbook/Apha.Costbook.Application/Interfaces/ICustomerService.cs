using Apha.Costbook.Application.Dtos;

namespace Apha.Costbook.Application.Interfaces
{
    public interface ICustomerService
    {
               Task<List<CustomerDto>> GetAllCustomersAsync();
    }
}
