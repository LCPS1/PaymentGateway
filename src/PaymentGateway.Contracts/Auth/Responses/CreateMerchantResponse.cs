using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PaymentGateway.Contracts.Authentication.Responses
{
    public class CreateMerchantResponse
    {
        public Guid MerchantId { get; init; } = Guid.Empty;
        
        public string Name { get; init; } = string.Empty;
        
        public string ApiKey { get; init; } = string.Empty;
        
        public string ApiSecret { get; init; } = string.Empty;
        public DateTime CreatedAt { get; init; }
    }
}