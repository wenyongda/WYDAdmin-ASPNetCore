using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WYD.Model.System;

namespace WYD.Entity.Mappings.System;

/// <summary>
/// SysRoleMenu 实体配置 - sys_role_menu 表（角色与菜单多对多）
/// </summary>
public class SysRoleMenuMap : IEntityTypeConfiguration<SysRoleMenu>
{
    public void Configure(EntityTypeBuilder<SysRoleMenu> builder)
    {
        builder.ToTable("sys_role_menu");

        builder.HasKey(e => new { e.RoleId, e.MenuId });

        builder.Property(e => e.RoleId)
            .HasColumnName("role_id")
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(e => e.MenuId)
            .HasColumnName("menu_id")
            .HasColumnType("uuid")
            .IsRequired();

        builder.HasIndex(e => e.MenuId)
            .HasDatabaseName("IX_SYS_ROLE_MENU_MENU_ID");
    }
}
