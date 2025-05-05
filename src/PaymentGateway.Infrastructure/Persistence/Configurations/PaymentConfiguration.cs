using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PaymentGateway.Domain.Payments.Entities;

namespace PaymentGateway.Infrastructure.Persistence.Configurations
{
   
    public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
    {
        public void Configure(EntityTypeBuilder<Payment> builder)
        {
            builder.HasKey(p => p.Id);
            
            builder.Property(p => p.MerchantId)
                .IsRequired();
                
            builder.Property(p => p.Status)
                .IsRequired();
                
            builder.Property(p => p.CreatedAt)
                .IsRequired();
                
            builder.Property(p => p.IdempotencyKey)
                .HasMaxLength(100);
                
            // Configure Money value object
            builder.OwnsOne(p => p.Amount, a =>
            {
                a.Property(m => m.Amount)
                    .HasColumnName("Amount")
                    .HasPrecision(18, 2)
                    .IsRequired();
                    
                a.Property(m => m.Currency)
                    .HasColumnName("Currency")
                    .HasMaxLength(3)
                    .IsRequired();
            });
            
            // Configure Card value object
            builder.OwnsOne(p => p.Card, c =>
            {
                c.Property(card => card.CardHolderName)
                    .HasColumnName("CardHolderName")
                    .HasMaxLength(100)
                    .IsRequired();
                    
                c.Property(card => card.CardNumberHash)
                    .HasColumnName("CardNumberHash")
                    .HasMaxLength(100)
                    .IsRequired();
                    
                c.Property(card => card.LastFourDigits)
                    .HasColumnName("CardLastFour")
                    .HasMaxLength(4)
                    .IsRequired();
                    
                c.Property(card => card.ExpiryMonth)
                    .HasColumnName("CardExpiryMonth")
                    .IsRequired();
                    
                c.Property(card => card.ExpiryYear)
                    .HasColumnName("CardExpiryYear")
                    .IsRequired();
                    
                c.Property(card => card.CardBrand)
                    .HasColumnName("CardBrand")
                    .HasMaxLength(50)
                    .IsRequired();
            });
            
            // Indexes for faster queries
            builder.HasIndex(p => p.MerchantId);
            
            // Create index for idempotency key - must be unique per merchant
            builder.HasIndex(p => new { p.MerchantId, p.IdempotencyKey })
                .IsUnique()
                .HasFilter("\"IdempotencyKey\" IS NOT NULL");
        }
    }
    }