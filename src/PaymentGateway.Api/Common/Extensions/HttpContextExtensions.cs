using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PaymentGateway.Api.Common.Extensions
{
    public static class HttpContextExtensions
    {
        public static Guid? GetMerchantIdFromClaims(this HttpContext httpContext)
        {
            var merchantIdString = httpContext.User?.Claims
                .FirstOrDefault(c => c.Type == "merchant_id")?.Value;
                
            if (string.IsNullOrEmpty(merchantIdString))
                return null;
                
            if (Guid.TryParse(merchantIdString, out var merchantId))
                return merchantId;
                
            return null;
        }
        
        public static string? GetClientIpAddress(this HttpContext context)
        {
            return context.Connection.RemoteIpAddress?.ToString();
        }
    }
}