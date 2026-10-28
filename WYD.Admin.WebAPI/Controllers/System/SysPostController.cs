using System.Text;
using Microsoft.AspNetCore.Mvc;
using WYD.Infrastructure.Attributes;
using WYD.Infrastructure.Controllers;
using WYD.Infrastructure.Exceptions;
using WYD.Infrastructure.Model;
using WYD.Infrastructure.WebExtensions;
using WYD.Model.Dto.System;
using WYD.Model.Mappers.System;
using WYD.Model.System;
using WYD.ServiceCore.Services.IService;

namespace WYD.Admin.WebAPI.Controllers.System;

/// <summary>
/// 岗位管理
/// </summary>
[Route("system/post")]
[ApiController]
public class SysPostController : ApiBaseController
{
    private readonly ISysPostService _postService;
    private readonly SysPostMapper _mapper;

    public SysPostController(ISysPostService postService, SysPostMapper mapper)
    {
        _postService = postService;
        _mapper = mapper;
    }

    /// <summary>
    /// 分页查询岗位列表
    /// </summary>
    [HttpGet("list")]
    [ActionPermissionFilter(Permission = "system:post:list")]
    [Log(Title = "岗位管理-查询")]
    public async Task<IActionResult> List([FromQuery] SysPostQueryDto query)
    {
        var result = await _postService.GetPageListAsync(query);
        return OK(result);
    }

    /// <summary>
    /// 根据岗位ID获取详细信息
    /// </summary>
    [HttpGet("{postId:guid}")]
    [ActionPermissionFilter(Permission = "system:post:query")]
    [Log(Title = "岗位管理-详情")]
    public async Task<IActionResult> GetInfo(Guid postId)
    {
        var info = await _postService.GetByIdAsync(postId);
        return info == null ? ERROR("岗位不存在") : OK(info);
    }

    /// <summary>
    /// 获取岗位选择框列表
    /// </summary>
    [HttpGet("optionselect")]
    [ActionPermissionFilter(Permission = "system:post:list")]
    public async Task<IActionResult> OptionSelect()
    {
        var posts = await _postService.GetAllAsync();
        return OK(posts);
    }

    /// <summary>
    /// 新增岗位
    /// </summary>
    [HttpPost]
    [ActionPermissionFilter(Permission = "system:post:add")]
    [Log(Title = "岗位管理-新增")]
    public async Task<IActionResult> Add([FromBody] SysPostDto dto)
    {
        if (dto == null || string.IsNullOrWhiteSpace(dto.PostName) || string.IsNullOrWhiteSpace(dto.PostCode))
        {
            return ERROR("岗位名称和岗位编码不能为空");
        }

        var entity = _mapper.ToEntity(dto);
        entity.PostId = Guid.Empty;

        if (UserConstants.NOT_UNIQUE == await _postService.CheckPostNameUniqueAsync(entity))
        {
            return ERROR($"新增岗位'{dto.PostName}'失败，岗位名称已存在");
        }

        if (UserConstants.NOT_UNIQUE == await _postService.CheckPostCodeUniqueAsync(entity))
        {
            return ERROR($"新增岗位'{dto.PostName}'失败，岗位编码已存在");
        }

        var postId = await _postService.InsertPostAsync(entity, HttpContext.GetName());
        return ToResponse(ApiResult.Ok("新增成功", new { postId }));
    }

    /// <summary>
    /// 修改岗位
    /// </summary>
    [HttpPut]
    [ActionPermissionFilter(Permission = "system:post:edit")]
    [Log(Title = "岗位管理-修改")]
    public async Task<IActionResult> Update([FromBody] SysPostDto dto)
    {
        if (dto == null || dto.PostId == Guid.Empty
            || string.IsNullOrWhiteSpace(dto.PostName)
            || string.IsNullOrWhiteSpace(dto.PostCode))
        {
            return ERROR("请求参数错误");
        }

        var entity = _mapper.ToEntity(dto);

        if (UserConstants.NOT_UNIQUE == await _postService.CheckPostNameUniqueAsync(entity))
        {
            return ERROR($"修改岗位'{dto.PostName}'失败，岗位名称已存在");
        }

        if (UserConstants.NOT_UNIQUE == await _postService.CheckPostCodeUniqueAsync(entity))
        {
            return ERROR($"修改岗位'{dto.PostName}'失败，岗位编码已存在");
        }

        var result = await _postService.UpdatePostAsync(entity, HttpContext.GetName());
        return result > 0
            ? ToResponse(ApiResult.Ok("修改成功"))
            : ERROR("岗位不存在或已被删除");
    }

    /// <summary>
    /// 批量删除岗位
    /// </summary>
    [HttpDelete("{postIds}")]
    [ActionPermissionFilter(Permission = "system:post:remove")]
    [Log(Title = "岗位管理-删除")]
    public async Task<IActionResult> Remove(string postIds)
    {
        var ids = ParseGuids(postIds);
        if (ids.Length == 0)
        {
            return ERROR("请选择要删除的岗位");
        }

        try
        {
            var result = await _postService.DeletePostByIdsAsync(ids, HttpContext.GetName());
            return result > 0
                ? ToResponse(ApiResult.Ok("删除成功"))
                : ERROR("岗位不存在或已被删除");
        }
        catch (CustomException ex)
        {
            return ERROR(ex.Message);
        }
    }

    /// <summary>
    /// 岗位导出（CSV）
    /// </summary>
    [HttpGet("export")]
    [ActionPermissionFilter(Permission = "system:post:export")]
    [Log(Title = "岗位管理-导出")]
    public async Task<IActionResult> Export()
    {
        var list = await _postService.GetAllAsync();

        var sb = new StringBuilder();
        sb.AppendLine("岗位ID,岗位编码,岗位名称,显示顺序,状态,创建时间");
        foreach (var post in list)
        {
            sb.AppendLine(string.Join(',',
                post.PostId,
                Csv(post.PostCode),
                Csv(post.PostName),
                post.PostSort,
                post.Status == UserConstants.NORMAL ? "正常" : "停用",
                post.CreateTime.ToString("yyyy-MM-dd HH:mm:ss")));
        }

        var content = Encoding.UTF8.GetPreamble()
            .Concat(Encoding.UTF8.GetBytes(sb.ToString()))
            .ToArray();

        return File(content, "text/csv; charset=utf-8", $"posts_{DateTime.Now:yyyyMMddHHmmss}.csv");
    }

    private static Guid[] ParseGuids(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Array.Empty<Guid>();
        }

        return value
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(item => Guid.TryParse(item, out var id) ? id : Guid.Empty)
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToArray();
    }

    private static string Csv(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        return value.Contains(',') || value.Contains('"') || value.Contains('\n')
            ? $"\"{value.Replace("\"", "\"\"")}\""
            : value;
    }
}
