using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WYD.Model.System;
using WYD.Model.System.Enums;

namespace WYD.Entity.Mappings.System;

/// <summary>
/// SysRole 实体配置 - sys_role 表
/// 审计字段由 SysBaseMappingExtensions 统一配置。
/// </summary>
public class SysRoleMap : IEntityTypeConfiguration<SysRole>
{
    public void Configure(EntityTypeBuilder<SysRole> builder)
    {
        builder.ToTable("sys_role");

        builder.HasKey(e => e.RoleId);
        builder.Property(e => e.RoleId)
            .HasColumnName("role_id")
            .HasColumnType("uuid")
            .ValueGeneratedOnAdd();

        builder.Property(e => e.RoleName)
            .HasColumnName("role_name")
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(e => e.RoleKey)
            .HasColumnName("role_key")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.RoleSort)
            .HasColumnName("role_sort")
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(e => e.Status)
            .HasColumnName("status")
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(e => e.DelFlag)
            .HasColumnName("del_flag")
            .HasConversion<int>()
            .HasDefaultValue(DelFlagEnum.Normal)
            .IsRequired();

        builder.Property(e => e.DataScope)
            .HasColumnName("data_scope")
            .HasDefaultValue(1)
            .IsRequired();

        builder.Property(e => e.MenuCheckStrictly)
            .HasColumnName("menu_check_strictly")
            .HasDefaultValue(true)
            .IsRequired();

        builder.Property(e => e.DeptCheckStrictly)
            .HasColumnName("dept_check_strictly")
            .HasDefaultValue(true)
            .IsRequired();

        // 非数据库字段
        builder.Ignore(e => e.MenuIds);
        builder.Ignore(e => e.DeptIds);
        builder.Ignore(e => e.UserNum);

        builder.HasIndex(e => e.RoleKey)
            .HasDatabaseName("IX_SYS_ROLE_ROLE_KEY");
    }
}
