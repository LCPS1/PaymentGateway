using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace PaymentGateway.Infrastructure.Payments.Models
{
    public class AcquirerPaymentRequest
    {
        public decimal Amount { get; set; }

        public string Currency { get; set; } = string.Empty;
        
        public string CardNumber { get; set; } = string.Empty;
        
        public string CardHolderName { get; set; } = string.Empty;
        
        public int ExpiryMonth { get; set; }
        
        public int ExpiryYear { get; set; }
        
        public string MerchantReference { get; set; } = string.Empty;
    }
}