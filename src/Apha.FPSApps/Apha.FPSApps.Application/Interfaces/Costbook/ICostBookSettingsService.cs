using Apha.FPSApps.Application.Dtos;
using System;
using System.Collections.Generic;
using System.Text;

namespace Apha.FPSApps.Application.Interfaces.Costbook
{
    public interface ICostBookSettingsService
    {
        Task<ApiResponseDto<string>> GetSettingValueByIdAsync(string? id);
    }
}
