using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WYD.Model.System;

namespace WYD.Entity.Mappings.System;

/// <summary>
/// SysMenu 实体配置 - sys_menu 表
/// 当前仅映射权限计算所需字段。
/// </summary>
public class SysMenuMap : IEntityTypeConfiguration<SysMenu>
{
    public void Configure(EntityTypeBuilder<SysMenu> builder)
    {
        builder.ToTable("sys_menu");

        builder.HasKey(e => e.MenuId);
        builder.Property(e => e.MenuId)
            .HasColumnName("menu_id")
            .HasColumnType("uuid")
            .ValueGeneratedOnAdd();

        builder.Property(e => e.MenuName)
            .HasColumnName("menu_name")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.ParentId)
            .HasColumnName("parent_id")
            .HasColumnType("uuid")
            .HasDefaultValue(Guid.Empty)
            .IsRequired();

        builder.Property(e => e.OrderNum)
            .HasColumnName("order_num")
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(e => e.MenuType)
            .HasColumnName("menu_type")
            .HasMaxLength(1)
            .IsRequired();

        builder.Property(e => e.Visible)
            .HasColumnName("visible")
            .HasMaxLength(1)
            .HasDefaultValue(UserConstants.NORMAL)
            .IsRequired();

        builder.Property(e => e.Status)
            .HasColumnName("status")
            .HasMaxLength(1)
            .HasDefaultValue(UserConstants.NORMAL)
            .IsRequired();

        builder.Property(e => e.Perms)
            .HasColumnName("perms")
            .HasMaxLength(100);

        builder.Property(e => e.Icon)
            .HasColumnName("icon")
            .HasMaxLength(100);

        builder.HasIndex(e => e.ParentId)
            .HasDatabaseName("IX_SYS_MENU_PARENT_ID");
    }
}
