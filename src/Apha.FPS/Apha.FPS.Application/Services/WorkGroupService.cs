using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Core.Interfaces;
using AutoMapper;

namespace Apha.FPS.Application.Services
{
    /// <summary>
    /// Service implementation for Workgroup lookup operations.
    /// </summary>
    public class WorkGroupService : IWorkGroupService
    {
        private readonly IWorkGroupRepository _repository;
        private readonly IMapper _mapper;

        public WorkGroupService(IWorkGroupRepository repository, IMapper mapper)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<List<string>> GetAllWorkGroupNamesAsync()
            => await _repository.GetAllWorkGroupNamesAsync();

        public async Task<List<WorkGroupViewDto>> GetWorkGroupsAsync(string profitCentre)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(profitCentre);
            var entities = await _repository.GetWorkGroupsByProfitCentreAsync(profitCentre);
            return _mapper.Map<List<WorkGroupViewDto>>(entities);
        }
    }
}
