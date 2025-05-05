using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using PaymentGateway.Contracts.Enums;

namespace PaymentGateway.Contracts.Payments.Responses
{
    public class PaymentStatusResponse
    {
        public Guid PaymentId { get; init; }
        public PaymentStatusEnum Status { get; init; }
        public decimal Amount { get; init; }
        public string Currency { get; init; } = string.Empty;
        public string CardLast4 { get; init; } = string.Empty;
        public string CardBrand { get; init; } = string.Empty;
        public DateTime CreatedAt { get; init; }  
        public DateTime? ProcessedAt { get; init; }
        public string? Reference { get; init; }
    }
}