using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WYD.Model.System;

namespace WYD.Entity.Mappings.System;

/// <summary>
/// SysRoleDept 实体配置 - sys_role_dept 表（角色与部门多对多，用于数据权限）
/// </summary>
public class SysRoleDeptMap : IEntityTypeConfiguration<SysRoleDept>
{
    public void Configure(EntityTypeBuilder<SysRoleDept> builder)
    {
        builder.ToTable("sys_role_dept");

        builder.HasKey(e => new { e.RoleId, e.DeptId });

        builder.Property(e => e.RoleId)
            .HasColumnName("role_id")
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(e => e.DeptId)
            .HasColumnName("dept_id")
            .HasColumnType("uuid")
            .IsRequired();

        builder.HasIndex(e => e.DeptId)
            .HasDatabaseName("IX_SYS_ROLE_DEPT_DEPT_ID");
    }
}
