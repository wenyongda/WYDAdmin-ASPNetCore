using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text;

namespace WYD.Infrastructure.Extensions;

public static partial class Extensions
{

    #region 获取枚举的描述
    /// <summary>
    /// 获取枚举值对应的描述
    /// </summary>
    /// <param name="enumType"></param>
    /// <returns></returns>
    public static string GetDescription(this System.Enum enumType)
    {
        FieldInfo EnumInfo = enumType.GetType().GetField(enumType.ToString());
        if (EnumInfo != null)
        {
            DescriptionAttribute[] EnumAttributes = (DescriptionAttribute[])EnumInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (EnumAttributes.Length > 0)
            {
                return EnumAttributes[0].Description;
            }
        }
        return enumType.ToString();
    }
    #endregion
}

