using WYD.Model.Dto;
using WYD.Model.Dto.System;
using WYD.Model.System;

namespace WYD.ServiceCore.Services.IService;

/// <summary>
/// 岗位服务接口
/// </summary>
public interface ISysPostService
{
    /// <summary>
    /// 分页查询岗位列表
    /// </summary>
    Task<PageResult<SysPostDto>> GetPageListAsync(SysPostQueryDto query);

    /// <summary>
    /// 查询所有岗位
    /// </summary>
    Task<List<SysPost>> GetAllAsync();

    /// <summary>
    /// 根据岗位ID查询岗位
    /// </summary>
    Task<SysPost?> GetByIdAsync(Guid postId);

    /// <summary>
    /// 校验岗位名称是否唯一
    /// </summary>
    Task<string> CheckPostNameUniqueAsync(SysPost sysPost);

    /// <summary>
    /// 校验岗位编码是否唯一
    /// </summary>
    Task<string> CheckPostCodeUniqueAsync(SysPost sysPost);

    /// <summary>
    /// 新增岗位
    /// </summary>
    Task<Guid> InsertPostAsync(SysPost post, string? createdBy = null);

    /// <summary>
    /// 修改岗位
    /// </summary>
    Task<int> UpdatePostAsync(SysPost post, string? updatedBy = null);

    /// <summary>
    /// 批量删除岗位
    /// </summary>
    Task<int> DeletePostByIdsAsync(Guid[] postIds, string? updatedBy = null);
}
