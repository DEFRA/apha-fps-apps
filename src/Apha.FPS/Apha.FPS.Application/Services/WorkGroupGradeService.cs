using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Application.Pagination;
using Apha.FPS.Core.Interfaces;
using AutoMapper;

namespace Apha.FPS.Application.Services
{
    public class WorkGroupGradeService : IWorkGroupGradeService
    {
        private readonly IWorkGroupGradeRepository _repository;
        private readonly IMapper _mapper;

        public WorkGroupGradeService(IWorkGroupGradeRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<PaginatedResult<WorkgroupGradeDto>> GetWorkGroupGradeAsync(QueryParameters<string> query, string pcGrade)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(pcGrade);
            var filter = _mapper.Map<Apha.FPS.Core.Pagination.PaginationParameters<string>>(query);
            var pagedData = await _repository.GetWorkGroupGradeAsync(filter, pcGrade);
            return _mapper.Map<PaginatedResult<WorkgroupGradeDto>>(pagedData);
        }

        public async Task DeleteWorkGroupGradeAsync(string wgGrade)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(wgGrade);
            await _repository.DeleteWorkGroupGradeAsync(wgGrade);
        }
    }
}
