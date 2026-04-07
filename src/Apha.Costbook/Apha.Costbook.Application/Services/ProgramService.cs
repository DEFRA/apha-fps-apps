using Apha.Costbook.Application.Interfaces;
using Apha.Costbook.Core.Interfaces;
using Apha.Costbook.Application.Dtos;
using AutoMapper;

namespace Apha.Costbook.Application.Services
{
    public class ProgramService : IProgramService
    {
        private readonly IProgramRepository _repo;
        private readonly IMapper _mapper;

        public ProgramService(IProgramRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<List<ProgramDto>> GetAllProgramsAsync()
        {
            var programs = await _repo.GetAllProgramsAsync();
            return _mapper.Map<List<ProgramDto>>(programs);
        }
    }
}
