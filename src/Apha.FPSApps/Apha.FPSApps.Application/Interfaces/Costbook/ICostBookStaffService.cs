using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Dtos.CostBook;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apha.FPSApps.Application.Interfaces.Costbook
{
    public interface ICostBookStaffService
    {
        Task<ApiResponseDto<List<StaffDto>>> GetAllStaffAsync();
    }
}
