using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WYD.Model.System;

namespace WYD.Entity.Mappings.System;

/// <summary>
/// SysLoginInfo 实体配置 - sys_login_info 表
/// </summary>
public class SysLoginInfoMap : IEntityTypeConfiguration<SysLoginInfo>
{
    public void Configure(EntityTypeBuilder<SysLoginInfo> builder)
    {
        builder.ToTable("sys_login_info");

        builder.HasKey(e => e.InfoId);
        builder.Property(e => e.InfoId)
            .HasColumnName("info_id")
            .HasColumnType("uuid")
            .ValueGeneratedOnAdd();

        builder.Property(e => e.UserName)
            .HasColumnName("user_name")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.Status)
            .HasColumnName("status")
            .HasMaxLength(1)
            .HasDefaultValue("0")
            .IsRequired();

        builder.Property(e => e.IpAddr)
            .HasColumnName("ip_addr")
            .HasMaxLength(50);

        builder.Property(e => e.LoginLocation)
            .HasColumnName("login_location")
            .HasMaxLength(255);

        builder.Property(e => e.Browser)
            .HasColumnName("browser")
            .HasMaxLength(100);

        builder.Property(e => e.Os)
            .HasColumnName("os")
            .HasMaxLength(100);

        builder.Property(e => e.Msg)
            .HasColumnName("msg")
            .HasMaxLength(500);

        builder.Property(e => e.LoginTime)
            .HasColumnName("login_time")
            .IsRequired();

        builder.Property(e => e.ClientId)
            .HasColumnName("client_id")
            .HasMaxLength(100);

        builder.Property(e => e.UserId)
            .HasColumnName("user_id")
            .HasColumnType("uuid");

        // 查询条件/展示字段，不落库
        builder.Ignore(e => e.BeginTime);
        builder.Ignore(e => e.EndTime);
        builder.Ignore(e => e.Position);

        builder.HasIndex(e => e.LoginTime)
            .HasDatabaseName("IX_SYS_LOGIN_INFO_LOGIN_TIME");

        builder.HasIndex(e => e.UserName)
            .HasDatabaseName("IX_SYS_LOGIN_INFO_USERNAME");
    }
}
