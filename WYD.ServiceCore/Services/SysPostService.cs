using Microsoft.EntityFrameworkCore;
using WYD.Entity;
using WYD.Infrastructure.Exceptions;
using WYD.Model.Dto;
using WYD.Model.Dto.System;
using WYD.Model.System;
using WYD.ServiceCore.Services.IService;

namespace WYD.ServiceCore.Services;

/// <summary>
/// 岗位服务
/// </summary>
public class SysPostService : ISysPostService
{
    private readonly AdminDbContext _dbContext;

    public SysPostService(AdminDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <inheritdoc />
    public async Task<PageResult<SysPostDto>> GetPageListAsync(SysPostQueryDto query)
    {
        query ??= new SysPostQueryDto();
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize < 1 ? 20 : query.PageSize;

        var source = _dbContext.SysPosts.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.PostCode))
        {
            source = source.Where(p => p.PostCode.Contains(query.PostCode));
        }

        if (!string.IsNullOrWhiteSpace(query.PostName))
        {
            source = source.Where(p => p.PostName.Contains(query.PostName));
        }

        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            source = source.Where(p => p.Status == query.Status);
        }

        var total = await source.CountAsync();

        var items = await source
            .OrderBy(p => p.PostSort)
            .ThenBy(p => p.PostId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new SysPostDto
            {
                PostId = p.PostId,
                PostCode = p.PostCode,
                PostName = p.PostName,
                PostSort = p.PostSort,
                Status = p.Status,
                Remark = p.Remark,
                CreateBy = p.CreateBy,
                CreateTime = p.CreateTime,
                UpdateBy = p.UpdateBy,
                UpdateTime = p.UpdateTime,
                UserNum = _dbContext.SysUserPosts.Count(up => up.PostId == p.PostId)
            })
            .ToListAsync();

        return new PageResult<SysPostDto>
        {
            Items = items,
            Total = total,
            Page = page,
            PageSize = pageSize
        };
    }

    /// <inheritdoc />
    public Task<List<SysPost>> GetAllAsync()
    {
        return _dbContext.SysPosts
            .AsNoTracking()
            .OrderBy(p => p.PostSort)
            .ThenBy(p => p.PostId)
            .ToListAsync();
    }

    /// <inheritdoc />
    public Task<SysPost?> GetByIdAsync(Guid postId)
    {
        return _dbContext.SysPosts
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.PostId == postId);
    }

    /// <inheritdoc />
    public async Task<string> CheckPostNameUniqueAsync(SysPost sysPost)
    {
        if (sysPost == null || string.IsNullOrWhiteSpace(sysPost.PostName))
        {
            return UserConstants.UNIQUE;
        }

        var exists = await _dbContext.SysPosts.AnyAsync(p =>
            p.PostName == sysPost.PostName
            && p.PostId != sysPost.PostId);

        return exists ? UserConstants.NOT_UNIQUE : UserConstants.UNIQUE;
    }

    /// <inheritdoc />
    public async Task<string> CheckPostCodeUniqueAsync(SysPost sysPost)
    {
        if (sysPost == null || string.IsNullOrWhiteSpace(sysPost.PostCode))
        {
            return UserConstants.UNIQUE;
        }

        var exists = await _dbContext.SysPosts.AnyAsync(p =>
            p.PostCode == sysPost.PostCode
            && p.PostId != sysPost.PostId);

        return exists ? UserConstants.NOT_UNIQUE : UserConstants.UNIQUE;
    }

    /// <inheritdoc />
    public async Task<Guid> InsertPostAsync(SysPost post, string? createdBy = null)
    {
        ArgumentNullException.ThrowIfNull(post);

        if (post.PostId == Guid.Empty)
        {
            post.PostId = Guid.NewGuid();
        }

        post.CreateBy = string.IsNullOrWhiteSpace(createdBy) ? post.CreateBy : createdBy;
        post.CreateName = string.IsNullOrWhiteSpace(createdBy) ? post.CreateName : createdBy;
        post.CreateTime = DateTime.UtcNow;

        _dbContext.SysPosts.Add(post);
        await _dbContext.SaveChangesAsync();
        return post.PostId;
    }

    /// <inheritdoc />
    public async Task<int> UpdatePostAsync(SysPost post, string? updatedBy = null)
    {
        ArgumentNullException.ThrowIfNull(post);

        if (post.PostId == Guid.Empty)
        {
            return 0;
        }

        var entity = await _dbContext.SysPosts
            .FirstOrDefaultAsync(p => p.PostId == post.PostId);

        if (entity == null)
        {
            return 0;
        }

        entity.PostCode = post.PostCode;
        entity.PostName = post.PostName;
        entity.PostSort = post.PostSort;
        entity.Status = post.Status;
        entity.Remark = post.Remark;
        entity.UpdateBy = updatedBy;
        entity.UpdateName = updatedBy;
        entity.UpdateTime = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();
        return 1;
    }

    /// <inheritdoc />
    public async Task<int> DeletePostByIdsAsync(Guid[] postIds, string? updatedBy = null)
    {
        if (postIds == null || postIds.Length == 0)
        {
            return 0;
        }

        var ids = postIds.Where(id => id != Guid.Empty).Distinct().ToArray();
        if (ids.Length == 0)
        {
            return 0;
        }

        var assigned = await _dbContext.SysUserPosts
            .AnyAsync(up => ids.Contains(up.PostId));

        if (assigned)
        {
            throw new CustomException("岗位已分配用户,不能删除");
        }

        return await _dbContext.SysPosts
            .Where(p => ids.Contains(p.PostId))
            .ExecuteDeleteAsync();
    }
}
