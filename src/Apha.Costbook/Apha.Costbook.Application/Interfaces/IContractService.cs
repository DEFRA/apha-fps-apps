using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apha.Costbook.Application.Interfaces
{
    public interface IContractService
    {
       Task<List<string>> GetAllContractNumbersAsync();
    }
}
