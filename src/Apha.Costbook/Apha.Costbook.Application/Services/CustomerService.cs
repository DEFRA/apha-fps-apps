using Apha.Costbook.Application.Interfaces;
using Apha.Costbook.Core.Interfaces;
using Apha.Costbook.Application.Dtos;
using AutoMapper;

namespace Apha.Costbook.Application.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _repo;
        private readonly IMapper _mapper;
        public CustomerService(ICustomerRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }
        public async Task<List<CustomerDto>> GetAllCustomersAsync()
        {
            var customers = await _repo.GetAllCustomersAsync();
            return _mapper.Map<List<CustomerDto>>(customers);
        }
    }
}
