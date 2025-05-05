using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PaymentGateway.Infrastructure.Authentication.Models
{
    public class JwtSettings
    {
        public string Secret { get; set; } = string.Empty;
        public int ExpiryHours { get; set; } = 24;
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
    }
}