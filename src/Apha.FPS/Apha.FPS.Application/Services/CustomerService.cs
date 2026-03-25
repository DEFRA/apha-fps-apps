using Apha.FPS.Application.Interfaces;
using Apha.FPS.Core.Interfaces;

namespace Apha.FPS.Application.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _customerRepository;

        public CustomerService(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }

        public async Task<IEnumerable<string>> GetAllCustomersAsync()
        {
            var customers = await _customerRepository.GetAllCustomersAsync();

            return customers.Select(c => c.CustomerName);
        }
    }
}
