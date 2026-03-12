using Apha.FPS.Application.Interfaces;
using Apha.FPS.Core.Interfaces;

namespace Apha.FPS.Application.Services
{
    public class StatusService : IStatusService
    {
        private readonly IStatusRepository _statusRepository;

        public StatusService(IStatusRepository statusRepository)
        {
            _statusRepository = statusRepository;
        }

        public async Task<IEnumerable<string>> GetAllStatusesAsync()
        {
            var statuses = await _statusRepository.GetAllStatusesAsync();
            return statuses.Select(s => s.StatusValue);
        }
    }
}
