using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Core.Interfaces;
using AutoMapper;

namespace Apha.FPS.Application.Services
{
    public class WorkGroupService : IWorkGroupService
    {
        private readonly IWorkGroupRepository _repository;
        private readonly IMapper _mapper;

        public WorkGroupService(IWorkGroupRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<List<WorkGroupViewDto>> GetWorkGroupsAsync(string profitCentre)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(profitCentre);
            var entities = await _repository.GetWorkGroupsAsync(profitCentre);
            return _mapper.Map<List<WorkGroupViewDto>>(entities);
        }
    }
}
