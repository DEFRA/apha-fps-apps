using Apha.FPSApps.Application.Dtos;
using Apha.FPSApps.Application.Interfaces.Costbook;
using Apha.FPSApps.Application.Interfaces.CostBookApiClients;
using System;
using System.Collections.Generic;
using System.Text;

namespace Apha.FPSApps.Application.Services.Costbook
{
    public class CostBookSettingsService : ICostBookSettingsService
    {
        private readonly ICostBookApiClient _costBookClient;

        public CostBookSettingsService(ICostBookApiClient costBookClient)
        {
            _costBookClient = costBookClient;
        }
        public Task<ApiResponseDto<string>> GetSettingValueByIdAsync(string? id)
        {
            var response = _costBookClient.CostbookSettings.GetSettingValueByIdAsync(id);
            return response;
        }
    }
}
