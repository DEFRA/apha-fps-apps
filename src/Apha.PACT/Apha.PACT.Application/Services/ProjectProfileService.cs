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

        public async Task<IList<ProjectProfileDto>> GetProfileDataAsync(string project)
        {
            var data = await _repository.GetProfileDataAsync(project);
            return data.Select(d => new ProjectProfileDto
            {
                MonthNo = (int)d.MonthNo,
                Profile = d.Profile,
                TotalCost = d.Cost
            }).ToList();
        }

        public async Task<IList<ProjectProfileCumulativeDto>> GetCumulativeDataAsync(string project)
        {
            var data = await _repository.GetCumulativeDataAsync(project);
            return data.Select(d => new ProjectProfileCumulativeDto
            {
                MonthNo = (int)d.MonthNo,
                CumulativeProfile = d.Profile,
                CumulativeCost = d.Cost
            }).ToList();
        }
    }
}