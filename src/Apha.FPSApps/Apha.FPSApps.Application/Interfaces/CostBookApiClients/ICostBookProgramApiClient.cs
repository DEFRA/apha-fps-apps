using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.CostBook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apha.FPSApps.Application.Interfaces.CostBookApiClients
{
    public interface ICostBookProgramApiClient
    {
        Task<ApiResponseDto<List<ProgramDto>>> GetAllProgramsAsync();
    }
}
