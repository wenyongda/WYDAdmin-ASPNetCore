using Riok.Mapperly.Abstractions;
using WYD.Model.Dto.PhoneRebindRecord;
using PhoneRebindRecordEntity = WYD.Model.PhoneRebindRecord.PhoneRebindRecord;

namespace WYD.Model.Mappers;

/// <summary>
/// 电话号码换绑记录映射。
/// 使用 Mapperly 在编译期生成映射代码，替代 Mapster 的运行时反射映射。
/// 实体继承自 <see cref="WYD.Model.System.SysBase"/> 的审计字段
/// （UpdateBy/UpdateName/UpdateTime）以及 DelFlag 不参与 DTO 映射，
/// 因此关闭未映射成员告警。
/// </summary>
[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class PhoneRebindRecordMapper
{
    /// <summary>
    /// 实体 → DTO
    /// </summary>
    public partial PhoneRebindRecordDto ToDto(PhoneRebindRecordEntity source);

    /// <summary>
    /// 实体集合 → DTO 集合
    /// </summary>
    public partial List<PhoneRebindRecordDto> ToDto(List<PhoneRebindRecordEntity> sources);
}
