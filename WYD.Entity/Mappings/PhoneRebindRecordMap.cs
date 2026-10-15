using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WYD.Model;

namespace WYD.Entity.Mappings
{
    /// <summary>
    /// PhoneRebindRecord entity configuration - t_phone_rebind_record table
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

            builder.Property(e => e.Remark)
                .HasColumnName("remark")
                .HasMaxLength(500);

            builder.Property(e => e.CreateBy)
                 .HasColumnName("create_by")
                 .HasMaxLength(50)
                 .IsRequired();

            builder.Property(e => e.CreateName)
                .HasColumnName("create_name")
                .HasMaxLength(64)
                .IsRequired();

            builder.Property(e => e.CreateTime)
                .HasColumnName("create_time")
                .IsRequired();

            builder.Property(e => e.UpdateBy)
                .HasColumnName("update_by")
                .HasMaxLength(50);

            builder.Property(e => e.UpdateName)
                .HasColumnName("update_name")
                .HasMaxLength(64);

            builder.Property(e => e.UpdateTime)
                .HasColumnName("update_time");

            builder.Property(e => e.IsDeleted)
                .HasColumnName("ISDELETED")
                .IsRequired()
                .HasDefaultValue(false);

            // 索引
            builder.HasIndex(e => e.OldPhone).HasDatabaseName("IX_REBIND_OLDPHONE");
            builder.HasIndex(e => e.NewPhone).HasDatabaseName("IX_REBIND_NEWPHONE");
            builder.HasIndex(e => e.RebindTime).HasDatabaseName("IX_REBIND_TIME");
        }
    }
}
