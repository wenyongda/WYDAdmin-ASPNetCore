using Microsoft.EntityFrameworkCore;
using WYD.Entity;
using WYD.Infrastructure.Exceptions;
using WYD.Model.Dto;
using WYD.Model.Dto.System;
using WYD.Model.Mappers.System;
using WYD.Model.System;
using WYD.Model.System.Enums;
using WYD.ServiceCore.Services.IService;

namespace WYD.ServiceCore.Services;

/// <summary>
/// 角色服务
/// </summary>
public class SysRoleService : ISysRoleService
{
    private readonly AdminDbContext _dbContext;
    private readonly SysRoleMapper _mapper;

    public SysRoleService(AdminDbContext dbContext, SysRoleMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }

    /// <inheritdoc />
    public async Task<PageResult<SysRoleDto>> SelectRoleListAsync(SysRoleQueryDto query)
    {
        query ??= new SysRoleQueryDto();
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize < 1 ? 20 : query.PageSize;

        var source = _dbContext.SysRoles
            .AsNoTracking()
            .Where(r => r.DelFlag == DelFlagEnum.Normal);

        if (!string.IsNullOrWhiteSpace(query.RoleName))
        {
            source = source.Where(r => r.RoleName.Contains(query.RoleName));
        }

        if (!string.IsNullOrWhiteSpace(query.RoleKey))
        {
            source = source.Where(r => r.RoleKey == query.RoleKey);
        }

        if (query.Status.HasValue)
        {
            source = source.Where(r => r.Status == query.Status.Value);
        }

        if (query.BeginTime.HasValue)
        {
            source = source.Where(r => r.CreateTime >= query.BeginTime.Value);
        }

        if (query.EndTime.HasValue)
        {
            source = source.Where(r => r.CreateTime <= query.EndTime.Value);
        }

        var total = await source.CountAsync();

        var items = await source
            .OrderBy(r => r.RoleSort)
            .ThenBy(r => r.RoleId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new SysRoleDto
            {
                RoleId = r.RoleId,
                RoleName = r.RoleName,
                RoleKey = r.RoleKey,
                RoleSort = r.RoleSort,
                Status = r.Status,
                DataScope = r.DataScope,
                MenuCheckStrictly = r.MenuCheckStrictly,
                DeptCheckStrictly = r.DeptCheckStrictly,
                Remark = r.Remark,
                CreateBy = r.CreateBy,
                CreateTime = r.CreateTime,
                UpdateBy = r.UpdateBy,
                UpdateTime = r.UpdateTime,
                UserNum = _dbContext.SysUserRoles.Count(ur => ur.RoleId == r.RoleId)
            })
            .ToListAsync();

        return new PageResult<SysRoleDto>
        {
            Items = items,
            Total = total,
            Page = page,
            PageSize = pageSize
        };
    }

    /// <inheritdoc />
    public Task<List<SysRole>> SelectRoleAllAsync()
    {
        return _dbContext.SysRoles
            .AsNoTracking()
            .Where(r => r.DelFlag == DelFlagEnum.Normal)
            .OrderBy(r => r.RoleSort)
            .ThenBy(r => r.RoleId)
            .ToListAsync();
    }

    /// <inheritdoc />
    public Task<List<SysRole>> SelectRolePermissionByUserIdAsync(Guid userId)
    {
        var roleIds = _dbContext.SysUserRoles
            .Where(ur => ur.UserId == userId)
            .Select(ur => ur.RoleId);

        return _dbContext.SysRoles
            .AsNoTracking()
            .Where(r => r.DelFlag == DelFlagEnum.Normal && roleIds.Contains(r.RoleId))
            .OrderBy(r => r.RoleSort)
            .ThenBy(r => r.RoleId)
            .ToListAsync();
    }

    /// <inheritdoc />
    public Task<SysRole?> SelectRoleByIdAsync(Guid roleId)
    {
        return _dbContext.SysRoles
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.RoleId == roleId && r.DelFlag == DelFlagEnum.Normal);
    }

    /// <inheritdoc />
    public async Task<SysRoleDto?> GetRoleDetailAsync(Guid roleId)
    {
        var role = await _dbContext.SysRoles
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.RoleId == roleId && r.DelFlag == DelFlagEnum.Normal);

        if (role == null)
        {
            return null;
        }

        var dto = _mapper.ToDto(role);
        dto.MenuIds = await SelectUserRoleMenusAsync(roleId);
        dto.DeptIds = await SelectRoleDeptsAsync(roleId);
        dto.UserNum = await _dbContext.SysUserRoles.CountAsync(ur => ur.RoleId == roleId);
        return dto;
    }

    /// <inheritdoc />
    public async Task<int> DeleteRoleByRoleIdAsync(Guid[] roleIds, string? updatedBy = null)
    {
        if (roleIds == null || roleIds.Length == 0)
        {
            return 0;
        }

        var ids = roleIds.Where(id => id != Guid.Empty).Distinct().ToArray();
        if (ids.Length == 0)
        {
            return 0;
        }

        var roles = await _dbContext.SysRoles
            .Where(r => ids.Contains(r.RoleId) && r.DelFlag == DelFlagEnum.Normal)
            .ToListAsync();

        foreach (var role in roles)
        {
            if (IsAdminRole(role.RoleKey))
            {
                throw new CustomException("不允许操作超级管理员角色");
            }

            if (await _dbContext.SysUserRoles.AnyAsync(ur => ur.RoleId == role.RoleId))
            {
                throw new CustomException($"{role.RoleName}已分配,不能删除");
            }
        }

        if (roles.Count == 0)
        {
            return 0;
        }

        var now = DateTime.UtcNow;
        await using var transaction = await _dbContext.Database.BeginTransactionAsync();

        var affected = await _dbContext.SysRoles
            .Where(r => ids.Contains(r.RoleId) && r.DelFlag == DelFlagEnum.Normal)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(r => r.DelFlag, DelFlagEnum.Deleted)
                .SetProperty(r => r.UpdateTime, now)
                .SetProperty(r => r.UpdateBy, updatedBy));

        // 清理角色关联数据，避免残留脏数据
        await _dbContext.SysRoleMenus
            .Where(rm => ids.Contains(rm.RoleId))
            .ExecuteDeleteAsync();

        await _dbContext.SysRoleDepts
            .Where(rd => ids.Contains(rd.RoleId))
            .ExecuteDeleteAsync();

        await transaction.CommitAsync();
        return affected;
    }

    /// <inheritdoc />
    public async Task<int> UpdateRoleStatusAsync(SysRole roleDto, string? updatedBy = null)
    {
        if (roleDto == null || roleDto.RoleId == Guid.Empty)
        {
            return 0;
        }

        await CheckRoleAllowedAsync(roleDto);

        var now = DateTime.UtcNow;
        return await _dbContext.SysRoles
            .Where(r => r.RoleId == roleDto.RoleId && r.DelFlag == DelFlagEnum.Normal)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(r => r.Status, roleDto.Status)
                .SetProperty(r => r.UpdateTime, now)
                .SetProperty(r => r.UpdateBy, updatedBy));
    }

    /// <inheritdoc />
    public async Task<string> CheckRoleKeyUniqueAsync(SysRole sysRole)
    {
        if (sysRole == null || string.IsNullOrWhiteSpace(sysRole.RoleKey))
        {
            return UserConstants.UNIQUE;
        }

        var exists = await _dbContext.SysRoles.AnyAsync(r =>
            r.DelFlag == DelFlagEnum.Normal
            && r.RoleKey == sysRole.RoleKey
            && r.RoleId != sysRole.RoleId);

        return exists ? UserConstants.NOT_UNIQUE : UserConstants.UNIQUE;
    }

    /// <inheritdoc />
    public async Task CheckRoleAllowedAsync(SysRole role)
    {
        if (role == null || role.RoleId == Guid.Empty)
        {
            return;
        }

        var roleKey = await _dbContext.SysRoles
            .AsNoTracking()
            .Where(r => r.RoleId == role.RoleId)
            .Select(r => r.RoleKey)
            .FirstOrDefaultAsync();

        if (IsAdminRole(roleKey))
        {
            throw new CustomException("不允许操作超级管理员角色");
        }
    }

    /// <inheritdoc />
    public async Task<Guid> InsertRoleAsync(SysRole sysRole, string? createdBy = null)
    {
        ArgumentNullException.ThrowIfNull(sysRole);

        if (sysRole.RoleId == Guid.Empty)
        {
            sysRole.RoleId = Guid.NewGuid();
        }

        var now = DateTime.UtcNow;
        sysRole.DelFlag = DelFlagEnum.Normal;
        sysRole.CreateBy = string.IsNullOrWhiteSpace(createdBy) ? sysRole.CreateBy : createdBy;
        sysRole.CreateName = string.IsNullOrWhiteSpace(createdBy) ? sysRole.CreateName : createdBy;
        sysRole.CreateTime = now;

        await using var transaction = await _dbContext.Database.BeginTransactionAsync();

        _dbContext.SysRoles.Add(sysRole);
        await _dbContext.SaveChangesAsync();

        await ReplaceRoleDeptsAsync(sysRole.RoleId, sysRole.DeptIds);

        await transaction.CommitAsync();
        return sysRole.RoleId;
    }

    /// <inheritdoc />
    public async Task<int> DeleteRoleMenuByRoleIdAsync(Guid roleId)
    {
        if (roleId == Guid.Empty)
        {
            return 0;
        }

        return await _dbContext.SysRoleMenus
            .Where(rm => rm.RoleId == roleId)
            .ExecuteDeleteAsync();
    }

    /// <inheritdoc />
    public async Task<bool> AuthDataScopeAsync(SysRoleDto role, string? updatedBy = null)
    {
        if (role == null || role.RoleId == Guid.Empty)
        {
            return false;
        }

        var exists = await _dbContext.SysRoles
            .AnyAsync(r => r.RoleId == role.RoleId && r.DelFlag == DelFlagEnum.Normal);

        if (!exists)
        {
            return false;
        }

        await CheckRoleAllowedAsync(new SysRole { RoleId = role.RoleId });

        await using var transaction = await _dbContext.Database.BeginTransactionAsync();

        var now = DateTime.UtcNow;
        await _dbContext.SysRoles
            .Where(r => r.RoleId == role.RoleId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(r => r.UpdateTime, now)
                .SetProperty(r => r.UpdateBy, updatedBy));

        // 菜单授权：按增量增删，避免全量重建
        var oldMenuIds = await _dbContext.SysRoleMenus
            .Where(rm => rm.RoleId == role.RoleId)
            .Select(rm => rm.MenuId)
            .ToListAsync();

        var newMenuIds = (role.MenuIds ?? new List<Guid>())
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToList();
        var deleteMenuIds = oldMenuIds.Except(newMenuIds).ToList();
        var addMenuIds = newMenuIds.Except(oldMenuIds).ToList();

        if (deleteMenuIds.Count > 0)
        {
            await _dbContext.SysRoleMenus
                .Where(rm => rm.RoleId == role.RoleId && deleteMenuIds.Contains(rm.MenuId))
                .ExecuteDeleteAsync();
        }

        if (addMenuIds.Count > 0)
        {
            _dbContext.SysRoleMenus.AddRange(addMenuIds.Select(menuId => new SysRoleMenu
            {
                RoleId = role.RoleId,
                MenuId = menuId
            }));
            await _dbContext.SaveChangesAsync();
        }

        await transaction.CommitAsync();
        return true;
    }

    /// <inheritdoc />
    public async Task<int> InsertRoleMenuAsync(SysRoleDto sysRoleDto)
    {
        if (sysRoleDto == null || sysRoleDto.RoleId == Guid.Empty)
        {
            return 0;
        }

        var menuIds = (sysRoleDto.MenuIds ?? new List<Guid>())
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToList();

        if (menuIds.Count == 0)
        {
            return 0;
        }

        var existingMenuIds = await _dbContext.SysRoleMenus
            .Where(rm => rm.RoleId == sysRoleDto.RoleId)
            .Select(rm => rm.MenuId)
            .ToListAsync();

        var addMenuIds = menuIds.Except(existingMenuIds).ToList();
        if (addMenuIds.Count == 0)
        {
            return 0;
        }

        _dbContext.SysRoleMenus.AddRange(addMenuIds.Select(menuId => new SysRoleMenu
        {
            RoleId = sysRoleDto.RoleId,
            MenuId = menuId
        }));

        await _dbContext.SaveChangesAsync();
        return addMenuIds.Count;
    }

    /// <inheritdoc />
    public async Task<bool> IsAdminAsync(Guid userId)
    {
        var roleKeys = await SelectUserRoleKeysAsync(userId);
        return roleKeys.Any(IsAdminRole);
    }

    /// <inheritdoc />
    public Task<List<Guid>> SelectUserRoleMenusAsync(Guid roleId)
    {
        return _dbContext.SysRoleMenus
            .AsNoTracking()
            .Where(rm => rm.RoleId == roleId)
            .Select(rm => rm.MenuId)
            .Distinct()
            .ToListAsync();
    }

    /// <inheritdoc />
    public Task<List<Guid>> SelectRoleDeptsAsync(Guid roleId)
    {
        return _dbContext.SysRoleDepts
            .AsNoTracking()
            .Where(rd => rd.RoleId == roleId)
            .Select(rd => rd.DeptId)
            .Distinct()
            .ToListAsync();
    }

    /// <inheritdoc />
    public Task<List<Guid>> SelectRoleMenuByRoleIdsAsync(Guid[] roleIds)
    {
        if (roleIds == null || roleIds.Length == 0)
        {
            return Task.FromResult(new List<Guid>());
        }

        return _dbContext.SysRoleMenus
            .AsNoTracking()
            .Where(rm => roleIds.Contains(rm.RoleId))
            .Select(rm => rm.MenuId)
            .Distinct()
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<List<SysRole>> SelectUserRoleListByUserIdAsync(Guid userId)
    {
        var roleIds = _dbContext.SysUserRoles
            .Where(ur => ur.UserId == userId)
            .Select(ur => ur.RoleId);

        return await _dbContext.SysRoles
            .AsNoTracking()
            .Where(r => r.DelFlag == DelFlagEnum.Normal && roleIds.Contains(r.RoleId))
            .OrderBy(r => r.RoleSort)
            .ThenBy(r => r.RoleId)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<List<Guid>> SelectUserRolesAsync(Guid userId)
    {
        var roles = await SelectUserRoleListByUserIdAsync(userId);
        return roles
            .Where(r => r.Status == UserConstants.ROLE_NORMAL)
            .Select(r => r.RoleId)
            .ToList();
    }

    /// <inheritdoc />
    public async Task<List<string>> SelectUserRoleKeysAsync(Guid userId)
    {
        var roles = await SelectUserRoleListByUserIdAsync(userId);
        return roles
            .Select(r => r.RoleKey)
            .Where(key => !string.IsNullOrWhiteSpace(key))
            .Distinct()
            .ToList();
    }

    /// <inheritdoc />
    public async Task<List<string>> SelectUserRoleNamesAsync(Guid userId)
    {
        var roles = await SelectUserRoleListByUserIdAsync(userId);
        return roles
            .Select(r => r.RoleName)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Distinct()
            .ToList();
    }

    /// <inheritdoc />
    public async Task<int> UpdateRoleAsync(SysRole sysRole, string? updatedBy = null)
    {
        ArgumentNullException.ThrowIfNull(sysRole);

        if (sysRole.RoleId == Guid.Empty)
        {
            return 0;
        }

        await CheckRoleAllowedAsync(sysRole);

        var now = DateTime.UtcNow;
        await using var transaction = await _dbContext.Database.BeginTransactionAsync();

        var affected = await _dbContext.SysRoles
            .Where(r => r.RoleId == sysRole.RoleId && r.DelFlag == DelFlagEnum.Normal)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(r => r.RoleName, sysRole.RoleName)
                .SetProperty(r => r.RoleKey, sysRole.RoleKey)
                .SetProperty(r => r.RoleSort, sysRole.RoleSort)
                .SetProperty(r => r.DataScope, sysRole.DataScope)
                .SetProperty(r => r.MenuCheckStrictly, sysRole.MenuCheckStrictly)
                .SetProperty(r => r.DeptCheckStrictly, sysRole.DeptCheckStrictly)
                .SetProperty(r => r.Remark, sysRole.Remark)
                .SetProperty(r => r.UpdateTime, now)
                .SetProperty(r => r.UpdateBy, updatedBy));

        if (affected > 0)
        {
            await ReplaceRoleDeptsAsync(sysRole.RoleId, sysRole.DeptIds);
        }

        await transaction.CommitAsync();
        return affected > 0 ? 1 : 0;
    }

    /// <inheritdoc />
    public async Task<int> UpdateSysRoleAsync(SysRole sysRole, string? updatedBy = null)
    {
        ArgumentNullException.ThrowIfNull(sysRole);

        if (sysRole.RoleId == Guid.Empty)
        {
            return 0;
        }

        await CheckRoleAllowedAsync(sysRole);

        var now = DateTime.UtcNow;
        return await _dbContext.SysRoles
            .Where(r => r.RoleId == sysRole.RoleId && r.DelFlag == DelFlagEnum.Normal)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(r => r.RoleName, sysRole.RoleName)
                .SetProperty(r => r.RoleKey, sysRole.RoleKey)
                .SetProperty(r => r.RoleSort, sysRole.RoleSort)
                .SetProperty(r => r.DataScope, sysRole.DataScope)
                .SetProperty(r => r.Remark, sysRole.Remark)
                .SetProperty(r => r.DeptCheckStrictly, sysRole.DeptCheckStrictly)
                .SetProperty(r => r.UpdateTime, now)
                .SetProperty(r => r.UpdateBy, updatedBy));
    }

    /// <summary>
    /// 全量替换角色部门数据权限
    /// </summary>
    private async Task ReplaceRoleDeptsAsync(Guid roleId, IEnumerable<Guid>? deptIds)
    {
        await _dbContext.SysRoleDepts
            .Where(rd => rd.RoleId == roleId)
            .ExecuteDeleteAsync();

        var ids = deptIds?
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToList() ?? new List<Guid>();

        if (ids.Count == 0)
        {
            return;
        }

        _dbContext.SysRoleDepts.AddRange(ids.Select(deptId => new SysRoleDept
        {
            RoleId = roleId,
            DeptId = deptId
        }));

        await _dbContext.SaveChangesAsync();
    }

    /// <summary>
    /// 是否管理员角色
    /// </summary>
    private static bool IsAdminRole(string? roleKey)
    {
        return string.Equals(roleKey, UserConstants.AdminRole, StringComparison.OrdinalIgnoreCase);
    }
}
