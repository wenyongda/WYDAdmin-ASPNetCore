using System;
using System.Collections.Generic;
using System.Text;
using WYD.Model.System;

namespace WYD.ServiceCore.Services.IService;

public interface ISysConfigService
{
    Task<SysConfig?> GetSysConfigByKeyAsync(string key);
}
