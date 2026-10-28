using Riok.Mapperly.Abstractions;
using WYD.Model.Dto.System;
using WYD.Model.System;

namespace WYD.Model.Mappers.System;

/// <summary>
/// 角色映射。
/// 使用 Mapperly 在编译期生成映射代码，替代 Mapster 的运行时反射映射。
/// 实体的 DelFlag / CreateName / UpdateName 与 DTO 专有的 DelMenuIds 不参与映射，
/// 因此关闭未映射成员告警。
/// </summary>
[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class SysRoleMapper
{
    /// <summary>
    /// DTO → 实体
    /// </summary>
    [MapProperty(nameof(SysRoleDto.MenuIds), nameof(SysRole.MenuIds), Use = nameof(ToNullableGuidArray))]
    [MapProperty(nameof(SysRoleDto.DeptIds), nameof(SysRole.DeptIds), Use = nameof(ToNullableGuidArray))]
    public partial SysRole ToEntity(SysRoleDto source);

    /// <summary>
    /// 实体 → DTO
    /// </summary>
    public partial SysRoleDto ToDto(SysRole source);

    /// <summary>
    /// List&lt;Guid&gt; → Guid[]?，保持与 Mapster 一致的 null 语义（源为 null 时目标也为 null）
    /// </summary>
    [UserMapping(Default = false)]
    private static Guid[]? ToNullableGuidArray(List<Guid>? source) => source?.ToArray();
}
