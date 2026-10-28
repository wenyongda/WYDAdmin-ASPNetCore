using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WYD.Model.System;

namespace WYD.Entity.Mappings.System;

/// <summary>
/// SysUserPost 实体配置 - sys_user_post 表（用户与岗位多对多）
/// </summary>
public class SysUserPostMap : IEntityTypeConfiguration<SysUserPost>
{
    public void Configure(EntityTypeBuilder<SysUserPost> builder)
    {
        builder.ToTable("sys_user_post");

        builder.HasKey(e => new { e.UserId, e.PostId });

        builder.Property(e => e.UserId)
            .HasColumnName("user_id")
            .HasColumnType("uuid")
            .IsRequired();

        builder.Property(e => e.PostId)
            .HasColumnName("post_id")
            .HasColumnType("uuid")
            .IsRequired();

        builder.HasIndex(e => e.PostId)
            .HasDatabaseName("IX_SYS_USER_POST_POST_ID");
    }
}
