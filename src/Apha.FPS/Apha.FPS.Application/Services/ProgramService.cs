using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Application.Pagination;
using Apha.FPS.Core.Interfaces;
using Apha.FPS.Core.Pagination;
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

        public async Task<PaginatedResult<ProgramDto>> GetAllProgramsAsync(QueryParameters<string> query)
        {
            var filter = _mapper.Map<PaginationParameters<string>>(query);
            var programViews = await _programRepository.GetAllProgramsAsync(filter);
            return _mapper.Map<PaginatedResult<ProgramDto>>(programViews);
        }

        public async Task<ProgramDto?> GetProgramByIdAsync(string programNo)
        {
            var program = await _programRepository.GetProgramByIdAsync(programNo);
            return _mapper.Map<ProgramDto?>(program);
        }

        public async Task<List<string?>> GetAllDirectoratesAsync()
        {
            return await _programRepository.GetAllDirectoratesAsync();
        }

        public async Task<ProgramDto> AddProgramAsync(ProgramDto programDto)
        {
            if (string.IsNullOrWhiteSpace(programDto.ProgramNo))
            {
                throw new ArgumentException("Program number is required.");
            }

            if (string.IsNullOrWhiteSpace(programDto.ProgramName))
            {
                throw new ArgumentException("Program name is required.");
            }

            var program = _mapper.Map<Core.Entities.Program>(programDto);
            var addedProgram = await _programRepository.AddProgramAsync(program);
            return _mapper.Map<ProgramDto>(addedProgram);
        }

        public async Task<ProgramDto> UpdateProgramAsync(ProgramDto programDto)
        {           
            ArgumentNullException.ThrowIfNull(programDto);
           
            if (string.IsNullOrWhiteSpace(programDto.ProgramNo))
            {
                throw new ArgumentException("Program number is required.");
            }

            if (string.IsNullOrWhiteSpace(programDto.ProgramName))
            {
                throw new ArgumentException("Program name is required.");
            }
           
            var existingProgram = await _programRepository.GetProgramByIdAsync(programDto.ProgramNo);
            if (existingProgram == null)
            {
                throw new KeyNotFoundException($"Program with ID '{programDto.ProgramNo}' not found.");
            }
            _mapper.Map(programDto, existingProgram);
            var updatedProgram = await _programRepository.UpdateProgramAsync(existingProgram);
            return _mapper.Map<ProgramDto>(updatedProgram);
        }

        public async Task<bool> DeleteProgramAsync(string programNo)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(programNo);

            // Check if record exists
            var existingEntity = await _programRepository.GetProgramByIdAsync(programNo);
            if (existingEntity == null)
            {
                throw new KeyNotFoundException($"Program with ID '{programNo}' was not found.");
            }

            return await _programRepository.DeleteProgramAsync(programNo);
        }       
    }
}
