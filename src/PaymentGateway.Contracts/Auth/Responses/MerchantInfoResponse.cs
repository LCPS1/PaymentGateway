using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PaymentGateway.Contracts.Authentication.Responses
{
    public class MerchantInfoResponse
    {
        public string MerchantId { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;    
        public bool IsActive { get; init; }
        public DateTime CreatedAt { get; init; } 
    }
}