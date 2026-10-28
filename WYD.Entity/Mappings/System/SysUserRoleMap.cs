using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WYD.Model.System;

namespace WYD.Entity.Mappings.System;

/// <summary>
/// SysUserRole 实体配置 - sys_user_role 表（用户与角色多对多）
/// </summary>
public class SysUserRoleMap : IEntityTypeConfiguration<SysUserRole>
{
    public void Configure(EntityTypeBuilder<SysUserRole> builder)
    {
        builder.ToTable("sys_user_role");

        builder.HasKey(e => new { e.UserId, e.RoleId });

        builder.Property(e => e.UserId)
            .HasColumnName("user_id")
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(e => e.RoleId)
            .HasColumnName("role_id")
            .HasColumnType("uuid")
            .IsRequired();

        builder.HasIndex(e => e.RoleId)
            .HasDatabaseName("IX_SYS_USER_ROLE_ROLE_ID");
    }
}
