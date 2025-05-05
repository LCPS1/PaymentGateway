using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PaymentGateway.Domain.Merchants.Entities;

namespace PaymentGateway.Infrastructure.Persistence.Configurations
{
    public class MerchantConfiguration : IEntityTypeConfiguration<Merchant>
    {
        public void Configure(EntityTypeBuilder<Merchant> builder)
        {
            builder.HasKey(m => m.Id);
            
            builder.Property(m => m.Name)
                .IsRequired()
                .HasMaxLength(100);
                
            builder.Property(m => m.ApiKey)
                .IsRequired()
                .HasMaxLength(100);
                
            builder.Property(m => m.ApiSecret)
                .IsRequired()
                .HasMaxLength(100);
                
            builder.Property(m => m.IsActive)
                .IsRequired();
                
            builder.Property(m => m.CreatedAt)
                .IsRequired();
                
            // Create index for API key (used for authentication)
            builder.HasIndex(m => m.ApiKey)
                .IsUnique();
        }
    }

}