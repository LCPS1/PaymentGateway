using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace PaymentGateway.Infrastructure.Common.Extensions
{
    public static class RetryExtensions
    {
        public static async Task ExecuteWithRetryAsync(Func<Task> operation, ILogger logger, int maxRetries = 5)
        {
            var retryCount = 0;
            var delay = TimeSpan.FromSeconds(5);
            
            while (true)
            {
                try
                {
                    await operation();
                    return;
                }
                catch (Exception ex)
                {
                    retryCount++;
                    
                    if (retryCount > maxRetries)
                    {
                        logger.LogError(ex, "Operation failed after {RetryCount} retries", retryCount);
                        throw;
                    }
                    
                    logger.LogWarning(ex, "Operation failed. Retrying in {Delay} seconds (Attempt {RetryCount}/{MaxRetries})", 
                        delay.TotalSeconds, retryCount, maxRetries);
                        
                    await Task.Delay(delay);
                    delay = TimeSpan.FromSeconds(Math.Min(delay.TotalSeconds * 1.5, 30)); // Exponential backoff
                }
            }
        }

    }
}