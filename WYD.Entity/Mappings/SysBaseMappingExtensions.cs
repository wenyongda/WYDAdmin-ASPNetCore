using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using WYD.Model.System;

namespace WYD.Entity.Mappings
{
    /// <summary>
    /// 为所有继承 <see cref="SysBase"/> 的实体统一配置公共字段映射。
    /// 等价于 SqlSugar 在基类上标注 [SugarColumn] 后，所有子类自动复用映射。
    /// </summary>
    public static class SysBaseMappingExtensions
    {
        /// <summary>
        /// 统一配置 SysBase 中的审计字段与备注字段。
        /// 在 ApplyConfigurationsFromAssembly 之后调用，作为公共字段的最终约定。
        /// </summary>
        public static ModelBuilder ApplySysBaseMapping(this ModelBuilder modelBuilder)
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                if (!typeof(SysBase).IsAssignableFrom(entityType.ClrType))
                    continue;

                var builder = modelBuilder.Entity(entityType.ClrType);

                builder.Property(nameof(SysBase.CreateBy))
                    .HasColumnName("create_by")
                    .HasMaxLength(50)
                    .IsRequired();

                builder.Property(nameof(SysBase.CreateName))
                    .HasColumnName("create_name")
                    .HasMaxLength(64)
                    .IsRequired();

                builder.Property(nameof(SysBase.CreateTime))
                    .HasColumnName("create_time")
                    .IsRequired();

                builder.Property(nameof(SysBase.UpdateBy))
                    .HasColumnName("update_by")
                    .HasMaxLength(50);

                builder.Property(nameof(SysBase.UpdateName))
                    .HasColumnName("update_name")
                    .HasMaxLength(64);

                builder.Property(nameof(SysBase.UpdateTime))
                    .HasColumnName("update_time");

                builder.Property(nameof(SysBase.Remark))
                    .HasColumnName("remark")
                    .HasMaxLength(500);

                // 对应 SqlSugar 的 IsOnlyIgnoreUpdate：创建后不允许再修改
                builder.Property(nameof(SysBase.CreateBy))
                    .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
                builder.Property(nameof(SysBase.CreateName))
                    .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);
                builder.Property(nameof(SysBase.CreateTime))
                    .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Ignore);

                // 对应 SqlSugar 的 IsOnlyIgnoreInsert：新增时忽略，更新时写入
                builder.Property(nameof(SysBase.UpdateBy))
                    .Metadata.SetBeforeSaveBehavior(PropertySaveBehavior.Ignore);
                builder.Property(nameof(SysBase.UpdateName))
                    .Metadata.SetBeforeSaveBehavior(PropertySaveBehavior.Ignore);
                builder.Property(nameof(SysBase.UpdateTime))
                    .Metadata.SetBeforeSaveBehavior(PropertySaveBehavior.Ignore);
            }

            return modelBuilder;
        }
    }
}
