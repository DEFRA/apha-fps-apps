using Apha.PACT.Application.Dtos;
using Apha.PACT.Application.Interfaces;
using Apha.PACT.Core.Interfaces;

namespace Apha.PACT.Application.Services
{
    public class ProjectProfileService : IProjectProfileService
    {
        private readonly IProjectProfileRepository _repository;

        public ProjectProfileService(IProjectProfileRepository repository)
        {
            _repository = repository;
        }

        public async Task<IList<ProjectProfileGraphDto>> GetProfileGraphDataAsync(string project)
        {
            var data = await _repository.GetProfileGraphDataAsync(project);
            return data.Select(d => new ProjectProfileGraphDto
            {
                MonthNo = (int)d.MonthNo,
                Profile = d.Profile,
                TotalCost = d.Cost
            }).ToList();
        }

        public async Task<IList<ProjectProfileCumulativeGraphDto>> GetCumulativeGraphDataAsync(string project)
        {
            var data = await _repository.GetCumulativeGraphDataAsync(project);
            return data.Select(d => new ProjectProfileCumulativeGraphDto
            {
                MonthNo = (int)d.MonthNo,
                CumulativeProfile = d.Profile,
                CumulativeCost = d.Cost
            }).ToList();
        }
    }
}