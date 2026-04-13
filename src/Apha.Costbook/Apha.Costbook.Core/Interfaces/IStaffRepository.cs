using Apha.Costbook.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apha.Costbook.Core.Interfaces
{
    public interface IStaffRepository
    {
        Task<List<Staff>> GetAllStaffAsync();
    }
}
