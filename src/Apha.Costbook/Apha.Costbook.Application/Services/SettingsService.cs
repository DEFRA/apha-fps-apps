using Apha.Costbook.Application.Interfaces;
using Apha.Costbook.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Apha.Costbook.Application.Services
{
    public class SettingsService : ISettingsService
    {
        private readonly ISettingsRepository _settingsRepository;

        public SettingsService(ISettingsRepository settingsRepository)
        {
            _settingsRepository = settingsRepository;
        }

        public async Task<string?> GetSettingValueByIdAsync(string id)
        {
            return await _settingsRepository.GetSettingValueByIdAsync(id);
        }
    }
}
