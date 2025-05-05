using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PaymentGateway.Contracts.Authentication.Responses
{
    public class AuthenticationResponse
    {
        public string Token { get; init; } = string.Empty;
        
        public string MerchantId { get; init; } = string.Empty;
        
        public string MerchantName { get; init; } = string.Empty;
        
        public DateTime ExpiresAt { get; init; }
    }
}