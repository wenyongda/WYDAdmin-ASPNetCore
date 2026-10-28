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
/// 角色管理
/// </summary>
[Route("system/role")]
[ApiController]
public class SysRoleController : ApiBaseController
{
    private readonly ISysRoleService _roleService;
    private readonly SysRoleMapper _mapper;

    public SysRoleController(ISysRoleService roleService, SysRoleMapper mapper)
    {
        _roleService = roleService;
        _mapper = mapper;
    }

    /// <summary>
    /// 分页查询角色列表
    /// </summary>
    [HttpGet("list")]
    [ActionPermissionFilter(Permission = "system:role:list")]
    [Log(Title = "角色管理-查询")]
    public async Task<IActionResult> List([FromQuery] SysRoleQueryDto query)
    {
        var result = await _roleService.SelectRoleListAsync(query);
        return OK(result);
    }

    /// <summary>
    /// 根据角色ID获取详细信息
    /// </summary>
    [HttpGet("{roleId:guid}")]
    [ActionPermissionFilter(Permission = "system:role:query")]
    [Log(Title = "角色管理-详情")]
    public async Task<IActionResult> GetInfo(Guid roleId)
    {
        var info = await _roleService.GetRoleDetailAsync(roleId);
        return info == null ? ERROR("角色不存在") : OK(info);
    }

    /// <summary>
    /// 获取角色选择框列表
    /// </summary>
    [HttpGet("optionselect")]
    [ActionPermissionFilter(Permission = "system:role:list")]
    public async Task<IActionResult> OptionSelect()
    {
        var roles = await _roleService.SelectRoleAllAsync();
        return OK(roles);
    }

    /// <summary>
    /// 新增角色
    /// </summary>
    [HttpPost("edit")]
    [ActionPermissionFilter(Permission = "system:role:add")]
    [Log(Title = "角色管理-新增")]
    public async Task<IActionResult> Add([FromBody] SysRoleDto dto)
    {
        if (dto == null || string.IsNullOrWhiteSpace(dto.RoleName) || string.IsNullOrWhiteSpace(dto.RoleKey))
        {
            return ERROR("角色名称和权限字符不能为空");
        }

        var entity = _mapper.ToEntity(dto);
        entity.RoleId = Guid.Empty;

        if (UserConstants.NOT_UNIQUE == await _roleService.CheckRoleKeyUniqueAsync(entity))
        {
            return ERROR($"新增角色'{dto.RoleName}'失败，角色权限已存在");
        }

        var roleId = await _roleService.InsertRoleAsync(entity, HttpContext.GetName());
        return ToResponse(ApiResult.Ok("新增成功", new { roleId }));
    }

    /// <summary>
    /// 修改角色
    /// </summary>
    [HttpPut("edit")]
    [ActionPermissionFilter(Permission = "system:role:edit")]
    [Log(Title = "角色管理-修改")]
    public async Task<IActionResult> Edit([FromBody] SysRoleDto dto)
    {
        if (dto == null || dto.RoleId == Guid.Empty
            || string.IsNullOrWhiteSpace(dto.RoleName)
            || string.IsNullOrWhiteSpace(dto.RoleKey))
        {
            return ERROR("请求参数错误");
        }

        try
        {
            var entity = _mapper.ToEntity(dto);
            await _roleService.CheckRoleAllowedAsync(entity);

            var info = await _roleService.SelectRoleByIdAsync(dto.RoleId);
            if (info == null)
            {
                return ERROR("角色不存在");
            }

            if (!string.Equals(info.RoleKey, dto.RoleKey, StringComparison.Ordinal)
                && UserConstants.NOT_UNIQUE == await _roleService.CheckRoleKeyUniqueAsync(entity))
            {
                return ERROR($"修改角色'{dto.RoleName}'失败，角色权限已存在");
            }

            var result = await _roleService.UpdateRoleAsync(entity, HttpContext.GetName());
            return result > 0
                ? ToResponse(ApiResult.Ok("修改成功"))
                : ERROR($"修改角色'{dto.RoleName}'失败，请联系管理员");
        }
        catch (CustomException ex)
        {
            return ERROR(ex.Message);
        }
    }

    /// <summary>
    /// 根据角色分配菜单权限
    /// </summary>
    [HttpPut("dataScope")]
    [ActionPermissionFilter(Permission = "system:role:authorize")]
    [Log(Title = "角色管理-授权")]
    public async Task<IActionResult> DataScope([FromBody] SysRoleDto dto)
    {
        if (dto == null || dto.RoleId == Guid.Empty)
        {
            return ERROR("请求参数错误");
        }

        try
        {
            var result = await _roleService.AuthDataScopeAsync(dto, HttpContext.GetName());
            return result
                ? ToResponse(ApiResult.Ok("授权成功"))
                : ERROR("角色不存在");
        }
        catch (CustomException ex)
        {
            return ERROR(ex.Message);
        }
    }

    /// <summary>
    /// 修改角色状态
    /// </summary>
    [HttpPut("changeStatus")]
    [ActionPermissionFilter(Permission = "system:role:edit")]
    [Log(Title = "角色管理-修改状态")]
    public async Task<IActionResult> ChangeStatus([FromBody] SysRole dto)
    {
        if (dto == null || dto.RoleId == Guid.Empty)
        {
            return ERROR("请求参数错误");
        }

        try
        {
            var result = await _roleService.UpdateRoleStatusAsync(dto, HttpContext.GetName());
            return result > 0
                ? ToResponse(ApiResult.Ok("修改成功"))
                : ERROR("角色不存在或已被删除");
        }
        catch (CustomException ex)
        {
            return ERROR(ex.Message);
        }
    }

    /// <summary>
    /// 批量删除角色
    /// </summary>
    [HttpDelete("{roleIds}")]
    [ActionPermissionFilter(Permission = "system:role:remove")]
    [Log(Title = "角色管理-删除")]
    public async Task<IActionResult> Remove(string roleIds)
    {
        var ids = ParseGuids(roleIds);
        if (ids.Length == 0)
        {
            return ERROR("请选择要删除的角色");
        }

        try
        {
            var result = await _roleService.DeleteRoleByRoleIdAsync(ids, HttpContext.GetName());
            return result > 0
                ? ToResponse(ApiResult.Ok("删除成功"))
                : ERROR("角色不存在或已被删除");
        }
        catch (CustomException ex)
        {
            return ERROR(ex.Message);
        }
    }

    /// <summary>
    /// 角色导出（CSV）
    /// </summary>
    [HttpGet("export")]
    [ActionPermissionFilter(Permission = "system:role:export")]
    [Log(Title = "角色管理-导出")]
    public async Task<IActionResult> Export()
    {
        var list = await _roleService.SelectRoleAllAsync();

        var sb = new StringBuilder();
        sb.AppendLine("角色ID,角色名称,权限字符,显示顺序,状态,创建时间");
        foreach (var role in list)
        {
            sb.AppendLine(string.Join(',',
                role.RoleId,
                Csv(role.RoleName),
                Csv(role.RoleKey),
                role.RoleSort,
                role.Status == UserConstants.ROLE_NORMAL ? "正常" : "停用",
                role.CreateTime.ToString("yyyy-MM-dd HH:mm:ss")));
        }

        var content = Encoding.UTF8.GetPreamble()
            .Concat(Encoding.UTF8.GetBytes(sb.ToString()))
            .ToArray();

        return File(content, "text/csv; charset=utf-8", $"roles_{DateTime.Now:yyyyMMddHHmmss}.csv");
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
