using WYD.Model.Dto;
using WYD.Model.Dto.PhoneRebindRecord;

namespace WYD.Service.IService.PhoneRebindRecord
{
    /// <summary>
    /// 电话号码换绑记录服务接口
    /// </summary>
    public interface IPhoneRebindRecordService
    {
        /// <summary>
        /// 分页查询换绑记录
        /// </summary>
        Task<PageResult<PhoneRebindRecordDto>> GetPageListAsync(PhoneRebindRecordQueryDto query);

        /// <summary>
        /// 根据ID获取换绑记录详情
        /// </summary>
        Task<PhoneRebindRecordDto?> GetByIdAsync(Guid id);

        /// <summary>
        /// 新增换绑记录
        /// </summary>
        Task<Guid> CreateAsync(PhoneRebindRecordCreateDto dto, string createdBy);

        /// <summary>
        /// 更新换绑记录
        /// </summary>
        Task<bool> UpdateAsync(PhoneRebindRecordUpdateDto dto, string updatedBy);

        /// <summary>
        /// 删除换绑记录（软删除）
        /// </summary>
        Task<bool> DeleteAsync(Guid id, string deletedBy);

        /// <summary>
        /// 批量删除（软删除）
        /// </summary>
        Task<bool> BatchDeleteAsync(Guid[] ids, string deletedBy);
    }
}
