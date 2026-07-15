using Apha.FPS.Application.Dtos;
using Apha.FPS.Application.Interfaces;
using Apha.FPS.Core.Interfaces;
using AutoMapper;

namespace Apha.FPS.Application.Services
{
    /// <summary>
    /// Service implementation for Stage 2 Check Resource Allocation
    /// (frmResourceMain2) read-only grid data.
    /// </summary>
    public class ResourceMain2Service : IResourceMain2Service
    {
        private readonly IResourceMain2Repository _repository;
        private readonly IMapper _mapper;

        public ResourceMain2Service(IResourceMain2Repository repository, IMapper mapper)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public async Task<List<ResourceStaffAllocationDto>> GetStaffAllocationsByWorkGroupGradeAsync(string workGroupGrade)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(workGroupGrade);
            var entities = await _repository.GetStaffAllocationsByWorkGroupGradeAsync(workGroupGrade);
            return _mapper.Map<List<ResourceStaffAllocationDto>>(entities);
        }

        public async Task<List<ResourceStaffJobDto>> GetStaffJobsByStaffIdAsync(int staffId)
        {
            var entities = await _repository.GetStaffJobsByStaffIdAsync(staffId);
            return _mapper.Map<List<ResourceStaffJobDto>>(entities);
        }
    }
}
