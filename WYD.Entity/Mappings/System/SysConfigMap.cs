using Microsoft.EntityFrameworkCore;
using System;
using WYD.Model.System;

namespace WYD.Entity.Mappings.System;

public class SysConfigMap : IEntityTypeConfiguration<SysConfig>
{
    public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<SysConfig> builder)
    {
        // Table Mappings
        builder.ToTable("sys_config", "public");

        // Primary Key
        builder.HasKey(t => t.ConfigId);

        // Properties
        builder.Property(t => t.ConfigId)
            .ValueGeneratedNever();

        // Column Mappings
        builder.Property(t => t.ConfigId).HasColumnName("config_id");
        builder.Property(t => t.ConfigName).HasColumnName("config_name").HasMaxLength(100);
        builder.Property(t => t.ConfigKey).HasColumnName("config_key").HasMaxLength(100);
        builder.Property(t => t.ConfigValue).HasColumnName("config_value").HasMaxLength(500);
        builder.Property(t => t.ConfigType).HasColumnName("config_type").HasMaxLength(1);
    }
}

