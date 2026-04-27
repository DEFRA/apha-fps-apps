using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Core.Interfaces;
using AutoMapper;

namespace Apha.FPS.Application.Services
{
    /// <summary>
    /// Service for Agency operations.
    /// </summary>
    public class AgencyService : IAgencyService
    {
        private readonly IAgencyRepository _agencyRepository;
        private readonly IMapper _mapper;

        public AgencyService(IAgencyRepository agencyRepository, IMapper mapper)
        {
            _agencyRepository = agencyRepository ?? throw new ArgumentNullException(nameof(agencyRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<AgencyDto>> GetAllAgenciesAsync()
        {
            var agencies = await _agencyRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<AgencyDto>>(agencies);
        }
    }
}
