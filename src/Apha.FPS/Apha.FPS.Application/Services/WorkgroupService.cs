using Apha.FPS.Application.Interfaces;
using Apha.FPS.Core.Interfaces;

namespace Apha.FPS.Application.Services
{
    /// <summary>
    /// Service implementation for Workgroup lookup operations.
    /// </summary>
    public class WorkgroupService : IWorkgroupService
    {
        private readonly IWorkgroupRepository _repository;

        public WorkgroupService(IWorkgroupRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task<List<string>> GetAllWorkgroupNamesAsync()
            => await _repository.GetAllWorkgroupNamesAsync();
    }
}
