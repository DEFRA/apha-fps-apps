using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.CostBook;
using Apha.FPSApps.Application.Interfaces.CostBookApiClients;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Apha.FPSApps.Application.Interfaces.Costbook;

namespace Apha.FPSApps.Application.Services.Costbook
{
    public class CostBookProgramService : ICostBookProgramService
    {
        private readonly ICostBookApiClient _costBookClient;

        public CostBookProgramService(ICostBookApiClient costBookClient)
        {
            _costBookClient = costBookClient;
        }

        public Task<ApiResponseDto<List<ProgramDto>>> GetAllProgramsAsync()
        {
            var response = _costBookClient.Programs.GetAllProgramsAsync();
            return response;
        }
    }
}
