using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WYD.Model.System;
using WYD.Model.System.Enums;

namespace WYD.Entity.Mappings.System;

/// <summary>
/// SysUser 实体配置 - sys_user 表
/// 审计字段（CreateBy/CreateName/CreateTime/UpdateBy/UpdateName/UpdateTime/Remark）
/// 由 SysBaseMappingExtensions 统一配置，此处不再重复。
/// </summary>
public class SysUserMap : IEntityTypeConfiguration<SysUser>
{
    public void Configure(EntityTypeBuilder<SysUser> builder)
    {
        builder.ToTable("sys_user");

        builder.HasKey(e => e.UserId);
        builder.Property(e => e.UserId)
            .HasColumnName("user_id")
            .HasColumnType("uuid")
            .ValueGeneratedOnAdd();

        builder.Property(e => e.UserName)
            .HasColumnName("user_name")
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(e => e.NickName)
            .HasColumnName("nick_name")
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(e => e.UserType)
            .HasColumnName("user_type")
            .HasMaxLength(2)
            .HasDefaultValue("00")
            .IsRequired();

        builder.Property(e => e.Avatar)
            .HasColumnName("avatar")
            .HasMaxLength(255);

        builder.Property(e => e.Email)
            .HasColumnName("email")
            .HasMaxLength(50);

        builder.Property(e => e.Password)
            .HasColumnName("password")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.PhoneNumber)
            .HasColumnName("phone_number")
            .HasMaxLength(20);

        builder.Property(e => e.Gender)
            .HasColumnName("gender")
            .HasMaxLength(10);

        builder.Property(e => e.Status)
            .HasColumnName("status")
            .HasMaxLength(1)
            .HasDefaultValue("0")
            .IsRequired();

        builder.Property(e => e.DelFlag)
            .HasColumnName("del_flag")
            .HasConversion<int>()
            .HasDefaultValue(DelFlagEnum.Normal)
            .IsRequired();

        builder.Property(e => e.LoginIP)
            .HasColumnName("login_ip")
            .HasMaxLength(50);

        builder.Property(e => e.LoginDate)
            .HasColumnName("login_date");

        builder.Property(e => e.DeptId)
            .HasColumnName("dept_id")
            .HasColumnType("uuid")
            .HasDefaultValue(Guid.Empty)
            .IsRequired();

        builder.Property(e => e.Province)
            .HasColumnName("province")
            .HasMaxLength(50);

        builder.Property(e => e.City)
            .HasColumnName("city")
            .HasMaxLength(50);

        builder.Property(e => e.PwdUpdateTime)
            .HasColumnName("pwd_update_time");

        // 非数据库字段
        builder.Ignore(e => e.IsAdmin);
        builder.Ignore(e => e.RoleIds);
        builder.Ignore(e => e.PostIds);

        // 登录名唯一
        builder.HasIndex(e => e.UserName)
            .IsUnique()
            .HasDatabaseName("IX_SYS_USER_USERNAME");
    }
}
