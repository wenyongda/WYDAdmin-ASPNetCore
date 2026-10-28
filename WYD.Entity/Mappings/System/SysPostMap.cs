using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WYD.Model.System;

namespace WYD.Entity.Mappings.System;

/// <summary>
/// SysPost 实体配置 - sys_post 表
/// 审计字段由 SysBaseMappingExtensions 统一配置。
/// </summary>
public class SysPostMap : IEntityTypeConfiguration<SysPost>
{
    public void Configure(EntityTypeBuilder<SysPost> builder)
    {
        builder.ToTable("sys_post");

        builder.HasKey(e => e.PostId);
        builder.Property(e => e.PostId)
            .HasColumnName("post_id")
            .HasColumnType("uuid")
            .ValueGeneratedOnAdd();

        builder.Property(e => e.PostCode)
            .HasColumnName("post_code")
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(e => e.PostName)
            .HasColumnName("post_name")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.PostSort)
            .HasColumnName("post_sort")
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(e => e.Status)
            .HasColumnName("status")
            .HasMaxLength(1)
            .HasDefaultValue(UserConstants.NORMAL)
            .IsRequired();

        builder.HasIndex(e => e.PostCode)
            .HasDatabaseName("IX_SYS_POST_POST_CODE");

        builder.HasIndex(e => e.PostName)
            .HasDatabaseName("IX_SYS_POST_POST_NAME");
    }
}
