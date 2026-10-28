using Microsoft.AspNetCore.Mvc;
using WYD.Infrastructure.Attributes;
using WYD.Infrastructure.Controllers;
using WYD.Infrastructure.Exceptions;
using WYD.Infrastructure.Model;
using WYD.Infrastructure.WebExtensions;
using WYD.Model.Dto.System;
using WYD.ServiceCore.Services.IService;

namespace WYD.Admin.WebAPI.Controllers.System;

/// <summary>
/// 角色与菜单权限查询
/// </summary>
[Route("system/permission")]
[ApiController]
public class SysPermissionController : ApiBaseController
{
    private readonly ISysPermissionService _permissionService;
    private readonly ISysUserService _userService;

    public SysPermissionController(ISysPermissionService permissionService, ISysUserService userService)
    {
        _permissionService = permissionService;
        _userService = userService;
    }

    /// <summary>
    /// 获取指定用户的角色权限
    /// </summary>
    [HttpGet("{userId:guid}/roles")]
    [ActionPermissionFilter(Permission = "system:permission:query")]
    [Log(Title = "权限查询-角色")]
    public async Task<IActionResult> GetRolePermission(Guid userId)
    {
        var user = await _userService.GetByIdAsync(userId);
        if (user == null)
        {
            return ERROR("用户不存在");
        }

        var dto = new SysUserDto
        {
            UserId = user.UserId,
            UserName = user.UserName,
            NickName = user.NickName,
            DeptId = user.DeptId
        };

        var roles = await _permissionService.GetRolePermissionAsync(dto);
        return OK(roles);
    }

    /// <summary>
    /// 获取指定用户的菜单权限
    /// </summary>
    [HttpGet("{userId:guid}/menus")]
    [ActionPermissionFilter(Permission = "system:permission:query")]
    [Log(Title = "权限查询-菜单")]
    public async Task<IActionResult> GetMenuPermission(Guid userId)
    {
        var user = await _userService.GetByIdAsync(userId);
        if (user == null)
        {
            return ERROR("用户不存在");
        }

        var dto = new SysUserDto
        {
            UserId = user.UserId,
            UserName = user.UserName,
            NickName = user.NickName,
            DeptId = user.DeptId
        };

        var menus = await _permissionService.GetMenuPermissionAsync(dto);
        return OK(menus);
    }

    /// <summary>
    /// 获取当前登录用户的角色和菜单权限
    /// </summary>
    [HttpGet("current")]
    [Log(Title = "权限查询-当前用户")]
    public async Task<IActionResult> GetCurrentPermission()
    {
        var currentUser = HttpContext.GetCurrentUser();
        if (currentUser == null)
        {
            return ToResponse(new ApiResult((int)ResultCode.DENY, "未登录"));
        }

        var dto = new SysUserDto
        {
            UserId = currentUser.UserId,
            UserName = currentUser.UserName,
            NickName = currentUser.NickName,
            DeptId = currentUser.DeptId
        };

        var roles = await _permissionService.GetRolePermissionAsync(dto);
        var menus = await _permissionService.GetMenuPermissionAsync(dto);

        return OK(new
        {
            roles,
            permissions = menus
        });
    }
}
