using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces.FPS;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Services.FPS
{
    /// <summary>
    /// Service implementation for the Resource Management Re-plan screen (frmRM_RePlan).
    /// </summary>
    public class ResourceMgmtReplanService : IResourceMgmtReplanService
    {
        private readonly IFpsApiClient _fpsClient;

        public ResourceMgmtReplanService(IFpsApiClient fpsClient)
        {
            _fpsClient = fpsClient;
        }

        /// <inheritdoc/>
        public async Task<ApiResponseDto<List<ResourceMgmtReplanViewDto>>> GetRePlanGridAsync(string workGroup, QueryParameters<string> query)
        {
            return await _fpsClient.FpsResourceMgmtReplan.GetRePlanGridAsync(workGroup, query);
        }

        /// <inheritdoc/>
        public async Task<ApiResponseDto<List<ResourceMgmtReplanStaffJobDto>>> GetStaffJobsAsync(string jobCode, string wgGrade, QueryParameters<string> query)
        {
            return await _fpsClient.FpsResourceMgmtReplan.GetStaffJobsAsync(jobCode, wgGrade, query);
        }

        /// <inheritdoc/>
        public async Task<ApiResponseDto<List<ResourceMgmtReplanStaffJobDto>>> GetStagedRowsAsync(string jobCode, string wgGrade)
        {
            return await _fpsClient.FpsResourceMgmtReplan.GetStagedRowsAsync(jobCode, wgGrade);
        }

        /// <inheritdoc/>
        public async Task<ApiResponseDto<bool>> CommitReplanAsync(string jobCode, string wgGrade)
        {
            return await _fpsClient.FpsResourceMgmtReplan.CommitReplanAsync(jobCode, wgGrade);
        }
    }
}
