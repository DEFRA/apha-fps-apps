using Apha.Costbook.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apha.Costbook.Core.Interfaces
{
    public interface IDiseaseRepository
    {
        Task<List<Disease>> GetAllDiseasesAsync();
    }
}
