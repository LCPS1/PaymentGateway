using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PaymentGateway.Contracts.Payments.Responses
{
    public class PaymentResponse
    {
        public Guid PaymentId { get; init; }      
        public string Status { get; init; } = string.Empty;      
        public decimal Amount { get; init; }
        public string Currency { get; init; } = string.Empty;
        public string CardLast4 { get; init; } = string.Empty;
        public string CardBrand { get; init; } = string.Empty;     
        public DateTime CreatedAt { get; init; }
        public DateTime? ProcessedAt { get; init; }       
        public string? Reference { get; init; }
        public string? IdempotencyKey { get; init; }
    }
}