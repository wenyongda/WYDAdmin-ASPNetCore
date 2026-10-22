using System;
using System.Collections.Generic;
using System.Text;

namespace WYD.Model.System;

public class SysConfig
{
    /// <summary>
    /// 配置id
    /// </summary>
    public int ConfigId { get; set; }

    /// <summary>
    /// 参数名称
    /// </summary>
    public string ConfigName { get; set; }

    /// <summary>
    /// 参数键名
    /// </summary>
    public string ConfigKey { get; set; }

    /// <summary>
    /// 参数键值
    /// </summary>
    public string ConfigValue { get; set; }

    /// <summary>
    /// 系统内置（Y是 N否）
    /// </summary>
    public string ConfigType { get; set; }
}

