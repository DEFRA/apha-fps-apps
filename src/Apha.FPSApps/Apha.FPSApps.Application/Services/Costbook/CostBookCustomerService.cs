using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.CostBook;
using Apha.FPSApps.Application.Interfaces.Costbook;
using Apha.FPSApps.Application.Interfaces.CostBookApiClients;

namespace Apha.FPSApps.Application.Services.Costbook
{
    public class CostBookCustomerService : ICostBookCustomerService
    {
        private readonly ICostBookApiClient _costBookClient;

        public CostBookCustomerService(ICostBookApiClient costBookClient)
        {
            _costBookClient = costBookClient;
        }

        public Task<ApiResponseDto<List<CustomerDto>>> GetAllCustomersAsync()
        {
            var response = _costBookClient.Customers.GetAllCustomersAsync();
            return response;
        }
    }
}
