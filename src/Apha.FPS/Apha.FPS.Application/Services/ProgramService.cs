using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Core.Interfaces;
using AutoMapper;

namespace Apha.FPS.Application.Services
{
    public class ProgramService : IProgramService
    {
        private readonly IProgramRepository _programRepository;
        private readonly IMapper _mapper;

        public ProgramService(IProgramRepository programRepository, IMapper mapper)
        {
            _programRepository = programRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ProgramDto>> GetAllProgramsAsync()
        {
            var programs =  await _programRepository.GetAllProgramsAsync();
            return _mapper.Map<IEnumerable<ProgramDto>>(programs);
        }
    }
}
