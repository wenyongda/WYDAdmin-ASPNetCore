using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WYD.Model.PhoneRebindRecord;
using WYD.Model.System.Enums;

namespace WYD.Entity.Mappings
{
    /// <summary>
    /// PhoneRebindRecord entity configuration - t_phone_rebind_record table
    /// 审计字段（CreateBy/CreateName/CreateTime/UpdateBy/UpdateName/UpdateTime/Remark）
    /// 由 SysBaseMappingExtensions 统一配置，此处不再重复。
    /// </summary>
    public class PhoneRebindRecordMap : IEntityTypeConfiguration<PhoneRebindRecord>
    {
        public void Configure(EntityTypeBuilder<PhoneRebindRecord> builder)
        {
            builder.ToTable("t_phone_rebind_record");

            builder.HasKey(e => e.Id);
            builder.Property(e => e.Id)
                .HasColumnName("id")
                .HasColumnType("uuid")
                .ValueGeneratedOnAdd();

            builder.Property(e => e.OldPhone)
                .HasColumnName("old_phone")
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(e => e.NewPhone)
                .HasColumnName("new_phone")
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(e => e.AppName)
                .HasColumnName("app_name")
                .HasMaxLength(100)
                .IsRequired();

            builder.Property(e => e.RebindTime)
                .HasColumnName("rebind_time")
                .IsRequired();

            builder.Property(e => e.DelFlag)
                .HasColumnName("del_flag")
                .HasConversion<int>()
                .IsRequired()
                .HasDefaultValue(DelFlagEnum.Normal);

            // 索引
            builder.HasIndex(e => e.OldPhone).HasDatabaseName("IX_REBIND_OLDPHONE");
            builder.HasIndex(e => e.NewPhone).HasDatabaseName("IX_REBIND_NEWPHONE");
            builder.HasIndex(e => e.RebindTime).HasDatabaseName("IX_REBIND_TIME");
        }
    }
}
