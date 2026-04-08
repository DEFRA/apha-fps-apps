using Apha.FPSApps.Application.Dtos.CostBook;
using Apha.FPSApps.Application.Interfaces.CostBookApiClients;
using Apha.FPSApps.Application.Interfaces.Costbook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Apha.FPSApps.Application.Dtos;

namespace Apha.FPSApps.Application.Services.Costbook
{
    public class CostBookContractService : ICostBookContractService
    {
        private readonly ICostBookApiClient _costBookClient;

        public CostBookContractService(ICostBookApiClient costBookClient)
        {
            _costBookClient = costBookClient;
        }

        public Task<ApiResponseDto<List<ContractDto>>> GetAllContractNumbersAsync()
        {
            var response = _costBookClient.Contracts.GetAllContractNumbersAsync();
            return response;
        }
    }
}
