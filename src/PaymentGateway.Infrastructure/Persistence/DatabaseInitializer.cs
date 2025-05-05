using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PaymentGateway.Domain.Merchants.Entities;
using PaymentGateway.Domain.Payments.Entities;
using PaymentGateway.Domain.Payments.ValueObjects;

namespace PaymentGateway.Infrastructure.Persistence
{
    public static class DatabaseInitializer
    { 
            
        private static async Task SeedMerchantsAsync(ApplicationDbContext dbContext)
        {
            if (await dbContext.Merchants.AnyAsync())
            {
                return;
            }
                
            var merchants = new List<Merchant>();
            
            // Admin merchant
            var adminMerchant = Merchant.Create(
                Guid.NewGuid(),
                "Admin",
                "admin_api_key_123",
                "admin_api_secret_xyz");
                
            if (!adminMerchant.IsError)
                merchants.Add(adminMerchant.Value);
                
            // Test merchant 1
            var testMerchant1 = Merchant.Create(
                Guid.NewGuid(),
                "Test Shop",
                "test_shop_key_456",
                "test_shop_secret_abc");
                
            if (!testMerchant1.IsError)
                merchants.Add(testMerchant1.Value);
                
            // Test merchant 2
            var testMerchant2 = Merchant.Create(
                Guid.NewGuid(),
                "Demo Store",
                "demo_store_key_789",
                "demo_store_secret_def");
                
            if (!testMerchant2.IsError)
                merchants.Add(testMerchant2.Value);
                
            // Add all valid merchants
            await dbContext.Merchants.AddRangeAsync(merchants);
            await dbContext.SaveChangesAsync();
        }
        
        private static async Task SeedPaymentsAsync(ApplicationDbContext dbContext)
        {
            // Check if we have any payments already
            if (await dbContext.Payments.AnyAsync())
                return;
                
            // Get merchant ids
            var merchants = await dbContext.Merchants.ToListAsync();
            if (!merchants.Any())
                return;
                
            var payments = new List<Payment>();
            
            foreach (var merchant in merchants)
            {
                // Skip admin merchant
                if (merchant.Name == "Admin")
                    continue;
                    
                // Add successful payment
                var money1 = Money.Create(99.99m, "USD");
                var card1 = Card.Create(
                    "4111111111111111", 
                    "Test Customer", 
                    12, 
                    DateTime.UtcNow.Year + 2, 
                    "123");
                    
                if (!money1.IsError && !card1.IsError)
                {
                    var payment1 = Payment.Create(
                        Guid.NewGuid(),
                        merchant.Id,
                        money1.Value,
                        card1.Value,
                        "idempotency_1_" + merchant.Id);
                        
                    if (!payment1.IsError)
                    {
                        var succeeded = payment1.Value.MarkAsSuccessful("acq_ref_" + Guid.NewGuid().ToString("N").Substring(0, 8));
                        if (!succeeded.IsError)
                            payments.Add(payment1.Value);
                    }
                }
                
                // Add failed payment
                var money2 = Money.Create(199.99m, "EUR");
                var card2 = Card.Create(
                    "5555555555554444", 
                    "Another Customer", 
                    10, 
                    DateTime.UtcNow.Year + 1, 
                    "456");
                    
                if (!money2.IsError && !card2.IsError)
                {
                    var payment2 = Payment.Create(
                        Guid.NewGuid(),
                        merchant.Id,
                        money2.Value,
                        card2.Value,
                        "idempotency_2_" + merchant.Id);
                        
                    if (!payment2.IsError)
                    {
                        payment2.Value.MarkAsFailed();
                        payments.Add(payment2.Value);
                    }
                }
                
                // Add pending payment
                var money3 = Money.Create(299.99m, "GBP");
                var card3 = Card.Create(
                    "378282246310005", 
                    "Third Customer", 
                    9, 
                    DateTime.UtcNow.Year + 3, 
                    "789");
                    
                if (!money3.IsError && !card3.IsError)
                {
                    var payment3 = Payment.Create(
                        Guid.NewGuid(),
                        merchant.Id,
                        money3.Value,
                        card3.Value,
                        "idempotency_3_" + merchant.Id);
                        
                    if (!payment3.IsError)
                        payments.Add(payment3.Value);
                }
            }
            
            // Add all valid payments
            await dbContext.Payments.AddRangeAsync(payments);
            await dbContext.SaveChangesAsync();
        }

        public static async Task SeedDirectAsync(ApplicationDbContext dbContext, IServiceProvider serviceProvider)
        {
            var logger = serviceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();
            
            try
            {
                // Check for existing data using direct SQL
                var connection = dbContext.Database.GetDbConnection();
                await connection.OpenAsync();
                
                try
                {
                    // Check merchants data
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "SELECT COUNT(*) FROM Merchants";
                        var merchantCount = Convert.ToInt32(await command.ExecuteScalarAsync());
                        
                        if (merchantCount == 0)
                        {
                            logger.LogInformation("Seeding merchant data");
                            await SeedMerchantsAsync(dbContext);
                        }
                        else
                        {
                            logger.LogInformation("Merchants table already has {count} records - skipping seed", merchantCount);
                        }
                    }
                    
                    // Check payments data
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = "SELECT COUNT(*) FROM Payments";
                        var paymentCount = Convert.ToInt32(await command.ExecuteScalarAsync());
                        
                        if (paymentCount == 0)
                        {
                            logger.LogInformation("Seeding payment data");
                            await SeedPaymentsAsync(dbContext);
                        }
                        else
                        {
                            logger.LogInformation("Payments table already has {count} records - skipping seed", paymentCount);
                        }
                    }
                }
                finally
                {
                    if (connection.State == System.Data.ConnectionState.Open)
                        await connection.CloseAsync();
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred during database seeding");
                throw;
            }
        }

    }
}