using Apha.Costbook.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Apha.Costbook.Core.Interfaces
{
    public interface IYearMasterRepository
    {
        Task<int> GetOpenYearAsync();
    }
}
