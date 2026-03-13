using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Core.Interfaces;
using AutoMapper;

namespace Apha.FPS.Application.Services
{
    public class AccountCodeService : IAccountCodeService
    {
        private readonly IAccountCodeRepository _accountCodeRepository;
        private readonly IMapper _mapper;
        public AccountCodeService(IAccountCodeRepository accountCodeRepository, IMapper mapper)
        {
            _accountCodeRepository = accountCodeRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<AccountCodeDto>> GetAllAccountCodeAsync()
        {
            var accountCodes = await _accountCodeRepository.GetAllAccountCodeAsync();
            return _mapper.Map<IEnumerable<AccountCodeDto>>(accountCodes);
        }
    }
}
