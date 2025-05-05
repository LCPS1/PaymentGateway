using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PaymentGateway.Contracts.Payments.Requests
{
    public class ProcessPaymentRequest
    {
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than zero")]
        public decimal Amount { get; init; }
        
        [Required]
        [StringLength(3, MinimumLength = 3)]
        public string Currency { get; init; } = string.Empty;
        
        [Required]
        [CreditCard]
        public string CardNumber { get; init; } = string.Empty;
        
        [Required]
        public string CardHolderName { get; init; } = string.Empty;
        
        [Required]
        [Range(1, 12)]
        public int ExpiryMonth { get; init; }
        
        [Required]
        [Range(2000, 2100)]
        public int ExpiryYear { get; init; }
        
        [Required]
        [RegularExpression(@"^\d{3,4}$", ErrorMessage = "CVV must be 3 or 4 digits")]
        public string CVV { get; init; } = string.Empty;
        
        [StringLength(100)]
        public string? IdempotencyKey { get; init; }
    }
}