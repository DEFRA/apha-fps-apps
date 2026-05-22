using Apha.Costbook.Application.Interfaces;
using Apha.Costbook.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Apha.Costbook.Application.Services
{
    public class YearMasterService : IYearMasterService
    {
        private readonly IYearMasterRepository _yearMasterRepository;

        public YearMasterService(IYearMasterRepository yearMasterRepository)
        {
            _yearMasterRepository = yearMasterRepository;
        }

        public async Task<int> GetOpenYearAsync()
        {
            return await _yearMasterRepository.GetOpenYearAsync();
        }
    }
}
