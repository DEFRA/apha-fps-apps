using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Core.Interfaces;
using AutoMapper;

namespace Apha.FPS.Application.Services
{
    public class JobCodeService : IJobCodeService
    {
        private readonly IJobCodeRepository _jobCodeRepository;       
        private readonly IMapper _mapper;

        public JobCodeService(IJobCodeRepository jobCodeRepository, IMapper mapper)
        {
            _jobCodeRepository = jobCodeRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<JobCodeDto>> GetJobCodeListAsync()
        {
            var jobCodes = await _jobCodeRepository.GetAllJobCodesAsync();
            return _mapper.Map<IEnumerable<JobCodeDto>>(jobCodes);            
        }

        public async Task<IEnumerable<JobCodeDto>> GetZtCodeLookupAsync()
        {
            var jobCodes = await _jobCodeRepository.GetZtJobCodesAsync();
            return _mapper.Map<IEnumerable<JobCodeDto>>(jobCodes);
        }
    }
}
