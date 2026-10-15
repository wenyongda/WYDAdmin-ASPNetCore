using Microsoft.EntityFrameworkCore;
using WYD.Entity;
using WYD.Model;
using WYD.Service.IService;
using Mapster;
using WYD.Model.Dto;

namespace WYD.Service
{
    /// <summary>
    /// 电话号码换绑记录服务实现
    /// </summary>
    public class PhoneRebindRecordService : IPhoneRebindRecordService
    {
        private readonly AdminDbContext _dbContext;

        public PhoneRebindRecordService(AdminDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        /// <inheritdoc />
        public async Task<PageResult<PhoneRebindRecordDto>> GetPageListAsync(PhoneRebindRecordQueryDto query)
        {
            var page = query.Page < 1 ? 1 : query.Page;
            var pageSize = query.PageSize < 1 ? 20 : query.PageSize;

            // 构建基础查询（只读，不跟踪）
            var source = _dbContext.PhoneRebindRecords
                .AsNoTracking()
                .Where(e => !e.IsDeleted);

            // 动态过滤条件
            source = ApplyFilters(source, query);

            var total = await source.CountAsync();

            var items = await source
                .OrderByDescending(e => e.RebindTime)
                .ThenByDescending(e => e.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var dtos = items.Adapt<List<PhoneRebindRecordDto>>();

            return new PageResult<PhoneRebindRecordDto>
            {
                Items = dtos,
                Total = total,
                Page = page,
                PageSize = pageSize
            };
        }

        /// <inheritdoc />
        public async Task<PhoneRebindRecordDto?> GetByIdAsync(Guid id)
        {
            var entity = await _dbContext.PhoneRebindRecords
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);

            return entity?.Adapt<PhoneRebindRecordDto>();
        }

        /// <inheritdoc />
        public async Task<Guid> CreateAsync(PhoneRebindRecordCreateDto dto, string createdBy)
        {
            var entity = new PhoneRebindRecord
            {
                OldPhone = dto.OldPhone,
                NewPhone = dto.NewPhone,
                AppName = dto.AppName,
                Remark = dto.Remark,
                RebindTime = DateTime.UtcNow,
                CreateTime = DateTime.UtcNow,
                CreateBy = createdBy,
                IsDeleted = false
            };

            await _dbContext.PhoneRebindRecords.AddAsync(entity);
            await _dbContext.SaveChangesAsync();

            return entity.Id;
        }

        /// <inheritdoc />
        public async Task<bool> UpdateAsync(PhoneRebindRecordUpdateDto dto, string updatedBy)
        {
            var entity = await _dbContext.PhoneRebindRecords
                .FirstOrDefaultAsync(e => e.Id == dto.Id && !e.IsDeleted);

            if (entity == null) return false;

            entity.OldPhone = dto.OldPhone;
            entity.NewPhone = dto.NewPhone;
            entity.AppName = dto.AppName;
            entity.Remark = dto.Remark;
            entity.UpdateTime = DateTime.UtcNow;
            entity.UpdateBy = updatedBy;

            _dbContext.PhoneRebindRecords.Update(entity);
            await _dbContext.SaveChangesAsync();

            return true;
        }

        /// <inheritdoc />
        public async Task<bool> DeleteAsync(Guid id, string deletedBy)
        {
            var entity = await _dbContext.PhoneRebindRecords
                .FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted);

            if (entity == null) return false;

            entity.IsDeleted = true;
            entity.UpdateTime = DateTime.UtcNow;
            entity.UpdateBy = deletedBy;

            _dbContext.PhoneRebindRecords.Update(entity);
            await _dbContext.SaveChangesAsync();

            return true;
        }

        /// <inheritdoc />
        public async Task<bool> BatchDeleteAsync(Guid[] ids, string deletedBy)
        {
            if (ids == null || ids.Length == 0) return false;

            var now = DateTime.UtcNow;
            var affected = await _dbContext.PhoneRebindRecords
                .Where(e => ids.Contains(e.Id) && !e.IsDeleted)
                .ExecuteUpdateAsync(p => p
                    .SetProperty(x => x.IsDeleted, true)
                    .SetProperty(x => x.UpdateTime, now)
                    .SetProperty(x => x.UpdateBy, deletedBy));

            return affected > 0;
        }

        /// <summary>
        /// 应用过滤条件
        /// </summary>
        private static IQueryable<PhoneRebindRecord> ApplyFilters(IQueryable<PhoneRebindRecord> source, PhoneRebindRecordQueryDto query)
        {
            if (!string.IsNullOrWhiteSpace(query.OldPhone))
                source = source.Where(e => e.OldPhone.Contains(query.OldPhone));

            if (!string.IsNullOrWhiteSpace(query.NewPhone))
                source = source.Where(e => e.NewPhone.Contains(query.NewPhone));

            if (!string.IsNullOrWhiteSpace(query.AppName))
                source = source.Where(e => e.AppName.Contains(query.AppName));

            if (query.RebindTimeStart.HasValue)
                source = source.Where(e => e.RebindTime >= query.RebindTimeStart.Value);

            if (query.RebindTimeEnd.HasValue)
                source = source.Where(e => e.RebindTime <= query.RebindTimeEnd.Value);

            return source;
        }
    }
}
