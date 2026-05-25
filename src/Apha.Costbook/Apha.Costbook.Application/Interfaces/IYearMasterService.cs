using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Apha.Costbook.Application.Interfaces
{
    public interface IYearMasterService
    {
        Task<int> GetOpenYearAsync();
    }
}
