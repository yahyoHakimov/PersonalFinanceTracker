using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PersonalFinanceTracker.Domain.Entities;

namespace PersonalFinanceTracker.Infrastructure.Data.Configurations
{
    public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
    {
        public void Configure(EntityTypeBuilder<Transaction> builder)
        {
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Amount)
                .IsRequired()
                .HasPrecision(18, 2);

            builder.Property(t => t.Type)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(t => t.CategoryId)
                .IsRequired();

            builder.Property(t => t.UserId)
                .IsRequired();

            builder.Property(t => t.Note)
                .HasMaxLength(500);

            builder.Property(t => t.CreatedAt)
                .IsRequired();

            // Optimistic Concurrency
            builder.Property(t => t.RowVersion)
                .IsRowVersion()
                .ValueGeneratedOnAddOrUpdate(); // This ensures DB generates the value

            // Indexes
            builder.HasIndex(t => t.UserId);
            builder.HasIndex(t => t.CategoryId);
            builder.HasIndex(t => t.CreatedAt);
            builder.HasIndex(t => new { t.UserId, t.CreatedAt });

            // Relationships
            builder.HasOne(t => t.User)
                .WithMany(u => u.Transactions)
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(t => t.Category)
                .WithMany(c => c.Transactions)
                .HasForeignKey(t => t.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
