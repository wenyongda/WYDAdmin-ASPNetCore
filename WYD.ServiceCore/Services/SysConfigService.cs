using Microsoft.EntityFrameworkCore;
using WYD.Entity;
using WYD.Model.System;
using WYD.ServiceCore.Services.IService;

namespace WYD.ServiceCore.Services;

/// <summary>
/// 参数配置Service业务层处理
/// </summary>
public class SysConfigService : ISysConfigService
{
    private readonly AdminDbContext _adminDbContext;

    public SysConfigService(AdminDbContext adminDbContext)
    {
        _adminDbContext = adminDbContext;
    }

    public Task<SysConfig?> GetSysConfigByKeyAsync(string key)
    => _adminDbContext.SysConfigs.FirstOrDefaultAsync(c => c.ConfigKey == key);
    
}
