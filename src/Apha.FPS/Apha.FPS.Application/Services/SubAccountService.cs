using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Core.Interfaces;
using AutoMapper;


namespace Apha.FPS.Application.Services
{
    public class SubAccountService : ISubAccountService
    {
        private readonly ISubAccountRepository _subAccountRepository;
        private readonly IMapper _mapper;
        public SubAccountService(ISubAccountRepository subAccountRepository, IMapper mapper)
        {
            _subAccountRepository = subAccountRepository;
            _mapper = mapper;
        }
        public async Task<IEnumerable<SubAccountDto>> GetAllSubAccountsAsync()
        {
            var subAccounts = await _subAccountRepository.GetAllSubAccountsAsync();
            return _mapper.Map<IEnumerable<SubAccountDto>>(subAccounts);
        }
    }
}
