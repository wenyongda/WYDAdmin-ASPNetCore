using Riok.Mapperly.Abstractions;
using WYD.Model.Dto.System;
using WYD.Model.System;

namespace WYD.Model.Mappers.System;

/// <summary>
/// 岗位映射。
/// 使用 Mapperly 在编译期生成映射代码，替代 Mapster 的运行时反射映射。
/// 实体的 CreateName / UpdateName 与 DTO 专有的 UserNum 不参与映射，
/// 因此关闭未映射成员告警。
/// </summary>
[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class SysPostMapper
{
    /// <summary>
    /// DTO → 实体
    /// </summary>
    public partial SysPost ToEntity(SysPostDto source);

    /// <summary>
    /// 实体 → DTO
    /// </summary>
    public partial SysPostDto ToDto(SysPost source);
}
