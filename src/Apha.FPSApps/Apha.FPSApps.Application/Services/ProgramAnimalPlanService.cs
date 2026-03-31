using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.FPS;
using Apha.FPSApps.Application.Interfaces;
using Apha.FPSApps.Application.Interfaces.FpsApiClients;
using Apha.FPSApps.Application.Pagination;

namespace Apha.FPSApps.Application.Services
{
    public class ProgramAnimalPlanService : IProgramAnimalPlanService
    {
        private readonly IFpsApiClient _fpsClient;

        public ProgramAnimalPlanService(IFpsApiClient fpsClient)
        {
            _fpsClient = fpsClient;
        }

        public async Task<ApiResponseDto<List<AnimalCostViewDto>>> GetAllAnimalCostAsync(QueryParameters<string> query, string jobCode)
            => await _fpsClient.FpsAnimalPlan.GetAllAnimalCostAsync(query, jobCode);

        public async Task<ApiResponseDto<List<AnimalDto>>> GetAnimalLookupAsync()
            => await _fpsClient.FpsAnimalPlan.GetAnimalLookupAsync();

        public async Task<ApiResponseDto<decimal?>> GetAnimalRateAsync(string animalType)
            => await _fpsClient.FpsAnimalPlan.GetAnimalRateAsync(animalType);

        public async Task<ApiResponseDto<AnimalRequestDto>> CreateAnimalCostAsync(AnimalRequestDto animalRequest)
            => await _fpsClient.FpsAnimalPlan.CreateAnimalCostAsync(animalRequest);

        public async Task<ApiResponseDto<AnimalRequestDto>> UpdateAnimalCostAsync(AnimalRequestDto animalRequest)
            => await _fpsClient.FpsAnimalPlan.UpdateAnimalCostAsync(animalRequest);

        public async Task<ApiResponseDto<bool>> DeleteAnimalCostAsync(int indCounter)
            => await _fpsClient.FpsAnimalPlan.DeleteAnimalCostAsync(indCounter);

        public async Task<ApiResponseDto<decimal>> GetTotalAnimalCostAsync(string jobCode)
            => await _fpsClient.FpsAnimalPlan.GetTotalAnimalCostAsync(jobCode);

        public async Task<ApiResponseDto<AnimalCostViewDto?>> GetAnimalCostViewByIdAsync(int indCounter, string jobCode)
            => await _fpsClient.FpsAnimalPlan.GetAnimalCostViewByIdAsync(indCounter, jobCode);
    }
}
