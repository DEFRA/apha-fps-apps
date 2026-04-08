using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.CostBook;
using Apha.FPSApps.Application.Interfaces.CostBookApiClients;
using Apha.FPSApps.Application.Interfaces.Costbook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apha.FPSApps.Application.Services.Costbook
{
    public class CostBookDiseaseService : ICostBookDiseaseService
    {
        private readonly ICostBookApiClient _costBookClient;

        public CostBookDiseaseService(ICostBookApiClient costBookClient)
        {
            _costBookClient = costBookClient;
        }

        public Task<ApiResponseDto<List<DiseaseDto>>> GetAllDiseasesAsync()
        {
            var response = _costBookClient.Diseases.GetAllDiseasesAsync();
            return response;
        }
    }
}
