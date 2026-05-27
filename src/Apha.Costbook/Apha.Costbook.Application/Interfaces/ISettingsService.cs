using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Apha.Costbook.Application.Interfaces
{
    public interface ISettingsService
    {
        Task<string?> GetSettingValueByIdAsync(string id);
    }
}
