    using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Apha.Costbook.Core.Interfaces
{
    public interface ISettingsRepository
    {
        Task<string?> GetSettingValueByIdAsync(string id);
    }
}
