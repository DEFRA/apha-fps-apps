using Apha.Costbook.Application.Interfaces;
using Apha.Costbook.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Apha.Costbook.Application.Services
{
    public class ContractService : IContractService
    {
        private readonly IContractRepository _repo;
        public ContractService(IContractRepository repo) => _repo = repo;

        public async Task<List<string>> GetAllContractNumbersAsync() =>
            await _repo.GetAllContractNumbersAsync();
    }
}
