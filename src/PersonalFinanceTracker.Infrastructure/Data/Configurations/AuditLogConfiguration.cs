using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using PersonalFinanceTracker.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalFinanceTracker.Infrastructure.Data.Configurations
{
    public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
    {
        public void Configure(EntityTypeBuilder<AuditLog> builder)
        {
            builder.HasKey(a => a.Id);

            builder.Property(a => a.UserId)
                .IsRequired();

            builder.Property(a => a.Action)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(a => a.EntityName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(a => a.EntityId)
                .IsRequired();

            builder.Property(a => a.OldValue)
                .HasColumnType("jsonb");

            builder.Property(a => a.NewValue)
                .HasColumnType("jsonb");

            builder.Property(a => a.IpAddress)
                .HasMaxLength(45);

            builder.Property(a => a.UserAgent)
                .HasMaxLength(500);

            builder.Property(a => a.CreatedAt)
                .IsRequired();

            // Indexes
            builder.HasIndex(a => a.UserId);
            builder.HasIndex(a => a.EntityName);
            builder.HasIndex(a => a.CreatedAt);
            builder.HasIndex(a => new { a.EntityName, a.EntityId });

            // Relationships
            builder.HasOne(a => a.User)
            .WithMany(u => u.AuditLogs)
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);
        }
    }
}
